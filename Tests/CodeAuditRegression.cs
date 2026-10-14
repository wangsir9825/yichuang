using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class CodeAuditRegression
{
    static Assembly App;
    static string Data;
    static int Requests;
    static List<string> Log = new List<string>();
    static BindingFlags Hidden=BindingFlags.Instance|BindingFlags.NonPublic, Static=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder folder) { return Data; }
    class Guard:IWebRequestCreate { public WebRequest Create(Uri uri) { Requests++;throw new Exception("Unexpected network request"); } }
    static object Call(object obj,string name,params object[] args) { return obj.GetType().GetMethod(name,Hidden|BindingFlags.Public).Invoke(obj,args); }
    static object Invoke(string type,string name,params object[] args) { return App.GetType("YiChuang."+type).GetMethods(Static).Single(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(null,args); }
    static object New(string type,params object[] args) { return Activator.CreateInstance(App.GetType("YiChuang."+type),Hidden|BindingFlags.Public,null,args,null); }
    static object Field(object obj,string name) { return obj.GetType().GetField(name,Hidden).GetValue(obj); }
    static void Check(bool ok,string message) { if(!ok)throw new Exception(message); }
    static string Error(Action action) { try { action();return ""; } catch(Exception ex) { while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;return ex.Message; } }
    static void Pump(int ms) { var until=DateTime.UtcNow.AddMilliseconds(ms);while(DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(10);} }
    static void Wait(Func<bool> condition,int milliseconds=5000) { var until=DateTime.UtcNow.AddMilliseconds(milliseconds);while(!condition()&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(10);}Check(condition(),"Timed out waiting for audited operation"); }
    static void Settings()
    {
        string folder=Path.Combine(Data,"YiChuang");Directory.CreateDirectory(folder);string path=Path.Combine(folder,"setting.txt");
        const string first="{\"DeepSeekAI_key\":\"audit-test-key-only\",\"Master_switch\":\"0\"}",second="{\"DeepSeekAI_key\":\"audit-test-key-two\",\"Master_switch\":\"0\"}";
        File.WriteAllText(path,(string)Invoke("RSA","RSAEncrypt",first));
        Check((string)Invoke("ClassMain","readJiekou")==first,"Legacy settings unreadable");
        Invoke("ClassMain","writeJiekou",first);
        Check(File.ReadAllText(path).StartsWith("YICHUANG-DPAPI-1:")&&!File.ReadAllText(path).Contains("audit-test-key-only"),"Current-user protection absent");
        Check((string)Invoke("ClassMain","readJiekou")==first,"Protected settings round trip failed");
        Invoke("ClassMain","writeJiekou",second);File.WriteAllText(path,"damaged-primary");
        Check((string)Invoke("ClassMain","readJiekou")==first,"Valid backup not recovered");
        Invoke("ClassMain","writeJiekou",second);
        Check(Directory.GetFiles(folder,"setting.txt.corrupt-*").Any(p=>File.ReadAllText(p)=="damaged-primary"),"Damaged original not preserved");
        File.WriteAllText(path,"damaged-primary-two");File.WriteAllText(path+".bak","damaged-backup");
        string safe=(string)Invoke("ClassMain","readJiekou");Check(safe.Contains("invalid-protected-settings"),"Both-broken configuration did not pause");
        Check(Error(()=>Invoke("ClassMain","writeJiekou",first))!="","Broken settings allowed automatic overwrite");
        Check(File.ReadAllText(path)=="damaged-primary-two"&&File.ReadAllText(path+".bak")=="damaged-backup","Broken files overwritten");
        Invoke("SettingsStore","AllowRecovery",second);Invoke("ClassMain","writeJiekou",second);
        Check((string)Invoke("ClassMain","readJiekou")==second,"Explicit valid recovery failed");
        Check(Error(()=>Invoke("ClassMain","writeJiekou","broken"))!=""&&(string)Invoke("ClassMain","readJiekou")==second,"Invalid JSON corrupted saved settings");
        string active=(string)Invoke("EngineRegistry","Export");
        foreach(string bad in new[]{"{\"unrelated\":\"file\"}","{\"Master_switch\":\"1\",\"Translation_interface\":\"4\",\"GeminiKey\":{}}","{\"Master_switch\":\"1\",\"Translation_interface\":\"4\",\"EngineRegistry\":\"broken\"}","{\"Master_switch\":\"1\",\"Translation_interface\":\"4\",\"translationFontSize\":\"NaN\"}"}) {
            Check(Error(()=>Invoke("SettingsStore","ImportBackup",bad))!="","Malformed backup accepted");
            Check((string)Invoke("ClassMain","readJiekou")==second&&(string)Invoke("EngineRegistry","Export")==active,"Rejected backup changed active settings");
        }
        string originalModel=(string)App.GetType("YiChuang.LocalModelSettings").GetField("GemmaModel").GetValue(null);
        App.GetType("YiChuang.LocalModelSettings").GetField("GemmaModel").SetValue(null,"audit-saved-local-model:latest");
        string complete=(string)Invoke("ClassMain","moren");Invoke("SettingsStore","ImportBackup",complete);
        App.GetType("YiChuang.LocalModelSettings").GetField("GemmaModel").SetValue(null,"changed");Invoke("ClassMain","read_setSetting",(string)Invoke("ClassMain","readJiekou"));
        Check((string)App.GetType("YiChuang.LocalModelSettings").GetField("GemmaModel").GetValue(null)=="audit-saved-local-model:latest","Local model missing from backup round trip");
        App.GetType("YiChuang.LocalModelSettings").GetField("GemmaModel").SetValue(null,originalModel);
        Log.Add("Settings: legacy->DPAPI, valid backup recovery, preserve corruption, block empty overwrite, explicit recovery, invalid-save protection: passed");
        Log.Add("Backup import: reject unrelated/structured/bad-registry/invalid-font input without changing saved settings or active engines; local model round trip: passed");
    }
    static void Streams()
    {
        Func<string,string> parse=text=>(string)Invoke("ChatGPT","ReadTranslation",new StringReader(text),new Action(()=>{}));
        Check(parse("data: {\"choices\":[{\"delta\":{\"content\":\"final chunk\"},\"finish_reason\":\"stop\"}]}\n")=="final chunk","Final streaming chunk was dropped");
        string partial="data: {\"choices\":[{\"delta\":{\"content\":\"partial\"},\"finish_reason\":null}]}\n";
        Check(parse(partial+"data: [DONE]\n")=="partial","DONE framing not accepted");
        foreach(string bad in new[]{partial,partial+"data: broken\n",partial+"data: {\"error\":{\"message\":\"provider error\"}}\n",partial+"data: {\"choices\":[{\"delta\":{},\"finish_reason\":\"length\"}]}\n","data: [DONE]\n"})Check(Error(()=>parse(bad))!="","Incomplete/error stream accepted");
        Check(Error(()=>Invoke("ResponseReader","Read",new StringReader(new string('x',101)),100))!="","Oversized response not bounded");
        Check(Error(()=>Invoke("ResponseReader","Line",new StringReader(new string('x',101)),100))!="","Oversized SSE line not bounded");
        using(var lifetime=(IDisposable)New("RequestLifetime",new Func<bool>(()=>true),60)) { Thread.Sleep(100);Check(Error(()=>Call(lifetime,"Check")).Contains("超时"),"Request deadline not enforced"); }
        Log.Add("Streaming: final delta, DONE, premature EOF, malformed JSON, provider error, truncation, empty output and response-size/time limits: passed");
    }
    static void CaptureAndScheduler()
    {
        var gate=New("ExplicitCaptureGate");IntPtr a=new IntPtr(100),b=new IntPtr(200);
        Func<IntPtr,uint,long,bool,bool> accept=(w,s,t,e)=>(bool)Call(gate,"Accept",w,s,t,e);
        Check(!accept(a,5,10,true),"Clipboard accepted without explicit action");Call(gate,"Arm",a,(uint)5,(long)10);
        Check(!accept(a,5,11,true)&&accept(a,6,12,true),"Sequence check incorrect");Call(gate,"WindowChanged",b);Call(gate,"WindowChanged",a);
        Check(!accept(a,7,13,true),"Away-and-back revived explicit capture");Call(gate,"Arm",a,(uint)8,(long)20);Check(!accept(a,9,5021,true),"Expired capture accepted");
        Call(gate,"Arm",a,(uint)8,(long)20);Check(!accept(a,9,21,false)&&!accept(a,9,22,true),"Paused capture revived");
        using(var scheduler=(IDisposable)New("TranslationScheduler")) {
            int executed=0,last=-1;
            for(int i=0;i<100;i++){int value=i;Call(scheduler,"Schedule",new Action(()=>{last=value;Interlocked.Increment(ref executed);}),100);}
            Check(executed==0,"Debounce dispatched synchronously");Wait(()=>executed==1);Check(last==99,"Old input dispatched");Pump(250);Check(executed==1,"Discarded inputs executed");
            Call(scheduler,"Schedule",new Action(()=>Interlocked.Increment(ref executed)),100);Call(scheduler,"Cancel");Pump(200);Check(executed==1,"Cancelled pending work executed");
        }
        Log.Add("Explicit capture gate: action/window/sequence/expiry/pause; 100 rapid edits -> exactly one dispatch; cancellation: passed");
    }
    static Task SendPipe(byte[] data,int chunk=7)
    {
        return Task.Run(()=>{using(var pipe=new NamedPipeClientStream(".","YiChuangOcr2918",PipeDirection.Out)){pipe.Connect(3000);for(int i=0;i<data.Length;i+=chunk)pipe.Write(data,i,Math.Min(chunk,data.Length-i));}});
    }
    static void FormAndOcr()
    {
        // Build fresh defaults so the isolated window can load all settings.
        string fresh=(string)Invoke("ClassMain","moren");Invoke("ClassMain","writeJiekou",fresh);
        for(int round=0;round<2;round++)
        using(var main=(Form)New("Form2")) {
            main.Opacity=0;main.Show();Application.DoEvents();main.GetType().GetField("_isInitializing",Hidden).SetValue(main,true);Control.CheckForIllegalCrossThreadCalls=true;
            Invoke("EngineRegistry","SetVisible",17,true);App.GetType("YiChuang.ClassMain").GetField("isjiekounum").SetValue(null,17);
            using(var settings=(Form)New("Frmsetting")) {
                settings.Opacity=0;settings.Show(main);Application.DoEvents();
                Check((bool)App.GetType("YiChuang.ClassSetHotKey").GetField("Suspended",Static).GetValue(null),"Settings did not suspend hotkeys");
                settings.Dispose();
                Check(!(bool)App.GetType("YiChuang.ClassSetHotKey").GetField("Suspended",Static).GetValue(null),"Direct settings disposal left hotkeys suspended");
            }
            var source=(RichTextBox)Field(main,"rtb原文");string expected=string.Concat(Enumerable.Repeat("中文😀 Alpha. ",500));
            var sent=SendPipe(Encoding.UTF8.GetBytes(expected));Wait(()=>sent.IsCompleted&&source.Text==expected,7000);sent.GetAwaiter().GetResult();
            Check(source.Text.Length>1024&&!source.Text.Contains("�"),"Long fragmented OCR lost UTF-8 characters");
            if(round==0) {
                var target=(RichTextBox)Field(main,"rtb译文");
                Log.Add("OCR invalid UTF-8 begins");sent=SendPipe(new byte[]{255});Wait(()=>sent.IsCompleted&&target.Text.Contains("接收失败"));sent.GetAwaiter().GetResult();Check(source.Text==expected,"Invalid UTF-8 replaced original");
                Log.Add("OCR oversized input begins");sent=SendPipe(new byte[256*1024+1],4096);Wait(()=>target.Text.Contains("过长"));Wait(()=>sent.IsCompleted);if(sent.IsFaulted)Check(sent.Exception.GetBaseException() is IOException,"Unexpected oversized sender error");Check(source.Text==expected,"Oversized OCR replaced original");
                Log.Add("OCR idle timeout begins");using(var idle=new NamedPipeClientStream(".","YiChuangOcr2918",PipeDirection.Out)) { idle.Connect(3000);Wait(()=>target.Text.Contains("超时"),6500); }
                Log.Add("OCR recovery begins");sent=SendPipe(Encoding.UTF8.GetBytes("Recovered OCR."));Wait(()=>sent.IsCompleted&&source.Text=="Recovered OCR.");sent.GetAwaiter().GetResult();source.Text=expected;
            }
            foreach(int mode in new[]{1,2,3}) {Call(main,"SetCaptureMode",mode,true);Call(main,"WndProc",Message.Create(main.Handle,0x031D,IntPtr.Zero,IntPtr.Zero));Check(source.Text==expected,"Unsolicited clipboard replaced original in mode "+mode);}
            var receiver=Field(main,"ocrReceiver");main.Close();
            var completion=(Task)receiver.GetType().GetProperty("Completion",Hidden).GetValue(receiver,null);Wait(()=>completion.IsCompleted);completion.GetAwaiter().GetResult();
            var subscribers=App.GetType("YiChuang.ChatGPT").GetField("MyChatGPTEvent",Static).GetValue(null);Check(subscribers==null,"Closed form retained static streaming handler");
        }
        Log.Add("Actual Form2: fragmented >1KB Unicode OCR, invalid UTF-8/oversize/idle-client recovery, twice across close/reopen, settings-dispose restores hotkeys, all modes ignore unsolicited clipboard, pipe shutdown and static-event unsubscribe: passed");
    }
    static void IdleOcrShutdown()
    {
        for(int i=0;i<20;i++) {
            var receiver=New("OcrPipeReceiver",new Action<string>(_=>{}),new Action<string>(_=>{}));
            var completion=(Task)receiver.GetType().GetProperty("Completion",Hidden).GetValue(receiver,null);
            var closing=Task.Run(()=>((IDisposable)receiver).Dispose());
            Check(closing.Wait(2000),"Idle OCR cancellation deadlocked while holding the pipe lock");
            Check(completion.Wait(2000),"Idle OCR receiver failed to stop");
            ((IDisposable)receiver).Dispose();
        }
        Log.Add("20 idle OCR receiver shutdown/reopen cycles and repeated Dispose: passed");
    }
    [STAThread]static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,60000,Timeout.Infinite))try {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
            App=Assembly.LoadFrom(exe);Data=Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(CodeAuditRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",Static).SetValue(null,output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
            Settings();Streams();CaptureAndScheduler();IdleOcrShutdown();FormAndOcr();Check(Requests==0,"Unexpected outbound request");
            File.WriteAllLines(Path.Combine(output,"code-audit-regression.txt"),Log);Console.WriteLine("Code audit regression passed.");return 0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"code-audit-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
