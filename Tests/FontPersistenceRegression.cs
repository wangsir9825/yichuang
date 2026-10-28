using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

class FontPersistenceRegression
{
    static string Data,Output;
    static int Requests;
    static Assembly App;
    static Type MainType;
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly List<string> Log=new List<string>();
    static void Stage(string text){File.AppendAllText(Path.Combine(Output,"progress.txt"),DateTime.Now.ToString("HH:mm:ss ")+text+Environment.NewLine);}
    public static string Folder(Environment.SpecialFolder folder) {return Data;}
    class Guard:IWebRequestCreate {public WebRequest Create(Uri uri){Requests++;throw new Exception("Unexpected font-test network request");}}
    static object Member(object instance,string name){return instance.GetType().GetField(name,Private).GetValue(instance);}
    static float Size(){return (float)MainType.GetField("translationFontSize").GetValue(null);}
    static void Assert(bool ok,string text){if(!ok)throw new Exception(text);}
    static ComboBox Combo(Form settings,string name)
    {
        var pages=(IDictionary)Member(settings,"pages");
        return (ComboBox)Member(pages["常用设置"],name);
    }
    // Invoke the application's exact settings button/modal/save path with a WinForms
    // timer driving its controls. No replacement settings implementation or API mock.
    static void Settings(Form main,Action<Form> action)
    {
        Stage("Opening modal settings");
        Exception failure=null;bool handled=false;
        using(var timer=new System.Windows.Forms.Timer{Interval=60})
        {
            timer.Tick+=(s,e)=>{
                Form settings=null;
                foreach(Form open in Application.OpenForms) if(open.GetType().FullName=="YiChuang.Frmsetting") {settings=open;break;}
                if(settings==null)return;
                timer.Stop();handled=true;
                Stage("Modal found; checking controls");
                try{action(settings);}catch(Exception ex){failure=ex;}
                Button save=null;
                Action<Control> find=null;find=c=>{if(c is Button&&c.Name=="SaveSettings")save=(Button)c;foreach(Control child in c.Controls)find(child);};find(settings);
                if(save!=null)save.PerformClick();else settings.Close();
                Stage("Modal save/close complete");
            };
            timer.Start();
            main.GetType().GetMethod("toolStripButton1_Click",Private).Invoke(main,new object[]{null,EventArgs.Empty});
        }
        Assert(handled,"Settings modal did not load");if(failure!=null)throw failure;
        Stage("Settings method returned");
    }
    static void Saved(float expected)
    {
        string raw=(string)MainType.GetMethod("readJiekou").Invoke(null,null);
        var values=new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(raw);
        Assert(Convert.ToSingle(values["translationFontSize"])==expected,"Saved translation size differs");
        Assert(Convert.ToSingle(values["originalFontSize"])==12,"Translation font changes original font size");
    }
    static Form MainForm()
    {
        var main=(Form)Activator.CreateInstance(App.GetType("YiChuang.Form2"));
        main.Opacity=0;main.Show();Application.DoEvents();return main;
    }
    [STAThread]
    static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>{Stage("Timed out");File.WriteAllLines(Path.Combine(Output,"font-persistence-regression.txt"),Log);Environment.Exit(124);},null,45000,Timeout.Infinite))
        try
        {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};
            App=Assembly.LoadFrom(exe);MainType=App.GetType("YiChuang.ClassMain");
            bool probe=args.Length>3&&args[2]=="--read";
            Data=probe ? Path.GetFullPath(args[3]) : Path.Combine(Output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(FontPersistenceRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());
            Application.EnableVisualStyles();
            Form main=MainForm();
            Stage("First main loaded");
            var target=(RichTextBox)Member(main,"rtb译文");
            if(probe)
            {
                Assert(Size()==12&&Math.Abs(target.Font.SizeInPoints-12)<0.1,"Fresh process did not read saved 12 pt");
                Settings(main,settings=>Assert(Combo(settings,"cbb译文区字体大小").Text=="12","Restart settings do not show 12 pt"));
                main.Close();Assert(Requests==0,"Restart probe issued a request");
                File.WriteAllText(Path.Combine(Output,"restart-read.txt"),"Fresh process: saved configuration, main font and reopened settings all show 12 pt; zero requests.");
                return 0;
            }
            foreach(int size in new[]{12,16,12})
            {
                Stage("Checking size "+size);
                Settings(main,settings=>{
                    Combo(settings,"cbb译文区字体大小").SelectedValue=size.ToString();Application.DoEvents();
                    Assert(Size()==size,"Selected font size not applied to runtime setting");
                    Assert(Math.Abs(target.Font.SizeInPoints-size)<0.1,"Displayed translation font differs from chosen size");
                    Assert(Combo(settings,"cbb译文区字体大小").Text==size.ToString(),"Font dropdown does not show chosen size");
                });
                Saved(size);
                foreach(Size window in new[]{new Size(780,610),new Size(620,790),new Size(940,680)})
                {
                    main.Size=window;Application.DoEvents();
                    Assert(Size()==size&&Math.Abs(target.Font.SizeInPoints-size)<0.1,"Resize resets font size");
                    Settings(main,settings=>{
                        Assert(Combo(settings,"cbb译文区字体大小").Text==size.ToString(),"Reopened settings show 9 instead of saved size");
                        Assert(Size()==size,"Opening settings overwrites saved size");
                        settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"快捷键"});
                        settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{"常用设置"});Application.DoEvents();
                        Assert(Combo(settings,"cbb译文区字体大小").Text==size.ToString()&&Size()==size,"Switching settings page resets font size");
                    });
                    Saved(size);
                    Log.Add("Saved "+size+" pt -> resize "+window.Width+"x"+window.Height+" -> reopen/switch settings -> display/runtime/font/config all remain "+size+" pt: passed");
                }
                main.WindowState=FormWindowState.Maximized;Application.DoEvents();main.WindowState=FormWindowState.Normal;Application.DoEvents();
                Assert(Size()==size&&Math.Abs(target.Font.SizeInPoints-size)<0.1,"Maximize/restore resets font size");
            }
            Stage("Closing first main");main.Close();Application.DoEvents();Stage("First main closed");
            // Verify.ps1 runs the read probe after this process exits, ensuring the
            // application's native clipboard/pipe resources are actually released.
            File.WriteAllText(Path.Combine(Output,"restart-data-path.txt"),Data);
            Assert(Requests==0,"Font test made a network request");
            Log.Add("Maximization/restore and zero network calls: passed; read probe is a separate sequential process.");
            Log.Add("Test used actual current executable, modal settings event/save handlers and native controls with isolated private configuration; not a live mouse-driven production-window test.");
            File.WriteAllLines(Path.Combine(Output,"font-persistence-regression.txt"),Log);return 0;
        }
        catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"font-persistence-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
