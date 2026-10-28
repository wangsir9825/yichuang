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

// All settings dialogs are opened by the real Form2 settings-button handler.
// This includes its post-modal jiluset(), which must not persist a discarded
// font preview. Configuration and logs are isolated before ClassMain is used.
class FontDiscardRegression
{
    public class FontState
    {
        public string SourceFamily,TargetFamily;
        public float SourceSize,TargetSize;
    }
    static string Data,Output;
    static Assembly App;
    static Type Config;
    static int Requests,Prompts,ModalCount;
    static readonly List<string> Log=new List<string>();
    static readonly Queue<DialogResult> Answers=new Queue<DialogResult>();
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static readonly JavaScriptSerializer Json=new JavaScriptSerializer();
    public static string Folder(Environment.SpecialFolder folder){return Data;}
    class DenyNetwork:IWebRequestCreate
    {
        public WebRequest Create(Uri uri){Interlocked.Increment(ref Requests);throw new InvalidOperationException("Unexpected font-regression network request: "+uri.Host);}
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
    static ComboBox Combo(Form settings,string name)
    {
        var pages=(IDictionary)Field(settings,"pages");Check(pages.Contains("常用设置"),"Text-display page was not created");
        var control=(ComboBox)Field(pages["常用设置"],name);Check(control!=null,"Font selector missing: "+name);return control;
    }
    static Control Page(Form settings)
    {
        var panel=(Panel)Field(settings,"content");Check(panel.Controls.Count==1,"Settings content is missing");return panel.Controls[0];
    }
    static DialogResult Prompt()
    {
        Prompts++;Check(Answers.Count>0,"Unexpected unsaved-font prompt");return Answers.Dequeue();
    }
    static void Expect(DialogResult answer,Action action,string reason)
    {
        Check(Answers.Count==0,"Unconsumed answer before "+reason);int before=Prompts;Answers.Enqueue(answer);action();Pump();
        Check(Answers.Count==0&&Prompts==before+1,"Expected exactly one prompt for "+reason);
    }
    static void NoPrompt(Action action,string reason)
    {
        int before=Prompts;action();Pump();Check(Prompts==before,"Saved/unchanged fonts prompted during "+reason);
    }
    static void ConfigurePrompts(Form settings)
    {
        var prompt=settings.GetType().GetField("EngineChangesPrompt",Private);Check(prompt!=null,"Unsaved-settings prompt hook missing");
        prompt.SetValue(settings,new Func<DialogResult>(Prompt));
        var error=settings.GetType().GetField("SaveErrorPrompt",Private);Check(error!=null,"Settings save-error hook missing");
        error.SetValue(settings,new Action<string>(message=>{throw new Exception("Unexpected isolated settings save failure: "+message);}));
    }
    static void Modal(Form main,Action<Form> action)
    {
        Exception failure=null;bool handled=false;Form opened=null;int number=ModalCount+1;
        Log.Add("Opening real settings modal "+number);
        using(var timer=new System.Windows.Forms.Timer{Interval=45})
        {
            timer.Tick+=(s,e)=>{
                Form settings=null;
                foreach(Form open in Application.OpenForms)if(open.GetType().FullName=="YiChuang.Frmsetting"){settings=open;break;}
                if(settings==null)return;
                timer.Stop();handled=true;ModalCount++;opened=settings;settings.Opacity=0;
                try
                {
                    ConfigurePrompts(settings);action(settings);
                    // Close sets DialogResult inside this Timer callback; the
                    // modal loop hides/disposes the dialog only after we return.
                    string closeState="Visible="+settings.Visible+", IsDisposed="+settings.IsDisposed+", DialogResult="+settings.DialogResult;
                    Log.Add("Modal "+number+" close state: "+closeState);
                    Check(settings.DialogResult!=DialogResult.None||!settings.Visible||settings.IsDisposed,"Modal test action did not request closing settings dialog "+number+" ("+closeState+")");
                }
                catch(Exception ex)
                {
                    failure=ex;
                    // Never leave a failing regression waiting in the modal loop.
                    try{settings.GetType().GetField("EngineChangesPrompt",Private).SetValue(settings,new Func<DialogResult>(()=>DialogResult.No));settings.Close();}catch{}
                    if(settings.Visible)settings.Dispose();
                }
            };
            timer.Start();main.GetType().GetMethod("toolStripButton1_Click",Private).Invoke(main,new object[]{null,EventArgs.Empty});
        }
        Check(handled,"The real Form2 settings handler did not open a modal dialog");
        if(failure!=null)throw failure;
        Check(opened!=null&&(opened.IsDisposed||!opened.Visible),"Real settings handler returned with modal still visible: "+number);
        Log.Add("Modal "+number+" returned through Form2 after close");
    }
    static FontState State(string sourceFamily,float sourceSize,string targetFamily,float targetSize)
    {
        return new FontState{SourceFamily=sourceFamily,SourceSize=sourceSize,TargetFamily=targetFamily,TargetSize=targetSize};
    }
    static void Choose(Form settings,FontState state)
    {
        var sourceFamily=Combo(settings,"cbb原文区字体类型");var targetFamily=Combo(settings,"cbb译文区字体类型");
        Check(sourceFamily.Items.Contains(state.SourceFamily)&&targetFamily.Items.Contains(state.TargetFamily),"Fixture font family is unavailable");
        sourceFamily.SelectedItem=state.SourceFamily;targetFamily.SelectedItem=state.TargetFamily;
        Combo(settings,"cbb原文区字体大小").SelectedValue=state.SourceSize.ToString(CultureInfo.InvariantCulture);
        Combo(settings,"cbb译文区字体大小").SelectedValue=state.TargetSize.ToString(CultureInfo.InvariantCulture);Pump();
    }
    static void Runtime(Form main,FontState expected,string reason)
    {
        Check((string)Get("originalFontType")==expected.SourceFamily&&(string)Get("translationFontType")==expected.TargetFamily,"Runtime font families differ: "+reason);
        Check(Math.Abs((float)Get("originalFontSize")-expected.SourceSize)<.05&&Math.Abs((float)Get("translationFontSize")-expected.TargetSize)<.05,"Runtime font sizes differ: "+reason);
        foreach(string name in new[]{"rtb原文","rtb译文"})
        {
            var view=(RichTextBox)Field(main,name);bool source=name=="rtb原文";string family=source?expected.SourceFamily:expected.TargetFamily;float size=source?expected.SourceSize:expected.TargetSize;
            using(var font=new Font(family,size))Check(view.Font.Name==font.Name&&Math.Abs(view.Font.SizeInPoints-size)<.1,"Actual "+name+" font differs from runtime/preview: "+reason+" ("+view.Font.Name+" "+view.Font.SizeInPoints+")");
        }
    }
    static void Selectors(Form settings,FontState expected,string reason)
    {
        Check(Combo(settings,"cbb原文区字体类型").Text==expected.SourceFamily&&Combo(settings,"cbb译文区字体类型").Text==expected.TargetFamily,"Reopened font-family selectors differ: "+reason);
        Check(Combo(settings,"cbb原文区字体大小").Text==expected.SourceSize.ToString(CultureInfo.InvariantCulture)&&Combo(settings,"cbb译文区字体大小").Text==expected.TargetSize.ToString(CultureInfo.InvariantCulture),"Reopened font-size selectors differ: "+reason);
    }
    static void Saved(FontState expected,string reason)
    {
        var values=Json.Deserialize<Dictionary<string,object>>((string)Config.GetMethod("readJiekou").Invoke(null,null));
        Check(values["originalFontType"].ToString()==expected.SourceFamily&&values["translationFontType"].ToString()==expected.TargetFamily,"Persisted font families differ after Form2 outer save: "+reason);
        Check(Math.Abs(float.Parse(values["originalFontSize"].ToString(),CultureInfo.CurrentCulture)-expected.SourceSize)<.05&&Math.Abs(float.Parse(values["translationFontSize"].ToString(),CultureInfo.CurrentCulture)-expected.TargetSize)<.05,"Persisted font sizes differ after Form2 outer save: "+reason);
    }
    static void Sidebar(Form settings,string key)
    {
        foreach(Control control in ((FlowLayoutPanel)Field(settings,"navigation")).Controls)
        {
            var button=control as Button;if(button!=null&&(string)button.Tag==key){button.PerformClick();Pump();return;}
        }
        throw new Exception("Settings sidebar item missing: "+key);
    }
    static void SaveButton(Form settings)
    {
        RootActions(settings);var save=(Button)Find(settings,"SaveSettings");NoPrompt(()=>save.PerformClick(),"explicit font save and close");
    }
    static void RootActions(Form settings)
    {
        var save=(Button)Find(settings,"SaveSettings");var apply=(Button)Find(settings,"ApplySettings");var back=(Button)Find(settings,"ReturnSettings");
        Check(save!=null&&apply!=null&&back!=null&&save.Visible&&apply.Visible&&back.Visible,"Font page is missing root settings actions");
        Check(save.Text=="保存"&&apply.Text=="应用"&&back.Text=="退出","Font page must expose save/apply/exit");
    }
    static void ExitButton(Form settings)
    {
        RootActions(settings);((Button)Find(settings,"ReturnSettings")).PerformClick();
    }
    static void RunCases(Form main,FontState initial,FontState accepted,FontState preview)
    {
        Runtime(main,initial,"isolated baseline");Saved(initial,"isolated baseline");
        Modal(main,settings=>{
            Selectors(settings,initial,"initial entry");Choose(settings,preview);Runtime(main,preview,"live font/size preview before X No");
            Expect(DialogResult.No,()=>settings.Close(),"font X discard");Runtime(main,initial,"immediate X No restoration");
        });
        Runtime(main,initial,"after modal X No and outer jiluset");Saved(initial,"X No");
        Modal(main,settings=>{Selectors(settings,initial,"reopen after X No");NoPrompt(()=>settings.Close(),"unchanged font close");});Saved(initial,"unchanged reopen");
        Log.Add("Actual modal X No restores both ClassMain and source/translation RichTextBox families/sizes; reopen and Form2 outer persistence retain baseline: passed");
        Modal(main,settings=>{
            var page=Page(settings);Choose(settings,preview);Runtime(main,preview,"live preview before cancel");
            Expect(DialogResult.Cancel,()=>settings.Close(),"font X cancel");Check(settings.Visible&&Page(settings)==page,"Cancel did not retain font page");
            Selectors(settings,preview,"X Cancel");Runtime(main,preview,"X Cancel retains live preview");Saved(initial,"X Cancel leaves accepted disk configuration");
            Expect(DialogResult.No,()=>settings.Close(),"discard after font X Cancel");
        });Runtime(main,initial,"after cancelled-then-discarded modal");Saved(initial,"Cancel followed by No");
        Modal(main,settings=>{Choose(settings,accepted);Runtime(main,accepted,"live accepted font preview");Expect(DialogResult.Yes,()=>settings.Close(),"font X save");});
        Runtime(main,accepted,"after X Yes");Saved(accepted,"X Yes");
        Log.Add("Actual modal X Cancel retains preview/page, X Yes saves both font families/sizes: passed");
        Modal(main,settings=>{
            RootActions(settings);var page=Page(settings);var apply=(Button)Find(settings,"ApplySettings");Check(!apply.Enabled,"Accepted font settings start with apply enabled");
            Choose(settings,preview);Check(apply.Enabled,"Font edit did not enable root apply");NoPrompt(()=>apply.PerformClick(),"font apply in place");
            Check(settings.Visible&&Page(settings)==page&&!apply.Enabled,"Font apply did not stay on page and accept baseline");Runtime(main,preview,"applied font preview");Saved(preview,"root font apply");
            Choose(settings,initial);Expect(DialogResult.No,()=>ExitButton(settings),"font exit discard after apply");Runtime(main,preview,"font exit restores applied baseline");
        });Runtime(main,preview,"after applied-font exit No outer save");Saved(preview,"applied-font exit No");
        Modal(main,settings=>{Choose(settings,accepted);SaveButton(settings);});Saved(accepted,"accepted fonts restored after apply test");
        Modal(main,settings=>{
            var page=Page(settings);Choose(settings,preview);Expect(DialogResult.Cancel,()=>ExitButton(settings),"font exit cancel");
            Check(settings.Visible&&Page(settings)==page,"Exit Cancel lost font page");Runtime(main,preview,"font exit Cancel retains preview");Saved(accepted,"font exit Cancel does not save");
            Expect(DialogResult.Yes,()=>ExitButton(settings),"font exit save");
        });Runtime(main,preview,"font exit Yes outer save");Saved(preview,"font exit Yes");
        Modal(main,settings=>{Choose(settings,accepted);SaveButton(settings);});Saved(accepted,"accepted fonts restored after exit test");
        Log.Add("Root font apply persists while staying on page; exit No restores last applied baseline, exit Cancel retains preview and exit Yes saves: passed");
        foreach(Size size in new[]{new Size(780,610),new Size(620,790),new Size(940,680)})
        {
            main.Size=size;Pump();Runtime(main,accepted,"resize "+size);
            Modal(main,settings=>{Selectors(settings,accepted,"resize/reopen");Runtime(main,accepted,"resize/reopen runtime");NoPrompt(()=>settings.Close(),"unchanged close after resize");});
            Saved(accepted,"resize/reopen");
        }
        main.WindowState=FormWindowState.Maximized;Pump();main.WindowState=FormWindowState.Normal;Pump();Runtime(main,accepted,"maximize/restore");
        Modal(main,settings=>{Selectors(settings,accepted,"maximized/restored reopen");Choose(settings,preview);Runtime(main,preview,"save-button live preview");SaveButton(settings);});
        Runtime(main,preview,"save button");Saved(preview,"save button");
        Modal(main,settings=>{Choose(settings,accepted);SaveButton(settings);});Saved(accepted,"explicitly restored accepted fonts");
        Log.Add("Translation 12 pt survives repeated resize/reopen/maximize instead of resetting to 9; save-and-close button persists real previews: passed");
        Modal(main,settings=>{
            Choose(settings,preview);Expect(DialogResult.No,()=>Sidebar(settings,"引擎管理"),"font sidebar discard");
            Check(Find(Page(settings),"ManagedEngines")!=null,"Sidebar No did not navigate to engine list");Runtime(main,accepted,"sidebar No restores actual fonts");
            NoPrompt(()=>settings.Close(),"close after sidebar discard");
        });Runtime(main,accepted,"after sidebar No outer save");Saved(accepted,"sidebar No");
        Modal(main,settings=>{
            var page=Page(settings);Choose(settings,preview);Expect(DialogResult.Cancel,()=>Sidebar(settings,"引擎管理"),"font sidebar cancel");
            Check(Page(settings)==page,"Sidebar Cancel changed font page");Selectors(settings,preview,"sidebar Cancel");Runtime(main,preview,"sidebar Cancel retains actual fonts");
            Expect(DialogResult.No,()=>settings.Close(),"discard sidebar-cancelled font preview");
        });Saved(accepted,"sidebar Cancel then discard");Runtime(main,accepted,"sidebar Cancel then discard");
        Modal(main,settings=>{
            Choose(settings,preview);Expect(DialogResult.Yes,()=>Sidebar(settings,"引擎管理"),"font sidebar save");
            Check(Find(Page(settings),"ManagedEngines")!=null,"Sidebar Yes did not navigate to engine list");Runtime(main,preview,"sidebar Yes retains accepted font preview");Saved(preview,"sidebar Yes");
            NoPrompt(()=>settings.Close(),"close after sidebar save");
        });Runtime(main,preview,"after sidebar Yes outer save");Saved(preview,"sidebar Yes outer save");
        Modal(main,settings=>{Choose(settings,accepted);SaveButton(settings);});Runtime(main,accepted,"final accepted fonts");Saved(accepted,"final accepted fonts");
        Log.Add("Font sidebar No/Cancel/Yes restores, retains or saves previews as chosen, including subsequent Form2 outer save: passed");
    }
    [STAThread]static int Main(string[] args)
    {
        if(args.Length!=2&&!(args.Length==4&&args[2]=="--read")){Console.Error.WriteLine("Usage: FontDiscardRegression.exe appExe outputDir [--read isolatedDataDir]");return 2;}
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,75000,Timeout.Infinite))
        try
        {
            Check(IntPtr.Size==4,"Compile/run the font regression as x86");
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string path=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(path)?Assembly.LoadFrom(path):null;};
            App=Assembly.LoadFrom(exe);bool probe=args.Length==4;
            Data=probe?Path.GetFullPath(args[3]):Path.Combine(Output,"isolated-fonts-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(FontDiscardRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,Output);
            Check(WebRequest.RegisterPrefix("http://",new DenyNetwork())&&WebRequest.RegisterPrefix("https://",new DenyNetwork()),"Network guard could not be installed");
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);Application.EnableVisualStyles();Config=App.GetType("YiChuang.ClassMain");
            FontState initial=State(FontFamily.GenericSansSerif.Name,9,FontFamily.GenericSerif.Name,9);
            FontState accepted=State(FontFamily.GenericMonospace.Name,14,FontFamily.GenericSansSerif.Name,12);
            FontState preview=State(FontFamily.GenericSerif.Name,18,FontFamily.GenericMonospace.Name,20);
            if(!probe)
            {
                Config.GetMethod("read_setSetting").Invoke(null,new[]{Config.GetMethod("readJiekou").Invoke(null,null)});
                Set("originalFontType",initial.SourceFamily);Set("originalFontSize",initial.SourceSize);Set("translationFontType",initial.TargetFamily);Set("translationFontSize",initial.TargetSize);
                Set("Master_switch","0");Set("_is自动翻译总开关",false);Set("Translation_Model","2");Config.GetMethod("jiluset").Invoke(null,null);
                Check(Get("SettingsSaveError")==null,"Could not persist isolated font baseline");
            }
            using(var main=(Form)Activator.CreateInstance(App.GetType("YiChuang.Form2")))
            {
                main.Opacity=0;main.Show();Pump();
                if(probe)
                {
                    Runtime(main,accepted,"fresh process read");Saved(accepted,"fresh process read");
                    Modal(main,settings=>{Selectors(settings,accepted,"fresh process reopened settings");NoPrompt(()=>settings.Close(),"fresh process unchanged close");});
                    Runtime(main,accepted,"fresh process outer save");Saved(accepted,"fresh process outer save");
                    Log.Add("Fresh process reloads saved source family/14 pt and translation family/12 pt in ClassMain, native RichTextBoxes and reopened modal selectors: passed");
                }
                else RunCases(main,initial,accepted,preview);
                main.Close();Pump();
            }
            Check(Requests==0,"Font regression attempted a network request");Check(Answers.Count==0,"Unused prompt choices remain");
            if(!probe)File.WriteAllText(Path.Combine(Output,"restart-data-path.txt"),Data);
            Log.Add("Zero network requests; real modal settings paths exercised: "+ModalCount+"; choice prompts: "+Prompts+"; isolated data: "+Data);
            Log.Add("No model downloads or translation requests; screenshots/OCR capture were not invoked.");
            File.WriteAllLines(Path.Combine(Output,"font-discard-regression.txt"),Log);Console.WriteLine(string.Join(Environment.NewLine,Log));return 0;
        }
        catch(Exception ex)
        {
            while(ex is TargetInvocationException&&ex.InnerException!=null)ex=ex.InnerException;
            Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(Output,"font-discard-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;
        }
    }
}
