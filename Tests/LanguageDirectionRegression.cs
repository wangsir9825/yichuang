using System;
using System.Collections;
using System.Collections.Generic;
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

class LanguageDirectionRegression
{
    static string Data,Output,Loaded="",Fault="",LastBody="",LastPath="";
    static int Port,Requests,Unexpected,Unloads,Overlap,Chats,Deletes;
    static Assembly App;static Type Registry,Definition,Config;
    static readonly List<string> Log=new List<string>();
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    static object Field(object target,string name){return target.GetType().GetField(name,Private).GetValue(target);}
    static object Call(object target,string name,params object[] values){return target.GetType().GetMethod(name,Private).Invoke(target,values);}
    static object Reg(string name,params object[] values){return Registry.GetMethod(name).Invoke(null,values);}
    static object Read(object target,string name){return Definition.GetField(name).GetValue(target);}
    static void Write(object target,string name,object value){Definition.GetField(name).SetValue(target,value);}
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Wait(Func<bool> f,int seconds=8){var until=DateTime.UtcNow.AddSeconds(seconds);while(!f()&&DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(15);}Check(f(),"UI operation timed out");}
    static void Pump(int ms){var until=DateTime.UtcNow.AddMilliseconds(ms);while(DateTime.UtcNow<until){Application.DoEvents();Thread.Sleep(15);}}
    static void Render(Control c,string file){using(var b=new Bitmap(c.Width,c.Height)){c.DrawToBitmap(b,new Rectangle(Point.Empty,c.Size));b.Save(Path.Combine(Output,file));}}
    static string Error(Action f){try{f();return "";}catch(Exception ex){while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;return ex.Message;}}
    static object New(string name,string provider,string endpoint,string model){var e=Activator.CreateInstance(Definition);Write(e,"Name",name);Write(e,"Provider",provider);Write(e,"Endpoint",endpoint);Write(e,"Model",model);Write(e,"ApiKey","test-only-key-12345");Write(e,"TimeoutSeconds",10);return e;}
    class Guard:IWebRequestCreate
    {
        public WebRequest Create(Uri uri){
            if(uri.Scheme=="https"&&(uri.Host=="translation.example"||uri.Host=="generativelanguage.googleapis.com"||uri.Host=="dashscope.aliyuncs.com"||uri.Host=="api-free.deepl.com"||uri.Host=="api.deepseek.com"||uri.Host=="transmart.qq.com"||uri.Host=="fanyi.sogou.com"))return WebRequest.CreateDefault(new Uri("http://127.0.0.1:"+Port+uri.PathAndQuery));
            if(uri.Scheme=="http"&&uri.Host=="127.0.0.1"&&uri.Port==Port)return WebRequest.CreateDefault(uri);
            Interlocked.Increment(ref Unexpected);throw new Exception("Unexpected external service");
        }
    }
    static void Serve(TcpListener listener){while(true){TcpClient client;try{client=listener.AcceptTcpClient();}catch{return;}Task.Run(()=>Respond(client));}}
    static void Respond(TcpClient client)
    {
        using(client)try{using(var stream=client.GetStream()){
            stream.ReadTimeout=5000;var raw=new MemoryStream();int c;
            while((c=stream.ReadByte())>=0){raw.WriteByte((byte)c);int n=(int)raw.Length;var a=raw.GetBuffer();if(n>=4&&a[n-4]==13&&a[n-3]==10&&a[n-2]==13&&a[n-1]==10)break;}
            string headers=Encoding.ASCII.GetString(raw.ToArray()),path=headers.Split(' ')[1];int length=0;
            foreach(string line in headers.Split(new[]{"\r\n"},StringSplitOptions.None))if(line.StartsWith("Content-Length:",StringComparison.OrdinalIgnoreCase))length=int.Parse(line.Substring(15));
            byte[] bytes=new byte[length];int offset=0;while(offset<length){int n=stream.Read(bytes,offset,length-offset);if(n==0)break;offset+=n;}
            var json=new JavaScriptSerializer();string body=Encoding.UTF8.GetString(bytes),fault=Fault;var payload=length==0?null:json.Deserialize<Dictionary<string,object>>(body);
            Interlocked.Increment(ref Requests);LastBody=body;LastPath=path;if(headers.StartsWith("DELETE"))Interlocked.Increment(ref Deletes);
            string status="200 OK",response="{}",extra="";
            if(path=="/")response="{}";
            else if(path=="/api/imt")response="{\"auto_translation\":[\"Transmart direction result\"]}";
            else if(path=="/api/transpc/text/result")response="{\"data\":{\"translate\":{\"dit\":\"Sogou direction result\"},\"sgtkn\":\"\"}}";
            else if(path=="/api/tags")response="{\"models\":[{\"name\":\"local-a:latest\"},{\"name\":\"local-b:latest\"},{\"name\":\"remote:latest\",\"remote_host\":\"example.com\"}]}";
            else if(path=="/api/ps")response=Loaded==""?"{\"models\":[]}":json.Serialize(new{models=new[]{new{name=Loaded}}});
            else if(path=="/api/generate"){Check(Convert.ToInt32(payload["keep_alive"])==0,"Only unload allowed");Loaded="";Interlocked.Increment(ref Unloads);}
            else if(path=="/api/chat"){
                string model=payload["model"].ToString();if(Loaded!=""&&Loaded!=model)Interlocked.Increment(ref Overlap);Loaded=model;Interlocked.Increment(ref Chats);
                var messages=(IList)payload["messages"];string prompt=((Dictionary<string,object>)messages[0])["content"].ToString();
                response=json.Serialize(new{message=new{content=model+":"+prompt.Substring(prompt.LastIndexOf('\n')+1)},done_reason="stop"});
            } else if(path.Contains(":generateContent"))response="{\"candidates\":[{\"finishReason\":\"STOP\",\"content\":{\"parts\":[{\"text\":\"Gemini custom translation\"}]}}]}";
            else if(path=="/v2/translate")response="{\"translations\":[{\"text\":\"DeepL custom translation\"}]}";
            else if(path.EndsWith("/chat/completions")){
                Check(headers.Contains("Authorization: Bearer test-only-key-12345"),"Missing configured authorization");
                var messages=(IList)payload["messages"];string text=((Dictionary<string,object>)messages[messages.Count-1])["content"].ToString();
                if(text.StartsWith("slow"))Thread.Sleep(1800);
                response=json.Serialize(new{choices=new[]{new{message=new{content=(body.Contains("into English")?"EN:":"ZH:")+text},finish_reason="stop"}}});
                if(payload.ContainsKey("stream")&&(bool)payload["stream"])response="data: {\"choices\":[{\"delta\":{\"content\":\"DeepSeek translation\"},\"finish_reason\":null}]}\n\ndata: {\"choices\":[{\"delta\":{},\"finish_reason\":\"stop\"}]}\n\n";
                if(fault=="timeout")Thread.Sleep(11500);
                if(fault=="401"){status="401 Unauthorized";response="{}";}
                if(fault=="redirect"){status="302 Found";extra="Location: https://not-authorized.example/steal\r\n";}
                if(fault=="invalid")response="bad json";
                if(fault=="empty")response="{\"choices\":[]}";
                if(fault=="length")response="{\"choices\":[{\"finish_reason\":\"length\",\"message\":{\"content\":\"partial\"}}]}";
            } else throw new Exception("Unexpected API path");
            bytes=Encoding.UTF8.GetBytes(response);byte[] head=Encoding.ASCII.GetBytes("HTTP/1.1 "+status+"\r\n"+extra+"Content-Type: application/json\r\nContent-Length: "+bytes.Length+"\r\nConnection: close\r\n\r\n");stream.Write(head,0,head.Length);stream.Write(bytes,0,bytes.Length);
        }}catch(IOException){}catch(SocketException){}catch(Exception ex){lock(Log)Log.Add(ex.ToString());Interlocked.Increment(ref Unexpected);}
    }

