using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;

class IndependentOcrRegression
{
    static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watch=new Timer(s=>Environment.Exit(124),null,60000,Timeout.Infinite))try {
            string exe=Path.GetFullPath(args[0]),app=Path.GetDirectoryName(exe);Directory.SetCurrentDirectory(app);
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string file=Path.Combine(app,new AssemblyName(e.Name).Name+".dll");return File.Exists(file)?Assembly.LoadFrom(file):null;};
            var assembly=Assembly.LoadFrom(exe);string image=Path.Combine(output,"offline-ocr.png");
            using(var bitmap=new Bitmap(1600,300))using(var graphics=Graphics.FromImage(bitmap))using(var font=new Font("Microsoft YaHei",32)) {
                graphics.Clear(Color.White);
                graphics.DrawString("Offline OCR works without WeChat.",font,Brushes.Black,30,30);
                graphics.DrawString("中文离线识别测试，翻译工具可以使用。",font,Brushes.Black,30,125);
                bitmap.Save(image,System.Drawing.Imaging.ImageFormat.Png);
            }
            // Force the exact production fallback path; never initialize or call the WeChat engine.
            string text=(string)assembly.GetType("wxocr.Form1").GetMethod("RecognizeImage").Invoke(null,new object[]{image,true});
            File.WriteAllText(Path.Combine(output,"recognized.txt"),text);
            string normalized=text.Replace(" ","").Replace("\r","").Replace("\n","");
            if(!normalized.Contains("OfflineOCRworkswithoutWeChat")||!normalized.Contains("中文离线识别测试")||!normalized.Contains("翻译工具可以使用"))throw new Exception("Independent Chinese/English recognition mismatch: "+text);
            File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: production independent OCR, Chinese and English image recognized, no WeChat engine invocation.\nClean-machine prerequisite installation is a separate unverified scenario.");
            Console.WriteLine("Independent offline OCR Chinese/English: passed.");return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(output,"failure.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
