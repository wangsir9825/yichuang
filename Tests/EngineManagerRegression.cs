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

class EngineManagerRegression
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
            if(uri.Scheme=="https"&&(uri.Host=="translation.example"||uri.Host=="generativelanguage.googleapis.com"||uri.Host=="dashscope.aliyuncs.com"||uri.Host=="api-free.deepl.com"))return WebRequest.CreateDefault(new Uri("http://127.0.0.1:"+Port+uri.PathAndQuery));
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
            if(path=="/api/tags")response="{\"models\":[{\"name\":\"local-a:latest\"},{\"name\":\"local-b:latest\"},{\"name\":\"remote:latest\",\"remote_host\":\"example.com\"}]}";
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
                response=json.Serialize(new{choices=new[]{new{message=new{content="custom:"+text},finish_reason="stop"}}});
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
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>{File.WriteAllLines(Path.Combine(Output,"engine-manager-regression.txt"),Log);Environment.Exit(124);},null,95000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            App=Assembly.LoadFrom(exe);Registry=App.GetType("YiChuang.EngineRegistry");Definition=App.GetType("YiChuang.EngineDefinition");Config=App.GetType("YiChuang.ClassMain");
            Data=args.Length>3?args[3]:Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(EngineManagerRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));App.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            var listener=new TcpListener(IPAddress.Loopback,0);listener.Start();Port=((IPEndPoint)listener.LocalEndpoint).Port;Task.Run(()=>Serve(listener));WebRequest.RegisterPrefix("https://",new Guard());WebRequest.RegisterPrefix("http://",new Guard());
            Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(App.GetType("YiChuang.Form2"))){
                main.Opacity=0;main.Show();Application.DoEvents();
                if(args.Length>3){Check(Reg("Get",6)==null,"Removed built-in reappeared after restart");var ids=(int[])Registry.GetProperty("ActiveIds").GetValue(null,null);Check(ids.Length==6&&ids.Any(id=>id>=1000),"Custom registry not restored");int selected=(int)Config.GetField("isjiekounum").GetValue(null);Check(selected>=1000&&Read(Reg("Get",selected),"Name").ToString()=="Persisted engine","Selected custom engine lost");Check(Read(Reg("Get",selected),"ApiKey").ToString()=="test-only-key-12345","Key lost");Check(Requests==0,"Restart sent a request");main.Close();File.WriteAllText(Path.Combine(Output,"restart-pass.txt"),"Custom definition, key, selected ID, removed built-in and dynamic menu restored in new process; no network request.");return 0;}
                var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");
                Config.GetField("Deeplkey_official").SetValue(null,"saved-deepl-test-key");Config.GetField("isjiekounum").SetValue(null,6);Reg("Remove",6);Check((int)Config.GetField("isjiekounum").GetValue(null)==0&&Reg("Get",6)==null,"Remove selected did not pause");Reg("Restore",6);Check(Config.GetField("Deeplkey_official").GetValue(null).ToString()=="saved-deepl-test-key","Restore reset credentials");Reg("Import",new object[]{null});
                foreach(int id in (int[])Registry.GetProperty("ActiveIds").GetValue(null,null))Reg("Remove",id);Call(main,"RebuildEngineMenu");source.Text="no engine should send nothing";Pump(750);Check(Requests==0&&target.Text.Contains("选择"),"Empty registry silently called another engine");source.Clear();Reg("Import",new object[]{null});Call(main,"RebuildEngineMenu");
                Log.Add("Backward defaults, remove/restore built-in preserving credentials, remove all without fallback: passed");
                var e=New("Custom test","openai","https://translation.example/v1","custom-model");int idCustom=(int)Reg("Save",e);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",idCustom);int beforeLegacy=Requests;Call(main,"翻译","old legacy word");Check(Requests==beforeLegacy,"Custom selection escaped to a legacy route");Call(main,"xianshiyuanwen","hello");Wait(()=>target.Text=="custom:hello");Check(LastPath=="/v1/chat/completions"&&LastBody.Contains("custom-model")&&LastBody.Contains("Simplified Chinese"),"Wrong custom model/route/direction");
                source.Text="这是 一段中文。\n第二行。";Wait(()=>target.Text=="custom:这是 一段中文。\n第二行。");Check(LastBody.Contains("into English"),"Wrong CN-EN target");
                source.Text="slow stale";Wait(()=>LastBody.Contains("slow stale"));source.Text="fresh";Wait(()=>target.Text=="custom:fresh");Pump(1800);Check(target.Text=="custom:fresh","Stale result replaced new result");var timing=(ToolStripStatusLabel)Field(main,"modernStatus");Check(timing.Text.StartsWith("翻译耗时："),"Custom elapsed result missing");string timed=timing.Text;Pump(500);Check(timing.Text==timed,"Completed timing keeps changing");
                source.Text="slow cancel";Wait(()=>LastBody.Contains("slow cancel"));((ToolStripButton)Field(main,"localCancelButton")).PerformClick();Pump(2100);Check(target.Text.Contains("已取消"),"Cancelled result arrived");Check(timing.Text.StartsWith("已取消 · 耗时"),"Cancel timing incorrect");
                foreach(string fault in new[]{"401","redirect","invalid","empty","length"}){Fault=fault;source.Text="fault "+fault;Wait(()=>target.Text.StartsWith("翻译失败："));Check(!target.Text.Contains("test-only-key"),"Error leaked key");Check(timing.Text.StartsWith("失败 · 耗时"),"Failed request reported successful timing");}
                Fault="timeout";source.Text="timeout";Wait(()=>target.Text.Contains("超时"),14);Fault="";((ToolStripButton)Field(main,"localRetryButton")).PerformClick();Wait(()=>target.Text=="custom:timeout");
                Log.Add("Custom OpenAI word/CN-EN/EN-CN, encoding, exact endpoint/model/key, cancel/stale, timeout/retry, 401/redirect/bad/empty/truncated response: passed");
                source.Clear();
                foreach(string p in new[]{"gemini","qwen","deepl"}){
                    string endpoint=p=="gemini"?"https://generativelanguage.googleapis.com/v1beta":p=="qwen"?"https://dashscope.aliyuncs.com/compatible-mode/v1":"https://api-free.deepl.com";
                    var special=New("Custom "+p,p,endpoint,p=="gemini"?"gemini-test":p=="qwen"?"qwen-mt-plus":"");int specialId=(int)Reg("Save",special);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",specialId);source.Text="hello "+p;Wait(()=>target.Text.Contains(p=="gemini"?"Gemini custom":p=="deepl"?"DeepL custom":"custom:hello qwen"));source.Clear();Reg("Remove",specialId);
                }
                Log.Add("Custom Gemini/Qwen-MT/DeepL use their dedicated payloads/adapters: passed");
                string localUrl="http://127.0.0.1:"+Port;var localA=New("Local A","ollama",localUrl,"local-a:latest");Write(localA,"Prompt","hy");var localB=New("Local B","ollama",localUrl,"local-b:latest");Write(localB,"Prompt","gemma");int idA=(int)Reg("Save",localA),idB=(int)Reg("Save",localB);
                var installed=(string[])App.GetType("YiChuang.LocalModelClient").GetMethod("ListInstalled").Invoke(null,new object[]{localUrl,new Func<bool>(()=>true)});Check(installed.Length==2&&!installed.Contains("remote:latest"),"Remote model offered as local");
                Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",idA);Call(main,"xianshiyuanwen","word");Wait(()=>target.Text=="local-a:latest:word");source.Clear();Call(main,"SelectManagedEngine",idB);source.Text="本地句子。";Wait(()=>target.Text=="local-b:latest:本地句子。");Check(Unloads==1&&Overlap==0&&Chats==2,"Local models overlapped");
                source.Clear();Reg("Remove",idB);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",idA);source.Text="switch after remove";Wait(()=>target.Text=="local-a:latest:switch after remove");Check(Unloads==2&&Deletes==0,"Removed local model was not released or weights deleted");
                source.Clear();var localEdit=Reg("Get",idA);Write(localEdit,"Model","missing:latest");Reg("Save",localEdit);source.Text="missing";Wait(()=>target.Text.Contains("尚未下载"));Check(Chats==3,"Missing model generated");
                source.Clear();Reg("Remove",idA);Log.Add("Custom Ollama installed list filters remote models; words and switch use selected model; removed running model released, zero weight deletes; missing model error: passed");
                var bad=New("Bad","openai","http://example.com/v1","model");Check(Error(()=>Reg("Save",bad)).Contains("HTTPS"),"Unsafe URL accepted");Write(bad,"Endpoint","https://example.com/v1/chat/completions");Check(Error(()=>Reg("Save",bad)).Contains("基础地址"),"Full URL silently doubled");
                string before=Reg("Export").ToString();Reg("Import","{broken");Check(((int[])Registry.GetProperty("ActiveIds").GetValue(null,null)).Length==0&&Reg("Export").ToString()=="{broken","Corruption reset to cloud or destroyed recovery data");Reg("Import",before);
                using(var settings=(Form)Activator.CreateInstance(App.GetType("YiChuang.Frmsetting"))){
                    settings.Opacity=0;settings.Show(main);Application.DoEvents();settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"引擎管理"});Application.DoEvents();Render(settings,"engine-manager-native.png");var list=(ListBox)settings.Controls.Find("ManagedEngines",true)[0];Check(list.Items.Count==7,"Settings registry list incorrect");Check(settings.Controls.Find("SaveSettings",true)[0].Visible,"Settings save hidden");
                    // Exercise the actual Add -> modal draft -> installed list -> OK -> manager event chain.
                    using(var timer=new System.Windows.Forms.Timer()){
                        timer.Interval=80;bool fetching=false;timer.Tick+=(a,b)=>{
                            var editor=Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f.GetType().Name=="EngineEditorForm");if(editor==null)return;
                            var modelBox=(ComboBox)editor.Controls.Find("EngineModel",true)[0];
                            if(!fetching){fetching=true;((TextBox)editor.Controls.Find("EngineName",true)[0]).Text="Added through form";((ComboBox)editor.Controls.Find("EngineProvider",true)[0]).SelectedIndex=1;((TextBox)editor.Controls.Find("EngineEndpoint",true)[0]).Text=localUrl;((Button)editor.Controls.Find("FetchInstalledModels",true)[0]).PerformClick();return;}
                            if(modelBox.Items.Count!=2)return;timer.Stop();modelBox.SelectedIndex=0;((ComboBox)editor.Controls.Find("EnginePrompt",true)[0]).SelectedIndex=1;((Button)editor.Controls.Find("SaveEngine",true)[0]).PerformClick();
                        };timer.Start();((Button)settings.Controls.Find("AddEngine",true)[0]).PerformClick();
                    }
                    Check(list.Items.Count==8,"Add form did not update manager");int added=(int)Read(list.SelectedItem,"Id");Check(Read(Reg("Get",added),"Model").ToString()=="local-a:latest"&&Read(Reg("Get",added),"Prompt").ToString()=="hy","Installed-model selection not saved");
                    ((Button)settings.Controls.Find("SaveSettings",true)[0]).PerformClick();Check(settings.IsDisposed,"Save and close did not close settings");Reg("Remove",added);
                }
                using(var editor=(Form)Activator.CreateInstance(App.GetType("YiChuang.EngineEditorForm"),new object[]{null})){
                    editor.Opacity=0;editor.Show(main);Application.DoEvents();var type=(ComboBox)editor.Controls.Find("EngineProvider",true)[0];
                    for(int i=0;i<5;i++){type.SelectedIndex=i;Application.DoEvents();Render(editor,"engine-editor-"+i+".png");Check(editor.Controls.Find("SaveEngine",true)[0].Visible,"Editor save hidden");}
                    type.SelectedIndex=0;Application.DoEvents();Check(((TextBox)editor.Controls.Find("EngineApiKey",true)[0]).UseSystemPasswordChar,"Key unmasked");editor.Close();
                }
                Log.Add("Configuration validation, corrupt-state recovery, actual native Add/installed-list/select/OK/manager/save-close event chain, and all five editor types with fixed footer/masked keys: passed");
                var persisted=Reg("Get",idCustom);Write(persisted,"Name","Persisted engine");Write(persisted,"Model","edited-model");Reg("Save",persisted);Reg("Remove",6);Call(main,"RebuildEngineMenu");Call(main,"SelectManagedEngine",idCustom);Config.GetMethod("jiluset").Invoke(null,null);Check(Config.GetField("SettingsSaveError").GetValue(null)==null,"Save error");
                string configPath=Path.Combine(Data,"YiChuang","setting.txt");Check(!File.ReadAllText(configPath).Contains("test-only-key-12345"),"Plaintext key persisted");string saved=(string)Config.GetMethod("readJiekou").Invoke(null,null);Reg("Import",new object[]{null});Config.GetMethod("read_setSetting").Invoke(null,new object[]{saved});Check(Reg("Get",6)==null&&Read(Reg("Get",idCustom),"Model").ToString()=="edited-model","Registry encrypted save/read failed");
                File.WriteAllText(Path.Combine(Output,"restart-data-path.txt"),Data);main.Close();
            }
            listener.Stop();Check(Unexpected==0&&Deletes==0,"Unexpected request or model deletion");Log.Add("Encrypted registry and edited model save/read; no external API/weight deletion requests: passed");File.WriteAllLines(Path.Combine(Output,"engine-manager-regression.txt"),Log);return 0;
        }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"engine-manager-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
