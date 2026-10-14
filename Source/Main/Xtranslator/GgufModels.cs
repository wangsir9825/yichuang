using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace Xtranslator
{
    internal sealed class GgufDownload
    {
        internal string Name, FileName, Digest, Url;
        internal long Size;
    }

    // Model weights live in user storage, never in the installed application payload.
    internal static class GgufModels
    {
        internal static readonly GgufDownload Hy = new GgufDownload {Name="腾讯 Hy-MT2-7B",FileName="hy-mt2-7b-q6.gguf",Size=6164482720,
            Digest="88ef0aba59952a4cfe4be36cb5baf797dbb370bc60e9dcbd7297036021e52831",Url="https://registry.ollama.ai/v2/kaelri/hy-mt2/blobs/sha256:88ef0aba59952a4cfe4be36cb5baf797dbb370bc60e9dcbd7297036021e52831"};
        internal static readonly GgufDownload Gemma = new GgufDownload {Name="Google TranslateGemma-12B",FileName="translategemma-12b-ollama.gguf",Size=8109818272,
            Digest="1b2b95e2f0eb9a98a839249ed41dfca71b300f9c389e14581210649fada910ed",Url="https://registry.ollama.ai/v2/library/translategemma/blobs/sha256:1b2b95e2f0eb9a98a839249ed41dfca71b300f9c389e14581210649fada910ed"};
        internal static string DefaultDirectory { get { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"YiChuang","Models"); } }
        internal static string DirectoryFor(LocalRuntimeOptions o) { return string.IsNullOrWhiteSpace(o.GgufDirectory)?DefaultDirectory:Path.GetFullPath(o.GgufDirectory); }
        internal static string Reusable(LocalRuntimeOptions options,GgufDownload spec)
        {
            foreach(string folder in new[]{options.ModelsDirectory,LocalRuntime.DefaultModelsDirectory()}.Where(s=>!string.IsNullOrWhiteSpace(s)).Distinct()) {
                string path=Path.Combine(folder,"blobs","sha256-"+spec.Digest);
                if(File.Exists(path)&&new FileInfo(path).Length==spec.Size)return path;
            }
            string downloaded=Path.Combine(DirectoryFor(options),spec.FileName);
            return File.Exists(downloaded)&&new FileInfo(downloaded).Length==spec.Size?downloaded:"";
        }
        internal static string Resolve(LocalRuntimeOptions options,bool hy)
        {
            string selected=hy?options.HyGguf:options.GemmaGguf;
            if(!string.IsNullOrWhiteSpace(selected))return selected;
            if(hy)return Reusable(options,Hy);
            string converted=Path.Combine(DirectoryFor(options),"translategemma-12b-llama-v1.gguf");
            return File.Exists(converted)&&File.Exists(converted+".receipt.json")?converted:"";
        }
        internal static void ValidateFile(string path)
        {
            if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new InvalidOperationException("未找到本地 GGUF 模型。请在设置 → 翻译引擎 → 本地模型配置向导中准备模型或选择已有文件。");
            using(var r=new BinaryReader(File.Open(path,FileMode.Open,FileAccess.Read,FileShare.Read))) {
                if(r.BaseStream.Length<24||r.ReadUInt32()!=0x46554747)throw new InvalidOperationException("所选文件不是 GGUF 模型，请重新选择完整的模型文件。");
                uint version=r.ReadUInt32();if(version<2||version>3)throw new InvalidOperationException("当前内置运行器支持 GGUF v2/v3，请选择兼容模型。");
            }
        }
        internal static string Prepare(LocalRuntimeOptions options,bool hy,Action<string> progress,CancellationToken token)
        {
            string selected=hy?options.HyGguf:options.GemmaGguf;
            if(!string.IsNullOrWhiteSpace(selected)){ValidateFile(selected);return hy?selected:PrepareGemma(selected,options,progress,token);}
            string existing=Resolve(options,hy);
            if(existing.Length>0){ValidateFile(existing);if(hy)Verify(existing,Hy.Digest,token,progress);else {var receipt=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(existing+".receipt.json"));Verify(existing,receipt["output"],token,progress);}return existing;}
            var spec=hy?Hy:Gemma;string source=Reusable(options,spec);
            if(source.Length==0){source=Path.Combine(DirectoryFor(options),spec.FileName);Download(spec,source,progress,token);}
            else {progress?.Invoke("正在校验已有模型："+spec.Name);Verify(source,spec.Digest,token,progress);}
            return hy?source:PrepareGemma(source,options,progress,token);
        }
        private static string PrepareGemma(string source,LocalRuntimeOptions options,Action<string> progress,CancellationToken token)
        {
            if(!LegacyGemmaConverter.NeedsConversion(source))return source;
            progress?.Invoke("正在校验 Google 原权重，准备生成 llama.cpp 兼容副本…");
            Verify(source,Gemma.Digest,token,progress);
            string target=Path.Combine(DirectoryFor(options),"translategemma-12b-llama-v1.gguf");
            // An existing conversion is accepted only with its matching source and output checksum.
            string receipt=target+".receipt.json";
            if(File.Exists(target)&&File.Exists(receipt))try {
                var item=JsonConvert.DeserializeObject<Dictionary<string,string>>(File.ReadAllText(receipt));
                if(item["source"]==Gemma.Digest){Verify(target,item["output"],token,progress);return target;}
            }catch(OperationCanceledException){throw;}catch{}
            if(File.Exists(target))throw new InvalidOperationException("转换目录中已有未通过校验的兼容副本。请选择新的模型目录以重新生成；原文件已保留。");
            string building=target+"."+Guid.NewGuid().ToString("N")+".building";
            try {
                LegacyGemmaConverter.Convert(source,building,progress,token);string hash=Hash(building,token,progress);token.ThrowIfCancellationRequested();File.Move(building,target);
                File.WriteAllText(receipt,JsonConvert.SerializeObject(new Dictionary<string,string>{{"source",Gemma.Digest},{"output",hash}}));
            }finally{if(File.Exists(building))File.Delete(building);}
            return target;
        }
        internal static string Hash(string path,CancellationToken token,Action<string> progress)
        {
            using(var sha=SHA256.Create())using(var input=File.OpenRead(path)) {
                byte[] b=new byte[1024*1024];int n;long done=0;var clock=Stopwatch.StartNew();
                while((n=input.Read(b,0,b.Length))>0){token.ThrowIfCancellationRequested();sha.TransformBlock(b,0,n,b,0);done+=n;if(clock.ElapsedMilliseconds>500){progress?.Invoke("正在校验模型完整性："+(done*100/input.Length)+"%");clock.Restart();}}
                sha.TransformFinalBlock(new byte[0],0,0);return BitConverter.ToString(sha.Hash).Replace("-","").ToLowerInvariant();
            }
        }
        internal static void Verify(string path,string digest,CancellationToken token,Action<string> progress)
        { if(!string.Equals(Hash(path,token,progress),digest,StringComparison.OrdinalIgnoreCase))throw new InvalidDataException("模型 SHA256 校验失败，文件已保留，请重新选择完整模型或新的下载目录。"); }
        internal static void Download(GgufDownload spec,string target,Action<string> progress,CancellationToken token)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(target));
            if(File.Exists(target)){Verify(target,spec.Digest,token,progress);return;}
            string partial=target+".partial";long offset=File.Exists(partial)?new FileInfo(partial).Length:0;
            if(offset>spec.Size)throw new IOException("未完成下载的大小异常，请选择新的下载目录。");
            if(offset<spec.Size) {
                var uri=new Uri(spec.Url);if(uri.Scheme!="https"&&!uri.IsLoopback)throw new IOException("模型下载地址必须使用 HTTPS。");
                ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
                var request=(HttpWebRequest)WebRequest.Create(uri);request.Timeout=30000;request.ReadWriteTimeout=60000;request.UserAgent="YiChuang/1.5";
                if(offset>0)request.AddRange(offset);
                using(token.Register(()=>request.Abort()))try {
                    using(var response=(HttpWebResponse)request.GetResponse()) {
                        if(response.ResponseUri.Scheme!="https"&&!response.ResponseUri.IsLoopback)throw new IOException("下载地址发生不安全的跳转。");
                        bool resume=offset>0&&response.StatusCode==HttpStatusCode.PartialContent;
                        if(resume && !string.Equals(response.Headers[HttpResponseHeader.ContentRange],"bytes "+offset+"-"+(spec.Size-1)+"/"+spec.Size,StringComparison.Ordinal))throw new IOException("下载服务器返回了不正确的续传范围，请重试。");
                        if(!resume && response.StatusCode!=HttpStatusCode.OK)throw new IOException("模型下载响应不正确。");
                        if(!resume)offset=0;
                        if(response.ContentLength>=0 && response.ContentLength!=spec.Size-offset)throw new IOException("模型下载大小与发布清单不一致。");
                        var drive=new DriveInfo(Path.GetPathRoot(Path.GetFullPath(target)));if(drive.AvailableFreeSpace<spec.Size-offset+64L*1024*1024)throw new IOException("模型目录的磁盘空间不足，请选择其他目录。");
                        using(var input=response.GetResponseStream())using(var output=new FileStream(partial,resume?FileMode.Append:FileMode.Create,FileAccess.Write,FileShare.None)) {
                            byte[] b=new byte[1024*1024];int n;var clock=Stopwatch.StartNew();
                            while((n=input.Read(b,0,b.Length))>0){token.ThrowIfCancellationRequested();if(offset+n>spec.Size)throw new IOException("模型下载超出发布清单大小，已停止。");output.Write(b,0,n);offset+=n;if(clock.ElapsedMilliseconds>200){progress?.Invoke("正在下载 "+spec.Name+"："+(offset*100.0/spec.Size).ToString("F1")+"%（"+(offset/1048576)+" / "+(spec.Size/1048576)+" MB）");clock.Restart();}}
                        }
                    }
                }catch(WebException ex){ex.Response?.Dispose();token.ThrowIfCancellationRequested();throw new IOException("模型下载连接失败，请检查网络后重试；已下载部分会保留。",ex);}
                catch(IOException){token.ThrowIfCancellationRequested();throw;}
            }
            token.ThrowIfCancellationRequested();if(new FileInfo(partial).Length!=spec.Size)throw new IOException("下载连接提前结束，请重试以继续下载。");
            Verify(partial,spec.Digest,token,progress);File.Move(partial,target);
        }
    }
}
