using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class LocalModelRegression
{
    static string Data, Output;
    static int Port, ExternalCalls;
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
    static readonly List<Dictionary<string,object>> Chats = new List<Dictionary<string,object>>();
    static int Unloads;
    static string Loaded = "translategemma:12b";
    static volatile string Fault = "";
    static string ErrorOf(Action action) {
        try { action(); return ""; } catch(Exception ex) { while(ex is TargetInvocationException && ex.InnerException != null) ex=ex.InnerException; return ex.Message; }
    }
    static void Assert(bool value,string text) { if (!value) throw new Exception(text); }
    public static string Folder(Environment.SpecialFolder folder) { return Data; }
    static object Member(object instance,string name) { return instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance); }
    static void Set(Type type,string name,object value) { type.GetField(name).SetValue(null,value); }
    static void Wait(Func<bool> condition,int seconds=8)
    {
        DateTime until = DateTime.UtcNow.AddSeconds(seconds);
        while (!condition() && DateTime.UtcNow < until) { Application.DoEvents(); Thread.Sleep(15); }
        Assert(condition(),"UI operation timed out");
    }
    class Guard : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            if (uri.Scheme != "http" || uri.Host != "127.0.0.1" || uri.Port != Port) { Interlocked.Increment(ref ExternalCalls); throw new Exception("Unexpected external request: " + uri.Host); }
            return WebRequest.CreateDefault(uri);
        }
    }
    static void Serve(TcpListener listener)
    {
        while (true)
        {
            TcpClient socket;
            try { socket = listener.AcceptTcpClient(); } catch { return; }
            using (socket) try { using (var stream = socket.GetStream())
            {
                stream.ReadTimeout = 5000;
                var head = new MemoryStream(); int value;
                while ((value = stream.ReadByte()) >= 0) {
                    head.WriteByte((byte)value); int n = (int)head.Length; byte[] data = head.GetBuffer();
                    if (n >= 4 && data[n-4] == 13 && data[n-3] == 10 && data[n-2] == 13 && data[n-1] == 10) break;
                }
                string headers = Encoding.ASCII.GetString(head.ToArray());
                string path = headers.Split(' ')[1]; int length = 0;
                foreach (string line in headers.Split(new[]{"\r\n"},StringSplitOptions.None))
                    if (line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase)) length = int.Parse(line.Substring(15).Trim());
                byte[] body = new byte[length]; int offset = 0;
                while (offset < length) { int count = stream.Read(body,offset,length-offset); if (count == 0) break; offset += count; }
                Dictionary<string,object> payload = length == 0 ? null : Json.Deserialize<Dictionary<string,object>>(Encoding.UTF8.GetString(body));
                string response; string fault = Fault; string httpStatus = "200 OK";
                if (path == "/api/tags") response = "{\"models\":[{\"name\":\"yichuang-hy-mt2:7b-q6\"},{\"name\":\"translategemma:12b\"}]}";
                else if (path == "/api/ps") response = string.IsNullOrEmpty(Loaded) ? "{\"models\":[]}" : Json.Serialize(new { models = new[]{new {name=Loaded}} });
                else if (path == "/api/generate") { Assert(Convert.ToInt32(payload["keep_alive"]) == 0,"Unload did not release the other model"); Loaded=""; Interlocked.Increment(ref Unloads); response="{}"; }
                else if (path == "/api/chat") {
                    var messages = (System.Collections.IList)payload["messages"];
                    string prompt = ((Dictionary<string,object>)messages[0])["content"].ToString();
                    lock (Chats) Chats.Add(payload);
                    if (prompt.Contains("slow")) Thread.Sleep(350);
                    Loaded=payload["model"].ToString();
                    response=Json.Serialize(new { message=new {content=(Loaded.StartsWith("yichuang",StringComparison.OrdinalIgnoreCase)?"腾讯译文：":"Google译文：") + prompt.Substring(prompt.LastIndexOf('\n')+1)}, done_reason="stop", load_duration=1000000, eval_count=20 });
                } else throw new Exception("Unexpected API path " + path);
                if(fault=="missing" && path=="/api/tags") response="{\"models\":[]}";
                if(fault=="500" && path=="/api/chat") {httpStatus="500 Internal Server Error";response="{\"error\":\"GPU memory unavailable\"}";}
                if(fault=="invalid" && path=="/api/chat") response="broken json";
                if(fault=="delay" && path=="/api/chat") Thread.Sleep(2000);
                if(fault=="timeout" && path=="/api/chat") Thread.Sleep(12000);
                byte[] bytes=Encoding.UTF8.GetBytes(response);
                byte[] header=Encoding.ASCII.GetBytes("HTTP/1.1 "+httpStatus+"\r\nContent-Type: application/json\r\nContent-Length: "+bytes.Length+"\r\nConnection: close\r\n\r\n");
                stream.Write(header,0,header.Length);stream.Write(bytes,0,bytes.Length);
            } } catch(IOException) { } catch(SocketException) { }
        }
    }
    [STAThread]
    static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]); Directory.CreateDirectory(Output);
        var log=new List<string>();
        using (var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,55000,Timeout.Infinite))
        try
        {
            string exe=Path.GetFullPath(args[0]); Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string file=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(file)?Assembly.LoadFrom(file):null;};
            Assembly app=Assembly.LoadFrom(exe); Type main=app.GetType("YiChuang.ClassMain"), local=app.GetType("YiChuang.LocalModelSettings"), client=app.GetType("YiChuang.LocalModelClient");
            Data=Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LocalModelRegression).GetMethod("Folder"));
            app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,Output);
            var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Port=((IPEndPoint)listener.LocalEndpoint).Port;
            var server=Task.Run(()=>Serve(listener));
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());
            Set(local,"Backend","ollama");Set(local,"Endpoint","http://127.0.0.1:"+Port);
            try { client.GetMethod("ValidateEndpoint").Invoke(null,new object[]{"http://example.com"});throw new Exception("Remote endpoint accepted"); }
            catch(TargetInvocationException ex) {Assert(ex.InnerException is InvalidOperationException,"Unexpected endpoint validation error");}
            log.Add("Remote endpoint rejected before any HTTP request: passed");
            Application.EnableVisualStyles();
            var form=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2"));form.Opacity=0;form.Show();Application.DoEvents();
            var registry=app.GetType("YiChuang.EngineRegistry");
            var definition=app.GetType("YiChuang.EngineDefinition");
            int[] localIds=new int[2];
            for(int i=0;i<2;i++){
                var model=Activator.CreateInstance(definition);
                definition.GetField("Name").SetValue(model,i==0?"User Hy":"User Gemma");
                definition.GetField("Provider").SetValue(model,"ollama");
                definition.GetField("Endpoint").SetValue(model,"http://127.0.0.1:"+Port);
                definition.GetField("Model").SetValue(model,i==0?"yichuang-hy-mt2:7b-q6":"translategemma:12b");
                definition.GetField("Prompt").SetValue(model,i==0?"hy":"gemma");
                localIds[i]=(int)registry.GetMethod("Save").Invoke(null,new[]{model});
            }
            form.GetType().GetMethod("RebuildEngineMenu",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null);

            Set(main,"is单词悬浮框",true);
            form.GetType().GetMethod("SelectLocalEngine",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(form,new object[]{localIds[0]});
            var source=(RichTextBox)Member(form,"rtb原文");var target=(RichTextBox)Member(form,"rtb译文");
            form.GetType().GetMethod("xianshiyuanwen",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(form,new object[]{"hello"});
            Wait(()=>target.Text.Contains("腾讯译文：hello"));Assert(((ToolStripStatusLabel)Member(form,"modernStatus")).Text.StartsWith("翻译耗时："),"Local success timing absent");
            log.Add("Captured single word bypasses online floating dictionary and uses local Hy-MT2: passed");
            source.Text="slow";Wait(()=>{lock(Chats)return Chats.Exists(c=>Json.Serialize(c).Contains("slow"));});source.Text="fresh";
            Wait(()=>target.Text.Contains("腾讯译文：fresh"));
            log.Add("A newer source text replaces an in-flight result without stale text overwriting it: passed");
            form.GetType().GetMethod("SelectLocalEngine",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(form,new object[]{localIds[1]});
            Wait(()=>target.Text.Contains("Google译文：fresh"));
            Assert(Unloads>=2,"Models were not unloaded when switching");
            log.Add("Engine switch uses TranslateGemma and unloads the other translation model: passed");
            main.GetMethod("jiluset").Invoke(null,null);
            var savedMain=Json.Deserialize<Dictionary<string,object>>((string)main.GetMethod("readJiekou").Invoke(null,null));
            Assert(savedMain["Translation_interface"].ToString()==localIds[1].ToString(),"Selected local engine was reset on save");
            log.Add("Local settings and selected engine survive encrypted main-config save/read: passed");
            string unusual="第一行\nSecond line: C:\\temp\\file \"quoted\"";
            client.GetMethod("Translate").Invoke(null,new object[]{localIds[0],unusual,"en-zh",null});
            Dictionary<string,object> last;lock(Chats)last=Chats[Chats.Count-1];
            string sent=((Dictionary<string,object>)((System.Collections.IList)last["messages"])[0])["content"].ToString();
            Assert(sent.EndsWith(unusual),"Unicode/newline/quote JSON did not round-trip");
            Assert(ExternalCalls==0,"An external service was contacted");
            log.Add("Unicode/newline/quote/backslash encoding and zero external-service requests: passed");
            Fault="missing"; source.Text="missing model test";
            Wait(()=>target.Text.Contains("尚未下载模型"));
            Assert(ExternalCalls==0,"Missing model fell back to cloud");
            log.Add("Missing model gives an actionable UI error without cloud fallback: passed");
            Fault="500"; source.Text="server failure test";Wait(()=>target.Text.Contains("GPU memory unavailable"));
            Fault=""; ((ToolStripButton)Member(form,"localRetryButton")).PerformClick();Wait(()=>target.Text.Contains("Google译文：server failure test"));
            log.Add("HTTP 500 appears in UI, retry of identical source succeeds: passed");
            Fault="invalid"; source.Text="invalid response test";Wait(()=>target.Text.Contains("无法识别的数据"));
            Assert(((ToolStripStatusLabel)Member(form,"modernStatus")).Text.StartsWith("失败 · 耗时"),"Failure reported as success timing");
            log.Add("Malformed response has a readable UI error: passed");
            var timedModel=registry.GetMethod("Get").Invoke(null,new object[]{localIds[1]});definition.GetField("TimeoutSeconds").SetValue(timedModel,10);registry.GetMethod("Save").Invoke(null,new[]{timedModel});
            Fault="timeout"; source.Text="timeout test";Wait(()=>target.Text.Contains("超时"),15);
            definition.GetField("TimeoutSeconds").SetValue(timedModel,180);registry.GetMethod("Save").Invoke(null,new[]{timedModel});
            Fault=""; Thread.Sleep(2100);
            int before; lock(Chats)before=Chats.Count;
            Fault="delay";source.Text="cancel active test";Wait(()=>{lock(Chats)return Chats.Count>before;});
            var cancelWatch=System.Diagnostics.Stopwatch.StartNew();((ToolStripButton)Member(form,"localCancelButton")).PerformClick();
            Wait(()=>target.Text.Contains("已取消"));Assert(cancelWatch.ElapsedMilliseconds<1000,"Cancel blocked UI");
            Thread.Sleep(2100);Application.DoEvents();Assert(target.Text.Contains("已取消"),"Cancelled result overwrote UI");Assert(((ToolStripStatusLabel)Member(form,"modernStatus")).Text.StartsWith("已取消 · 耗时"),"Canceled timing overwritten by stale result");
            Fault=""; ((ToolStripButton)Member(form,"localRetryButton")).PerformClick();Wait(()=>target.Text.Contains("Google译文：cancel active test"));
            log.Add("Timeout, active cancellation, stale suppression and recovery: passed");
            listener.Stop();source.Text="service offline test";
            try { Wait(()=>target.Text.Contains("无法连接本地模型服务")); }
            catch { log.Add("Offline UI text: " + target.Text); throw; }
            log.Add("Unavailable service is reported without freezing the UI: passed");
            form.Close();listener.Stop();
            File.WriteAllLines(Path.Combine(Output,"local-regression.txt"),log,Encoding.UTF8);Console.WriteLine(string.Join(Environment.NewLine,log));
        }
        catch(Exception ex) {while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"local-regression.txt"),log,Encoding.UTF8);Console.WriteLine(ex);Environment.ExitCode=1;}
        Environment.Exit(Environment.ExitCode);return Environment.ExitCode;
    }
}
