using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

class LocalSimplifyRegression
{
    static Assembly App;static string Data,Output;
    const BindingFlags F=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    static Type T(string n){return App.GetType("YiChuang."+n);}
    static object Call(string type,string name,params object[] values){return T(type).GetMethod(name,F).Invoke(null,values);}
    static object Get(object o,string name){return o.GetType().GetField(name,F).GetValue(o);}
    static void Check(bool b,string message){if(!b)throw new Exception(message);}
    static void Prompt(Form f,DialogResult result){f.GetType().GetField("ChangesPrompt",F).SetValue(f,(Func<DialogResult>)(()=>result));}
    static Form Wizard(){var f=(Form)Activator.CreateInstance(T("LocalSetupWizard"),true);f.Opacity=0;f.Show();Application.DoEvents();return f;}
    class Guard:IWebRequestCreate{public WebRequest Create(Uri u){throw new Exception("Unexpected network: "+u.Host);}}
    [STAThread]static int Main(string[] args)
    {
        Output=Path.GetFullPath(args[1]);Directory.CreateDirectory(Output);
        using(var guard=new System.Threading.Timer(_=>Environment.Exit(124),null,30000,Timeout.Infinite))try{
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};App=Assembly.LoadFrom(exe);
            Data=Path.Combine(Output,"data");Directory.CreateDirectory(Data);
            var delegates=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);delegates[318]=Delegate.CreateDelegate(delegates[318].GetType(),typeof(LocalSimplifyRegression).GetMethod("Folder"));
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());
            if(args.Contains("--startup")||args.Contains("--explicit-setup")){
                bool explicitSetup=args.Contains("--explicit-setup"),seen=false,sawSetup=false;int ticks=0;
                using(var timer=new System.Windows.Forms.Timer{Interval=100}){
                    timer.Tick+=(s,e)=>{
                        ticks++;var forms=Application.OpenForms.Cast<Form>().ToArray();
                        foreach(var form in forms)if(form.GetType()==T("LocalSetupWizard")){sawSetup=true;form.Close();}
                        var main=forms.FirstOrDefault(f=>f.GetType()==T("Form2"));if(main!=null){seen=true;if(ticks>=10)main.Close();}
                    };Application.Idle+=(s,e)=>{if(!timer.Enabled)timer.Start();};T("Program").GetMethod("Main",F).Invoke(null,new object[]{explicitSetup?new[]{"--setup"}:new string[0]});
                }
                Check(seen,"Application did not open");Check(sawSetup==explicitSetup,"Automatic popup / explicit setup behavior incorrect");
                File.WriteAllText(Path.Combine(Output,"startup.txt"),"Actual Program.Main with isolated fresh profile: main opened; setup visible="+sawSetup);return 0;
            }
            Application.EnableVisualStyles();Call("EngineRegistry","Import",new object[]{null});
            int[] expected={4,5,6,14,18,19};
            Check(((int[])T("EngineRegistry").GetProperty("ActiveIds").GetValue(null,null)).SequenceEqual(expected),"Fresh presets still present");
            string old="{\"Version\":2,\"NextId\":1002,\"Active\":[16,4,1001,17,5],\"Visible\":[16,1001,17,5],\"Custom\":[{\"Id\":1001,\"Name\":\"My own model\",\"Provider\":\"ollama\",\"Endpoint\":\"http://127.0.0.1:11434\",\"Model\":\"my-model:small\"}]}";
            T("ClassMain").GetField("isjiekounum").SetValue(null,16);Call("EngineRegistry","ValidateImport",old);Call("EngineRegistry","Import",old);
            Check(T("EngineRegistry").GetProperty("LoadError").GetValue(null,null)==null,"Legacy profile rejected");
            Check(((int[])T("EngineRegistry").GetProperty("ActiveIds").GetValue(null,null)).SequenceEqual(new[]{4,1001,5}),"Migration damaged order or custom model");
            Check(((int[])T("EngineRegistry").GetProperty("VisibleIds").GetValue(null,null)).SequenceEqual(new[]{1001,5}),"Migration damaged visibility");
            Check((int)T("ClassMain").GetField("isjiekounum").GetValue(null)==0,"Removed preset silently fell back to another engine");
            Check(Call("EngineRegistry","Get",1001)!=null,"Custom model removed");
            Call("EngineRegistry","Import",new object[]{null});
            Check((string)Get(Call("LocalRuntime","Load"),"Acceleration")=="cpu","Missing profile not CPU");
            string optionsPath=(string)T("LocalRuntime").GetProperty("OptionsPath",F).GetValue(null,null);Directory.CreateDirectory(Path.GetDirectoryName(optionsPath));
            foreach(string json in new[]{"{}","{\"Acceleration\":null}","{\"Acceleration\":\"invalid\"}"}){File.WriteAllText(optionsPath,json);Check((string)Get(Call("LocalRuntime","Load"),"Acceleration")=="cpu","Invalid/absent device not CPU");}
            File.WriteAllText(optionsPath,"{\"Acceleration\":\"auto\",\"Executable\":\"saved-ollama.exe\"}");
            using(var wizard=Wizard()){
                var device=(ComboBox)Get(wizard,"acceleration");Check(device.SelectedIndex==0,"Explicit acceleration overwritten");device.SelectedIndex=1;Prompt(wizard,DialogResult.Cancel);wizard.Close();Check(!wizard.IsDisposed,"Cancel closed modified wizard");
                Prompt(wizard,DialogResult.No);wizard.Close();Check((string)Get(Call("LocalRuntime","Load"),"Acceleration")=="auto","Discard saved device");
            }
            Call("ClassMain","jiluset");string primary=Path.Combine(Data,"YiChuang","setting.txt");byte[] before=File.ReadAllBytes(optionsPath);
            using(var wizard=Wizard()){
                ((ComboBox)Get(wizard,"acceleration")).SelectedIndex=1;
                using(var held=new FileStream(primary,FileMode.Open,FileAccess.Read,FileShare.Read)){
                    ((Button)Get(wizard,"done")).PerformClick();Check(!wizard.IsDisposed&&((TextBox)Get(wizard,"status")).Text.Contains("未保存"),"Failed save closed wizard");
                    Check(File.ReadAllBytes(optionsPath).SequenceEqual(before),"Failed save changed runtime options");
                }
                ((Button)Get(wizard,"done")).PerformClick();Check(wizard.IsDisposed,"Retry save failed");
            }
            var saved=Call("LocalRuntime","Load");Check((string)Get(saved,"Acceleration")=="cpu"&&(string)Get(saved,"Executable")=="saved-ollama.exe","Device save damaged service settings");
            File.WriteAllText(Path.Combine(Output,"result.txt"),"Fresh six engines; legacy preset retirement preserves custom/order/visibility; no silent fallback; CPU defaults; explicit GPU preserved; wizard cancel/discard; disk failure byte-exact rollback and retry passed.");return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(Output,"error.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
