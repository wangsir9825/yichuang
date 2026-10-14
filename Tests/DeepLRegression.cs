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
using System.Windows.Forms;
using System.Web.Script.Serialization;

class DeepLRegression
{
    static int Port;
    static string IsolatedData;
    static readonly List<string> Requests = new List<string>();
    static readonly List<string> Bodies = new List<string>();
    static readonly List<string> Headers = new List<string>();
    static readonly JavaScriptSerializer Json = new JavaScriptSerializer();
    static string Output;
    static string Phase;
    static void Stage(string text) {Phase=text;File.AppendAllText(Path.Combine(Output,"progress.txt"),text+Environment.NewLine);Console.WriteLine(text);}
    public static string Folder(Environment.SpecialFolder folder) { return IsolatedData; }
    class MockRequest : IWebRequestCreate {
        public WebRequest Create(Uri uri) {
            lock(Requests) Requests.Add(uri.Host + uri.AbsolutePath);
            if(uri.Host != "api-free.deepl.com") throw new InvalidOperationException("Unexpected external request: " + uri.Host);
            var request=(HttpWebRequest)WebRequest.CreateDefault(new Uri("http://127.0.0.1:"+Port+uri.PathAndQuery));
            request.Timeout=5000; request.ReadWriteTimeout=5000; return request;
        }
    }
    static object Field(Type type,string name) {return type.GetField(name).GetValue(null);}
    static void Set(Type type,string name,object value) {type.GetField(name).SetValue(null,value);}
    static object Member(object value,string name) {return value.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(value);}
    static void Assert(bool value,string message) {if(!value)throw new Exception(message);}
    static void Wait(Func<bool> condition) {
        DateTime limit=DateTime.UtcNow.AddSeconds(6);
        while(DateTime.UtcNow<limit&&!condition()){Application.DoEvents();Thread.Sleep(15);}
        Assert(condition(),"UI result was not displayed within 6 seconds");
    }
    [STAThread]
    static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        Output=output;
        var log=new List<string>();
        using(var watchdog=new System.Threading.Timer(s=>{Stage("Timeout at "+Phase);Environment.Exit(124);},null,45000,Timeout.Infinite))
        try {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{
                string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");
                return File.Exists(path)?Assembly.LoadFrom(path):null;
            };
            Assembly app=Assembly.LoadFrom(exe);Type main=app.GetType("YiChuang.ClassMain");
            IsolatedData=Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(IsolatedData);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(DeepLRegression).GetMethod("Folder"));
            WebRequest.RegisterPrefix("https://",new MockRequest()); WebRequest.RegisterPrefix("http://",new MockRequest());
            ServicePointManager.Expect100Continue=false;WebRequest.DefaultWebProxy=null;
            app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,output);
            Application.EnableVisualStyles();
            Form form=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2"));form.Opacity=0;form.Show();Application.DoEvents();
            foreach(int engineId in new[]{4,5,6,14,16,17,18,19})app.GetType("YiChuang.EngineRegistry").GetMethod("SetVisible").Invoke(null,new object[]{engineId,true});
            form.GetType().GetMethod("RebuildEngineMenu",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null);

            Stage("Main form loaded");
            Set(main,"isjiekounum",6); Set(main,"DeeplInterface","2");Set(main,"DeeplLanguage","1");
            Set(main,"DeeplFromLang","en");Set(main,"DeeplToLang","zh");
            Form settings=(Form)Activator.CreateInstance(app.GetType("YiChuang.Frmsetting"));settings.Opacity=0;settings.Show(form);
            settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"3：DeepL 官方API翻译"});Application.DoEvents();
            Stage("DeepL settings loaded");
            TextBox key=null;Button save=null;
            Action<Control> find=null;find=c=>{if(c is TextBox&&c.Name=="ApiKey")key=(TextBox)c;if(c is Button&&c.Name=="SaveSettings")save=(Button)c;foreach(Control child in c.Controls)find(child);};find(settings);
            Assert(key!=null&&key.UseSystemPasswordChar&&save!=null,"Masked API key field or Save button missing");
            Assert(settings.Controls.Find("SourceLanguageCode",true).Length==0&&settings.Controls.Find("TargetLanguageCode",true).Length==0&&settings.Controls.Find("AutomaticDirection",true).Length==0,"Per-engine direction controls remain");
            key.Text="local-test-key:fx";save.PerformClick();Application.DoEvents();
            Assert(!settings.IsDisposed&&settings.Controls.Find("ManagedEngines",true).Length==1,"Save-and-return did not keep settings on engine list");
            Stage("Save clicked");
            var saved=Json.Deserialize<Dictionary<string,object>>((string)main.GetMethod("readJiekou").Invoke(null,null));
            Assert(saved.ContainsKey("Deeplkey_official")&&(string)saved["Deeplkey_official"]=="local-test-key:fx","Settings did not persist the DeepL key");
            Assert(saved["TranslationDirection"].ToString()=="auto","Global direction not persisted");
            log.Add("DeepL account/key remain; per-engine language fields replaced by shared main-window direction: passed");
            log.Add("Masked key field -> Save and return -> isolated encrypted config -> read back: passed");
            settings.Close();Application.DoEvents();Assert(settings.IsDisposed,"Closing settings did not return to translation");
            Stage(log[log.Count-1]);
            TcpListener listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Port=((IPEndPoint)listener.LocalEndpoint).Port;
            Task server=Task.Run(()=>{
                for(int n=0;n<5;n++)using(TcpClient client=listener.AcceptTcpClient())using(NetworkStream stream=client.GetStream()) {
                    stream.ReadTimeout=6000;var raw=new MemoryStream();int value;
                    while((value=stream.ReadByte())>=0){raw.WriteByte((byte)value);byte[] b=raw.GetBuffer();int size=(int)raw.Length;if(size>=4&&b[size-4]==13&&b[size-3]==10&&b[size-2]==13&&b[size-1]==10)break;if(size>32768)throw new Exception("Headers too large");}
                    string headers=Encoding.ASCII.GetString(raw.ToArray());int length=0;
                    foreach(string line in headers.Split(new[]{"\r\n"},StringSplitOptions.None))if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))length=int.Parse(line.Substring(15).Trim());
                    byte[] body=new byte[length];int offset=0;while(offset<length){int count=stream.Read(body,offset,length-offset);if(count==0)break;offset+=count;}
                    lock(Bodies){Bodies.Add(Encoding.UTF8.GetString(body));Headers.Add(headers);}
                    string responseBody=n==0?"{\"message\":\"Invalid key\"}":n==4?"{\"message\":\"Quota exceeded\"}":"{\"translations\":[{\"text\":\"你好，世界！\"}]}";
                    string status=n==0?"403 Forbidden":n==4?"456 Quota Exceeded":"200 OK";
                    byte[] bytes=Encoding.UTF8.GetBytes(responseBody);byte[] head=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+"\r\nContent-Type: application/json\r\nContent-Length: "+bytes.Length+"\r\nConnection: close\r\n\r\n");stream.Write(head,0,head.Length);stream.Write(bytes,0,bytes.Length);stream.Flush();
                }
            });
            var source=(RichTextBox)Member(form,"rtb原文");var target=(RichTextBox)Member(form,"rtb译文");
            source.Text="Hello, world!";Wait(()=>target.Text.Contains("403"));
            log.Add("HTTP 403 is shown in the translation pane: passed");
            Stage(log[log.Count-1]);
            source.Text="Hello, world! ";Wait(()=>target.Text.Contains("你好"));
            log.Add("Retry after credential error with the same normalized source text: passed");
            Stage(log[log.Count-1]);
            int countBefore=Requests.Count;source.Text="hello";Wait(()=>Requests.Count>countBefore&&target.Text.Contains("你好"));
            log.Add("Single word uses the selected DeepL engine: passed");
            Stage(log[log.Count-1]);
            string unusual="第一行\nSecond line: C:\\temp\\file \"quoted\"";
            string result=(string)app.GetType("YiChuang.Form2").GetMethod("apiDeepl").Invoke(null,new object[]{unusual,"api-free.deepl.com","local-test-key:fx"});
            Assert(result.Contains("你好"),"Unicode request did not return a translation");
            var body4=Json.Deserialize<Dictionary<string,object>>(Bodies[3]);
            Assert(((System.Collections.IList)body4["text"])[0].ToString()==unusual,"Newlines, quotes, or backslashes did not survive JSON encoding");
            Assert(!body4.ContainsKey("source_lang"),"Automatic mode forced a source language");
            log.Add("Unicode, newline, quote, and backslash JSON encoding: passed");
            Stage(log[log.Count-1]);
            try {app.GetType("YiChuang.Form2").GetMethod("apiDeepl").Invoke(null,new object[]{"Quota test","api-free.deepl.com","local-test-key:fx"});throw new Exception("Quota error was swallowed");}
            catch(TargetInvocationException ex){Assert(ex.InnerException.Message.Contains("456"),"Quota error lacks HTTP status");}
            Assert(server.Wait(1000),"Mock server did not finish");listener.Stop();
            log.Add("HTTP 456 quota error is actionable: passed");
            foreach(string headers in Headers)Assert(headers.Contains("DeepL-Auth-Key local-test-key:fx"),"Authentication header incorrect");
            foreach(string uri in Requests)Assert(uri=="api-free.deepl.com/v2/translate","An unexpected provider was contacted");
            form.TopMost=true;bool owned=false;
            Stage("Testing settings owner");
            form.BeginInvoke(new Action(()=>{foreach(Form open in Application.OpenForms)if(open.GetType().Name=="Frmsetting"){owned=open.Owner==form&&open.TopMost;open.Close();break;}}));
            form.GetType().GetMethod("toolStripButton1_Click",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(form,new object[]{null,EventArgs.Empty});
            Assert(owned,"Settings did not belong to the topmost main window");
            log.Add("Settings remain above the topmost main window: passed");
            using(var bitmap=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(bitmap,new Rectangle(0,0,bitmap.Width,bitmap.Height));bitmap.Save(Path.Combine(output,"translation.png"));}
            log.Add("All 5 requests intercepted and redirected to loopback; real API calls: 0");
            form.Close();File.WriteAllLines(Path.Combine(output,"deepl-regression.txt"),log);Console.WriteLine(string.Join(Environment.NewLine,log));
        }
        catch(Exception ex){while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"deepl-regression.txt"),log);Console.WriteLine(ex);Environment.ExitCode=1;}
        Environment.Exit(Environment.ExitCode);return Environment.ExitCode;
    }
}
