using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class LocalSetupRegression
{
    static Assembly App;static string Data;static readonly List<string> Log=new List<string>();
    static BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    static object Invoke(string name,params object[] args){try{return App.GetType("YiChuang.LocalRuntime").GetMethod(name,Flags).Invoke(null,args);}catch(TargetInvocationException ex){throw ex.InnerException;}}
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static string Failure(Action action){try{action();return "";}catch(Exception ex){return ex.Message;}}
    static object New(string name){return Activator.CreateInstance(App.GetType("YiChuang."+name),true);}
    static object Field(object obj,string name){return obj.GetType().GetField(name,Flags).GetValue(obj);}
    static void Set(object obj,string name,object value){obj.GetType().GetField(name,Flags).SetValue(obj,value);}
    static void PumpUntil(Func<bool> ready){var until=DateTime.UtcNow.AddSeconds(10);while(!ready()&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(10);}Check(ready(),"Wizard operation did not finish");}
    class Server:IDisposable
    {
        public readonly string Address;public readonly HashSet<string> Models=new HashSet<string>();public readonly List<string> Calls=new List<string>();
        public bool Forget,Slow,Error,InvalidTags;public string CreateBody="";readonly HttpListener listener=new HttpListener();readonly ManualResetEvent stop=new ManualResetEvent(false);
        public readonly ManualResetEvent PullStarted=new ManualResetEvent(false);
        public Server(){var socket=new TcpListener(IPAddress.Loopback,0);socket.Start();int port=((IPEndPoint)socket.LocalEndpoint).Port;socket.Stop();Address="http://127.0.0.1:"+port+"/";listener.Prefixes.Add(Address);listener.Start();Task.Run(()=>{while(listener.IsListening)try{var context=listener.GetContext();Task.Run(()=>Handle(context));}catch{break;}});}
        void Handle(HttpListenerContext context){try {
            string path=context.Request.Url.AbsolutePath;lock(Calls)Calls.Add(path);
            string body=new StreamReader(context.Request.InputStream).ReadToEnd();string response;
            if(path=="/api/tags"){lock(Models)response=InvalidTags?"{}":new JavaScriptSerializer().Serialize(new{models=Models.Select(x=>new{name=x}).ToArray()});}
            else {
                var payload=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(body);
                if(path=="/api/create")CreateBody=body;
                if(Error)response="{\"error\":\"test failure\"}\n";
                else if(Slow){context.Response.SendChunked=true;byte[] pending=Encoding.UTF8.GetBytes("{\"status\":\"pulling\"}\n");context.Response.OutputStream.Write(pending,0,pending.Length);context.Response.OutputStream.Flush();PullStarted.Set();stop.WaitOne(10000);return;}
                else{if(!Forget)lock(Models)Models.Add((string)payload["model"]);response="{\"status\":\"pulling\",\"total\":100,\"completed\":70}\n{\"status\":\"success\"}\n";}
            }
            byte[] bytes=Encoding.UTF8.GetBytes(response);context.Response.ContentType="application/json";context.Response.ContentLength64=bytes.Length;context.Response.OutputStream.Write(bytes,0,bytes.Length);
        }catch{}finally{try{context.Response.Close();}catch{}}}
        public int Writes{get{lock(Calls)return Calls.Count(p=>p!="/api/tags");}}
        public void Dispose(){stop.Set();listener.Close();}
    }
    [STAThread]static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watch=new System.Threading.Timer(s=>Environment.Exit(124),null,60000,Timeout.Infinite))try {
            string exe=Path.GetFullPath(args[0]),app=Path.GetDirectoryName(exe);Directory.SetCurrentDirectory(app);
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(app,new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
            App=Assembly.LoadFrom(exe);Data=Path.Combine(output,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LocalSetupRegression).GetMethod("Folder"));
            var options=New("LocalRuntimeOptions");Set(options,"Executable","C:\\test\\ollama.exe");Set(options,"ModelsDirectory",Path.Combine(Data,"models"));Set(options,"WizardCompleted",true);Invoke("Save",options);var loaded=Invoke("Load");Check((string)Field(loaded,"Executable")=="C:\\test\\ollama.exe"&&(bool)Field(loaded,"WizardCompleted"),"Runtime settings roundtrip");
            string previous=Environment.GetEnvironmentVariable("OLLAMA_MODELS");try{Environment.SetEnvironmentVariable("OLLAMA_MODELS",Path.Combine(Data,"custom-cache"));Check((string)Invoke("DefaultModelsDirectory")==Path.Combine(Data,"custom-cache"),"Environment model directory ignored");}finally{Environment.SetEnvironmentVariable("OLLAMA_MODELS",previous);}
            string preferred=Path.Combine(Data,"ollama.exe");File.WriteAllText(preferred,"test path only; never executed");Check((string)Invoke("FindExecutable",preferred)==preferred,"Preferred executable discovery");
            var token=CancellationToken.None;Action<string> progress=s=>{};
            Invoke("ReadProgress",new StringReader("{\"status\":\"success\"}\n"),progress,token);
            foreach(string invalid in new[]{"{\"status\":\"pulling\"}\n","{\"error\":\"broken\"}\n","not-json\n"})Check(Failure(()=>Invoke("ReadProgress",new StringReader(invalid),progress,token))!="","Incomplete download accepted");
            using(var cancel=new CancellationTokenSource()){cancel.Cancel();Check(Failure(()=>Invoke("ReadProgress",new StringReader("{\"status\":\"success\"}\n"),progress,cancel.Token))!="","Cancellation ignored");}
            Log.Add("Runtime settings, environment/default path selection, preferred program, stream success/error/EOF/malformed/cancel: passed.");
            using(var server=new Server()) {
                server.Models.Add("yichuang-hy-mt2:7b-q6");server.Models.Add("translategemma:12b");
                string untouched=Path.Combine(Data,"must-not-create");Set(options,"ModelsDirectory",untouched);
                Check(!(bool)Invoke("EnsureService",options,server.Address,progress,token)&&!Directory.Exists(untouched),"Existing service was replaced or cache directory changed");
                Invoke("PrepareModels",server.Address,true,true,progress,token);Check(server.Writes==0,"Existing models were re-downloaded");
                Check(Failure(()=>Invoke("PrepareModels",server.Address,false,false,progress,token)).Contains("勾选"),"Empty selection accepted");
            }
            using(var server=new Server()){server.Models.Add("kaelri/hy-mt2:7b");Invoke("PrepareModels",server.Address,true,false,progress,token);Check(server.Writes==1&&server.Calls.Contains("/api/create")&&!server.Calls.Contains("/api/pull"),"Existing Hy base not reused");Check(server.CreateBody.Contains("<|extra_4|>")&&server.CreateBody.Contains("repeat_penalty"),"Hy template or generation parameters lost");}
            using(var server=new Server()){server.Models.Add("previous-hy-mt2:7b-q6");Invoke("PrepareModels",server.Address,true,false,progress,token);Check(server.Writes==1&&!server.Calls.Contains("/api/pull")&&server.CreateBody.Contains("previous-hy-mt2:7b-q6"),"Previous Hy alias caused a weight download");}
            using(var server=new Server()){Invoke("PrepareModels",server.Address,false,true,progress,token);Check(server.Writes==1&&server.Models.SetEquals(new[]{"translategemma:12b"}),"Unchecked model was downloaded");}
            using(var server=new Server()){server.Forget=true;Check(Failure(()=>Invoke("PrepareModels",server.Address,false,true,progress,token)).Contains("尚未列出"),"Download success accepted without actual model tag");}
            using(var server=new Server()){server.Error=true;Check(Failure(()=>Invoke("PrepareModels",server.Address,false,true,progress,token)).Contains("test failure"),"Service failure discarded");}
            using(var server=new Server()){server.InvalidTags=true;Check(Failure(()=>Invoke("EnsureService",options,server.Address,progress,token)).Contains("有效的模型列表"),"Non-Ollama service mistaken for offline service");}
            using(var server=new Server())using(var cancel=new CancellationTokenSource()) {
                server.Slow=true;var task=Task.Run(()=>{try{Invoke("PrepareModels",server.Address,false,true,progress,cancel.Token);return false;}catch(OperationCanceledException){return true;}});
                Check(server.PullStarted.WaitOne(5000),"Pull did not begin");cancel.Cancel();Check(task.Wait(5000)&&task.Result,"Blocked download not canceled promptly");
            }
            Check(Failure(()=>Invoke("EnsureService",options,"https://example.com",progress,token)).Contains("本机"),"Remote service launch accepted");
            Log.Add("Loopback service reuse; zero re-download for cached models; Hy alias-only creation; selected model only; actual tags required; server errors; blocked stream cancellation: passed.");
            Application.EnableVisualStyles();
            using(var server=new Server())using(var owner=new Form{TopMost=true})using(var wizard=(Form)New("OllamaSetupWizard")) {
                owner.Show();((TextBox)Field(wizard,"endpoint")).Text=server.Address;wizard.Show(owner);Application.DoEvents();Check(wizard.TopMost,"Wizard hidden below topmost owner");
                var done=(Button)Field(wizard,"done");Check(done.Visible&&done.Parent.ClientRectangle.Contains(done.Bounds),"Wizard footer clipped");
                Control.CheckForIllegalCrossThreadCalls=true;SynchronizationContext.SetSynchronizationContext(null);
                var detect=(Button)Field(wizard,"detect");detect.PerformClick();PumpUntil(()=>detect.Enabled);Check((bool)Field(wizard,"serviceReady"),"Wizard detection failed");
                Check(!wizard.Controls.Find("PrepareHy",true).Any()&&!wizard.Controls.Find("PrepareGemma",true).Any(),"Ollama setup still recommends presets");
                using(var bitmap=new System.Drawing.Bitmap(wizard.Width,wizard.Height)){wizard.DrawToBitmap(bitmap,new System.Drawing.Rectangle(0,0,wizard.Width,wizard.Height));bitmap.Save(Path.Combine(output,"wizard.png"));}
                done.PerformClick();Check(wizard.IsDisposed,"Wizard completion did not close");
                for(int i=0;i<25;i++){Application.DoEvents();Thread.Sleep(5);}
            }
            Log.Add("Actual wizard controls: visible footer, async service check, actual model list and service settings saved in isolated profile, finish/close: passed.");
            File.WriteAllLines(Path.Combine(output,"result.txt"),Log);Console.WriteLine("Local setup regression passed.");return 0;
        }catch(Exception ex){Log.Add(ex.GetType().FullName+": "+ex.Message);File.WriteAllLines(Path.Combine(output,"result.txt"),Log);Console.Error.WriteLine(ex.GetType().FullName+": "+ex.Message);return 1;}
    }
}
