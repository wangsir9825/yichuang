using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class FontSettingsRegression
{
    static string Data;
    public static string Folder(Environment.SpecialFolder folder) { return Data; }
    static object Member(object value,string name) { return value.GetType().GetField(name,BindingFlags.Instance|BindingFlags.NonPublic).GetValue(value); }
    static object Value(Type main,string name) { return main.GetField(name).GetValue(null); }
    static void Set(Type main,string name,object value) { main.GetField(name).SetValue(null,value); }
    static void Assert(bool condition,string message) { if (!condition) throw new Exception(message); }
    static Control Find(Control parent,string name)
    {
        if (parent.Name == name) return parent;
        foreach(Control child in parent.Controls) { Control found=Find(child,name);if(found!=null)return found; }
        return null;
    }
    class DenyNetwork : IWebRequestCreate { public WebRequest Create(Uri uri) { throw new Exception("Unexpected network request: "+uri.Host); } }
    static Form Settings(Assembly app,Form form)
    {
        var settings=(Form)Activator.CreateInstance(app.GetType("Xtranslator.Frmsetting"));settings.Opacity=0;
        foreach(string entry in new[]{"Translation","Yuan"}) {
            var info=settings.GetType().GetEvent("Event_Set"+entry+"SizeType");
            info.AddEventHandler(settings,Delegate.CreateDelegate(info.EventHandlerType,form,form.GetType().GetMethod("F_Event_Set"+entry+"SizeType",BindingFlags.Instance|BindingFlags.NonPublic)));
        }
        settings.Show(form);Application.DoEvents();return settings;
    }
    [STAThread]
    static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);var log=new List<string>();
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,45000,Timeout.Infinite))
        try {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
            Assembly app=Assembly.LoadFrom(exe);Type main=app.GetType("Xtranslator.ClassMain");
            Data=Path.Combine(output,"isolated-data");Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(FontSettingsRegression).GetMethod("Folder"));
            app.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.NonPublic|BindingFlags.Static).SetValue(null,output);
            WebRequest.RegisterPrefix("http://",new DenyNetwork());WebRequest.RegisterPrefix("https://",new DenyNetwork());
            Application.EnableVisualStyles();
            var form=(Form)Activator.CreateInstance(app.GetType("Xtranslator.Form2"));form.Opacity=0;form.Show();Application.DoEvents();
            if(args.Length>2 && args[2]=="--restart-check") {
                Assert((float)Value(main,"translationFontSize")==12f,"Fresh process did not restore saved 12pt");
                Assert((float)Value(main,"originalFontSize")==14f,"Fresh process did not restore saved source 14pt");
                Assert(Math.Abs(((RichTextBox)Member(form,"rtb译文")).Font.SizeInPoints-12f)<0.01,"Fresh process did not apply saved 12pt to the translation pane");
                log.Add("Fresh process reloads and applies saved translation 12pt and source 14pt: passed");
                File.AppendAllLines(Path.Combine(output,"font-regression.txt"),log);Console.WriteLine(string.Join(Environment.NewLine,log));
                Environment.Exit(0);
            }
            using(var settings=Settings(app,form)) {
                var translatedSize=(ComboBox)Find(settings,"cbb译文区字体大小");
                var originalSize=(ComboBox)Find(settings,"cbb原文区字体大小");
                Assert(translatedSize!=null&&originalSize!=null,"Font controls missing");
                translatedSize.SelectedValue="12";originalSize.SelectedValue="14";Application.DoEvents();
                Assert((float)Value(main,"translationFontSize")==12f,"Selecting 12 did not update the saved setting");
                Assert(Math.Abs(((RichTextBox)Member(form,"rtb译文")).Font.SizeInPoints-12f)<0.01,"Selecting 12 did not update the displayed font");
                settings.Close();
            }
            log.Add("Selecting translation 12pt and source 14pt updates display and saves: passed");
            var json=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>((string)main.GetMethod("readJiekou").Invoke(null,null));
            Assert(json["translationFontSize"].ToString()=="12"&&json["originalFontSize"].ToString()=="14","Font sizes did not persist in encrypted config");
            form.Size=new Size(form.Width+110,form.Height+80);Application.DoEvents();
            for(int i=0;i<3;i++) using(var reopened=Settings(app,form)) {
                Assert((float)Value(main,"translationFontSize")==12f,"Opening settings overwrote translation 12pt");
                Assert((float)Value(main,"originalFontSize")==14f,"Opening settings overwrote source 14pt");
                Assert(((ComboBox)Find(reopened,"cbb译文区字体大小")).SelectedValue.ToString()=="12","Reopened size control did not display 12");
                Assert(Math.Abs(((RichTextBox)Member(form,"rtb译文")).Font.SizeInPoints-12f)<0.01,"Window resize changed the displayed font");
                reopened.Close();
            }
            log.Add("Resize then reopen/close settings three times keeps translation 12pt and source 14pt: passed");
            File.WriteAllLines(Path.Combine(output,"font-regression.txt"),log);Console.WriteLine(string.Join(Environment.NewLine,log));
            form.Close();
        } catch(Exception ex) {while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"font-regression.txt"),log);Console.WriteLine(ex);Environment.ExitCode=1;}
        Environment.Exit(Environment.ExitCode);return Environment.ExitCode;
    }
}
