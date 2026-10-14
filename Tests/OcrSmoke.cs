using System;
using System.IO;
using System.Reflection;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class OcrSmoke
{
    static string result="";
    public static void Callback(IntPtr value) {
        int length=0;while(length<16*1024*1024&&Marshal.ReadByte(value,length)!=0)length++;
        byte[] bytes=new byte[length];Marshal.Copy(value,bytes,0,length);result=Encoding.UTF8.GetString(bytes);
    }
    static int Main(string[] args) {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        string report=Path.Combine(output,"ocr-test.txt");
        using(Timer timer=new Timer(s=>Environment.Exit(124),null,25000,Timeout.Infinite)) {
            try {
                string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
                Assembly a=Assembly.LoadFile(exe);
                object paths=a.GetType("WeChatPathResolver").GetMethod("GetWeChatPaths").Invoke(null,null);
                Type tuple=paths.GetType();string engine=(string)tuple.GetField("Item1").GetValue(paths);
                string runtime=(string)tuple.GetField("Item2").GetValue(paths);
                File.WriteAllText(report,"Engine="+engine+"\nRuntime="+runtime+"\n");
                if(string.IsNullOrEmpty(engine)||string.IsNullOrEmpty(runtime)){File.AppendAllText(report,"SKIP: optional WeChat component absent. Bundled OCR is checked by IndependentOcrRegression; pipe transport is checked by CodeAuditRegression.\n");Console.WriteLine("Optional WeChat OCR unavailable: skipped; independent OCR suite required.");return 0;}
                string image=Path.Combine(output,"ocr-test.png");
                using(Bitmap bmp=new Bitmap(1200,240))using(Graphics g=Graphics.FromImage(bmp))using(Font font=new Font("Arial",32)) {
                    g.Clear(Color.White);g.DrawString("Rebuilt translator OCR test 2918",font,Brushes.Black,30,35);
                    g.DrawString("The quick brown fox jumps over the lazy dog.",font,Brushes.Black,30,115);
                    bmp.Save(image,System.Drawing.Imaging.ImageFormat.Png);
                }
                Type form=a.GetType("wxocr.Form1");Type dtype=form.GetNestedType("SetResultDelegate");
                Delegate callback=Delegate.CreateDelegate(dtype,typeof(OcrSmoke).GetMethod("Callback"));
                bool ok=(bool)form.GetMethod("wechat_ocr").Invoke(null,new object[]{engine,runtime,image,callback});
                form.GetMethod("stop_ocr").Invoke(null,null);
                File.AppendAllText(report,"NativeCallOK="+ok+"\nResult="+result+"\n");
                bool pipeOK=false;
                using(var pipe=new System.IO.Pipes.NamedPipeServerStream("YiChuangOcr2918",System.IO.Pipes.PipeDirection.InOut)) {
                    var server=System.Threading.Tasks.Task.Run(()=>{
                        pipe.WaitForConnection();byte[] bytes=new byte[4096];int count=pipe.Read(bytes,0,bytes.Length);
                        pipeOK=Encoding.UTF8.GetString(bytes,0,count)=="OCR pipe transfer test 2918";
                    });
                    var task=(System.Threading.Tasks.Task)form.GetMethod("sendData",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{"OCR pipe transfer test 2918"});
                    task.GetAwaiter().GetResult();if(!server.Wait(5000))throw new Exception("Pipe receive timeout");
                }
                File.AppendAllText(report,"PipeTransferOK="+pipeOK+"\n");
                return ok&&result.Contains("quick brown fox")&&pipeOK?0:1;
            }catch(Exception ex){File.AppendAllText(report,ex.ToString());return 1;}
        }
    }
}
