using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

class SlimUiRegression
{
    static string Data;static int Requests;
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    class Guard:IWebRequestCreate {public WebRequest Create(Uri uri){Requests++;throw new Exception("Unexpected network request");}}
    static void Assert(bool value,string text){if(!value)throw new Exception(text);}
    static object Member(object target,string name){return target.GetType().GetField(name,Private).GetValue(target);}
    [STAThread] static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        try {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFile(p):null;};
            var app=Assembly.LoadFrom(exe);Data=Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(SlimUiRegression).GetMethod("Folder"));
            app.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());
            Application.EnableVisualStyles();var config=app.GetType("Xtranslator.ClassMain");
            config.GetField("is单词悬浮框").SetValue(null,true);config.GetField("formLocation").SetValue(null,"4");
            using(var main=(Form)Activator.CreateInstance(app.GetType("Xtranslator.Form2"))) {
                main.Opacity=0;main.Show();Application.DoEvents();
                Assert(main.Text=="译窗 1.5.4","Product title not updated");
                Assert(System.Diagnostics.FileVersionInfo.GetVersionInfo(exe).FileVersion=="1.5.4.0","Version not updated");
                foreach(string name in new[]{"Xtranslator.LocalComparisonForm","Xtranslator.Frm悬浮","Xtranslator.Form1","Xtranslator.Form3","Xtranslator.UserControl百度API"})Assert(app.GetType(name)==null,"Removed type remains compiled: "+name);
                foreach(string field in new[]{"tbtnhide","tbtnclockwise","tbtnanticlockwise","tbtnInput_trans_mode"})Assert(main.GetType().GetField(field,Private)==null,"Removed button remains compiled: "+field);
                var menu=(ToolStripDropDownButton)Member(main,"toolStripDropDownButton1");int count=0;
                foreach(ToolStripItem item in menu.DropDownItems)if(item is ToolStripMenuItem){count++;Assert(item.Text.StartsWith(count+"："),"Engine numbering changed or comparison remains");}
                Assert(count==4,"Default visible engines should be the requested four");
                app.GetType("Xtranslator.EngineRegistry").GetMethod("SetVisible").Invoke(null,new object[]{16,true});
                Assert(!(bool)config.GetField("is单词悬浮框").GetValue(null)&&config.GetField("formLocation").GetValue(null).ToString()=="1","Old floating/layout setting not normalized");
                main.GetType().GetField("_isInitializing",Private).SetValue(main,true);
                main.GetType().GetMethod("SelectLocalEngine",Private).Invoke(main,new object[]{16});
                config.GetField("is单词悬浮框").SetValue(null,true);
                main.GetType().GetMethod("xianshiyuanwen",Private).Invoke(main,new object[]{"hello"});Application.DoEvents();
                Assert(((RichTextBox)Member(main,"rtb原文")).Text=="hello"&&Application.OpenForms.Count==1,"Captured word opened floating window");
                ((RichTextBox)Member(main,"rtb原文")).Clear();
                var proc=main.GetType().GetMethod("WndProc",Private);main.WindowState=FormWindowState.Normal;
                var message=Message.Create(main.Handle,0x312,new IntPtr(100),IntPtr.Zero);proc.Invoke(main,new object[]{message});Application.DoEvents();
                Assert(main.WindowState==FormWindowState.Minimized,"F6 no longer minimizes");
                proc.Invoke(main,new object[]{message});Application.DoEvents();Assert(main.WindowState==FormWindowState.Normal,"F6 no longer restores");
                using(var settings=(Form)Activator.CreateInstance(app.GetType("Xtranslator.Frmsetting"))) {
                    settings.Opacity=0;settings.Show(main);Application.DoEvents();Assert(settings.Text=="译窗设置","Settings title not updated");
                    settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"常用设置"});Application.DoEvents();
                    Assert(settings.Controls.Find("rdb单词翻译跟随鼠标悬浮显示",true).Length==0,"Floating display option remains");
                    settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"快捷键"});Application.DoEvents();
                    var input=(TextBox)settings.Controls.Find("ShowHideShortcut",true)[0];
                    typeof(Control).GetMethod("OnKeyDown",Private).Invoke(input,new object[]{new KeyEventArgs(Keys.Control|Keys.F2)});
                    Assert(input.Text=="Ctrl + F2","Shortcut draft not updated");
                    typeof(Control).GetMethod("OnKeyDown",Private).Invoke(input,new object[]{new KeyEventArgs(Keys.Back)});
                    Assert(input.Text=="F6","Restore F6 failed");
                    settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"关于译窗"});Application.DoEvents();
                    var tutorial=(Button)settings.Controls.Find("TutorialPlaceholder",true)[0];Assert(tutorial.Text=="筑途房教程（待补充）"&&!tutorial.Enabled,"Tutorial placeholder opens an old link");settings.Close();
                }
                main.Close();
            }
            Assert(Requests==0,"Cleanup UI sent a network request");
            File.WriteAllText(Path.Combine(output,"slim-ui-regression.txt"),"Removed types/buttons/menus and legacy setting migration: passed\r\nFour default visible engines and captured word main-window routing: passed\r\nF6 minimize/restore message and shortcut editing/reset: passed\r\nProduct/version/About/tutorial placeholder and zero network calls: passed\r\n");return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(output,"slim-ui-regression.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
