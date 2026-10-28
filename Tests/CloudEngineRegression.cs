using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class CloudEngineRegression
{
    static string Data,Output;
    static int Port,Unexpected;
    static volatile string Fault="";
    static readonly JavaScriptSerializer Json=new JavaScriptSerializer();
    static readonly List<string> Log=new List<string>();
    static readonly List<string> Captures=new List<string>();
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    static object Member(object obj,string name){return obj.GetType().GetField(name,Private).GetValue(obj);}
    static void Set(Type type,string name,object value){type.GetField(name).SetValue(null,value);}
    static void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Wait(Func<bool> predicate){DateTime until=DateTime.UtcNow.AddSeconds(8);while(!predicate()&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(15);}Assert(predicate(),"UI timeout");}
    class Guard:IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            if(uri.Scheme!="https" || (uri.Host!="generativelanguage.googleapis.com"&&uri.Host!="dashscope.aliyuncs.com")) {Unexpected++;throw new Exception("Unexpected route "+uri.Host);}
            return WebRequest.CreateDefault(new Uri("http://127.0.0.1:"+Port+uri.PathAndQuery));
        }
    }
    static void Serve(TcpListener listener)
    {
        while(true)
        {
            TcpClient socket;try{socket=listener.AcceptTcpClient();}catch{return;}
            using(socket)try{using(var stream=socket.GetStream())
            {
                stream.ReadTimeout=6000;var raw=new MemoryStream();int value;
                while((value=stream.ReadByte())>=0){raw.WriteByte((byte)value);int n=(int)raw.Length;byte[] a=raw.GetBuffer();if(n>=4&&a[n-4]==13&&a[n-3]==10&&a[n-2]==13&&a[n-1]==10)break;}
                string headers=Encoding.ASCII.GetString(raw.ToArray());int length=0;
                foreach(string line in headers.Split(new[]{"\r\n"},StringSplitOptions.None))if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))length=int.Parse(line.Substring(15).Trim());
                byte[] body=new byte[length];int offset=0;while(offset<length){int read=stream.Read(body,offset,length-offset);if(read==0)break;offset+=read;}
                string payload=Encoding.UTF8.GetString(body);var json=Json.Deserialize<Dictionary<string,object>>(payload);
                lock(Captures)Captures.Add(headers+payload);
                bool gemini=headers.Contains(":generateContent");string text;
                if(gemini)
                {
                    var content=(System.Collections.IList)json["contents"];var parts=(System.Collections.IList)((Dictionary<string,object>)content[0])["parts"];
                    text=((Dictionary<string,object>)parts[0])["text"].ToString();
                    Assert(headers.Contains("x-goog-api-key: gemini-test-key"),"Gemini key header incorrect");
                    Assert(!headers.Split('\n')[0].Contains("key="),"Gemini key leaked into URL");
                }
                else
                {
                    var messages=(System.Collections.IList)json["messages"];Assert(messages.Count==1,"Qwen should receive one source message");
                    text=((Dictionary<string,object>)messages[0])["content"].ToString();
                    Assert(json["model"].ToString()=="qwen-mt-plus"&&json.ContainsKey("translation_options"),"Wrong Qwen model/schema");
                    Assert(headers.Contains("Authorization: Bearer qwen-test-key"),"Qwen auth incorrect");
                }
                string fault=Fault;if(text.Contains("slow")||fault=="delay")Thread.Sleep(1100);
                string response=gemini ? Json.Serialize(new {candidates=new[]{new {finishReason="STOP",content=new {parts=new object[]{new {thought=true,text="not a translation"},new {text="Gemini译文："+text}}}}}}) :
                    Json.Serialize(new {choices=new[]{new {message=new {content="Qwen译文："+text}}}});
                string status="200 OK";
                if(fault=="401"){status="401 Unauthorized";response="{}";}
                if(fault=="429"){status="429 Too Many Requests";response="{}";}
                if(fault=="invalid")response="broken json";
                if(fault=="empty")response=gemini ? "{\"candidates\":[]}" : "{\"choices\":[{\"message\":{\"content\":\"\"}}]}";
                byte[] bytes=Encoding.UTF8.GetBytes(response),head=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+"\r\nContent-Type: application/json\r\nContent-Length: "+bytes.Length+"\r\nConnection: close\r\n\r\n");
                stream.Write(head,0,head.Length);stream.Write(bytes,0,bytes.Length);
            }}catch(IOException){}catch(SocketException){}
        }
    }
    static void Select(Form main,int engine){main.GetType().GetMethod("SelectCloudEngine",Private).Invoke(main,new object[]{engine});}
    static string Capture(){lock(Captures)return Captures[Captures.Count-1];}
    [STAThread]
    static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>{File.WriteAllLines(Path.Combine(Output,"cloud-engine-regression.txt"),Log);Environment.Exit(124);},null,45000,Timeout.Infinite))
        try
        {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            var app=Assembly.LoadFrom(exe);Type config=app.GetType("YiChuang.ClassMain"),client=app.GetType("YiChuang.CloudTranslationClient");
            Data=Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(CloudEngineRegression).GetMethod("Folder"));
            app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Port=((IPEndPoint)listener.LocalEndpoint).Port;Task.Run(()=>Serve(listener));
            WebRequest.RegisterPrefix("https://",new Guard());WebRequest.RegisterPrefix("http://",new Guard());
            Application.EnableVisualStyles();var main=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2"));main.Opacity=0;main.Show();Application.DoEvents();
            foreach(int engineId in new[]{4,5,6,14,16,17,18,19})app.GetType("YiChuang.EngineRegistry").GetMethod("SetVisible").Invoke(null,new object[]{engineId,true});
            main.GetType().GetMethod("RebuildEngineMenu",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(main,null);

            var source=(RichTextBox)Member(main,"rtb原文");var target=(RichTextBox)Member(main,"rtb译文");
            var allowed=app.GetType("NoLoginBuild").GetMethod("IsAllowedEngine",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(int engine in new[]{1,3,8,9,10,11,12,13})Assert(!(bool)allowed.Invoke(null,new object[]{engine}),"Removed engine remains allowed");
            foreach(string name in new[]{"百度翻译引擎","学术翻译引擎","火山翻译","小牛翻译","a百度千帆AI翻译","b智谱AI翻译","csiliconFlowAI翻译","d字节豆包AI翻译"})
                Assert(main.GetType().GetField(name+"ToolStripMenuItem",Private)==null,"Removed menu remains compiled");
            using(var settings=(Form)Activator.CreateInstance(app.GetType("YiChuang.Frmsetting")))
            {
                settings.GetType().GetField("EngineChangesPrompt",Private).SetValue(settings,(Func<DialogResult>)(()=>DialogResult.Yes));
                settings.Opacity=0;settings.Show(main);Application.DoEvents();
                var tree=(FlowLayoutPanel)settings.Controls.Find("SettingsNavigation",true)[0];Assert(tree.Controls.Count==6&&((string[])settings.GetType().GetProperty("SettingsPageTitles").GetValue(settings,null)).Length==10,"Settings pages did not remove eight/add two");
                settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"5：Google Gemini AI翻译"});Application.DoEvents();
                var key=(TextBox)settings.Controls.Find("GeminiApiKey",true)[0];Assert(key.UseSystemPasswordChar,"Gemini key visible");key.Text="gemini-test-key";
                ((TextBox)settings.Controls.Find("GeminiModel",true)[0]).Text="gemini-3.8-flash";
                Assert(settings.Controls.Find("GeminiDirection",true).Length==0,"Gemini direction remains in settings");
                settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"6：Qwen-MT Plus AI翻译"});Application.DoEvents();
                key=(TextBox)settings.Controls.Find("QwenApiKey",true)[0];Assert(key.UseSystemPasswordChar,"Qwen key visible");key.Text="qwen-test-key";
                Assert(settings.Controls.Find("QwenDirection",true).Length==0,"Qwen direction remains in settings");settings.Close();
            }
            string saved=(string)config.GetMethod("readJiekou").Invoke(null,null);var data=Json.Deserialize<Dictionary<string,object>>(saved);
            Assert(data["GeminiKey"].ToString()=="gemini-test-key"&&data["QwenKey"].ToString()=="qwen-test-key","New keys not saved");
            Set(config,"GeminiKey","");Set(config,"QwenKey","");config.GetMethod("read_setSetting").Invoke(null,new object[]{saved});
            Assert(data["TranslationDirection"].ToString()=="auto","Shared direction did not save/read");
            Log.Add("Removed engines disabled; masked cloud credentials persist; per-engine directions removed: passed");
            Set(config,"is单词悬浮框",true);Select(main,18);
            Assert(((ToolStripMenuItem)Member(main,"localHunyuanMenu")).Image==null&&((ToolStripMenuItem)Member(main,"localGemmaMenu")).Image==null,"Local tick remains after switching to cloud");
            main.GetType().GetMethod("xianshiyuanwen",Private).Invoke(main,new object[]{"hello"});Wait(()=>target.Text=="Gemini译文：hello");
            Assert(!target.Text.Contains("not a translation"),"Gemini thought was displayed");
            source.Text="Hello “world”.\nSecond line.";Wait(()=>target.Text.Contains("Second line."));Assert(Capture().Contains("\\n"),"Raw newline not JSON encoded");
            Assert(Capture().Contains("Simplified Chinese"),"Gemini target direction absent");
            source.Text="";Select(main,19);main.GetType().GetMethod("xianshiyuanwen",Private).Invoke(main,new object[]{"你好"});Wait(()=>target.Text=="Qwen译文：你好");
            Assert(Capture().Contains("\"source_lang\":\"auto\"")&&Capture().Contains("\"target_lang\":\"English\""),"Qwen language names/options incorrect");
            app.GetType("YiChuang.TranslationDirection").GetField("Mode").SetValue(null,"to-zh");source.Text="English sentence.";Wait(()=>target.Text=="Qwen译文：English sentence.");
            Assert(Capture().Contains("\"target_lang\":\"Chinese\""),"Qwen en-zh direction incorrect");
            app.GetType("YiChuang.TranslationDirection").GetField("Mode").SetValue(null,"to-en");source.Text="";Select(main,18);source.Text="这是中文。";Wait(()=>target.Text=="Gemini译文：这是中文。");Assert(Capture().Contains("into English"),"Gemini zh-en direction incorrect");
            Log.Add("Gemini/Qwen bilingual and captured words use selected provider; authentic API headers/schema, Unicode/newline preserved, thoughts excluded, no dictionary/cloud fallback: passed");
            source.Text="slow old";Wait(()=>Capture().Contains("slow old"));source.Text="fresh new";Wait(()=>target.Text=="Gemini译文：fresh new");
            source.Text="slow cancel";Wait(()=>Capture().Contains("slow cancel"));((ToolStripButton)Member(main,"localCancelButton")).PerformClick();Wait(()=>target.Text.Contains("已取消"));Thread.Sleep(1200);Application.DoEvents();Assert(target.Text.Contains("已取消"),"Cancelled cloud result overwrites UI");
            foreach(string fault in new[]{"401","429","invalid","empty"})
            {
                Fault=fault;source.Text="check "+fault;Wait(()=>target.Text.StartsWith("翻译失败："));
                if(fault=="401"||fault=="429")Assert(target.Text.Contains(fault),"Status missing from error");
            }
            Fault="delay";client.GetField("TimeoutMs",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,300);source.Text="timeout";Wait(()=>target.Text.Contains("超时"));
            Fault="";client.GetField("TimeoutMs",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,90000);((ToolStripButton)Member(main,"localRetryButton")).PerformClick();Wait(()=>target.Text=="Gemini译文：timeout");
            Set(config,"GeminiKey","");source.Text="missing credential";Wait(()=>target.Text.Contains("填写 API Key"));
            Assert(Unexpected==0,"Unexpected provider/dictionary request");
            Log.Add("Stale requests, cancel, 401/429/malformed/empty responses, bounded timeout, same-text retry and missing key errors without UI freeze: passed");
            main.Close();listener.Stop();File.WriteAllLines(Path.Combine(Output,"cloud-engine-regression.txt"),Log);return 0;
        }
        catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"cloud-engine-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
