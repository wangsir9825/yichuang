using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
class UiRepairRegression {
 static string Data;static int Requests;static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
 static List<string> Log=new List<string>();
 public static string Folder(Environment.SpecialFolder folder){return Data;}
 class Guard:IWebRequestCreate {public WebRequest Create(Uri uri){Requests++;throw new Exception("Unexpected request");}}
 static object Field(object o,string n){return o.GetType().GetField(n,Private).GetValue(o);}
 static object Call(object o,string n,params object[] values){return o.GetType().GetMethod(n,Private).Invoke(o,values);}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Render(Control c,string file){using(var b=new Bitmap(c.Width,c.Height)){c.DrawToBitmap(b,new Rectangle(0,0,c.Width,c.Height));b.Save(file);}}
 static void CheckSettingsNavigation(Form settings,Assembly app,string output){
  settings.GetType().GetField("EngineChangesPrompt",Private).SetValue(settings,(Func<DialogResult>)(()=>DialogResult.Yes));
  Action<string> show=key=>{settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{key});Application.DoEvents();};
  show("引擎管理");var content=(Panel)Field(settings,"content");var manager=content.Controls[0];
  var list=(CheckedListBox)manager.Controls.Find("ManagedEngines",true)[0];var edit=(Button)manager.Controls.Find("EditEngine",true)[0];
  var save=(Button)settings.Controls.Find("SaveSettings",true)[0];
  var registry=app.GetType("YiChuang.EngineRegistry");string before=string.Join(",",(int[])registry.GetProperty("VisibleIds").GetValue(null,null));string order=string.Join(",",(int[])registry.GetProperty("ActiveIds").GetValue(null,null));
  foreach(int id in new[]{6,14,18,19}){
   for(int i=0;i<list.Items.Count;i++)if((int)list.Items[i].GetType().GetField("Id").GetValue(list.Items[i])==id)list.SelectedIndex=i;
   edit.PerformClick();Application.DoEvents();Check(content.Controls[0]!=manager&&save.Text=="保存"&&settings.Controls.Find("ApplySettings",true)[0].Visible&&settings.Controls.Find("ReturnSettings",true)[0].Visible,"Missing engine-level actions: "+id);
   var picker=(ComboBox)settings.Controls.Find("SettingsEnginePicker",true)[0];Check(picker.Width>180&&picker.Parent.ClientRectangle.Contains(picker.Bounds),"Engine picker is clipped: "+id);
   if(id==14){settings.Controls.Find("DeepSeekModel",true)[0].Text="navigation-draft-model";}
   foreach(Button b in ((FlowLayoutPanel)Field(settings,"navigation")).Controls)if((string)b.Tag=="引擎管理")b.PerformClick();Application.DoEvents();Check(!settings.IsDisposed&&settings.Visible&&content.Controls[0]==manager,"Back closed settings: "+id);
   Check((int)list.SelectedItem.GetType().GetField("Id").GetValue(list.SelectedItem)==id,"Back lost selected engine: "+id);
   edit.PerformClick();Application.DoEvents();if(id==14)Check(settings.Controls.Find("DeepSeekModel",true)[0].Text=="navigation-draft-model","Return discarded draft");
   if(id==6)Render(settings,Path.Combine(output,"native-settings-engine-navigation.png"));
   save.PerformClick();Application.DoEvents();Check(!settings.IsDisposed&&settings.Visible&&content.Controls[0]==manager&&save.Text=="保存"&&settings.Controls.Find("ReturnSettings",true)[0].Text=="退出","Save-and-return closed settings: "+id);
   Check((int)list.SelectedItem.GetType().GetField("Id").GetValue(list.SelectedItem)==id,"Save lost selection: "+id);
  }
  edit.PerformClick();Application.DoEvents();foreach(Button b in ((FlowLayoutPanel)Field(settings,"navigation")).Controls)if((string)b.Tag=="引擎管理")b.PerformClick();
  Check(content.Controls[0]==manager&&settings.Visible,"Active sidebar engine item did not return to list");
  Check(before==string.Join(",",(int[])registry.GetProperty("VisibleIds").GetValue(null,null))&&order==string.Join(",",(int[])registry.GetProperty("ActiveIds").GetValue(null,null)),"Navigation changed visibility/order");
  Log.Add("Four built-in editors: save/apply/return available; confirmed sidebar save and save return keep settings open, preserve selection/order/visibility and engine-picker geometry: passed");
 }
 [STAThread]static int Main(string[] args){
  string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
  using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,35000,Timeout.Infinite))try{
   string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
   AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
   var app=Assembly.LoadFrom(exe);var config=app.GetType("YiChuang.ClassMain");
   Data=Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
   var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(UiRepairRegression).GetMethod("Folder"));
   app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
   app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,output);
   WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
   using(var main=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2"))){
    main.Opacity=0;main.Show();Application.DoEvents();main.GetType().GetField("_isInitializing",Private).SetValue(main,true);
    var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");
    foreach(Control child in ((Panel)Field(main,"brandBar")).Controls)Log.Add("Brand "+child.GetType().Name+" "+child.Bounds+" visible="+child.Visible+" image="+(child is PictureBox?((PictureBox)child).Image!=null:false));
    foreach(string name in new[]{"titleStrip","toolStrip1","captureStrip"}){var strip=(ToolStrip)Field(main,name);Log.Add(name+" "+strip.Bounds+" visible="+strip.Visible);foreach(ToolStripItem item in strip.Items)Log.Add(item.Name+" "+item.Bounds+" available="+item.Available+" placement="+item.Placement+" display="+item.DisplayStyle);}
    Render(main,Path.Combine(output,"native-wide.png"));
    Check(!((ToolStripItem)Field(main,"toolStripButton3")).Visible,"Redundant exit is visible");
    Check(!((StatusStrip)Field(main,"modernStatusStrip")).Visible,"Idle footer hint remains");
    foreach(ToolStripItem item in ((ToolStrip)Field(main,"titleStrip")).Items)Check(item.Placement==ToolStripItemPlacement.Main,"Title action missing: "+item.Name);
    var preferences=app.GetType("YiChuang.UiPreferences");
    var layoutPreference=preferences.GetField("Layout",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
    foreach(string mode in new[]{"compact","wide","compact","wide"}){
     layoutPreference.SetValue(null,mode);Call(main,"ApplyModernLayout",true);main.Size=main.MinimumSize;Application.DoEvents();
     var logo=(PictureBox)Field(main,"brandLogo");var name=(Label)Field(main,"brandName");var version=(Label)Field(main,"brandVersion");var actions=(ToolStrip)Field(main,"titleStrip");
     foreach(Control item in new Control[]{logo,name,version})Check(item.Visible&&item.Parent.ClientRectangle.Contains(item.Bounds)&&item.Right<=actions.Left,"Brand clipped at minimum "+mode+": "+item.Bounds);
     Check(logo.Image!=null,"Brand icon is missing");Check(!name.Bounds.IntersectsWith(version.Bounds),"Name overlaps version in "+mode);
     foreach(ToolStripItem item in actions.Items)Check(item.Placement==ToolStripItemPlacement.Main,"Title action clipped at minimum "+mode+": "+item.Name);
     Render(main,Path.Combine(output,"native-brand-"+mode+".png"));
    }
    Call(main,"ApplyModernLayout",true);
    var normalBounds=main.Bounds;
    for(int transition=0;transition<2;transition++){
     main.WindowState=FormWindowState.Maximized;Application.DoEvents();main.WindowState=FormWindowState.Normal;Application.DoEvents();
     Check(main.Bounds==normalBounds,"Native frame changed restored bounds: "+normalBounds+" -> "+main.Bounds);
    }
    Log.Add("Repeated maximize/restore preserves exact normal bounds: passed");
    Log.Add("Brand icon, name, version and all actions fit minimum compact/wide sizes across repeated layout switches: passed");
    Log.Add("DWM shadow configured="+Field(main,"modernShadowAvailable"));
    Call(main,"SetCaptureMode",2,true);
    foreach(string name in new[]{"rtb原文_MouseEnter","rtb原文_MouseLeave","rtb译文_MouseEnter","rtb译文_MouseLeave"})Call(main,name,source,EventArgs.Empty);
    Check((bool)config.GetField("_is自动翻译总开关").GetValue(null),"Hover turned capture off");
    Call(main,"SetCaptureMode",2,false);Call(main,"tbtnmodectrl_Click",null,EventArgs.Empty);
    Check((bool)config.GetField("_is自动翻译总开关").GetValue(null),"Choosing a paused mode did not resume it");
    Call(main,"SetCaptureMode",1,true);Check(((System.Windows.Forms.Timer)Field(main,"timer1")).Enabled,"Mouse selection timer not started");
    Call(main,"SetCaptureMode",1,false);Check(!((System.Windows.Forms.Timer)Field(main,"timer1")).Enabled,"Pause left selection timer running");
    Call(main,"SetCaptureMode",2,true);Log.Add("Capture mode pause/reselect/restart and hover state preservation: passed");
    source.Text="The first sentence stays readable.\nLarge letters remain inside their highlight.\nThe final paragraph stays readable too.";
    target.Text="第一句保持清晰。\n大号文字应被高亮背景完整覆盖。\n最后一段文字也应完整显示。";
    foreach(int size in new[]{12,24,32}){
     source.Font=new Font("Microsoft YaHei UI",size);target.Font=new Font("宋体",size);
     foreach(var view in new[]{source,target}){
      view.Select(2,4);int start=view.SelectionStart,length=view.SelectionLength;config.GetMethod("SetLineSpace").Invoke(null,new object[]{view,250});
      Check(view.SelectionStart==start&&view.SelectionLength==length,"Line spacing moved selection");
      Check(!view.Rtf.Contains("\\sl-"),"Exact line height remains in some paragraphs");
      view.Select(0,0);Application.DoEvents();
      view.GetType().GetMethod("SetHoverRange").Invoke(view,new object[]{0,view.TextLength});
      int rows=view.GetLineFromCharIndex(view.TextLength-1)+1;
      for(int row=0;row<rows;row++){
       int first=view.GetFirstCharIndexFromLine(row),next=view.GetFirstCharIndexFromLine(row+1);if(next<0)next=view.TextLength;
       var bounds=(Rectangle)Call(view,"RowBounds",first,next);
       using(var g=view.CreateGraphics())Check(bounds.Height>=view.Font.GetHeight(g)*.80,"Highlight shorter than actual font: "+size+" "+bounds);
       if(row+1<rows){var position=view.GetPositionFromCharIndex(next);Check(position.Y>=bounds.Bottom-2,"Large glyph rows overlap: "+size);}
      }
     }
     Render(main,Path.Combine(output,"native-font-"+size+".png"));
    }
    Log.Add("12/24/32 pt, multiple paragraphs, actual glyph height, nonoverlap and preserved selection: passed");
    using(var settings=(Form)Activator.CreateInstance(app.GetType("YiChuang.Frmsetting"))){
     settings.Opacity=0;settings.Show(main);Application.DoEvents();
     var save=(Button)settings.Controls.Find("SaveSettings",true)[0];
     CheckSettingsNavigation(settings,app,output);
     int pageIndex=0;
     foreach(string title in (string[])settings.GetType().GetProperty("SettingsPageTitles").GetValue(settings,null)){
      settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{title});Application.DoEvents();
      var page=((Panel)Field(settings,"content")).Controls[0];Log.Add(title+" page="+page.Size+" dialog="+settings.Size);Check(page.Visible&&page.Width>300&&page.Height>100,"Settings page is empty: "+title+" "+page.Bounds);Check(save.Parent.ClientRectangle.Contains(save.Bounds),"Save button clipped: "+title);
      Check(save.Height>=TextRenderer.MeasureText(save.Text,save.Font).Height+10,"Save button too short: "+title);
      Check(settings.RectangleToScreen(settings.ClientRectangle).Contains(save.RectangleToScreen(save.ClientRectangle)),"Save button outside dialog");
      Render(settings,Path.Combine(output,"native-settings-page-"+(pageIndex++)+".png"));
     }
     settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"常用设置"});Render(settings,Path.Combine(output,"native-settings.png"));settings.Close();
    }
    Log.Add("All settings pages retain fully visible independent save footer: passed");
    main.Close();
   }
   Check(Requests==0,"Real API used in regression");File.WriteAllLines(Path.Combine(output,"ui-repair-regression.txt"),Log);return 0;
  }catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"ui-repair-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
 }
}
