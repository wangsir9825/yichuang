using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

class ParagraphCaptureRegression {
 static string Data,Output;static bool Live;static int Requests;
 static readonly List<string> Log=new List<string>();
 static readonly BindingFlags Private=BindingFlags.NonPublic|BindingFlags.Instance;
 public static string Folder(Environment.SpecialFolder folder){return Data;}
 class Guard:IWebRequestCreate {public WebRequest Create(Uri u){if(Live&&u.Scheme=="https"&&(u.Host=="fanyi.sogou.com"||u.Host=="transmart.qq.com")){Requests++;return WebRequest.CreateDefault(u);}throw new Exception("Unexpected network request: "+u.Host);}}
 static object Field(object o,string n){return o.GetType().GetField(n,Private).GetValue(o);}
 static void Call(object o,string n,params object[] v){o.GetType().GetMethod(n,Private).Invoke(o,v);}
 static void Check(bool ok,string m){if(!ok)throw new Exception(m);}
 static void Render(Control c,string name){using(var image=new Bitmap(c.Width,c.Height)){c.DrawToBitmap(image,new Rectangle(Point.Empty,c.Size));image.Save(Path.Combine(Output,name+".png"));}}
 static string Lines(string s){return s.Replace("\r\n","\n").Replace("\r","\n").Replace("\u2028","\n").Replace("\u2029","\n").Replace("\u00a0"," ");}
 [STAThread]static int Main(string[] args){
  Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);Live=Array.IndexOf(args,"--live")>=0;
  using(var watchdog=new System.Threading.Timer(_=>Environment.Exit(124),null,90000,Timeout.Infinite))try{
   string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
   AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
   var app=Assembly.LoadFrom(exe);var config=app.GetType("YiChuang.ClassMain");Data=Path.Combine(Output,"data");Directory.CreateDirectory(Data);
   var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(ParagraphCaptureRegression).GetMethod("Folder"));
   app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
   app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,Output);
   WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
   using(var main=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2"))){
    main.Opacity=0;main.Show();Application.DoEvents();Call(main,"SetCaptureMode",0,false);
    var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");
    foreach(string prefix in new[]{"original","translation"}){
     Check((string)config.GetField(prefix+"FontType").GetValue(null)=="微软雅黑","Wrong fresh default font");
     Check((float)config.GetField(prefix+"FontSize").GetValue(null)==12f,"Wrong fresh default size");
    }
    using(var expected=new Font("微软雅黑",12))foreach(var view in new[]{source,target})Check(view.Font.FontFamily.Name==expected.FontFamily.Name&&Math.Abs(view.Font.SizeInPoints-12)<.01,"Main view did not use YaHei 12");
    Log.Add("Fresh isolated config: both real text views use Microsoft YaHei 12 pt.");
    var initializing=main.GetType().GetField("_isInitializing",Private);initializing.SetValue(main,true);
    string sample="This is what I did in my first year\r\n\r\n1. Posted every day\r\n2. Left comments\r\n3. Shared new ideas\r\n\r\nSame applies now";
    string[] cases={sample,"alpha\nbeta\n\ngamma","第一行\r第二行\r\r第三行","one\u2028two\u2029three","  - First item\n  - Second item","Ends with -\r\nNext line","Literal @zhw@ and\u00a0space","One unpunctuated sentence"};
    foreach(int engine in new[]{4,5,6,14,16,17,18,19})foreach(string input in cases){
     config.GetField("isjiekounum").SetValue(null,engine);Call(main,"xianshi",input);Application.DoEvents();
     Check(source.Text==Lines(input),"Capture lost line structure for engine "+engine+": "+source.Text);
    }
    Log.Add("64 capture cases: CRLF/LF/CR, Unicode separators, blank lines, list indent, literal marker and hyphen preserved across all 8 built-in engines.");
    config.GetField("isjiekounum").SetValue(null,5);Call(main,"xianshi",sample);target.Text="分段测试";Render(main,"captured-lines");
    if(Live){
     string input="1. Read a book every day\r\n2. Write down new ideas\r\n\r\nKeep learning";
     foreach(int engine in new[]{5,4}){
      initializing.SetValue(main,true);source.Clear();target.Clear();Call(main,"SelectManagedEngine",engine);initializing.SetValue(main,false);
      int before=Requests;Call(main,"xianshi",input);
      var until=DateTime.UtcNow.AddSeconds(30);while(DateTime.UtcNow<until&&(Requests==before||((ToolStripButton)Field(main,"localCancelButton")).Enabled)){Application.DoEvents();Thread.Sleep(20);}
      Check(Requests>before&&!((ToolStripButton)Field(main,"localCancelButton")).Enabled,"Real translation timed out");
      Check(source.Text==Lines(input),"Source paragraphs changed after request");string translated=Lines(target.Text).Trim();
      File.WriteAllText(Path.Combine(Output,"real-"+engine+".txt"),translated);
      Check(!translated.StartsWith("翻译失败")&&translated.Split('\n').Length==4&&translated.Contains("\n\n"),"Real translation did not preserve rows: "+translated);
      Render(main,"real-"+engine);Log.Add("Real main-window capture -> "+(engine==5?"Sogou":"Transmart")+" -> translated display: numbered lines and blank paragraph retained.");
     }
    }else Check(Requests==0,"Offline regression used network");
    initializing.SetValue(main,true);source.Clear();main.Close();
   }
   File.WriteAllLines(Path.Combine(Output,"results.txt"),Log);Console.WriteLine("Paragraph/default font regression passed; requests="+Requests);return 0;
  }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"results.txt"),Log);Console.Error.WriteLine(ex);return 1;}
 }
}
