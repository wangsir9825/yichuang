using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

class NetworkProxyRegression
{
    static Assembly App;static string Data,Output,LastError;static int Prompts;
    static DialogResult Answer=DialogResult.No;
    static BindingFlags Static=BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public,Hidden=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    static readonly List<string> Log=new List<string>();static readonly JavaScriptSerializer Json=new JavaScriptSerializer();
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static Type T(string name){return App.GetType("YiChuang."+name,true);}
    static object Call(string type,string name,params object[] args){return T(type).GetMethods(Static).Single(m=>m.Name==name&&m.GetParameters().Length==args.Length).Invoke(null,args);}
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static string Error(Action work){try{work();return "";}catch(Exception ex){while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;return ex.GetType().Name+": "+ex.Message;}}
    static object Options(string mode,string address){return Activator.CreateInstance(T("NetworkProxyOptions"),Hidden,null,new object[]{mode,address},null);}
    static void Configure(string mode,string address=""){T("NetworkProxy").GetField("Current",Static).SetValue(null,Options(mode,address));}
    static string Current(string key){var options=T("NetworkProxy").GetField("Current",Static).GetValue(null);return (string)options.GetType().GetField(key,Hidden).GetValue(options);}
    static HttpWebRequest Request(string url){return (HttpWebRequest)T("NetworkProxy").GetMethod("Create",Static,null,new[]{typeof(string)},null).Invoke(null,new object[]{url});}
    static Control Find(Control parent,string name){if(parent.Name==name)return parent;foreach(Control c in parent.Controls){var found=Find(c,name);if(found!=null)return found;}return null;}
    static void Pump(){Application.DoEvents();}
    static void Show(Form form,string key){form.GetType().GetMethod("ShowPage").Invoke(form,new object[]{key});Pump();}
    static Button Button(Form form,string name){return (Button)Find(form,name);}
    static void Draft(Form f,int index,string address){((ComboBox)Find(f,"NetworkProxyMode")).SelectedIndex=index;((TextBox)Find(f,"NetworkProxyAddress")).Text=address;Pump();}
    static Form Settings()
    {
        var f=(Form)Activator.CreateInstance(T("Frmsetting"));f.Opacity=0;
        f.GetType().GetField("EngineChangesPrompt",Hidden).SetValue(f,new Func<DialogResult>(()=>{Prompts++;return Answer;}));
        f.GetType().GetField("SaveErrorPrompt",Hidden).SetValue(f,new Action<string>(s=>LastError=s));f.Show();Pump();Show(f,"网络代理");return f;
    }
    static Dictionary<string,object> Saved(){return Json.Deserialize<Dictionary<string,object>>((string)Call("ClassMain","readJiekou"));}
    static void SettingsTests()
    {
        Configure("system");Call("ClassMain","jiluset");
        using(var f=Settings())
        {
            Check(!Find(f,"NetworkProxyAddress").Enabled,"System mode address must be disabled");
            Draft(f,2,"http://127.0.0.1:10808");Check(Button(f,"ApplySettings").Enabled&&Current("Mode")=="system","Draft changed active route");
            Answer=DialogResult.Cancel;f.Close();Pump();Check(!f.IsDisposed&&Current("Mode")=="system","Cancel did not retain draft");
            Answer=DialogResult.No;Show(f,"引擎管理");Check(Current("Mode")=="system"&&Saved()["NetworkProxyMode"].ToString()=="system","Discard persisted draft");
            Show(f,"网络代理");Check(((ComboBox)Find(f,"NetworkProxyMode")).SelectedIndex==0,"Reopen retained discarded mode");
            Draft(f,2,"socks5://localhost:10808");LastError=null;Button(f,"ApplySettings").PerformClick();Pump();Check(LastError!=null&&LastError.Contains("HTTP")&&Current("Mode")=="system","Invalid proxy applied");
            Draft(f,2,"http://127.0.0.1:10808");LastError=null;
            using(var locked=new FileStream(Path.Combine(Data,"YiChuang","setting.txt"),FileMode.Open,FileAccess.Read,FileShare.None))Button(f,"ApplySettings").PerformClick();
            Check(LastError!=null&&Current("Mode")=="system","Failed save changed active route");
            LastError=null;Button(f,"ApplySettings").PerformClick();Pump();Check(LastError==null&&Current("Mode")=="manual"&&!Button(f,"ApplySettings").Enabled&&!f.IsDisposed,"Apply did not persist and stay");
            Check(Saved()["NetworkProxyAddress"].ToString()=="http://127.0.0.1:10808","Proxy address not persisted");
            Draft(f,1,"");Answer=DialogResult.No;f.Close();Pump();Check(f.IsDisposed&&Current("Mode")=="manual","Titlebar discard lost last applied route");
        }
        using(var f=Settings())
        {
            Check(((ComboBox)Find(f,"NetworkProxyMode")).SelectedIndex==2,"Saved mode missing on reopen");
            Draft(f,1,"");Button(f,"SaveSettings").PerformClick();Pump();Check(f.IsDisposed&&Current("Mode")=="direct","Save did not close and persist");
        }
        using(var f=Settings())
        {
            Draft(f,0,"");Answer=DialogResult.Yes;Button(f,"ReturnSettings").PerformClick();Pump();Check(f.IsDisposed&&Current("Mode")=="system","Exit Yes did not save");
        }
        foreach(var value in new[]{"socks5://localhost:10808","https://localhost:10808","http://localhost:0","http://localhost:65536","http://u:p@localhost:10808","http://localhost:10808/sub","http://localhost:10808/?token=x","bad","http://localhost:10808/#x"})
            Check(Error(()=>T("NetworkProxyOptions").GetMethod("Validate",Hidden).Invoke(Options("manual",value),null)).Contains("HTTP"),"Invalid address accepted: "+value);
        var backup=Saved();backup["NetworkProxyMode"]="manual";backup["NetworkProxyAddress"]="socks5://localhost:10808";
        Check(Error(()=>Call("SettingsStore","ImportBackup",Json.Serialize(backup))).Contains("HTTP"),"Invalid imported proxy accepted");
        Check(Saved()["NetworkProxyMode"].ToString()=="system","Rejected backup modified config");
        Configure("manual","http://127.0.0.1:10808");Call("ClassMain","jiluset");File.WriteAllText(Path.Combine(Output,"restart-data-path.txt"),Data);
        Log.Add("Settings: drafts isolated; Apply/Save/Exit/close Yes/No/Cancel; page discard; disk-write failure rollback; invalid URL/import; protected persistence passed (prompts="+Prompts+")");
    }
    sealed class Server:IDisposable
    {
        readonly TcpListener listener;internal readonly List<string> Requests=new List<string>();internal int Status=200,Delay;internal string Address;
        internal Server(){listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Address="http://127.0.0.1:"+((IPEndPoint)listener.LocalEndpoint).Port;Task.Run((Action)Loop);}
        void Loop(){while(true){TcpClient socket;try{socket=listener.AcceptTcpClient();}catch{return;}Task.Run(()=>Serve(socket));}}
        void Serve(TcpClient socket)
        {
            using(socket)try{using(var s=socket.GetStream())
            {
                s.ReadTimeout=5000;var raw=new List<byte>();int b;while((b=s.ReadByte())>=0){raw.Add((byte)b);int n=raw.Count;if(n>=4&&raw[n-4]==13&&raw[n-3]==10&&raw[n-2]==13&&raw[n-1]==10)break;}
                string headers=Encoding.ASCII.GetString(raw.ToArray());
                if(headers.IndexOf("Expect: 100-continue",StringComparison.OrdinalIgnoreCase)>=0){var cont=Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");s.Write(cont,0,cont.Length);}
                int length=0;foreach(string line in headers.Split('\n'))if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))length=int.Parse(line.Substring(15).Trim());
                byte[] body=new byte[length];int read=0;while(read<length){int n=s.Read(body,read,length-read);if(n==0)break;read+=n;}
                lock(Requests)Requests.Add(headers.Split('\n')[0]);int status=Status,delay=Delay;
                string result="{\"auto_translation\":[\"route-ok\"],\"data\":{\"translate\":{\"dit\":\"route-ok\"}},\"translations\":[{\"text\":\"route-ok\"}],\"candidates\":[{\"content\":{\"parts\":[{\"text\":\"route-ok\"}]}}],\"choices\":[{\"message\":{\"content\":\"route-ok\"}}]}";
                if(headers.Contains("api.deepseek.com"))result="data: {\"choices\":[{\"delta\":{\"content\":\"route-ok\"},\"finish_reason\":\"stop\"}]}\n\ndata: [DONE]\n\n";
                byte[] content=Encoding.UTF8.GetBytes(result),response=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+" "+(status==200?"OK":"Proxy Authentication Required")+"\r\nContent-Length: "+content.Length+"\r\nConnection: close\r\n\r\n");
                if(delay>0)Thread.Sleep(delay);s.Write(response,0,response.Length);s.Write(content,0,content.Length);
            }}catch(IOException){}catch(ObjectDisposedException){}
        }
        public void Dispose(){listener.Stop();}
    }
    // The fake proxy receives only synthetic text/keys. HTTPS endpoint creation
    // is mapped to HTTP .invalid hosts to test every adapter without real services.
    sealed class FakeEndpoints:IWebRequestCreate
    {
        public WebRequest Create(Uri uri){return WebRequest.CreateDefault(new Uri("http://"+uri.Host+".invalid"+uri.PathAndQuery));}
    }
    static string Read(HttpWebRequest request){request.Timeout=2000;using(var r=request.GetResponse())using(var reader=new StreamReader(r.GetResponseStream()))return reader.ReadToEnd();}
    static void TransportTests()
    {
        using(var proxy=new Server())using(var origin=new Server())
        {
            Configure("manual",proxy.Address);Read(Request("http://route.invalid/check"));Check(proxy.Requests.Count==1,"Manual proxy did not receive request");
            T("NetworkProxy").GetField("SystemProxyFactory",Static).SetValue(null,new Func<IWebProxy>(()=>new WebProxy(proxy.Address)));
            Configure("system");Read(Request("http://system.invalid/check"));Check(proxy.Requests.Count==2,"System proxy factory unused");
            Configure("direct");var direct=Request("http://direct.invalid/check");Check(direct.Proxy==null,"Direct mode retained proxy");direct.Abort();
            Configure("manual",proxy.Address);Read(Request(origin.Address+"/local"));Check(origin.Requests.Count==1&&proxy.Requests.Count==2,"Loopback was proxied");
            Configure("manual","invalid");foreach(string url in new[]{"http://localhost:11434/","http://127.0.0.2:11434/","http://[::1]:11434/"}){var request=Request(url);Check(request.Proxy==null,"IPv4/IPv6/localhost must bypass invalid proxy");request.Abort();}
            Check(Error(()=>Request("http://blocked.invalid/")).Contains("HTTP"),"Invalid active manual proxy silently fell back");
            Configure("manual",proxy.Address);Check(WebRequest.RegisterPrefix("https://",new FakeEndpoints()),"Could not guard real network");
            Func<bool> current=()=>true;Action<string,string> translated=(name,text)=>Check(text.Contains("route-ok"),name+" did not translate through mock proxy");
            translated("Transmart",(string)Call("Classfanyi","getTransmart","hello","ZH",current));
            translated("Sogou",(string)Call("Classfanyi","getSouGou","hello","ZH",current));
            translated("DeepL",(string)Call("DeepLClient","TranslateCancellable","hello","api.deepl.com","synthetic-key",true,"EN","ZH",current,2500));
            translated("DeepSeek",(string)Call("ChatGPT","LoadSourceCode4","hello","api.deepseek.com","synthetic-key","test-model",current));
            T("ClassMain").GetField("GeminiKey").SetValue(null,"synthetic-key");T("ClassMain").GetField("QwenKey").SetValue(null,"synthetic-key");
            translated("Gemini",(string)Call("CloudTranslationClient","Translate",18,"hello","en-zh",current));
            translated("Qwen",(string)Call("CloudTranslationClient","Translate",19,"hello","en-zh",current));
            var custom=Activator.CreateInstance(T("EngineDefinition"));foreach(var p in new Dictionary<string,object>{{"Id",1000},{"Name","Test"},{"Provider","openai"},{"Endpoint","https://custom.invalid/v1"},{"Model","test"},{"ApiKey","synthetic-key"},{"Direction","en-zh"},{"TimeoutSeconds",10}})custom.GetType().GetField(p.Key).SetValue(custom,p.Value);
            translated("Custom OpenAI",(string)Call("CustomTranslationClient","Translate",custom,"hello",current));
            Check(proxy.Requests.Count==10,"Not all seven engines (+Sogou preflight) used proxy");
            foreach(string host in new[]{"transmart.qq.com","fanyi.sogou.com","api.deepl.com","api.deepseek.com","generativelanguage.googleapis.com","dashscope.aliyuncs.com","custom.invalid"})Check(proxy.Requests.Any(r=>r.Contains(host)),"Missing route "+host);
            var pending=Request("http://snapshot.invalid/check");Configure("direct");Read(pending);Check(proxy.Requests.Count==11,"Existing request lost its route snapshot");Configure("manual",proxy.Address);
            proxy.Status=407;Check(Error(()=>Call("Classfanyi","getTransmart","hello","ZH",current)).Contains("代理要求身份验证"),"407 not understandable");
            proxy.Status=200;proxy.Delay=2000;var timer=Stopwatch.StartNew();string timeout=Error(()=>Call("DeepLClient","TranslateCancellable","hello","api.deepl.com","synthetic-key",true,"EN","ZH",current,200));Check(timeout.Contains("超时")&&timer.ElapsedMilliseconds<1600,"Proxy timeout hung");
            bool active=true;using(var cancel=new System.Threading.Timer(s=>active=false,null,180,Timeout.Infinite)){timer.Restart();string failure=Error(()=>Call("Classfanyi","getTransmart","hello","ZH",new Func<bool>(()=>active)));Check(failure.Contains("OperationCanceledException")&&timer.ElapsedMilliseconds<1600,"Proxy cancellation hung");}
            // A refused proxy cannot fall back to the configured origin.
            var reserved=new TcpListener(IPAddress.Loopback,0);reserved.Start();int closedPort=((IPEndPoint)reserved.LocalEndpoint).Port;reserved.Stop();Configure("manual","http://127.0.0.1:"+closedPort);
            Check(Error(()=>Call("Classfanyi","getTransmart","hello","ZH",current)).Contains("网络代理"),"Connection failure missing settings guidance");
            Check(origin.Requests.Count==1,"Unexpected direct fallback");
        }
        Log.Add("Transport: actual loopback mock proxy received all 7 online adapters; system/manual/direct routing; in-flight route snapshot; local bypass incl IPv6; 407, timeout, cancel, bad/refused proxy without fallback passed. No real paid API called.");
    }
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);bool ui=args.Length>2&&args[2]=="--ui",live=args.Length>2&&args[2]=="--live",restart=args.Length>3&&args[2]=="--read";
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,ui?900000:live?100000:60000,Timeout.Infinite))try
        {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
            App=Assembly.LoadFrom(exe);Data=restart?Path.GetFullPath(args[3]):Path.Combine(Output,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var delegates=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);delegates[318]=Delegate.CreateDelegate(delegates[318].GetType(),typeof(NetworkProxyRegression).GetMethod("Folder"));
            T("Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            Call("ClassMain","read_setSetting",Call("ClassMain","readJiekou"));Application.EnableVisualStyles();
            if(restart){Check(Current("Mode")=="manual"&&Current("Address")=="http://127.0.0.1:10808","New process lost proxy settings");Log.Add("Fresh-process protected proxy settings load passed");}
            else if(live)
            {
                foreach(string mode in new[]{"direct","system"})foreach(string engine in new[]{"getTransmart","getSouGou"})
                {
                    Configure(mode);string result="";var elapsed=Stopwatch.StartNew();
                    string error=Error(()=>result=(string)Call("Classfanyi",engine,"Good tools make things simple.","ZH",new Func<bool>(()=>elapsed.ElapsedMilliseconds<18000)));
                    string line=mode+" "+engine+" "+elapsed.ElapsedMilliseconds+"ms "+(error.Length==0?"translation="+result.Trim():error);
                    Log.Add(line);Console.WriteLine(line);File.WriteAllLines(Path.Combine(Output,"live-web-translation.txt"),Log);
                }
            }
            else if(ui){Configure("system");var f=(Form)Activator.CreateInstance(T("Frmsetting"));f.Shown+=(s,e)=>Show(f,"网络代理");Application.Run(f);Log.Add("Manual UI review closed normally");}
            else {SettingsTests();TransportTests();}
            File.WriteAllLines(Path.Combine(Output,"network-proxy-regression.txt"),Log);Console.WriteLine(string.Join(Environment.NewLine,Log));return 0;
        }catch(Exception ex){while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"network-proxy-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
