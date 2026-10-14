using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Xtranslator
{
    // Text-only compatibility conversion of the pinned legacy Ollama TranslateGemma GGUF.
    // Parameter defaults match Google's Gemma 3 config and Ollama's compatmigrate/gemma3.go.
    // Tensor values are copied byte-for-byte. The source is opened read-only and never changed.
    internal static class LegacyGemmaConverter
    {
        private sealed class Tensor { internal string Name;internal ulong[] Shape;internal uint Type;internal long Offset,Length; }
        private sealed class Header {
            internal readonly Dictionary<string,byte[]> Values=new Dictionary<string,byte[]>();
            internal readonly List<Tensor> Tensors=new List<Tensor>();internal long DataOffset;internal int Alignment=32;
        }
        private static string ReadString(BinaryReader r)
        {
            ulong n=r.ReadUInt64();if(n>16*1024*1024||n>(ulong)(r.BaseStream.Length-r.BaseStream.Position))throw new InvalidDataException("GGUF 字符串长度无效。");
            byte[] bytes=r.ReadBytes((int)n);if(bytes.Length!=(int)n)throw new EndOfStreamException();return new UTF8Encoding(false,true).GetString(bytes);
        }
        private static void SkipValue(BinaryReader r,uint type,int depth)
        {
            if(depth>2)throw new InvalidDataException("GGUF 数组嵌套无效。");
            int size;
            switch(type) {
                case 0:case 1:case 7:size=1;break;case 2:case 3:size=2;break;
                case 4:case 5:case 6:size=4;break;case 10:case 11:case 12:size=8;break;
                case 8:ulong n=r.ReadUInt64();if(n>16*1024*1024)throw new InvalidDataException("GGUF 字符串过大。");size=(int)n;break;
                case 9:uint child=r.ReadUInt32();ulong count=r.ReadUInt64();if(count>2*1024*1024)throw new InvalidDataException("GGUF 数组过大。");for(ulong i=0;i<count;i++)SkipValue(r,child,depth+1);return;
                default:throw new InvalidDataException("不支持的 GGUF 元数据类型。");
            }
            if(size>r.BaseStream.Length-r.BaseStream.Position)throw new EndOfStreamException();r.BaseStream.Position+=size;
        }
        private static long Aligned(long n,int alignment){return checked((n+alignment-1)/alignment*alignment);}
        private static Header Read(BinaryReader r,CancellationToken token)
        {
            if(r.ReadUInt32()!=0x46554747)throw new InvalidDataException("不是 GGUF 文件。");uint version=r.ReadUInt32();if(version<2||version>3)throw new InvalidDataException("GGUF 版本不支持。");
            ulong tensors=r.ReadUInt64(),values=r.ReadUInt64();if(tensors>100000||values>100000)throw new InvalidDataException("GGUF 元数据数量异常。");
            var h=new Header();
            for(ulong i=0;i<values;i++) {
                token.ThrowIfCancellationRequested();string key=ReadString(r);long start=r.BaseStream.Position;SkipValue(r,r.ReadUInt32(),0);long end=r.BaseStream.Position;
                if(end>64*1024*1024||end-start>32*1024*1024)throw new InvalidDataException("GGUF 元数据超出限制。");r.BaseStream.Position=start;h.Values.Add(key,r.ReadBytes((int)(end-start)));
            }
            byte[] raw;if(h.Values.TryGetValue("general.alignment",out raw)) {
                if(raw.Length!=8||BitConverter.ToUInt32(raw,0)!=4)throw new InvalidDataException("GGUF 对齐参数无效。");h.Alignment=(int)BitConverter.ToUInt32(raw,4);
                if(h.Alignment<1||h.Alignment>4096||(h.Alignment&(h.Alignment-1))!=0)throw new InvalidDataException("GGUF 对齐参数无效。");
            }
            for(ulong i=0;i<tensors;i++) {
                token.ThrowIfCancellationRequested();var t=new Tensor{Name=ReadString(r)};uint dims=r.ReadUInt32();if(dims<1||dims>4)throw new InvalidDataException("GGUF 张量维数无效。");t.Shape=new ulong[dims];for(int j=0;j<dims;j++){t.Shape[j]=r.ReadUInt64();if(t.Shape[j]==0)throw new InvalidDataException("GGUF 张量为空。");}
                t.Type=r.ReadUInt32();t.Offset=checked((long)r.ReadUInt64());h.Tensors.Add(t);
            }
            h.DataOffset=Aligned(r.BaseStream.Position,h.Alignment);h.Tensors.Sort((a,b)=>a.Offset.CompareTo(b.Offset));
            if(h.Tensors.Select(t=>t.Name).Distinct().Count()!=h.Tensors.Count)throw new InvalidDataException("GGUF 张量名称重复。");
            for(int i=0;i<h.Tensors.Count;i++) {
                var t=h.Tensors[i];long end=i+1<h.Tensors.Count?h.Tensors[i+1].Offset:r.BaseStream.Length-h.DataOffset;
                if(t.Offset<0||t.Offset%h.Alignment!=0||end<=t.Offset||end>r.BaseStream.Length-h.DataOffset)throw new InvalidDataException("GGUF 张量范围无效或文件不完整。");t.Length=end-t.Offset;
            }
            return h;
        }
        internal static bool NeedsConversion(string path)
        {
            using(var r=new BinaryReader(File.OpenRead(path))){var h=Read(r,CancellationToken.None);return h.Values.ContainsKey("gemma3.block_count")&&(!h.Values.ContainsKey("gemma3.attention.layer_norm_rms_epsilon")||h.Tensors.Any(t=>t.Name.StartsWith("v.")||t.Name.StartsWith("mm.")));}
        }
        private static byte[] FloatValue(float v){using(var m=new MemoryStream())using(var w=new BinaryWriter(m)){w.Write((uint)6);w.Write(v);return m.ToArray();}}
        private static byte[] StringValue(string v){using(var m=new MemoryStream())using(var w=new BinaryWriter(m)){w.Write((uint)8);WriteString(w,v);return m.ToArray();}}
        private static void WriteString(BinaryWriter w,string v){byte[] b=Encoding.UTF8.GetBytes(v);w.Write((ulong)b.Length);w.Write(b);}
        private static byte[] TruncateArray(byte[] raw,ulong count)
        {
            using(var m=new MemoryStream(raw))using(var r=new BinaryReader(m)) {
                if(r.ReadUInt32()!=9)throw new InvalidDataException("GGUF 分词表不是数组。");uint type=r.ReadUInt32();ulong size=r.ReadUInt64();if(size<=count)return raw;
                for(ulong i=0;i<count;i++)SkipValue(r,type,0);int end=(int)m.Position;
                using(var dest=new MemoryStream())using(var w=new BinaryWriter(dest)){w.Write((uint)9);w.Write(type);w.Write(count);w.Write(raw,16,end-16);return dest.ToArray();}
            }
        }
        internal static void Convert(string source,string target,Action<string> progress,CancellationToken token)
        {
            if(File.Exists(target))throw new IOException("兼容模型已经存在，已保留原文件。");Directory.CreateDirectory(Path.GetDirectoryName(target));string temporary=target+"."+Guid.NewGuid().ToString("N")+".partial";
            try {
                using(var r=new BinaryReader(File.Open(source,FileMode.Open,FileAccess.Read,FileShare.Read))) {
                    Header h=Read(r,token);if(!h.Values.ContainsKey("gemma3.block_count"))throw new InvalidDataException("只有旧版 Gemma 3 模型需要此兼容转换。");
                    var tensors=h.Tensors.Where(t=>!t.Name.StartsWith("v.")&&!t.Name.StartsWith("mm.")).ToArray();var embedding=tensors.FirstOrDefault(t=>t.Name=="token_embd.weight");if(embedding==null)throw new InvalidDataException("模型缺少词嵌入张量。");ulong vocab=embedding.Shape.Last();
                    foreach(string key in new[]{"tokenizer.ggml.tokens","tokenizer.ggml.scores","tokenizer.ggml.token_type"})if(h.Values.ContainsKey(key))h.Values[key]=TruncateArray(h.Values[key],vocab);
                    foreach(string key in h.Values.Keys.Where(k=>k.StartsWith("gemma3.vision.")||k.StartsWith("gemma3.mm.")||k=="tokenizer.ggml.add_padding_token"||k=="tokenizer.ggml.add_unknown_token").ToArray())h.Values.Remove(key);
                    h.Values["gemma3.attention.layer_norm_rms_epsilon"]=FloatValue(1e-6f);h.Values["gemma3.rope.freq_base"]=FloatValue(1000000);h.Values["gemma3.rope.freq_base_swa"]=FloatValue(10000);h.Values["gemma3.rope.scaling.type"]=StringValue("linear");h.Values["gemma3.rope.scaling.factor"]=FloatValue(8);
                    long total=tensors.Sum(t=>t.Length);if(new DriveInfo(Path.GetPathRoot(Path.GetFullPath(target))).AvailableFreeSpace<total+128L*1024*1024)throw new IOException("生成 Google 兼容副本需要额外约 7.4 GB 磁盘空间，请选择其他模型目录。");
                    using(var w=new BinaryWriter(File.Create(temporary))) {
                        w.Write(0x46554747u);w.Write(3u);w.Write((ulong)tensors.Length);w.Write((ulong)h.Values.Count);
                        foreach(var kv in h.Values){WriteString(w,kv.Key);w.Write(kv.Value);}
                        long pos=0;foreach(var t in tensors){WriteString(w,t.Name);w.Write((uint)t.Shape.Length);foreach(ulong d in t.Shape)w.Write(d);w.Write(t.Type);w.Write((ulong)pos);pos=Aligned(pos+t.Length,h.Alignment);}
                        w.Write(new byte[(int)(Aligned(w.BaseStream.Position,h.Alignment)-w.BaseStream.Position)]);
                        byte[] buffer=new byte[1024*1024];long done=0;var clock=Stopwatch.StartNew();
                        foreach(var t in tensors){r.BaseStream.Position=h.DataOffset+t.Offset;long left=t.Length;while(left>0){token.ThrowIfCancellationRequested();int n=r.Read(buffer,0,(int)Math.Min(left,buffer.Length));if(n==0)throw new EndOfStreamException();w.Write(buffer,0,n);left-=n;done+=n;if(clock.ElapsedMilliseconds>200){progress?.Invoke("正在生成 Google 兼容副本："+(done*100/total)+"%（保留原文件）");clock.Restart();}}w.Write(new byte[(int)(Aligned(t.Length,h.Alignment)-t.Length)]);}
                    }
                }
                token.ThrowIfCancellationRequested();File.Move(temporary,target);
            } finally {if(File.Exists(temporary))File.Delete(temporary);}
        }
    }
}
