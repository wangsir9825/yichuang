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

class NoLoginSmoke
{
    static readonly List<string> Requests = new List<string>();
    static int Port;
    static string Streamed="";
    static string IsolatedData;
    public static string Folder(Environment.SpecialFolder folder) { return IsolatedData; }
    public static void OnStream(string text) {Streamed+=text;}
    class MockRequest : IWebRequestCreate {
        public WebRequest Create(Uri uri) {
            lock(Requests) Requests.Add(uri.Host+uri.AbsolutePath);
            if(uri.Host != "api.deepseek.com") throw new InvalidOperationException("Unexpected network request during isolated UI test: "+uri.Host);
            return WebRequest.CreateDefault(new Uri("http://127.0.0.1:"+Port+uri.PathAndQuery));
        }
    }
    static object Get(Type type, string field) {return type.GetField(field).GetValue(null);}
    static void Set(Type type, string field, object value) {type.GetField(field).SetValue(null,value);}
    static object Member(object instance,string name) {return instance.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(instance);}
    static void Assert(bool condition,string message) {if(!condition)throw new Exception(message);}
    static void Render(Form form,string file) {
        using(Bitmap image=new Bitmap(form.Width,form.Height)) {
            form.DrawToBitmap(image,new Rectangle(0,0,image.Width,image.Height));image.Save(file);
        }
    }
    [STAThread]
    static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        List<string> log=new List<string>();Form form=null;Type main=null;string oldKey=null,oldModel=null;
        using(System.Threading.Timer watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,25000,Timeout.Infinite)) {
            try {
                string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
                AppDomain.CurrentDomain.AssemblyResolve += (s,e) => {
                    string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFile(path):null;
                };
                Assert(WebRequest.RegisterPrefix("https://",new MockRequest()),"HTTPS mock registration failed");
                Assert(WebRequest.RegisterPrefix("http://",new MockRequest()),"HTTP mock registration failed");
                WebRequest.DefaultWebProxy=null;ServicePointManager.Expect100Continue=false;
                Assembly assembly=Assembly.LoadFrom(exe);
                IsolatedData=Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(IsolatedData);
                var proxies=(object[])assembly.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
                proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(NoLoginSmoke).GetMethod("Folder"));
                EventInfo streamEvent=assembly.GetType("Xtranslator.ChatGPT").GetEvent("MyChatGPTEvent");
                streamEvent.AddEventHandler(null,Delegate.CreateDelegate(streamEvent.EventHandlerType,typeof(NoLoginSmoke).GetMethod("OnStream")));
                Assert(assembly.GetType("Xtranslator.FrmLogin")==null&&assembly.GetType("Xtranslator.Form5")==null,"Login forms still compiled");
                main=assembly.GetType("Xtranslator.ClassMain");
                Assert(main.GetField("isLogin")==null&&main.GetField("jifenNum")==null,"Account state still compiled");
                assembly.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,output);
                assembly.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
                Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);Application.EnableVisualStyles();
                form=(Form)Activator.CreateInstance(assembly.GetType("Xtranslator.Form2"));form.Opacity=0;form.Show();Application.DoEvents();
            foreach(int engineId in new[]{4,5,6,14,16,17,18,19})assembly.GetType("Xtranslator.EngineRegistry").GetMethod("SetVisible").Invoke(null,new object[]{engineId,true});
            form.GetType().GetMethod("RebuildEngineMenu",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,null);

                log.Add("Main form loaded without account or login cache");
                var source=(RichTextBox)Member(form,"rtb原文");var translated=(RichTextBox)Member(form,"rtb译文");source.Text="";
                using(Form settings=(Form)Activator.CreateInstance(assembly.GetType("Xtranslator.Frmsetting"))) {
                    settings.Opacity=0;settings.Show();Application.DoEvents();
                    var tree=(FlowLayoutPanel)settings.Controls.Find("SettingsNavigation",true)[0];
                    Assert(tree.Controls.Count==5,"Settings navigation missing");
                    foreach(string title in (string[])settings.GetType().GetProperty("SettingsPageTitles").GetValue(settings,null)) {
                        Console.WriteLine("Settings page: "+title);
                        Assert(!title.Contains("登录"),"Login settings entry present");
                        settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{title});Application.DoEvents();
                        var content=(Panel)Member(settings,"content");
                        Assert(content.PointToScreen(Point.Empty).X>=tree.PointToScreen(new Point(tree.Width,0)).X,"Settings content is covered by navigation");
                        Assert(content.ClientRectangle.Contains(content.Controls[0].Bounds),"Settings page clipped on initial display: "+title);
                        Assert(content.AutoScrollPosition==Point.Empty,"Settings page kept an old scroll offset: "+title);
                        log.Add("Settings page fully visible: "+title);
                    }
                    settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"4：DeepSeek AI翻译"});Application.DoEvents();
                    Render(settings,Path.Combine(output,"settings-deepseek.png"));
                    settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"3：DeepL 官方API翻译"});Application.DoEvents();
                    Render(settings,Path.Combine(output,"settings-deepl.png"));
                    settings.Close();
                }
                string[] selected={"Blind翻译","搜狗翻译引擎","deepl翻译引擎","eDeepseek官方AI翻译"};
                int[] numbers={4,5,6,14};
                for(int i=0;i<selected.Length;i++) {
                    string field=selected[i]+"ToolStripMenuItem";
                    form.GetType().GetMethod(field+"_Click",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(form,new object[]{Member(form,field),EventArgs.Empty});
                    Assert((int)Get(main,"isjiekounum")==numbers[i],"Engine blocked: "+numbers[i]);
                }
                foreach(string name in new[]{"谷歌翻译引擎","大模型AI翻译","知云通用AI大模型翻译"})
                    Assert(form.GetType().GetField(name+"ToolStripMenuItem",BindingFlags.Instance|BindingFlags.NonPublic)==null,"Vendor menu remains compiled");
                Assert(Requests.Count==0,"Startup or settings issued a network request");
                log.Add("Four retained online engine selections pass without login or points; shared-service menus absent");
                oldKey=(string)Get(main,"DeepSeekAI_key");oldModel=(string)Get(main,"DeepSeekAI_model");
                Set(main,"DeepSeekAI_key","local-mock-test-key");Set(main,"DeepSeekAI_model","mock-model");
                TcpListener listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Port=((IPEndPoint)listener.LocalEndpoint).Port;
                string body=null,headers=null;
                Task server=Task.Run(()=>{
                    for(int attempt=0;attempt<2;attempt++)
                    using(TcpClient client=listener.AcceptTcpClient())using(NetworkStream stream=client.GetStream()) {
                        MemoryStream raw=new MemoryStream();int value;
                        while((value=stream.ReadByte())>=0) {
                            raw.WriteByte((byte)value);byte[] data=raw.GetBuffer();int n=(int)raw.Length;
                            if(n>=4&&data[n-4]==13&&data[n-3]==10&&data[n-2]==13&&data[n-1]==10)break;
                            if(n>65536)throw new Exception("Request headers too long");
                        }
                        string currentHeaders=Encoding.ASCII.GetString(raw.ToArray());int length=0;
                        foreach(string line in currentHeaders.Split(new[]{"\r\n"},StringSplitOptions.None))
                            if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))length=int.Parse(line.Substring(15).Trim());
                        byte[] dataBody=new byte[length];int offset=0;while(offset<length){int n=stream.Read(dataBody,offset,length-offset);if(n==0)break;offset+=n;}
                        if(attempt==0){body=Encoding.UTF8.GetString(dataBody);headers=currentHeaders;}
                        string sse="data: {\"choices\":[{\"delta\":{\"content\":\"免登录翻译成功\"},\"finish_reason\":null}]}\n\ndata: {\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n";
                        byte[] payload=Encoding.UTF8.GetBytes(attempt==0?sse:"{\"error\":{\"message\":\"mock invalid key\"}}");
                        byte[] response=Encoding.ASCII.GetBytes("HTTP/1.1 "+(attempt==0?"200 OK":"401 Unauthorized")+"\r\nContent-Type: text/event-stream\r\nContent-Length: "+payload.Length+"\r\nConnection: close\r\n\r\n");
                        stream.Write(response,0,response.Length);stream.Write(payload,0,payload.Length);stream.Flush();
                    }
                });
                source.Text="The weather is nice today.";
                DateTime deadline=DateTime.UtcNow.AddSeconds(10);
                while(DateTime.UtcNow<deadline&&!translated.Text.Contains("免登录翻译成功")){Application.DoEvents();Thread.Sleep(20);}
                log.Add("Mock request received; SSE content="+Streamed+"; UI output="+translated.Text+"; engine="+Get(main,"isjiekounum"));
                File.WriteAllLines(Path.Combine(output,"no-login-test.txt"),log.ToArray());
                Assert(translated.Text.Contains("免登录翻译成功"),"Input to translated output failed: "+translated.Text);
                Assert(body.Contains("mock-model")&&body.Contains("The weather is nice today.")&&headers.Contains("Bearer local-mock-test-key"),"Request model, text or authorization incorrect");
                Assert(Requests.Count==1&&Requests[0]=="api.deepseek.com/v1/chat/completions","Unexpected account or shared-service request");
                foreach(Form open in Application.OpenForms)Assert(!open.GetType().Name.Contains("Login")&&open.GetType().Name!="Form5","Login dialog opened");
                Render(form,Path.Combine(output,"main-translation.png"));
                log.Add("TextChanged -> direct DeepSeek request -> streamed output passed with no login state");
                source.Text="Unauthorized API test sentence.";
                deadline=DateTime.UtcNow.AddSeconds(5);
                while(DateTime.UtcNow<deadline&&!translated.Text.Contains("API 认证失败")){Application.DoEvents();Thread.Sleep(20);}
                Assert(server.Wait(1000),"Mock server did not complete two requests");listener.Stop();
                Assert(translated.Text.Contains("API 认证失败")&&translated.Text.Contains("401"),"API error was not displayed");
                Assert(Requests.Count==2,"Unexpected additional request");
                log.Add("HTTP 401 is displayed as a useful API credential error; no login dialog");
                log.Add("Requests: "+string.Join(",",Requests.ToArray())+" (redirected to loopback by test only)");log.Add("Paid API calls=0");
                File.WriteAllLines(Path.Combine(output,"no-login-test.txt"),log.ToArray());
            } catch(Exception ex) {log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"no-login-test.txt"),log.ToArray());Environment.ExitCode=1;}
            finally {
                if(main!=null&&oldKey!=null){Set(main,"DeepSeekAI_key",oldKey);Set(main,"DeepSeekAI_model",oldModel);}
                if(form!=null)form.Close();
            }
            Environment.Exit(Environment.ExitCode);return Environment.ExitCode;
        }
    }
}
