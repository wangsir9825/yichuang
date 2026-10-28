using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class LlamaRuntimeRegression
{
    static Assembly App;static string Data;static List<string> Log=new List<string>();
    const BindingFlags F=BindingFlags.Static|BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static Type T(string name){return App.GetType("YiChuang."+name);}
    static object New(string type){return Activator.CreateInstance(T(type),true);}
    static object Call(string type,string name,params object[] values){try{return T(type).GetMethod(name,F).Invoke(null,values);}catch(TargetInvocationException ex){throw ex.InnerException;}}
    static void Set(object instance,string name,object value){instance.GetType().GetField(name,F).SetValue(instance,value);}
    static object Get(object instance,string name){return instance.GetType().GetField(name,F).GetValue(instance);}
    static void Check(bool value,string message){if(!value)throw new Exception(message);}
    static string Fail(Action a){try{a();return "";}catch(Exception ex){return ex.Message;}}
    static string Hash(byte[] bytes){using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-","").ToLowerInvariant();}
    class Server:IDisposable
    {
        public string Address;public byte[] Bytes;public bool IgnoreRange,CorruptRange,Slow,Invalid,Failure;public string Range,Auth;public int Requests;
        readonly HttpListener listener=new HttpListener();readonly ManualResetEvent stop=new ManualResetEvent(false);public readonly ManualResetEvent Began=new ManualResetEvent(false);
        public Server(byte[] bytes){Bytes=bytes;var s=new TcpListener(IPAddress.Loopback,0);s.Start();int p=((IPEndPoint)s.LocalEndpoint).Port;s.Stop();Address="http://127.0.0.1:"+p;listener.Prefixes.Add(Address+"/");listener.Start();Task.Run(()=>{while(listener.IsListening)try{var c=listener.GetContext();Task.Run(()=>Handle(c));}catch{break;}});}
        void Handle(HttpListenerContext c){try{
            Interlocked.Increment(ref Requests);Range=c.Request.Headers["Range"];Auth=c.Request.Headers["Authorization"];
            byte[] bytes=c.Request.Url.AbsolutePath=="/completion"?Encoding.UTF8.GetBytes(Invalid?"not json":Failure?"{\"error\":{\"message\":\"simulated failure\"}}":"{\"content\":\"你好\"}"):Bytes;if(Failure)c.Response.StatusCode=500;
            long start=0;if(!IgnoreRange&&!string.IsNullOrEmpty(Range)){start=long.Parse(Range.Substring(6).TrimEnd('-'));c.Response.StatusCode=206;c.Response.Headers["Content-Range"]="bytes "+(CorruptRange?start+1:start)+"-"+(bytes.Length-1)+"/"+bytes.Length;}
            c.Response.ContentLength64=bytes.Length-start;c.Response.ContentType="application/json";
            if(Slow){int count=(int)Math.Min(32,bytes.Length-start);c.Response.OutputStream.Write(bytes,(int)start,count);c.Response.OutputStream.Flush();Began.Set();stop.WaitOne(10000);return;}
            c.Response.OutputStream.Write(bytes,(int)start,bytes.Length-(int)start);
        }catch{}finally{try{c.Response.Close();}catch{}}}
        public void Dispose(){stop.Set();listener.Close();}
    }
    static object Spec(Server server,byte[] bytes){var s=New("GgufDownload");Set(s,"Name","fixture");Set(s,"FileName","fixture.gguf");Set(s,"Url",server.Address+"/weights");Set(s,"Digest",Hash(bytes));Set(s,"Size",(long)bytes.Length);return s;}
    static void Str(BinaryWriter w,string s){byte[] b=Encoding.UTF8.GetBytes(s);w.Write((ulong)b.Length);w.Write(b);}
    static void Fixture(string path){using(var w=new BinaryWriter(File.Create(path))){w.Write(0x46554747u);w.Write(3u);w.Write(2ul);w.Write(2ul);Str(w,"general.architecture");w.Write(8u);Str(w,"gemma3");Str(w,"gemma3.block_count");w.Write(4u);w.Write(48u);Str(w,"token_embd.weight");w.Write(2u);w.Write(4ul);w.Write(3ul);w.Write(0u);w.Write(0ul);Str(w,"v.projector.weight");w.Write(1u);w.Write(8ul);w.Write(0u);w.Write(64ul);while(w.BaseStream.Position%32!=0)w.Write((byte)0);w.Write(Enumerable.Range(0,96).Select(i=>(byte)i).ToArray());}}
    [STAThread]static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,90000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]),dir=Path.GetDirectoryName(exe);Directory.SetCurrentDirectory(dir);AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(dir,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};App=Assembly.LoadFrom(exe);
            Data=args.Length>2?args[2]:Path.Combine(output,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LlamaRuntimeRegression).GetMethod("Folder"));
            if(args.Length>2){Call("ClassMain","read_setSetting",(string)Call("ClassMain","readJiekou"));Check((string)T("LocalModelSettings").GetField("Backend").GetValue(null)=="llamacpp","Backend lost across process restart");Check((string)Get(Call("LocalRuntime","Load"),"Acceleration")=="cpu","Acceleration lost across restart");File.WriteAllText(Path.Combine(output,"restart-passed.txt"),"llama.cpp + CPU settings survive a new process.");return 0;}
            Check((string)T("LocalModelSettings").GetField("Backend").GetValue(null)=="llamacpp","Fresh install must default to llama.cpp");
            var token=CancellationToken.None;Action<string> progress=s=>{};byte[] bytes=Enumerable.Range(0,8192).Select(i=>(byte)(i%251)).ToArray();
            foreach(bool ignore in new[]{false,true})using(var server=new Server(bytes)){server.IgnoreRange=ignore;string path=Path.Combine(Data,"resume-"+ignore+".gguf");File.WriteAllBytes(path+".partial",bytes.Take(1234).ToArray());var spec=Spec(server,bytes);Call("GgufModels","Download",spec,path,progress,token);Check(File.ReadAllBytes(path).SequenceEqual(bytes),"Resumed download differs");Check(server.Range=="bytes=1234-","Range header absent");Call("GgufModels","Download",spec,path,progress,token);Check(server.Requests==1,"Verified cache was downloaded twice");}
            using(var server=new Server(bytes)){server.CorruptRange=true;string p=Path.Combine(Data,"bad-range.gguf");File.WriteAllBytes(p+".partial",bytes.Take(30).ToArray());Check(Fail(()=>Call("GgufModels","Download",Spec(server,bytes),p,progress,token)).Contains("范围"),"Bad Content-Range accepted");Check(!File.Exists(p),"Bad range published a model");}
            using(var server=new Server(bytes)){var spec=Spec(server,bytes);Set(spec,"Digest",new string('0',64));string p=Path.Combine(Data,"bad-hash.gguf");Check(Fail(()=>Call("GgufModels","Download",spec,p,progress,token)).Contains("SHA256"),"Bad checksum accepted");Check(!File.Exists(p)&&File.Exists(p+".partial"),"Corrupt download replaced model or partial lost");}
            using(var server=new Server(bytes))using(var cancel=new CancellationTokenSource()){server.Slow=true;string p=Path.Combine(Data,"cancel.gguf");var spec=Spec(server,bytes);var task=Task.Run(()=>Fail(()=>Call("GgufModels","Download",spec,p,progress,cancel.Token)));Check(server.Began.WaitOne(5000),"Download did not begin");cancel.Cancel();Check(task.Wait(4000)&&task.Result.Length>0,"Blocked download did not cancel");Check(!File.Exists(p)&&File.Exists(p+".partial"),"Canceled download published a model");}
            Log.Add("Download: Range resume, ignored Range restart, verified-cache reuse, wrong range/checksum rejection, blocked download cancellation: passed.");
            string source=Path.Combine(Data,"legacy.gguf"),converted=Path.Combine(Data,"converted.gguf");Fixture(source);byte[] original=File.ReadAllBytes(source);Check((bool)Call("LegacyGemmaConverter","NeedsConversion",source),"Legacy fixture not detected");Call("LegacyGemmaConverter","Convert",source,converted,progress,token);Check(!(bool)Call("LegacyGemmaConverter","NeedsConversion",converted),"Converted metadata still invalid");byte[] final=File.ReadAllBytes(converted);Check(!Encoding.UTF8.GetString(final).Contains("v.projector.weight"),"Projector was not separated");Check(final.Skip(final.Length-64).SequenceEqual(Enumerable.Range(0,64).Select(i=>(byte)i)),"Text tensor bytes changed");Check(File.ReadAllBytes(source).SequenceEqual(original),"Original model modified");using(var cancel=new CancellationTokenSource()){cancel.Cancel();string path=Path.Combine(Data,"cancel-convert.gguf");Check(Fail(()=>Call("LegacyGemmaConverter","Convert",source,path,progress,cancel.Token)).Length>0&&!File.Exists(path),"Canceled conversion published incomplete file");}
            Check(Fail(()=>Call("GgufModels","ValidateFile",Path.Combine(Data,"absent.gguf"))).Contains("未找到"),"Missing model not explained");Log.Add("Legacy Gemma converter: immutable source, exact tensor bytes, text-only extraction, metadata repaired, cancellation cleanup: passed.");
            using(var server=new Server(bytes)){
                T("LlamaRuntime").GetField("address",F).SetValue(null,server.Address);T("LlamaRuntime").GetField("key",F).SetValue(null,"unit-test-secret");
                var result=Call("LlamaRuntime","Request","/completion",null,1000,(Func<bool>)(()=>true));Check(result.ToString().Contains("你好")&&server.Auth=="Bearer unit-test-secret","Local authentication/Unicode failed");
                server.Invalid=true;Check(Fail(()=>Call("LlamaRuntime","Request","/completion",null,1000,(Func<bool>)(()=>true))).Contains("无法识别"),"Malformed output accepted");server.Invalid=false;server.Failure=true;Check(Fail(()=>Call("LlamaRuntime","Request","/completion",null,1000,(Func<bool>)(()=>true))).Contains("simulated failure"),"Server failure hidden");server.Failure=false;server.Slow=true;Check(Fail(()=>Call("LlamaRuntime","Request","/weights",null,150,(Func<bool>)(()=>true))).Contains("超时"),"Timeout absent");
            }Call("LlamaRuntime","Stop");Log.Add("Loopback protocol: per-process authentication, Unicode, invalid response, server error and timeout messages: passed.");
            Application.EnableVisualStyles();using(var owner=new Form{TopMost=true})using(var wizard=(Form)New("LocalSetupWizard")){
                owner.Show();wizard.Show(owner);Application.DoEvents();Check(wizard.TopMost,"Wizard hidden under owner");((TabControl)Get(wizard,"pages")).SelectedIndex=1;var done=(Button)Get(wizard,"done");Check(done.Visible&&done.Parent.ClientRectangle.Contains(done.Bounds),"Footer clipped");Check(((ComboBox)Get(wizard,"acceleration")).SelectedIndex==1,"Fresh device must be CPU");done.PerformClick();Check(wizard.IsDisposed,"Save did not close wizard");
            }
            Call("ClassMain","read_setSetting",(string)Call("ClassMain","readJiekou"));Check((string)T("LocalModelSettings").GetField("Backend").GetValue(null)=="llamacpp","Backend not saved");File.WriteAllText(Path.Combine(output,"restart-data-path.txt"),Data);
            var entry=New("EngineDefinition");Set(entry,"Name","My GGUF");Set(entry,"Provider","llamacpp");Set(entry,"Model",source);int id=(int)Call("EngineRegistry","Save",entry);Check((bool)Call("LocalModelClient","IsLocalEngine",id),"Custom GGUF routes to cloud");Call("EngineRegistry","Remove",id);Check(File.Exists(source),"Removing engine deleted model");
            Log.Add("Settings UI: default llama.cpp, Ollama switch, topmost owner, complete footer, save; custom GGUF routes locally and removal preserves file: passed.");
            File.WriteAllLines(Path.Combine(output,"result.txt"),Log);Console.WriteLine("llama.cpp runtime regression passed.");return 0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"result.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
