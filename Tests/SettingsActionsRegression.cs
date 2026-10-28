using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows.Forms;

// Exercises the production settings form without starting Form2, OCR, models,
// or a network server. Every application-data lookup is redirected before
// ClassMain is initialized; the real user's configuration is never opened.
class SettingsActionsRegression
{
    static Assembly App;
    static Type Config,Local;
    static string Data,Output;
    static int NetworkRequests,Prompts;
    static readonly Queue<DialogResult> Answers=new Queue<DialogResult>();
    static readonly List<string> Log=new List<string>();
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly JavaScriptSerializer Json=new JavaScriptSerializer();

    public static string Folder(Environment.SpecialFolder folder){return Data;}
    class DenyNetwork:IWebRequestCreate
    {
        public WebRequest Create(Uri uri){Interlocked.Increment(ref NetworkRequests);throw new InvalidOperationException("Unexpected network request in settings-only test: "+uri.Host);}
    }
    static void Check(bool condition,string message){if(!condition)throw new Exception(message);}
    static object Field(object target,string name){return target.GetType().GetField(name,Private).GetValue(target);}
    static object Get(Type type,string name){return type.GetField(name).GetValue(null);}
    static void Set(Type type,string name,object value){type.GetField(name).SetValue(null,value);}
    static void Pump(){Application.DoEvents();}
    static Control Find(Control parent,string name)
    {
        if(parent.Name==name)return parent;
        foreach(Control child in parent.Controls){Control found=Find(child,name);if(found!=null)return found;}
        return null;
    }
    static Button Action(Form settings,string name){var button=Find(settings,name) as Button;Check(button!=null,"Missing settings action: "+name);return button;}
    static TextBox Text(Form settings,string name){var text=Find(settings,name) as TextBox;Check(text!=null,"Missing settings text box: "+name);return text;}
    static Panel Content(Form settings){return (Panel)Field(settings,"content");}
    static Control Page(Form settings){Check(Content(settings).Controls.Count==1,"Settings must show exactly one content page");return Content(settings).Controls[0];}
    static void Show(Form settings,string key){settings.GetType().GetMethod("ShowPage").Invoke(settings,new object[]{key});Pump();}
    static void Nav(Form settings,string key)
    {
        foreach(Control child in ((FlowLayoutPanel)Field(settings,"navigation")).Controls)
        {
            var button=child as Button;if(button!=null&&(string)button.Tag==key){button.PerformClick();Pump();return;}
        }
        throw new Exception("Navigation item not found: "+key);
    }
    static DialogResult Prompt()
    {
        Prompts++;Check(Answers.Count>0,"Unexpected unsaved-changes prompt");return Answers.Dequeue();
    }
    static void Expect(DialogResult answer,Action operation,string reason)
    {
        Check(Answers.Count==0,"Unconsumed prompt answer before "+reason);
        int before=Prompts;Answers.Enqueue(answer);operation();Pump();
        Check(Answers.Count==0&&Prompts==before+1,"Expected one unsaved-changes prompt for "+reason);
    }
    static void NoPrompt(Action operation,string reason)
    {
        Check(Answers.Count==0,"Unexpected queued answer before "+reason);int before=Prompts;operation();Pump();
        Check(Prompts==before,"Unchanged/saved settings prompted during "+reason);
    }
    static Form NewSettings()
    {
        var settings=(Form)Activator.CreateInstance(App.GetType("YiChuang.Frmsetting"));
        var callback=settings.GetType().GetField("EngineChangesPrompt",Private);Check(callback!=null,"Production prompt hook is missing");
        callback.SetValue(settings,new Func<DialogResult>(Prompt));settings.Opacity=0;settings.Show();Pump();return settings;
    }
    static Control OpenEditor(Form settings,int id)
    {
        Show(settings,"引擎管理");Control manager=Page(settings);
        var list=Find(manager,"ManagedEngines") as CheckedListBox;Check(list!=null,"Engine manager list missing");
        bool selected=false;
        for(int i=0;i<list.Items.Count;i++)if((int)list.Items[i].GetType().GetField("Id").GetValue(list.Items[i])==id){list.SelectedIndex=i;selected=true;break;}
        Check(selected,"Built-in engine missing: "+id);
        ((Button)Find(manager,"EditEngine")).PerformClick();Pump();Check(Page(settings)!=manager,"Edit did not open built-in engine: "+id);
        Check(Action(settings,"SaveSettings").Text=="保存"&&Action(settings,"ApplySettings").Text=="应用"&&Action(settings,"ReturnSettings").Text=="返回","Wrong editor action labels");
        foreach(string name in new[]{"SaveSettings","ApplySettings","ReturnSettings"})Check(Action(settings,name).Visible,"Editor action hidden: "+name);
        return Page(settings);
    }
    static void IsManager(Form settings)
    {
        Check(!settings.IsDisposed&&settings.Visible,"Navigation unexpectedly closed settings");
        Check(Find(Page(settings),"ManagedEngines")!=null,"Expected engine list after returning");
        Check(Action(settings,"SaveSettings").Text=="保存"&&Action(settings,"ApplySettings").Text=="应用"&&Action(settings,"ReturnSettings").Text=="退出","Manager lost root settings actions");
        Check(Action(settings,"ApplySettings").Visible&&Action(settings,"ReturnSettings").Visible,"Root settings actions missing in engine list");
    }
    static Dictionary<string,object> Saved()
    {
        string value=(string)Config.GetMethod("readJiekou").Invoke(null,null);return Json.Deserialize<Dictionary<string,object>>(value);
    }
    static string SavedValue(string key){var values=Saved();Check(values.ContainsKey(key),"Saved setting missing: "+key);return Convert.ToString(values[key]);}
    static void SaveValue(string key,string expected){Check(SavedValue(key)==expected,"Persisted "+key+" differs from accepted configuration");}
    static void Render(Form settings,string name)
    {
        Pump();using(var image=new Bitmap(settings.Width,settings.Height)){settings.DrawToBitmap(image,new Rectangle(Point.Empty,settings.Size));image.Save(Path.Combine(Output,name));}
    }
    static void CheckFooter(Form settings,string imageName)
    {
        foreach(string name in new[]{"SaveSettings","ApplySettings","ReturnSettings"})
        {
            var button=Action(settings,name);Check(button.Visible,"Action not visible at small size: "+name);
            Check(button.Parent.ClientRectangle.Contains(button.Bounds),"Action clipped by parent: "+name);
            Check(settings.RectangleToScreen(settings.ClientRectangle).Contains(button.RectangleToScreen(button.ClientRectangle)),"Action clipped by form: "+name);
            Check(button.Height>=TextRenderer.MeasureText(button.Text,button.Font).Height+8,"Action text clipped vertically: "+name);
            Check(button.Width>=TextRenderer.MeasureText(button.Text,button.Font).Width+12,"Action text clipped horizontally: "+name);
        }
        Check(!Action(settings,"SaveSettings").Bounds.IntersectsWith(Action(settings,"ApplySettings").Bounds),"Save overlaps apply");
        Check(!Action(settings,"ApplySettings").Bounds.IntersectsWith(Action(settings,"ReturnSettings").Bounds),"Apply overlaps return");
        Render(settings,imageName);
    }
    static void DeepSeekActions()
    {
        using(var settings=NewSettings())
        {
            var editor=OpenEditor(settings,14);var model=Text(settings,"DeepSeekModel");var key=Text(settings,"DeepSeekApiKey");var apply=Action(settings,"ApplySettings");
            Check(!apply.Enabled,"Apply initially enabled without changes");
            string original=model.Text;model.Text="draft-that-is-reverted";Pump();Check(apply.Enabled,"Changing model did not enable apply");
            model.Text=original;Pump();Check(!apply.Enabled,"Reverting model to baseline did not disable apply");
            key.Text="settings-regression-key";model.Text="accepted-apply-model";Pump();Check(apply.Enabled,"API key/model edit not tracked");
            NoPrompt(()=>apply.PerformClick(),"apply");
            Check(Page(settings)==editor&&settings.Visible,"Apply left the editor");Check(!apply.Enabled,"Apply did not accept new baseline");
            SaveValue("DeepSeekAI_model","accepted-apply-model");SaveValue("DeepSeekAI_key","settings-regression-key");
            string configPath=Path.Combine(Data,"YiChuang","setting.txt");Check(File.Exists(configPath)&&!File.ReadAllText(configPath).Contains("settings-regression-key"),"API key was written as plaintext");
            model.Text="unsaved-after-apply";key.Text="unsaved-key-after-apply";Pump();
            Expect(DialogResult.No,()=>Action(settings,"ReturnSettings").PerformClick(),"return with discard after apply");IsManager(settings);
            Check(editor.IsDisposed,"Discarded editor must be disposed to prevent stale draft restoration");
            Check((string)Get(Config,"DeepSeekAI_model")=="accepted-apply-model"&&(string)Get(Config,"DeepSeekAI_key")=="settings-regression-key","Discard did not restore last applied values");
            SaveValue("DeepSeekAI_model","accepted-apply-model");NoPrompt(()=>settings.Close(),"close after discard");
            SaveValue("DeepSeekAI_model","accepted-apply-model");SaveValue("DeepSeekAI_key","settings-regression-key");
        }
        Log.Add("DeepSeek edit/revert, encrypted apply in place, accepted baseline, discard and later close without draft leakage: passed");
        using(var settings=NewSettings())
        {
            OpenEditor(settings,14);Check(Text(settings,"DeepSeekModel").Text=="accepted-apply-model","Reopened editor lost applied model");
            Text(settings,"DeepSeekModel").Text="explicit-save-model";Pump();
            NoPrompt(()=>Action(settings,"SaveSettings").PerformClick(),"explicit save");IsManager(settings);SaveValue("DeepSeekAI_model","explicit-save-model");
            OpenEditor(settings,14);var editor=Page(settings);var picker=(ComboBox)Find(settings,"SettingsEnginePicker");object selected=picker.SelectedItem;
            Text(settings,"DeepSeekModel").Text="cancel-keeps-this-draft";Pump();
            settings.Size=settings.MinimumSize;Pump();CheckFooter(settings,"settings-engine-actions-minimum.png");
            Expect(DialogResult.Cancel,()=>Action(settings,"ReturnSettings").PerformClick(),"return cancellation");
            Check(Page(settings)==editor&&Text(settings,"DeepSeekModel").Text=="cancel-keeps-this-draft","Cancel lost draft or changed editor");
            Expect(DialogResult.Cancel,()=>Nav(settings,"引擎管理"),"engine sidebar cancellation");Check(Page(settings)==editor,"Sidebar ignored cancel");
            Expect(DialogResult.Cancel,()=>picker.SelectedIndex=0,"engine picker cancellation");
            Check(Page(settings)==editor&&picker.SelectedItem==selected,"Picker cancellation did not restore selection");
            Expect(DialogResult.Cancel,()=>settings.Close(),"window close cancellation");
            Check(!settings.IsDisposed&&settings.Visible&&Page(settings)==editor,"Window close ignored cancel");SaveValue("DeepSeekAI_model","explicit-save-model");
            Expect(DialogResult.Yes,()=>Action(settings,"ReturnSettings").PerformClick(),"return with save");IsManager(settings);SaveValue("DeepSeekAI_model","cancel-keeps-this-draft");
            OpenEditor(settings,14);NoPrompt(()=>Action(settings,"ReturnSettings").PerformClick(),"unchanged return");IsManager(settings);
            NoPrompt(()=>settings.Close(),"unchanged manager close");
        }
        Log.Add("Save returns, return Yes/No/Cancel, sidebar/picker/X cancellation, unchanged return and three unclipped footer actions at minimum size: passed");
    }
    static void NavigationDiscardAndClose()
    {
        using(var settings=NewSettings())
        {
            var editor=OpenEditor(settings,14);Text(settings,"DeepSeekModel").Text="sidebar-unsaved-model";
            Expect(DialogResult.No,()=>Nav(settings,"引擎管理"),"engine sidebar discard");IsManager(settings);Check(editor.IsDisposed,"Sidebar discard kept stale page");
            Check((string)Get(Config,"DeepSeekAI_model")=="cancel-keeps-this-draft","Sidebar discard changed accepted model");
            OpenEditor(settings,14);editor=Page(settings);Text(settings,"DeepSeekModel").Text="picker-unsaved-model";
            var picker=(ComboBox)Find(settings,"SettingsEnginePicker");
            Expect(DialogResult.No,()=>picker.SelectedIndex=0,"engine picker discard");IsManager(settings);Check(editor.IsDisposed,"Picker discard kept stale page");
            NoPrompt(()=>settings.Close(),"close after navigation discard");SaveValue("DeepSeekAI_model","cancel-keeps-this-draft");
        }
        using(var settings=NewSettings())
        {
            OpenEditor(settings,14);Text(settings,"DeepSeekModel").Text="close-unsaved-model";
            Expect(DialogResult.No,()=>settings.Close(),"window close discard");Check(settings.IsDisposed,"No on close did not close settings");
            Check((string)Get(Config,"DeepSeekAI_model")=="cancel-keeps-this-draft","Close discard leaked into runtime");SaveValue("DeepSeekAI_model","cancel-keeps-this-draft");
        }
        using(var settings=NewSettings())
        {
            OpenEditor(settings,14);Text(settings,"DeepSeekModel").Text="close-saved-model";
            Expect(DialogResult.Yes,()=>settings.Close(),"window close save");Check(settings.IsDisposed,"Yes on close did not close settings");SaveValue("DeepSeekAI_model","close-saved-model");
        }
        Log.Add("Sidebar/picker/close discard restores runtime, disposes drafts and prevents later save leakage; close Yes persists: passed");
    }
    static void OtherBuiltinActions()
    {
        using(var settings=NewSettings())
        {
            foreach(int id in new[]{6,18,19})
            {
                OpenEditor(settings,id);Check(!Action(settings,"ApplySettings").Enabled,"Unchanged built-in starts dirty: "+id);
                NoPrompt(()=>Action(settings,"ReturnSettings").PerformClick(),"unchanged built-in return "+id);IsManager(settings);
            }
            Show(settings,"常用设置");Check(Action(settings,"SaveSettings").Text=="保存"&&Action(settings,"ApplySettings").Visible&&Action(settings,"ReturnSettings").Visible&&Action(settings,"ReturnSettings").Text=="退出","Ordinary settings footer lacks save/apply/exit");
            NoPrompt(()=>settings.Close(),"ordinary settings close");
        }
        Log.Add("DeepL/Gemini/Qwen editor actions and unchanged return; root pages expose save/apply/exit: passed");
    }
    static bool SameBytes(byte[] left,byte[] right)
    {
        if(left.Length!=right.Length)return false;
        for(int i=0;i<left.Length;i++)if(left[i]!=right[i])return false;
        return true;
    }
    [STAThread]static int Main(string[] args)
    {
        if(args.Length!=2){Console.Error.WriteLine("Usage: SettingsActionsRegression.exe appExe outputDir");return 2;}
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,60000,Timeout.Infinite))
        try
        {
            Check(IntPtr.Size==4,"Run this regression as x86, matching the application");
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
            App=Assembly.LoadFrom(exe);Data=Path.Combine(Output,"isolated-settings-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(SettingsActionsRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            WebRequest.RegisterPrefix("http://",new DenyNetwork());WebRequest.RegisterPrefix("https://",new DenyNetwork());Application.EnableVisualStyles();
            Config=App.GetType("YiChuang.ClassMain");Local=App.GetType("YiChuang.LocalModelSettings");
            Config.GetMethod("read_setSetting").Invoke(null,new[]{Config.GetMethod("readJiekou").Invoke(null,null)});
            Set(Config,"FromMainHandle",IntPtr.Zero);Set(Config,"DeepSeekAI_key","");Set(Config,"DeepSeekAI_model","initial-model");
            Set(Local,"Backend","llamacpp");Set(Local,"Endpoint","http://127.0.0.1:11434");Set(Local,"HunyuanModel","test-hy:latest");Set(Local,"GemmaModel","test-gemma:latest");
            Config.GetMethod("jiluset").Invoke(null,null);Check(Get(Config,"SettingsSaveError")==null,"Isolated baseline could not be saved");
            DeepSeekActions();NavigationDiscardAndClose();OtherBuiltinActions();
            Check(NetworkRequests==0,"Settings-only regression attempted a network request");Check(Answers.Count==0,"Unused prompt choices remain");Check(Application.OpenForms.Count==0,"Test leaked a settings form");
            Log.Add("No Form2, OCR or local-model process started; no network calls; all configuration isolated at "+Data);
            Log.Add("All settings actions regression checks passed; prompts exercised: "+Prompts);
            File.WriteAllLines(Path.Combine(Output,"settings-actions-regression.txt"),Log);Console.WriteLine(string.Join(Environment.NewLine,Log));return 0;
        }
        catch(Exception ex)
        {
            while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;
            Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"settings-actions-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;
        }
    }
}
