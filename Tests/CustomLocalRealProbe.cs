using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

// Optional acceptance with an existing GGUF and optional Ollama model; no download and isolated preferences.
class CustomLocalRealProbe
{
    static Assembly App;static string Data,Output;static int External;const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static Type T(string n){return App.GetType("Xtranslator."+n);}
    static object Field(object o,string n){return o.GetType().GetField(n,F).GetValue(o);}
    static object Call(object o,string n,params object[] a){return o.GetType().GetMethod(n,F).Invoke(o,a);}
    static object Reg(string n,params object[] a){return T("EngineRegistry").GetMethod(n,F).Invoke(null,a);}
    static void Set(object e,string n,object v){e.GetType().GetField(n,F).SetValue(e,v);}
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    class Guard:IWebRequestCreate{public WebRequest Create(Uri u){if(u.Scheme!="http"||u.Host!="127.0.0.1"){External++;throw new Exception("External request forbidden");}return WebRequest.CreateDefault(u);}}
    static void Translate(Form main,string text,string direction,bool expectChinese)
    {
        var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");var cancel=(ToolStripButton)Field(main,"localCancelButton");source.Clear();Call(main,"SetTranslationDirection",direction);Call(main,"xianshiyuanwen",text);
        var until=DateTime.UtcNow.AddSeconds(190);while(cancel.Enabled&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(30);}Application.DoEvents();
        Check(!cancel.Enabled&&!string.IsNullOrWhiteSpace(target.Text)&&!target.Text.StartsWith("翻译失败")&&!target.Text.Contains("正在"),"Custom model failed: "+target.Text);
        Check(Regex.IsMatch(target.Text,"[\u4e00-\u9fff]")==expectChinese,"Wrong direction: "+target.Text);File.AppendAllText(Path.Combine(Output,"result.txt"),direction+" | "+text+" => "+target.Text+Environment.NewLine);
    }
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watch=new System.Threading.Timer(s=>Environment.Exit(124),null,360000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]),dir=Path.GetDirectoryName(exe);Directory.SetCurrentDirectory(dir);
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(dir,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};App=Assembly.LoadFrom(exe);Data=Path.Combine(Output,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(CustomLocalRealProbe).GetMethod("Folder"));
            T("Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));App.GetType("RebuildInfo").GetField("DiagnosticOutput",F).SetValue(null,Output);
            WebRequest.RegisterPrefix("https://",new Guard());WebRequest.RegisterPrefix("http://",new Guard());Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(T("Form2"))){
                main.Show();Application.DoEvents();Call(main,"SetCaptureMode",0,false);var entry=Activator.CreateInstance(T("EngineDefinition"));Set(entry,"Name","My independent GGUF");Set(entry,"Provider","llamacpp");Set(entry,"Model",Path.GetFullPath(args[2]));int id=(int)Reg("Save",entry);Check(id>=1000,"Not a custom model ID");Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",id);
                Translate(main,"Users can add their own local translation models.","to-zh",true);Translate(main,"用户可以自行配置本地模型。","to-en",false);Translate(main,"window","to-zh",true);
                Check((string)T("LlamaRuntime").GetProperty("LoadedModel",F).GetValue(null,null)==Path.GetFullPath(args[2]),"Wrong custom GGUF file loaded");
                File.AppendAllText(Path.Combine(Output,"result.txt"),"Custom GGUF uses generic GGUF chat template; ID="+id+"; CN/EN/word passed.\n");
                ((RichTextBox)Field(main,"rtb原文")).Clear();Reg("Remove",id);Call(main,"RebuildEngineMenu");Check((int)T("ClassMain").GetField("isjiekounum").GetValue(null)==0&&File.Exists(args[2]),"Removing current entry deleted file or selected another engine");
                if(args.Length>3){entry=Activator.CreateInstance(T("EngineDefinition"));Set(entry,"Name","My Ollama model");Set(entry,"Provider","ollama");Set(entry,"Model",args[3]);Set(entry,"Endpoint","http://127.0.0.1:11434");Set(entry,"Prompt","hy");int ollama=(int)Reg("Save",entry);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",ollama);Translate(main,"Local models can be managed by the user.","to-zh",true);Check((int)T("LlamaRuntime").GetProperty("ProcessId",F).GetValue(null,null)==0,"llama.cpp remained when switching to Ollama");Reg("Remove",ollama);}
                main.Close();Check((int)T("LlamaRuntime").GetProperty("ProcessId",F).GetValue(null,null)==0,"Custom runtime leaked on exit");
            }
            Check(External==0,"Unexpected online request");File.AppendAllText(Path.Combine(Output,"result.txt"),"Custom routing, removal, switching and exit passed. External translation API calls=0. No weights downloaded/deleted.\n");Console.WriteLine("Real custom GGUF/Ollama acceptance passed.");return 0;
        }catch(Exception ex){File.AppendAllText(Path.Combine(Output,"result.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
