using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class LayoutHotkeyRegression
{
    static string Data;static int Requests;
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic, Static=BindingFlags.Static|BindingFlags.NonPublic;
    static Assembly App;static Type Config,Prefs,KeysType;
    [DllImport("user32.dll")]static extern bool RegisterHotKey(IntPtr window,int id,uint modifiers,uint key);
    [DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr window,int id);
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    class Guard:IWebRequestCreate{public WebRequest Create(Uri uri){Requests++;throw new Exception("Unexpected network request");}}
    static void Check(bool condition,string text){if(!condition)throw new Exception(text);}
    static object Field(object o,string name){return o.GetType().GetField(name,Private).GetValue(o);}
    static object Call(object o,string name,params object[] args){return o.GetType().GetMethod(name,Private).Invoke(o,args);}
    static object P(string name){return Prefs.GetField(name,Static).GetValue(null);}
    static bool Apply(string show,string translate,out string error){var args=new object[]{show,translate,null};bool ok=(bool)KeysType.GetMethod("TryApply",Static).Invoke(null,args);error=(string)args[2];return ok;}
    static Dictionary<string,object> Saved(){return new JavaScriptSerializer().Deserialize<Dictionary<string,object>>((string)Config.GetMethod("readJiekou").Invoke(null,null));}
    [STAThread]static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,45000,Timeout.Infinite))
        try {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string file=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(file)?Assembly.LoadFrom(file):null;};
            App=Assembly.LoadFrom(exe);Config=App.GetType("Xtranslator.ClassMain");Prefs=App.GetType("Xtranslator.UiPreferences");KeysType=App.GetType("Xtranslator.ClassSetHotKey");
            bool probe=args.Length>3&&args[2]=="--read";Data=probe?Path.GetFullPath(args[3]):Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LayoutHotkeyRegression).GetMethod("Folder"));
            App.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",Static).SetValue(null,output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());Application.EnableVisualStyles();
            using(var main=(Form)Activator.CreateInstance(App.GetType("Xtranslator.Form2"))){
                main.Opacity=0;main.Show();Application.DoEvents();main.GetType().GetField("_isInitializing",Private).SetValue(main,true);
                var split=(SplitContainer)Field(main,"splitContainer1");var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");
                if(probe){
                    Check((string)P("Layout")=="compact"&&split.Orientation==Orientation.Horizontal,"Layout did not survive fresh process");
                    Check((string)P("TranslateShortcut")=="Ctrl + Alt + J","Translation hotkey did not survive fresh process");
                    Check((string)Config.GetField("快捷键隐藏显示").GetValue(null)=="Ctrl + Alt + K","Show/hide hotkey did not survive fresh process");
                    Check(Math.Abs(source.Font.SizeInPoints-12)<.1&&Math.Abs(target.Font.SizeInPoints-16)<.1,"Fonts changed after layout restart");
                    main.Close();File.WriteAllText(Path.Combine(output,"restart-read.txt"),"Fresh-process layout, two custom shortcuts, and 12/16 pt fonts: passed");return 0;
                }
                Check((string)P("TranslateShortcut")=="F5"&&(string)Config.GetField("快捷键隐藏显示").GetValue(null)=="F6","New defaults are not F5/F6");
                source.Text="First sentence.\r\nSecond sentence.";target.Text="第一句。\r\n第二句。";
                Config.GetField("originalFontSize").SetValue(null,12f);Config.GetField("translationFontSize").SetValue(null,16f);Call(main,"F_Event_SetYuanSizeType");Call(main,"F_Event_SetTranslationSizeType");
                source.Select(2,5);target.Select(1,3);string src=source.Rtf,dst=target.Rtf;
                var toggle=(ToolStripButton)Field(main,"layoutButton");
                for(int i=0;i<6;i++){
                    toggle.PerformClick();Application.DoEvents();bool compact=(string)P("Layout")=="compact";
                    Check(split.Orientation==(compact?Orientation.Horizontal:Orientation.Vertical),"Toggle did not update orientation");
                    Check(split.Panel1.Contains(source)&&split.Panel2.Contains(target),"Source/translation pane order changed");
                    Check(source.Rtf==src&&target.Rtf==dst&&source.SelectionStart==2&&source.SelectionLength==5&&target.SelectionStart==1&&target.SelectionLength==3,"Layout toggle changed content, formatting, or selection");
                    foreach(ToolStripItem item in ((ToolStrip)Field(main,"toolStrip1")).Items){Check(item.DisplayStyle==(compact?ToolStripItemDisplayStyle.Image:ToolStripItemDisplayStyle.ImageAndText),"Layout button labels incorrect");Check(item.Image!=null&&!string.IsNullOrWhiteSpace(item.ToolTipText),"Icon lacks tooltip");}
                }
                string message;Check(!Apply("F7","F7",out message)&&message.Contains("同一个"),"Duplicate key was accepted");
                using(var occupied=new Form()){
                    var handle=occupied.Handle;Check(RegisterHotKey(handle,999,0x4000,0x77),"Could not arrange F8 conflict test");
                    try{Check(!Apply("F8","F5",out message)&&message.Contains("占用"),"Occupied shortcut accepted");}finally{UnregisterHotKey(handle,999);}
                }
                Check((string)Config.GetField("快捷键隐藏显示").GetValue(null)=="F6","Rejected shortcut modified saved binding");
                Check(Apply("Ctrl + Alt + K","Ctrl + Alt + J",out message),"Custom key registration failed: "+message);
                source.Clear();target.Clear();main.GetType().GetField("_isInitializing",Private).SetValue(main,false);
                toggle.PerformClick();Application.DoEvents();Call(main,"jiluset");
                var saved=Saved();Check(saved["UiLayout"].ToString()=="compact"&&saved["TranslateShortcut"].ToString()=="Ctrl + Alt + J"&&saved["ShortcutkeyHiddenDisplay"].ToString()=="Ctrl + Alt + K","Layout/shortcut save failed");
                string json=new JavaScriptSerializer().Serialize(saved);
                saved.Remove("ShortcutSchema");saved["ShortcutkeyHiddenDisplay"]="F3";Config.GetMethod("read_setSetting").Invoke(null,new object[]{new JavaScriptSerializer().Serialize(saved)});
                Check((string)Config.GetField("快捷键隐藏显示").GetValue(null)=="F6","Former default not migrated");
                saved["ShortcutkeyHiddenDisplay"]="Ctrl + Alt + K";Config.GetMethod("read_setSetting").Invoke(null,new object[]{new JavaScriptSerializer().Serialize(saved)});
                Check((string)Config.GetField("快捷键隐藏显示").GetValue(null)=="Ctrl + Alt + K","Custom shortcut overwritten by migration");
                Config.GetMethod("read_setSetting").Invoke(null,new object[]{json});main.Close();
            }
            Check(Requests==0,"UI test made a network request");File.WriteAllText(Path.Combine(output,"restart-data-path.txt"),Data);
            File.WriteAllText(Path.Combine(output,"layout-hotkey-regression.txt"),"Repeated switching preserves RTF, selection, 12/16 pt fonts and pane order: passed\r\nCompact icons/tooltips and wide labels: passed\r\nF5/F6 defaults, duplicate/conflict rejection, custom registration and old-default migration: passed\r\nPersisted layout and both shortcuts; zero network requests: passed\r\n");return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(output,"layout-hotkey-regression.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
