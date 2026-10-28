using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

class SettingsButtonLayoutRegression {
 static string Data,Output; static readonly List<string> Log=new List<string>();
 [DllImport("user32.dll")] static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 public static string Folder(Environment.SpecialFolder folder){return Data;}
 class Guard:IWebRequestCreate {public WebRequest Create(Uri u){throw new Exception("Unexpected network request");}}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Render(Control c,string name){using(var image=new Bitmap(c.Width,c.Height)){c.DrawToBitmap(image,new Rectangle(Point.Empty,c.Size));image.Save(Path.Combine(Output,name+".png"));}}
 [STAThread]static int Main(string[] args){
  Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);bool observe=Array.IndexOf(args,"--observe")>=0;
  if(Array.IndexOf(args,"--96")>=0)SetThreadDpiAwarenessContext(new IntPtr(-1));
  using(var watchdog=new System.Threading.Timer(_=>Environment.Exit(124),null,30000,Timeout.Infinite))try{
   string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
   AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
   var app=Assembly.LoadFrom(exe);Data=Path.Combine(Output,"data");Directory.CreateDirectory(Data);
   var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(SettingsButtonLayoutRegression).GetMethod("Folder"));
   app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
   app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
   WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
   using(var settings=(Form)Activator.CreateInstance(app.GetType("YiChuang.Frmsetting"))){
    settings.Opacity=0;settings.Show();Application.DoEvents();
    foreach(string page in new[]{"引擎管理"}){
     settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{page});Application.DoEvents();
     foreach(string name in new[]{"OpenLocalSetup","RestoreEngine","AddEngine","EditEngine","MoveEngineUp","SaveSettings","ApplySettings","ReturnSettings"})foreach(Control found in settings.Controls.Find(name,true)){
      var b=(Button)found;using(var g=b.CreateGraphics()){
       Size text=TextRenderer.MeasureText(g,b.Text,b.Font,Size.Empty,TextFormatFlags.SingleLine);
       Size preferred=b.GetPreferredSize(Size.Empty);int required=Math.Max(text.Height+b.Padding.Vertical+6,preferred.Height);
       Log.Add(page+" / "+b.Text+" DPI="+g.DpiX+" bounds="+b.Bounds+" font="+b.Font.SizeInPoints+" text="+text+" padding="+b.Padding+" preferred="+preferred+" requiredHeight="+required);
       if(!observe){Check(b.ClientSize.Height>=required,"Button text height clipped: "+b.Text);Check(b.Width>=text.Width+b.Padding.Horizontal+6,"Button text width clipped: "+b.Text);Check(b.Parent.ClientRectangle.Contains(b.Bounds),"Button outside parent: "+b.Text);}
      }
     }
     Render(settings,page=="引擎管理"?"engine-list":"local-models");
     settings.Size=settings.MinimumSize;Application.DoEvents();
     foreach(string name in new[]{"SaveSettings","ApplySettings","ReturnSettings"}){var b=(Control)settings.Controls.Find(name,true)[0];Check(settings.RectangleToScreen(settings.ClientRectangle).Contains(b.RectangleToScreen(b.ClientRectangle)),"Footer hidden at minimum size");}
    }
    settings.Close();
   }
   File.WriteAllLines(Path.Combine(Output,"results.txt"),Log);Console.WriteLine("Settings button layout passed");return 0;
  }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"results.txt"),Log);Console.Error.WriteLine(ex);return 1;}
 }
}
