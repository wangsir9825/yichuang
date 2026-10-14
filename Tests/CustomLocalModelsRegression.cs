using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

class CustomLocalModelsRegression
{
    static Assembly App;static string Data,Output;const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance|BindingFlags.Static;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static Type T(string n){return App.GetType("YiChuang."+n);}
    static object Field(object o,string n){return o.GetType().GetField(n,F).GetValue(o);}
    static object Reg(string n,params object[] a){return T("EngineRegistry").GetMethod(n,F).Invoke(null,a);}
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static void Pump(){Application.DoEvents();Thread.Sleep(30);Application.DoEvents();}
    static Control Control(Control p,string n){return p.Controls.Find(n,true).Single();}
    static int Id(object e){return (int)Field(e,"Id");}
    static int Find(CheckedListBox list,string name){for(int i=0;i<list.Items.Count;i++)if((string)Field(list.Items[i],"Name")==name)return i;throw new Exception("Missing model: "+name);}
    static void Dialog(Control manager,string button,Action<Form> edit)
    {
        Exception failure=null;bool visited=false;
        using(var timer=new System.Windows.Forms.Timer{Interval=30}){
            timer.Tick+=(s,e)=>{var form=Application.OpenForms.Cast<Form>().FirstOrDefault(f=>f.GetType()==T("EngineEditorForm"));if(form==null)return;timer.Stop();try{visited=true;edit(form);}catch(Exception ex){failure=ex;form.Close();}};
            timer.Start();((Button)Control(manager,button)).PerformClick();
        }
        if(failure!=null)throw failure;Check(visited,"Editor did not open from manager button");Pump();
    }
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var watch=new System.Threading.Timer(s=>Environment.Exit(124),null,60000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]),dir=Path.GetDirectoryName(exe);Directory.SetCurrentDirectory(dir);
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(dir,new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};App=Assembly.LoadFrom(exe);
            Data=args.Length>3?args[3]:Path.Combine(Output,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(CustomLocalModelsRegression).GetMethod("Folder"));
            if(args.Length>3){
                T("ClassMain").GetMethod("read_setSetting").Invoke(null,new[]{T("ClassMain").GetMethod("readJiekou").Invoke(null,null)});
                var active=(int[])T("EngineRegistry").GetProperty("ActiveIds").GetValue(null,null);var a=active.Select(i=>Reg("Get",i)).Single(e=>(string)Field(e,"Name")=="Renamed GGUF");var b=active.Select(i=>Reg("Get",i)).Single(e=>(string)Field(e,"Name")=="My Ollama");
                Check((string)Field(a,"Provider")=="llamacpp"&&(string)Field(a,"Prompt")=="generic"&&(int)Field(a,"TimeoutSeconds")==240,"GGUF configuration lost across restart");
                Check(!(bool)Reg("IsVisible",Id(a))&&(bool)Reg("IsVisible",Id(b)),"Visibility lost across restart");
                Check((string)Field(b,"Model")=="my-translation:latest"&&(string)Field(b,"Provider")=="ollama","Ollama definition lost");
                Check(Array.IndexOf(active,Id(a))<Array.IndexOf(active,17),"Custom order lost across restart");
                File.WriteAllText(Path.Combine(Output,"restart-passed.txt"),"GGUF/Ollama names, providers, paths, template, timeout, visibility and ordering persist in a fresh process.");return 0;
            }
            Reg("Import",new object[]{null});Application.EnableVisualStyles();string file=Path.Combine(Data,"user-model.gguf");using(var w=new BinaryWriter(File.Create(file))){w.Write(0x46554747u);w.Write(3u);w.Write(0ul);w.Write(0ul);}
            var cloud=Activator.CreateInstance(T("EngineDefinition"));T("EngineDefinition").GetField("Name").SetValue(cloud,"Unrelated online");T("EngineDefinition").GetField("Endpoint").SetValue(cloud,"https://example.com/v1");T("EngineDefinition").GetField("Model").SetValue(cloud,"model");int cloudId=(int)Reg("Save",cloud);
            using(var owner=new Form{TopMost=true})using(var wizard=(Form)Activator.CreateInstance(T("LocalSetupWizard"),true)){
                owner.Show();wizard.Show(owner);Pump();var pages=(TabControl)Field(wizard,"pages");var manager=(Control)Field(wizard,"manager");var list=(CheckedListBox)Control(manager,"ManagedEngines");
                Check(pages.SelectedIndex==0&&manager.Visible&&list.Items.Count==2,"Local manager must be the default and exclude online engines");
                Check(!((Button)Field(wizard,"prepare")).Visible,"Preset-only download button on custom page");
                Dialog(manager,"AddEngine",form=>{
                    Check(form.TopMost,"Editor obscured by topmost wizard");var provider=(ComboBox)Control(form,"EngineProvider");Check(provider.Items.Count==2&&provider.SelectedItem.ToString().Contains("llama.cpp"),"Local editor default/provider scope wrong");
                    Check(!Control(form,"EngineApiKey").Visible&&!Control(form,"EngineEndpoint").Visible,"GGUF asks for a cloud key/address");
                    Control(form,"EngineName").Text="Custom GGUF";Control(form,"EngineModel").Text=Path.Combine(Data,"missing.gguf");((Button)Control(form,"SaveEngine")).PerformClick();
                    Check(!form.IsDisposed&&Control(form,"EngineEditorStatus").Text.Contains("未找到"),"Missing GGUF accepted without explanation");
                    Control(form,"EngineModel").Text=file;((Button)Control(form,"SaveEngine")).PerformClick();
                });
                Check(list.Items.Count==3,"Third model was not added");int index=Find(list,"Custom GGUF"),custom=Id(list.Items[index]);Check((bool)Reg("IsVisible",custom),"Added model not visible");
                list.SelectedIndex=index;
                Dialog(manager,"EditEngine",form=>{Check(Control(form,"EngineModel").Text==file,"Editor lost path");Control(form,"EngineName").Text="Renamed GGUF";((NumericUpDown)Control(form,"EngineTimeout")).Value=240;((Button)Control(form,"SaveEngine")).PerformClick();});
                Check(Id(list.Items[Find(list,"Renamed GGUF")])==custom,"Edit replaced stable ID");
                list.SelectedIndex=Find(list,"Renamed GGUF");Dialog(manager,"EditEngine",form=>{Control(form,"EngineName").Text="Canceled rename";((Button)Control(form,"CancelEngineEdit")).PerformClick();});Check(Find(list,"Renamed GGUF")>=0,"Canceled edit mutated registry");
                ((Button)Control(manager,"MoveEngineUp")).PerformClick();Pump();Check(list.SelectedIndex==1,"Move must skip interleaved online entries");Check(Reg("Get",cloudId)!=null,"Sorting lost unrelated online entry");
                list.SetItemChecked(Find(list,"Renamed GGUF"),false);Pump();Check(!(bool)Reg("IsVisible",custom),"Visibility did not apply");
                Dialog(manager,"AddEngine",form=>{((ComboBox)Control(form,"EngineProvider")).SelectedIndex=1;Control(form,"EngineName").Text="My Ollama";Control(form,"EngineModel").Text="my-translation:latest";((Button)Control(form,"SaveEngine")).PerformClick();});
                Check(list.Items.Count==4&&Reg("Get",cloudId)!=null,"Arbitrary Ollama model or online preservation failed");
                list.SelectedIndex=Find(list,"My Ollama");Dialog(manager,"EditEngine",form=>{Check(((ComboBox)Control(form,"EngineProvider")).SelectedItem.ToString().Contains("Ollama"),"Ollama provider lost");((Button)Control(form,"CancelEngineEdit")).PerformClick();});
                // Removal of a separate temporary entry must not remove the GGUF shared by the retained entry.
                var temp=Activator.CreateInstance(T("EngineDefinition"));T("EngineDefinition").GetField("Name").SetValue(temp,"Remove only entry");T("EngineDefinition").GetField("Provider").SetValue(temp,"llamacpp");T("EngineDefinition").GetField("Model").SetValue(temp,file);int tempId=(int)Reg("Save",temp);Reg("Remove",tempId);Check(File.Exists(file)&&Reg("Get",custom)!=null,"Removing model entry deleted weights or another entry");
                pages.SelectedIndex=1;Pump();Check(((Button)Field(wizard,"prepare")).Visible,"Preset preparation disappeared");pages.SelectedIndex=0;
                var done=(Button)Field(wizard,"done");Check(done.Visible&&done.Parent.ClientRectangle.Contains(done.Bounds),"Save footer clipped");done.PerformClick();Check(wizard.IsDisposed,"Save did not close wizard");
            }
            File.WriteAllText(Path.Combine(Output,"restart-data-path.txt"),Data);File.WriteAllText(Path.Combine(Output,"result.txt"),"Default local manager; actual add/edit/cancel dialogs; arbitrary GGUF/Ollama entries; missing-file explanation; filtered sorting; visibility; removal preserves files; preset page and save footer: passed.");Console.WriteLine("Custom local model manager regression passed.");return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(Output,"error.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
