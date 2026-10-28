using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// Settings-only functional and minimum-window coverage. The real Form2 modal
// font-preview/outer-save path is covered separately by FontDiscardRegression.
class SettingsRootActionsRegression
{
    static string Data,Output;
    static Assembly App;
    static Type Config;
    static int Requests,Prompts;
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly JavaScriptSerializer Json=new JavaScriptSerializer();
    static readonly Queue<DialogResult> Answers=new Queue<DialogResult>();
    static readonly List<string> Log=new List<string>();
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    class DenyNetwork:IWebRequestCreate
    {
        public WebRequest Create(Uri uri){Interlocked.Increment(ref Requests);throw new InvalidOperationException("Unexpected root-settings network request: "+uri.Host);}
    }
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    static object Field(object target,string name){return target.GetType().GetField(name,Private).GetValue(target);}
    static object Get(string name){return Config.GetField(name).GetValue(null);}
    static void Set(string name,object value){Config.GetField(name).SetValue(null,value);}
    static void Pump(){Application.DoEvents();}
    static Control Find(Control parent,string name)
    {
        if(parent.Name==name)return parent;
        foreach(Control child in parent.Controls){Control found=Find(child,name);if(found!=null)return found;}
        return null;
    }
    static Button Button(Form settings,string name)
    {
        var button=Find(settings,name) as Button;Check(button!=null,"Missing root settings action: "+name);return button;
    }
    static Control Page(Form settings)
    {
        var content=(Panel)Field(settings,"content");Check(content.Controls.Count==1,"Settings page is empty");return content.Controls[0];
    }
    static ComboBox FontSize(Form settings)
    {
        var pages=(IDictionary)Field(settings,"pages");Check(pages.Contains("常用设置"),"Font page missing");return (ComboBox)Field(pages["常用设置"],"cbb译文区字体大小");
    }
    static void Choose(Form settings,int size){FontSize(settings).SelectedValue=size.ToString(CultureInfo.InvariantCulture);Pump();Check((float)Get("translationFontSize")==size,"Font choice did not update live settings");}
    static DialogResult Prompt(){Prompts++;Check(Answers.Count>0,"Unexpected unsaved root-settings prompt");return Answers.Dequeue();}
    static void Expect(DialogResult answer,Action action,string reason)
    {
        Check(Answers.Count==0,"Unconsumed answer before "+reason);int before=Prompts;Answers.Enqueue(answer);action();Pump();Check(Answers.Count==0&&Prompts==before+1,"Expected one root-settings prompt for "+reason);
    }
    static void NoPrompt(Action action,string reason){int before=Prompts;action();Pump();Check(Prompts==before,"Unexpected prompt for "+reason);}
    static Form Settings()
    {
        var settings=(Form)Activator.CreateInstance(App.GetType("YiChuang.Frmsetting"));
        var prompt=settings.GetType().GetField("EngineChangesPrompt",Private);var error=settings.GetType().GetField("SaveErrorPrompt",Private);
        Check(prompt!=null&&error!=null,"Settings dialog hooks missing");prompt.SetValue(settings,new Func<DialogResult>(Prompt));
        error.SetValue(settings,new Action<string>(message=>{throw new Exception("Unexpected isolated root-settings save failure: "+message);}));
        settings.Opacity=0;settings.Show();Pump();
        Check(!settings.MinimizeBox&&!settings.MaximizeBox&&settings.ControlBox,"Settings caption must have close only");
        Check(settings.FormBorderStyle==FormBorderStyle.Sizable,"Settings must retain a resizable border");
        return settings;
    }
    static void RootFooter(Form settings)
    {
        Check(Button(settings,"SaveSettings").Text=="保存"&&Button(settings,"ApplySettings").Text=="应用"&&Button(settings,"ReturnSettings").Text=="退出","Root footer labels must be save/apply/exit");
        foreach(string name in new[]{"SaveSettings","ApplySettings","ReturnSettings"})Check(Button(settings,name).Visible,"Root footer action hidden: "+name);
    }
    static void Saved(int size,string reason)
    {
        var settings=Json.Deserialize<Dictionary<string,object>>((string)Config.GetMethod("readJiekou").Invoke(null,null));
        Check(Math.Abs(float.Parse(settings["translationFontSize"].ToString(),CultureInfo.CurrentCulture)-size)<.05,"Saved translation font differs for "+reason);
    }
    static void FunctionalActions()
    {
        using(var settings=Settings())
        {
            RootFooter(settings);var page=Page(settings);var apply=Button(settings,"ApplySettings");
            Check(!apply.Enabled,"Apply initially enabled without changes");Choose(settings,12);Check(apply.Enabled,"Changed root page did not enable apply");
            NoPrompt(()=>apply.PerformClick(),"root apply");Check(settings.Visible&&!settings.IsDisposed&&Page(settings)==page,"Root apply left current settings page");
            Check(!apply.Enabled,"Root apply did not accept baseline");Saved(12,"root apply");
            Choose(settings,14);Check(apply.Enabled,"Further root changes did not enable apply");
            Expect(DialogResult.No,()=>Button(settings,"ReturnSettings").PerformClick(),"root exit discard after apply");
            Check(settings.IsDisposed,"Root exit No did not close settings");Check((float)Get("translationFontSize")==12,"Root exit No did not restore last applied baseline");Saved(12,"exit No");
        }
        using(var settings=Settings())
        {
            RootFooter(settings);Check(FontSize(settings).Text=="12","Reopen lost applied root setting");var page=Page(settings);Choose(settings,14);
            Expect(DialogResult.Cancel,()=>Button(settings,"ReturnSettings").PerformClick(),"root exit cancel");
            Check(!settings.IsDisposed&&settings.Visible&&Page(settings)==page&&FontSize(settings).Text=="14","Root exit Cancel did not retain page and draft");
            Check((float)Get("translationFontSize")==14&&Button(settings,"ApplySettings").Enabled,"Root exit Cancel lost preview/dirty state");Saved(12,"exit Cancel");
            Expect(DialogResult.Yes,()=>Button(settings,"ReturnSettings").PerformClick(),"root exit save");Check(settings.IsDisposed,"Root exit Yes did not close settings");Saved(14,"exit Yes");
        }
        using(var settings=Settings())
        {
            RootFooter(settings);Choose(settings,16);NoPrompt(()=>Button(settings,"SaveSettings").PerformClick(),"root explicit save");
            Check(settings.IsDisposed,"Root save did not close settings");Saved(16,"root save");
        }
        using(var settings=Settings())
        {
            RootFooter(settings);Check(FontSize(settings).Text=="16","Root save value lost on reopen");
            Choose(settings,18);Choose(settings,16);Check(!Button(settings,"ApplySettings").Enabled,"Reverting root draft did not disable apply");
            NoPrompt(()=>Button(settings,"ReturnSettings").PerformClick(),"root unchanged exit");Check(settings.IsDisposed,"Unchanged root exit did not close settings");Saved(16,"unchanged exit");
        }
        Log.Add("Root save closes, apply persists in place, exit No restores applied baseline, Cancel retains draft, Yes saves, unchanged/reverted exit skips prompt: passed");
    }
    static void Geometry()
    {
        using(var settings=Settings())
        {
            var keys=new List<string>();foreach(Control control in ((FlowLayoutPanel)Field(settings,"navigation")).Controls)keys.Add((string)control.Tag);
            Check(keys.Count==6,"Expected six root settings pages");int index=0;
            foreach(string key in keys)
            {
                settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{key});Pump();RootFooter(settings);
                settings.Size=settings.MinimumSize;Pump();
                var bounds=new List<Rectangle>();
                foreach(string name in new[]{"SaveSettings","ApplySettings","ReturnSettings"})
                {
                    var button=Button(settings,name);Check(button.Parent.ClientRectangle.Contains(button.Bounds),"Root action clipped by footer at minimum size: "+key+" "+name);
                    Check(settings.RectangleToScreen(settings.ClientRectangle).Contains(button.RectangleToScreen(button.ClientRectangle)),"Root action outside minimum client area: "+key+" "+name);
                    var text=TextRenderer.MeasureText(button.Text,button.Font);Check(button.Width>=text.Width+12&&button.Height>=text.Height+8,"Root action text clipped at minimum size: "+key+" "+name);
                    var rectangle=button.RectangleToScreen(button.ClientRectangle);foreach(var prior in bounds)Check(!prior.IntersectsWith(rectangle),"Root footer actions overlap: "+key);bounds.Add(rectangle);
                }
                using(var image=new Bitmap(settings.Width,settings.Height)){settings.DrawToBitmap(image,new Rectangle(Point.Empty,settings.Size));image.Save(Path.Combine(Output,"settings-root-minimum-"+(index++)+".png"));}
            }
            // This geometry-only instance is disposed without applying its
            // untouched shortcut page, so no global hotkey registration occurs.
        }
        Log.Add("All six root pages display save/apply/exit with no overlap or clipping at the dialog minimum size; native bitmap evidence saved: passed");
    }
    static void Show(Form settings,string key){settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{key});Pump();RootFooter(settings);}
    static object Registry(string method,params object[] values){return App.GetType("YiChuang.EngineRegistry").GetMethod(method).Invoke(null,values);}
    static string SavedText(string name)
    {
        var saved=Json.Deserialize<Dictionary<string,object>>((string)Config.GetMethod("readJiekou").Invoke(null,null));Check(saved.ContainsKey(name),"Missing saved setting: "+name);return Convert.ToString(saved[name]);
    }
    static void Persist(){Config.GetMethod("jiluset").Invoke(null,null);Check(Get("SettingsSaveError")==null,"Could not save isolated semantics fixture");}
    static void Visibility(Form settings,int id,bool visible)
    {
        var list=(CheckedListBox)Find(settings,"ManagedEngines");Check(list!=null,"Engine checkbox list is missing");
        for(int i=0;i<list.Items.Count;i++)
        {
            if((int)list.Items[i].GetType().GetField("Id").GetValue(list.Items[i])!=id)continue;
            list.SelectedIndex=i;list.SetItemChecked(i,visible);Pump();
            Check((bool)Registry("IsVisible",id)==visible,"Engine checkbox did not change actual registry: "+id);return;
        }
        throw new Exception("Engine row not found: "+id);
    }
    static void EngineManagerSemantics()
    {
        Registry("Import",new object[]{null});Set("isjiekounum",4);Persist();string original=(string)Registry("Export");
        using(var settings=Settings())
        {
            Show(settings,"引擎管理");Check(!Button(settings,"ApplySettings").Enabled,"Unchanged engine manager starts dirty");
            Visibility(settings,4,false);Check((int)Get("isjiekounum")==0,"Hiding selected engine did not clear actual selected engine");
            Check(Button(settings,"ApplySettings").Enabled,"Engine visibility edit did not enable root Apply");
            Expect(DialogResult.No,()=>Button(settings,"ReturnSettings").PerformClick(),"engine manager selected-engine discard");
            Check(settings.IsDisposed&&(string)Registry("Export")==original&&(int)Get("isjiekounum")==4,"Engine manager discard did not restore registry and selected engine");
            Check(SavedText("EngineRegistry")==original&&SavedText("Translation_interface")=="4","Engine manager discard leaked through close persistence");
        }
        string accepted;
        using(var settings=Settings())
        {
            Show(settings,"引擎管理");var page=Page(settings);Visibility(settings,4,false);
            NoPrompt(()=>Button(settings,"ApplySettings").PerformClick(),"engine manager apply");accepted=(string)Registry("Export");
            Check(settings.Visible&&Page(settings)==page&&!Button(settings,"ApplySettings").Enabled,"Engine Apply did not retain page and accept baseline");
            Check((int)Get("isjiekounum")==0&&SavedText("EngineRegistry")==accepted&&SavedText("Translation_interface")=="0","Engine Apply did not persist registry and actual selected engine");
            Visibility(settings,5,false);Check(Button(settings,"ApplySettings").Enabled,"Further engine edit was not dirty");
            Expect(DialogResult.No,()=>Button(settings,"ReturnSettings").PerformClick(),"engine manager discard after apply");
            Check(settings.IsDisposed&&(string)Registry("Export")==accepted&&(int)Get("isjiekounum")==0,"Engine discard after Apply did not restore accepted state");
            Check(SavedText("EngineRegistry")==accepted&&SavedText("Translation_interface")=="0","Engine discard after Apply changed accepted disk state");
        }
        Log.Add("Engine root checkbox edits track visibility/selected-engine changes; exit No restores registry and actual selection, Apply stays/persists and later discard restores accepted baseline: passed");
    }
    static void BackupPathSemantics()
    {
        string baseline=Path.Combine(Data,"backup-baseline"),accepted=Path.Combine(Data,"backup-applied"),draft=Path.Combine(Data,"backup-draft");
        Directory.CreateDirectory(baseline);Directory.CreateDirectory(accepted);Directory.CreateDirectory(draft);Set("Backup_Path",baseline);Persist();
        using(var settings=Settings())
        {
            Show(settings,"配置备份设置");var page=Page(settings);var path=(TextBox)Find(settings,"BackupPath");Check(path!=null&&path.Text==baseline,"Backup path control lost baseline");
            path.Text=accepted;Pump();Check((string)Get("Backup_Path")==accepted&&Button(settings,"ApplySettings").Enabled,"Backup path edit did not update draft state");
            NoPrompt(()=>Button(settings,"ApplySettings").PerformClick(),"backup path apply");
            Check(settings.Visible&&Page(settings)==page&&!Button(settings,"ApplySettings").Enabled&&SavedText("Backup_Path")==accepted,"Backup path Apply did not persist in place");
            path.Text=draft;Pump();Expect(DialogResult.No,()=>Button(settings,"ReturnSettings").PerformClick(),"backup path discard after apply");
            Check(settings.IsDisposed&&(string)Get("Backup_Path")==accepted&&SavedText("Backup_Path")==accepted,"Backup exit No did not restore accepted path");
        }
        using(var settings=Settings())
        {
            Show(settings,"配置备份设置");Check(((TextBox)Find(settings,"BackupPath")).Text==accepted,"Reopened backup page lost applied path");
            NoPrompt(()=>Button(settings,"ReturnSettings").PerformClick(),"unchanged backup exit");
        }
        Log.Add("Backup root path edit enables Apply, persists without leaving page, exit No restores last applied path and reopened control: passed");
    }
    static void ShortcutDraftSemantics()
    {
        string display="Ctrl + Shift + F8",translate="Ctrl + Shift + F9";
        var preference=App.GetType("YiChuang.UiPreferences").GetField("TranslateShortcut",BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic);
        Set("快捷键隐藏显示",display);preference.SetValue(null,translate);Set("FromMainHandle",IntPtr.Zero);Persist();
        using(var settings=Settings())
        {
            Show(settings,"快捷键");var show=(TextBox)Find(settings,"ShowHideShortcut");var capture=(TextBox)Find(settings,"TranslateShortcut");
            Check(show.Text==display&&capture.Text==translate&&!Button(settings,"ApplySettings").Enabled,"Shortcut page did not start at accepted live values");
            show.Text="Ctrl + Alt + F10";capture.Text="Ctrl + Alt + F11";Pump();
            Check(Button(settings,"ApplySettings").Enabled,"Shortcut text draft did not enable root Apply");
            Check((string)Get("快捷键隐藏显示")==display&&(string)preference.GetValue(null)==translate,"Editing shortcut draft registered/applied it before save");
            // No main handle exists in this process. An attempted registration
            // would fail rather than acquiring another application's hotkeys.
            Expect(DialogResult.No,()=>Button(settings,"ReturnSettings").PerformClick(),"shortcut root draft discard");
            Check(settings.IsDisposed,"Shortcut No could not exit without attempting unnecessary hotkey registration");
            Check((string)Get("快捷键隐藏显示")==display&&(string)preference.GetValue(null)==translate,"Shortcut discard did not restore both live values");
            Check(SavedText("ShortcutkeyHiddenDisplay")==display&&SavedText("TranslateShortcut")==translate,"Shortcut discard leaked draft into protected settings");
        }
        using(var settings=Settings())
        {
            Show(settings,"快捷键");Check(((TextBox)Find(settings,"ShowHideShortcut")).Text==display&&((TextBox)Find(settings,"TranslateShortcut")).Text==translate,"Reopened shortcut controls did not restore accepted values");
            Check(!Button(settings,"ApplySettings").Enabled,"Restored shortcuts remain dirty");NoPrompt(()=>Button(settings,"ReturnSettings").PerformClick(),"unchanged shortcuts exit");
        }
        Check((IntPtr)Get("FromMainHandle")==IntPtr.Zero,"Shortcut semantics unexpectedly created a main handle");
        Log.Add("Shortcut drafts enable Apply while live bindings stay unchanged; exit No restores live fields and reopened controls without registering global hotkeys: passed");
    }
    [STAThread]static int Main(string[] args)
    {
        if(args.Length!=2){Console.Error.WriteLine("Usage: SettingsRootActionsRegression.exe appExe outputDir");return 2;}
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,45000,Timeout.Infinite))
        try
        {
            Check(IntPtr.Size==4,"Root settings regression must run as x86");string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
            App=Assembly.LoadFrom(exe);Data=Path.Combine(Output,"isolated-root-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(SettingsRootActionsRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            Check(WebRequest.RegisterPrefix("http://",new DenyNetwork())&&WebRequest.RegisterPrefix("https://",new DenyNetwork()),"Network guard installation failed");Application.EnableVisualStyles();
            Config=App.GetType("YiChuang.ClassMain");Config.GetMethod("read_setSetting").Invoke(null,new[]{Config.GetMethod("readJiekou").Invoke(null,null)});
            Set("FromMainHandle",IntPtr.Zero);Set("translationFontSize",9f);Config.GetMethod("jiluset").Invoke(null,null);Check(Get("SettingsSaveError")==null,"Isolated baseline save failed");
            FunctionalActions();Geometry();EngineManagerSemantics();BackupPathSemantics();ShortcutDraftSemantics();Check(Requests==0,"Root settings test attempted network access");Check(Answers.Count==0,"Unused prompt choices remain");Check(Application.OpenForms.Count==0,"Root settings regression leaked a form");
            Log.Add("All root settings regression checks passed; prompts: "+Prompts+"; no Form2/OCR/model process started, no network calls; isolated data: "+Data);
            File.WriteAllLines(Path.Combine(Output,"settings-root-actions-regression.txt"),Log);Console.WriteLine(string.Join(Environment.NewLine,Log));return 0;
        }
        catch(Exception ex)
        {
            while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"settings-root-actions-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;
        }
    }
}