    static int[] Ids(string name){return (int[])Registry.GetProperty(name).GetValue(null,null);}
    static void Mode(Form main,string mode){var menu=(ToolStripDropDownButton)Field(main,"directionButton");foreach(ToolStripMenuItem item in menu.DropDownItems)if((string)item.Tag==mode){item.PerformClick();return;}throw new Exception("Missing direction item");}
    static void Complete(Form main,Action action){int before=Requests;action();Wait(()=>Requests>before&&!((ToolStripButton)Field(main,"localCancelButton")).Enabled,12);Check(!((RichTextBox)Field(main,"rtb译文")).Text.StartsWith("翻译失败"),"Translation failed");}
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>{File.WriteAllLines(Path.Combine(Output,"language-direction-regression.txt"),Log);Environment.Exit(124);},null,90000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            App=Assembly.LoadFrom(exe);Registry=App.GetType("Xtranslator.EngineRegistry");Definition=App.GetType("Xtranslator.EngineDefinition");Config=App.GetType("Xtranslator.ClassMain");
            Data=args.Length>3?args[3]:Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LanguageDirectionRegression).GetMethod("Folder"));
            App.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));App.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Port=((IPEndPoint)listener.LocalEndpoint).Port;Task.Run(()=>Serve(listener));WebRequest.RegisterPrefix("https://",new Guard());WebRequest.RegisterPrefix("http://",new Guard());
            Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(App.GetType("Xtranslator.Form2"))){
                main.Opacity=0;main.Show();Application.DoEvents();Call(main,"SetCaptureMode",0,false);
                if(args.Length>3){
                    Check(Ids("VisibleIds").SequenceEqual(new[]{18,5}),"Checkbox/order did not survive restart");Check(App.GetType("Xtranslator.TranslationDirection").GetField("Mode").GetValue(null).ToString()=="to-en","Direction did not survive restart");Check((int)Config.GetField("isjiekounum").GetValue(null)==18,"Selected engine changed after restart");Check(Ids("ActiveIds").Length==8&&Reg("Get",16)!=null,"Hidden model config removed");Check(Requests==0,"Restart sent network requests");main.Close();File.WriteAllText(Path.Combine(Output,"restart-pass.txt"),"Global direction + checkbox states + order + current engine survive new process; hidden configurations preserved; zero requests.");return 0;
                }
                var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");
                Check(Ids("VisibleIds").SequenceEqual(new[]{4,5,14,18}),"Wrong default four engines");Check(Ids("ActiveIds").Length==8,"Default hides lost configurations");
                var dir=(ToolStripDropDownButton)Field(main,"directionButton");var engine=(ToolStripDropDownButton)Field(main,"toolStripDropDownButton1");
                Check(dir.Owner==Field(main,"sourceStrip")&&engine.Owner==Field(main,"targetStrip"),"Selectors not over correct panes");Check(dir.DropDownItems.Count==3,"Direction should have exactly three modes");
                using(var settings=(Form)Activator.CreateInstance(App.GetType("Xtranslator.Frmsetting"))){
                    settings.Opacity=0;settings.Show(main);settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"引擎管理"});Application.DoEvents();
                    var list=(CheckedListBox)settings.Controls.Find("ManagedEngines",true)[0];Check(list.CheckedItems.Count==4,"Default checkbox mismatch");
                    list.SetItemChecked(4,true);Check((bool)Reg("IsVisible",16),"Check did not show local model");list.SetItemChecked(4,false);Application.DoEvents();Check(Reg("Get",16)!=null&&!(bool)Reg("IsVisible",16),"Uncheck removed model config");
                    list.SelectedIndex=6;((Button)settings.Controls.Find("MoveEngineUp",true)[0]).PerformClick();Check(Ids("ActiveIds")[5]==18,"Move button did not reorder stable ID");
                    list.SelectedIndex=5;((Button)settings.Controls.Find("MoveEngineDown",true)[0]).PerformClick();Check(Ids("ActiveIds")[6]==18,"Move down failed");
                    Render(settings,"checked-engine-manager.png");((Button)settings.Controls.Find("SaveSettings",true)[0]).PerformClick();Check(settings.IsDisposed,"Save settings failed");
                }
                foreach(string layout in new[]{"wide","compact"}){
                    App.GetType("Xtranslator.UiPreferences").GetField("Layout",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,layout);Call(main,"ApplyModernLayout",true);Application.DoEvents();
                    foreach(var item in new[]{dir,engine}){Check(item.Visible&&item.Owner.ClientRectangle.Contains(item.Bounds),"Selector clipped in "+layout);Check(item.ShowDropDownArrow&&!string.IsNullOrWhiteSpace(item.ToolTipText),"Selector arrow/tooltip absent");}
                    Render(main,"selectors-"+layout+".png");
                }
                Log.Add("Default four, hidden config preservation, checkbox + reorder buttons, save, both layout selector placement/bounds/arrow: passed");
                // Upgrade a real version-one document: keep profile data, use requested four defaults only once.
                string old=Reg("Export").ToString().Replace("\"Version\":2","\"Version\":1");Reg("Import",old);Check(Ids("VisibleIds").SequenceEqual(new[]{4,5,14,18}),"Version-one migration wrong");
                var e=New("Direction test","openai","https://translation.example/v1","direction-model");Write(e,"Direction","zh-en");int custom=(int)Reg("Save",e);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",custom);
                Complete(main,()=>source.Text="word");Check(LastBody.Contains("Simplified Chinese"),"Old per-engine direction overrode global auto");
                Complete(main,()=>Mode(main,"to-en"));Check(LastBody.Contains("into English"),"Same source direction change did not retranslate");
                Complete(main,()=>Mode(main,"to-zh"));Check(LastBody.Contains("Simplified Chinese"),"Fixed Chinese not sent");
                Complete(main,()=>source.Text="这是中文。");Check(LastBody.Contains("Simplified Chinese"),"Fixed Chinese silently auto-switched");
                Complete(main,()=>Mode(main,"auto"));Check(LastBody.Contains("into English"),"Auto interchange not restored");
                source.Text="slow stale";Wait(()=>LastBody.Contains("slow stale"));Mode(main,"to-en");Pump(2400);Check(!((ToolStripButton)Field(main,"localCancelButton")).Enabled,"Direction switch remains loading");Check(target.Text=="EN:slow stale","Old-direction response overwrote new direction");
                source.Clear();Reg("Remove",custom);
                Log.Add("Actual main direction menu: fixed EN/ZH, CN-EN automatic, same-text retry and pending mode change; old engine setting ignored: passed");
                foreach(string provider in new[]{"gemini","qwen","deepl","ollama"}){
                    string url=provider=="gemini"?"https://generativelanguage.googleapis.com/v1beta":provider=="qwen"?"https://dashscope.aliyuncs.com/compatible-mode/v1":provider=="deepl"?"https://api-free.deepl.com":"http://127.0.0.1:"+Port;
                    var profile=New("Profile "+provider,provider,url,provider=="ollama"?"local-a:latest":"model");if(provider=="ollama")Write(profile,"Prompt","gemma");Write(profile,"SourceLanguage","DE");Write(profile,"TargetLanguage","JA");Write(profile,"Automatic",false);
                    int id=(int)Reg("Save",profile);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",id);
                    Complete(main,()=>Call(main,"xianshiyuanwen","Bonjour"));
                    if(provider=="deepl")Check(LastBody.Contains("\"target_lang\":\"EN\"")&&!LastBody.Contains("source_lang"),"DeepL did not use shared direction");
                    else if(provider=="qwen")Check(LastBody.Contains("\"source_lang\":\"auto\"")&&LastBody.Contains("\"target_lang\":\"English\""),"Qwen fixed target forced wrong source");
                    else Check(LastBody.Contains("English"),"LLM target missing");
                    Complete(main,()=>Mode(main,"to-zh"));Check(provider=="deepl"?LastBody.Contains("\"ZH\""):LastBody.Contains("Chinese"),"Fixed Chinese not propagated to "+provider);
                    source.Clear();Reg("Remove",id);Mode(main,"to-en");
                }
                foreach(int id in new[]{4,5}){
                    Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",id);Complete(main,()=>Call(main,"xianshiyuanwen","word"));Check(id==4?LastBody.Contains("\"target\":{\"lang\":\"en\""):LastBody.Contains("\"to\":\"en\""),"Web word target not English");
                    Complete(main,()=>Mode(main,"to-zh"));Check(id==4?LastBody.Contains("\"target\":{\"lang\":\"zh\""):LastBody.Contains("zh-CHS"),"Web target not Chinese");source.Clear();Mode(main,"to-en");
                }
                Config.GetField("DeepSeekAI_key").SetValue(null,"test-only-key-12345");Config.GetField("DeepSeekAI_model").SetValue(null,"model");Call(main,"SelectManagedEngine",14);
                Complete(main,()=>Call(main,"xianshiyuanwen","word"));Check(LastBody.Contains("翻译为英文"),"DeepSeek fixed English not used for word");
                Complete(main,()=>Mode(main,"to-zh"));Check(LastBody.Contains("翻译为简体中文"),"DeepSeek fixed Chinese not used");source.Clear();Mode(main,"to-en");
                Log.Add("Gemini/Qwen/DeepL/OpenAI/Ollama shared target payloads, source auto; Transmart/Sogou single words stay on selected provider: passed");
                foreach(int id in Ids("ActiveIds"))Reg("SetVisible",id,false);Call(main,"RebuildEngineMenu");int beforeEmpty=Requests;source.Text="hidden engine must not run";Pump(800);Check(Requests==beforeEmpty&&target.Text.Contains("选择"),"Hidden/empty selection called another provider");source.Clear();
                Reg("SetVisible",18,true);Reg("SetVisible",5,true);while(Ids("ActiveIds")[0]!=18)Reg("Move",18,-1);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",18);Check(engine.DropDownItems[0].Text.StartsWith("1：")&&engine.DropDownItems[1].Text.StartsWith("2："),"Visible menu numbering not continuous");
                Config.GetMethod("jiluset").Invoke(null,null);Check(Config.GetField("SettingsSaveError").GetValue(null)==null,"Config save failed");File.WriteAllText(Path.Combine(Output,"restart-data-path.txt"),Data);main.Close();
            }
            listener.Stop();Check(Unexpected==0&&Deletes==0,"Unexpected external/dictionary/download/delete request");Log.Add("Hide-all pauses without fallback; version-two persistence preserves empty/ordered visibility and user choices. Zero external API calls.");File.WriteAllLines(Path.Combine(Output,"language-direction-regression.txt"),Log);return 0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"language-direction-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
