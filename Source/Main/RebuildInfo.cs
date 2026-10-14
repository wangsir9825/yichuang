using System;
using System.IO;
using System.Windows.Forms;

internal static class RebuildInfo
{
    internal static readonly bool IsLocalBuild = true;
    internal static string DiagnosticOutput;
    internal static bool FormLoadCompleted;
    internal static void Stage(string message) {
        if(message=="Form load completed")FormLoadCompleted=true;
        if(DiagnosticOutput!=null)try{File.AppendAllText(Path.Combine(DiagnosticOutput,"startup.log"),DateTime.Now.ToString("o")+" "+message+Environment.NewLine);}catch{}
    }
    internal static void Log(Exception ex) {
        Stage(ex.ToString());
        try {
            string dir=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),"YiChuang","logs");
            Directory.CreateDirectory(dir);File.AppendAllText(Path.Combine(dir,"rebuild-errors.log"),DateTime.Now.ToString("o")+" "+ex+Environment.NewLine);
        }catch{}
    }
    internal static void Initialize() {
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        Application.ThreadException += (s,e) => {Log(e.Exception);if(DiagnosticOutput==null)MessageBox.Show(e.Exception.Message,"重建试用版遇到错误");};
        AppDomain.CurrentDomain.UnhandledException += (s,e) => {Log(e.ExceptionObject as Exception ?? new Exception(e.ExceptionObject.ToString()));};
    }
    internal static void PrepareStartupCheck(Form form) {
        form.Opacity=0;
        Stage("Startup check begins");
        var watchdog=new System.Threading.Timer(s=>{Stage("Watchdog timeout");Environment.Exit(124);},null,20000,System.Threading.Timeout.Infinite);
        var timer=new System.Windows.Forms.Timer();timer.Interval=4000;
        timer.Tick += (s,e) => {
            timer.Stop();
            try {
                Stage("Main title="+form.Text+" visible="+form.Visible+" controls="+form.Controls.Count);
                foreach(Form open in Application.OpenForms)Stage("Open form="+open.GetType().FullName+" title="+open.Text);
                using(Form settings=new YiChuang.Frmsetting()) {
                    settings.Opacity=0;settings.Show();Application.DoEvents();
                    using(var bitmap=new System.Drawing.Bitmap(settings.Width,settings.Height)) {
                        settings.DrawToBitmap(bitmap,new System.Drawing.Rectangle(0,0,bitmap.Width,bitmap.Height));
                        bitmap.Save(Path.Combine(DiagnosticOutput,"settings.png"),System.Drawing.Imaging.ImageFormat.Png);
                    }
                    Stage("Settings form displayed: "+settings.Text+" children="+settings.Controls.Count);
                    settings.Close();
                }
                using(var bmp=new System.Drawing.Bitmap(form.Width,form.Height)) {
                    form.DrawToBitmap(bmp,new System.Drawing.Rectangle(0,0,bmp.Width,bmp.Height));
                    bmp.Save(Path.Combine(DiagnosticOutput,"startup-main.png"),System.Drawing.Imaging.ImageFormat.Png);
                }
                File.WriteAllText(Path.Combine(DiagnosticOutput,"startup-result.txt"),"Title="+form.Text+"\nVisible="+form.Visible+"\nControls="+form.Controls.Count+"\nFormLoadCompleted="+FormLoadCompleted+"\n");
                Stage("Startup check finished");
                watchdog.Dispose();Environment.Exit(FormLoadCompleted ? 0 : 1);
            } catch(Exception ex) {Log(ex);Environment.Exit(1);}
        };
        timer.Start();
    }
}
