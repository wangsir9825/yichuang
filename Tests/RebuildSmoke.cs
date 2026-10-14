using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

class RebuildSmoke
{
    [STAThread]
    static int Main(string[] args) {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        List<string> log=new List<string>();int errors=0;
        try {
            string path=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(path));
            Assembly a=Assembly.LoadFile(path);
            a.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,output);
            object[] proxies=(object[])a.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            int count=0;foreach(object proxy in proxies)if(proxy is Delegate)count++;
            log.Add("Proxy count="+count);
            a.GetType("Xtranslator.Program").GetField("ProgramStarted").SetValue(null,new System.Threading.EventWaitHandle(false,System.Threading.EventResetMode.AutoReset));
            Application.EnableVisualStyles();
            foreach(Type t in a.GetTypes()) {
                if(!typeof(UserControl).IsAssignableFrom(t)||t.IsAbstract||t.GetConstructor(Type.EmptyTypes)==null)continue;
                try {
                    using(Control control=(Control)Activator.CreateInstance(t)) {control.CreateControl();log.Add("Control OK: "+t.FullName+" children="+control.Controls.Count);}
                } catch(Exception ex) {errors++;log.Add("Control FAIL: "+t.FullName+" "+ex);}
            }
            // Create the main form without displaying it or triggering its Load handler.
            try {
                using(Form form=(Form)Activator.CreateInstance(a.GetType("Xtranslator.Form2"))) {
                    log.Add("Main form OK: controls="+form.Controls.Count+" size="+form.Size);
                    File.WriteAllLines(Path.Combine(output,"smoke.log"),log.ToArray());
                    if(Path.GetFileName(Path.GetDirectoryName(output))=="AnalysisRun") {
                        form.Opacity=0;
                        using(System.Threading.Timer watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,15000,System.Threading.Timeout.Infinite)) {
                            form.Show();Application.DoEvents();
                            log.Add("Main form Load completed: title="+form.Text+" visible="+form.Visible);
                            for(int tick=0;tick<20;tick++){Application.DoEvents();System.Threading.Thread.Sleep(100);}
                        }
                    }
                    using(System.Drawing.Bitmap bmp=new System.Drawing.Bitmap(form.Width,form.Height)) {
                        form.DrawToBitmap(bmp,new System.Drawing.Rectangle(0,0,bmp.Width,bmp.Height));
                        bmp.Save(Path.Combine(output,"main-form.png"),System.Drawing.Imaging.ImageFormat.Png);
                    }
                }
            } catch(Exception ex) {errors++;log.Add("Main form FAIL: "+ex);}
        }catch(Exception ex){errors++;log.Add(ex.ToString());}
        log.Add("Errors="+errors);File.WriteAllLines(Path.Combine(output,"smoke.log"),log.ToArray());
        Environment.Exit(errors==0?0:1);return errors==0?0:1;
    }
}
