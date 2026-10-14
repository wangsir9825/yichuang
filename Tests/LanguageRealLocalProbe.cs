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
class LanguageRealLocalProbe
{
    static string Data,Output;static int Unexpected;static List<string> Log=new List<string>();
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static object Field(object o,string n){return o.GetType().GetField(n,Private).GetValue(o);}
    static object Call(object o,string n,params object[] values){return o.GetType().GetMethod(n,Private).Invoke(o,values);}
    class Guard:IWebRequestCreate{public WebRequest Create(Uri uri){if(uri.Scheme!="http"||uri.Host!="127.0.0.1"||uri.Port!=11434){Unexpected++;throw new Exception("External requests are disabled in this test");}return WebRequest.CreateDefault(uri);}}
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,600000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            var app=Assembly.LoadFrom(exe);var registry=app.GetType("YiChuang.EngineRegistry");var def=app.GetType("YiChuang.EngineDefinition");
            Data=Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LanguageRealLocalProbe).GetMethod("Folder"));
            app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2"))){
                main.Show();Application.DoEvents();Call(main,"SetCaptureMode",0,false);var source=(RichTextBox)Field(main,"rtb原文");var translated=(RichTextBox)Field(main,"rtb译文");var cancel=(ToolStripButton)Field(main,"localCancelButton");
                foreach(string model in new[]{"yichuang-hy-mt2:7b-q6","translategemma:12b"}){
                    int id=model.StartsWith("yichuang",StringComparison.OrdinalIgnoreCase)?16:17;registry.GetMethod("SetVisible").Invoke(null,new object[]{id,true});source.Clear();Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",id);
                    string[] samples={"The new settings let users choose their own translation model.","用户可以在设置中添加或移除翻译引擎。","architecture","Bonjour, cette maison est très lumineuse.","Bonjour, cette maison est très lumineuse.","这座房子的窗户很大。"};
                    string[] modes={"auto","auto","auto","to-en","to-zh","to-zh"};
                    for(int index=0;index<samples.Length;index++){
                        string text=samples[index];source.Clear();Call(main,"SetTranslationDirection",modes[index]);
                        Call(main,"xianshiyuanwen",text);var until=DateTime.UtcNow.AddSeconds(190);while(cancel.Enabled&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(30);}Application.DoEvents();
                        string result=translated.Text;if(cancel.Enabled||string.IsNullOrWhiteSpace(result)||result.StartsWith("翻译失败")||result.Contains("正在"))throw new Exception(model+" failed: "+result);
                        bool chinese=Regex.IsMatch(result,"[\u4e00-\u9fff]");if(index==1||index==3?chinese:!chinese)throw new Exception("Unexpected translation direction: "+result);
                        Log.Add(model+" | "+modes[index]+" | "+text+" => "+result);File.WriteAllLines(Path.Combine(Output,"real-local-results.txt"),Log);
                        using(var bitmap=new Bitmap(main.Width,main.Height)){main.DrawToBitmap(bitmap,new Rectangle(Point.Empty,main.Size));bitmap.Save(Path.Combine(Output,(model.StartsWith("yichuang",StringComparison.OrdinalIgnoreCase)?"hy":"gemma")+"-"+index+".png"));}
                        using(var wc=new WebClient()){wc.Proxy=null;File.WriteAllText(Path.Combine(Output,"loaded-after-"+(model.StartsWith("yichuang",StringComparison.OrdinalIgnoreCase)?"hy":"gemma")+"-"+index+".json"),wc.DownloadString("http://127.0.0.1:11434/api/ps"));}

                    }
                    Log.Add("Completed custom engine "+id+": "+model);Console.WriteLine(Log[Log.Count-1]);
                }
                main.Close();
            }
            if(Unexpected!=0)throw new Exception("Unexpected external request");Log.Add("Real shipping Form2 + main direction: both local models, CN-EN interchange, captured word, French source fixed EN/ZH, same-language fixed Chinese passed. User settings isolated; external API calls=0.");File.WriteAllLines(Path.Combine(Output,"real-local-results.txt"),Log);return 0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"real-local-results.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
