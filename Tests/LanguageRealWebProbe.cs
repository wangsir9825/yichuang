using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

// Optional real-model acceptance: run explicitly with Ollama and the two already downloaded models.
// Uses the shipping Form2 and translation pipeline with isolated preferences. Never downloads/deletes weights.
class LanguageRealWebProbe
{
    static string Data,Output;static int Unexpected;static List<string> Log=new List<string>();
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static object Field(object o,string n){return o.GetType().GetField(n,Private).GetValue(o);}
    static object Call(object o,string n,params object[] values){return o.GetType().GetMethod(n,Private).Invoke(o,values);}
    class Guard:IWebRequestCreate{public WebRequest Create(Uri uri){if(uri.Scheme!="https"||(uri.Host!="transmart.qq.com"&&uri.Host!="fanyi.sogou.com")){Unexpected++;throw new Exception("External requests are disabled in this test");}return WebRequest.CreateDefault(uri);}}
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,240000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            var app=Assembly.LoadFrom(exe);var registry=app.GetType("Xtranslator.EngineRegistry");var def=app.GetType("Xtranslator.EngineDefinition");
            Data=Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LanguageRealWebProbe).GetMethod("Folder"));
            app.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(app.GetType("Xtranslator.Form2"))){
                main.Show();Application.DoEvents();Call(main,"SetCaptureMode",0,false);var source=(RichTextBox)Field(main,"rtb原文");var translated=(RichTextBox)Field(main,"rtb译文");var cancel=(ToolStripButton)Field(main,"localCancelButton");
                foreach(int id in new[]{4,5}){
                    registry.GetMethod("SetVisible").Invoke(null,new object[]{id,true});source.Clear();Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",id);
                    string[] texts={"这座房子的窗户很大。","The house has large windows.","architecture"};string[] modes={"to-en","to-zh","to-zh"};
                    for(int i=0;i<texts.Length;i++){
                        source.Clear();Call(main,"SetTranslationDirection",modes[i]);Call(main,"xianshiyuanwen",texts[i]);var until=DateTime.UtcNow.AddSeconds(30);while(cancel.Enabled&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(30);}Application.DoEvents();
                        string result=translated.Text;bool ok=!cancel.Enabled&&!string.IsNullOrWhiteSpace(result)&&!result.StartsWith("翻译失败")&&!result.Contains("正在翻译");
                        if(ok){bool chinese=Regex.IsMatch(result,"[\u4e00-\u9fff]");ok=i==0?!chinese:chinese;}
                        Log.Add((ok?"PASS":"NOT VERIFIED")+" | "+id+" | "+modes[i]+" | "+texts[i]+" => "+result);File.WriteAllLines(Path.Combine(Output,"real-web-results.txt"),Log);
                        if(!ok)break; // Bound repeated failures from an unchanged external service.
                    }
                }
                main.Close();
            }
            if(Unexpected!=0)throw new Exception("Unexpected external request");File.WriteAllLines(Path.Combine(Output,"real-web-results.txt"),Log);return Log.Exists(x=>x.StartsWith("NOT VERIFIED"))?2:0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"real-web-results.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
