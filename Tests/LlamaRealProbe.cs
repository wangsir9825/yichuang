using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

// Optional real-model acceptance: run explicitly with the two already downloaded models; creates a separate compatible Google copy.
// Uses the shipping Form2 and translation pipeline with isolated preferences. Reuses cached weights, never deletes original weights.
class LlamaRealProbe
{
    static string Data,Output;static int Unexpected;static List<string> Log=new List<string>();
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static object Field(object o,string n){return o.GetType().GetField(n,Private).GetValue(o);}
    static object Call(object o,string n,params object[] values){return o.GetType().GetMethod(n,Private).Invoke(o,values);}
    class Guard:IWebRequestCreate{public WebRequest Create(Uri uri){if(uri.Scheme!="http"||uri.Host!="127.0.0.1"){Unexpected++;throw new Exception("External requests are disabled in this test");}return WebRequest.CreateDefault(uri);}}
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,600000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            var app=Assembly.LoadFrom(exe);var registry=app.GetType("Xtranslator.EngineRegistry");var def=app.GetType("Xtranslator.EngineDefinition");
            Data=Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LlamaRealProbe).GetMethod("Folder"));
            app.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            var runtime=app.GetType("Xtranslator.LocalRuntime");var opts=Activator.CreateInstance(app.GetType("Xtranslator.LocalRuntimeOptions"),true);
            var all=BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
            opts.GetType().GetField("ModelsDirectory").SetValue(opts,Path.GetFullPath(args[2]));opts.GetType().GetField("GgufDirectory").SetValue(opts,Path.GetFullPath(args[3]));
            Action<string> progress=value=>{};
            foreach(bool hy in new[]{true,false}){string path=(string)app.GetType("Xtranslator.GgufModels").GetMethod("Prepare",all).Invoke(null,new object[]{opts,hy,progress,CancellationToken.None});opts.GetType().GetField(hy?"HyGguf":"GemmaGguf").SetValue(opts,path);}
            runtime.GetMethod("Save",all).Invoke(null,new[]{opts});
            var llama=app.GetType("Xtranslator.LlamaRuntime");var settings=app.GetType("Xtranslator.LocalModelSettings");
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(app.GetType("Xtranslator.Form2"))){
                main.Show();Application.DoEvents();Call(main,"SetCaptureMode",0,false);var source=(RichTextBox)Field(main,"rtb原文");var translated=(RichTextBox)Field(main,"rtb译文");var cancel=(ToolStripButton)Field(main,"localCancelButton");
                foreach(string model in new[]{"xtranslator-hy-mt2:7b-q6","translategemma:12b"}){
                    int id=model.StartsWith("xtranslator")?16:17;registry.GetMethod("SetVisible").Invoke(null,new object[]{id,true});source.Clear();Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",id);
                    string[] samples={"The new settings let users choose their own translation model.","用户可以在设置中添加或移除翻译引擎。","architecture","Bonjour, cette maison est très lumineuse.","Bonjour, cette maison est très lumineuse.","这座房子的窗户很大。"};
                    string[] modes={"auto","auto","auto","to-en","to-zh","to-zh"};
                    for(int index=0;index<samples.Length;index++){
                        string text=samples[index];source.Clear();Call(main,"SetTranslationDirection",modes[index]);
                        Call(main,"xianshiyuanwen",text);var until=DateTime.UtcNow.AddSeconds(190);while(cancel.Enabled&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(30);}Application.DoEvents();
                        string result=translated.Text;if(cancel.Enabled||string.IsNullOrWhiteSpace(result)||result.StartsWith("翻译失败")||result.Contains("正在"))throw new Exception(model+" failed: "+result);
                        bool chinese=Regex.IsMatch(result,"[\u4e00-\u9fff]");if(index==1||index==3?chinese:!chinese)throw new Exception("Unexpected translation direction: "+result);
                        Log.Add(model+" | "+modes[index]+" | "+text+" => "+result);File.WriteAllLines(Path.Combine(Output,"real-local-results.txt"),Log);
                        using(var bitmap=new Bitmap(main.Width,main.Height)){main.DrawToBitmap(bitmap,new Rectangle(Point.Empty,main.Size));bitmap.Save(Path.Combine(Output,(model.StartsWith("xtranslator")?"hy":"gemma")+"-"+index+".png"));}
                        using(var wc=new WebClient()){wc.Proxy=null;File.WriteAllText(Path.Combine(Output,"loaded-after-"+(model.StartsWith("xtranslator")?"hy":"gemma")+"-"+index+".json"),wc.DownloadString("http://127.0.0.1:11434/api/ps"));}
                        int pid=(int)llama.GetProperty("ProcessId",all).GetValue(null,null);if(pid==0)throw new Exception("llama.cpp did not run");
                        Log.Add("Active llama.cpp PID="+pid+" path="+llama.GetProperty("LoadedModel",all).GetValue(null,null));
                        using(var proc=System.Diagnostics.Process.GetProcessById(pid)){if(proc.ProcessName!="llama-server")throw new Exception("Unexpected runtime process");}

                    }
                    Log.Add("Completed custom engine "+id+": "+model);Console.WriteLine(Log[Log.Count-1]);
                }
                source.Clear();settings.GetField("Backend").SetValue(null,"ollama");Call(main,"SelectManagedEngine",16);Call(main,"SetTranslationDirection","auto");Call(main,"xianshiyuanwen","The local service can be changed in settings.");
                var limit=DateTime.UtcNow.AddSeconds(190);while(cancel.Enabled&&DateTime.UtcNow<limit){Application.DoEvents();Thread.Sleep(30);}if(!Regex.IsMatch(translated.Text,"[\u4e00-\u9fff]")||translated.Text.Contains("失败"))throw new Exception("Ollama switch failed: "+translated.Text);if((int)llama.GetProperty("ProcessId",all).GetValue(null,null)!=0)throw new Exception("llama process remained during Ollama use");Log.Add("Switched to Ollama: "+translated.Text);
                source.Clear();settings.GetField("Backend").SetValue(null,"llamacpp");Call(main,"SelectManagedEngine",17);Call(main,"xianshiyuanwen","The local service can be changed in settings.");
                limit=DateTime.UtcNow.AddSeconds(190);while(cancel.Enabled&&DateTime.UtcNow<limit){Application.DoEvents();Thread.Sleep(30);}if(!Regex.IsMatch(translated.Text,"[\u4e00-\u9fff]")||translated.Text.Contains("失败"))throw new Exception("Return to llama failed: "+translated.Text);Log.Add("Returned to llama.cpp: "+translated.Text);
                source.Clear();Call(main,"xianshiyuanwen",string.Join(" ",new string[160].Select(x=>"The quick brown fox jumps over the lazy dog.")));Application.DoEvents();var start=DateTime.UtcNow;while(!translated.Text.Contains("正在使用内置")&&(DateTime.UtcNow-start).TotalSeconds<10){Application.DoEvents();Thread.Sleep(20);}if(!translated.Text.Contains("正在使用内置"))throw new Exception("Inference did not start: "+translated.Text);cancel.PerformClick();limit=DateTime.UtcNow.AddSeconds(5);while((int)llama.GetProperty("ProcessId",all).GetValue(null,null)!=0&&DateTime.UtcNow<limit){Application.DoEvents();Thread.Sleep(20);}if((int)llama.GetProperty("ProcessId",all).GetValue(null,null)!=0||!translated.Text.Contains("已取消"))throw new Exception("Cancellation did not stop runtime: "+translated.Text+" PID="+llama.GetProperty("ProcessId",all).GetValue(null,null));Log.Add("Cancel stops app-owned process and preserves canceled UI: passed.");
                main.Close();if((int)llama.GetProperty("ProcessId",all).GetValue(null,null)!=0)throw new Exception("Exit leaked llama process");
            }
            if(Unexpected!=0)throw new Exception("Unexpected external request");Log.Add("Real shipping Form2 + main direction: both local models, CN-EN interchange, captured word, French source fixed EN/ZH, same-language fixed Chinese passed. User settings isolated; external API calls=0.");File.WriteAllLines(Path.Combine(Output,"real-local-results.txt"),Log);return 0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"real-local-results.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
