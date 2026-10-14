using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

internal static class NamingMigrationRegression
{
    static Assembly App;
    static string Data;
    const BindingFlags Hidden=BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    public static string Folder(Environment.SpecialFolder _) { return Data; }
    static object Call(string type,string method,params object[] args) { return App.GetType("YiChuang."+type).GetMethods(Hidden).Single(m=>m.Name==method&&m.GetParameters().Length==args.Length).Invoke(null,args); }
    static void Check(bool ok,string message) { if(!ok)throw new Exception(message); }
    [STAThread] static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        try
        {
            App=Assembly.LoadFrom(Path.GetFullPath(args[0]));
            Data=Path.Combine(output,"upgrade");Directory.CreateDirectory(Data);
            var proxies=(object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(NamingMigrationRegression).GetMethod("Folder"));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput",Hidden).SetValue(null,output);
            string previous=Path.Combine(Data,"Previous-NoLogin-2.9.18B"),current=Path.Combine(Data,"YiChuang");Directory.CreateDirectory(previous);
            string json=(string)Call("ClassMain","moren"),protectedText=(string)Call("RSA","RSAEncrypt",json);
            File.WriteAllText(Path.Combine(previous,"setting.txt"),protectedText,Encoding.UTF8);
            File.WriteAllText(Path.Combine(previous,"setting.txt.bak"),protectedText,Encoding.UTF8);
            File.WriteAllText(Path.Combine(previous,"local-models.json"),"{\"hunyuan_model\":\"model-kept-for-upgrade:latest\",\"backend\":\"ollama\"}");
            File.WriteAllText(Path.Combine(previous,"local-runtime.json"),"{\"HyGguf\":\"C:\\\\existing\\\\model.gguf\",\"WizardCompleted\":true}");
            var names=Directory.GetFiles(previous).Select(Path.GetFileName).ToArray();
            Call("SettingsStore","ImportPreviousSettings");
            foreach(string name in names)Check(File.ReadAllBytes(Path.Combine(previous,name)).SequenceEqual(File.ReadAllBytes(Path.Combine(current,name))),"Imported bytes changed: "+name);
            Check((string)Call("ClassMain","readJiekou")==json,"Encrypted legacy settings did not load");
            Call("LocalModelSettings","Load");
            Check((string)App.GetType("YiChuang.LocalModelSettings").GetField("HunyuanModel").GetValue(null)=="model-kept-for-upgrade:latest","Existing model alias changed");
            object options=Call("LocalRuntime","Load");
            Check((bool)options.GetType().GetField("WizardCompleted").GetValue(options),"Local wizard state lost");
            Check(options.GetType().GetField("HyGguf").GetValue(options).ToString().Contains("existing"),"Existing GGUF path lost");
            Call("ClassMain","writeJiekou",json);
            string newSettings=File.ReadAllText(Path.Combine(current,"setting.txt"));
            File.WriteAllText(Path.Combine(previous,"setting.txt"),"later legacy change");Call("SettingsStore","ImportPreviousSettings");
            Check(File.ReadAllText(Path.Combine(current,"setting.txt"))==newSettings,"Existing current settings overwritten");
            Check(File.ReadAllText(Path.Combine(previous,"setting.txt"))=="later legacy change","Legacy source modified");
            Check(!Directory.GetDirectories(current,".upgrade-*").Any(),"Migration staging left behind");
            string[] resources=App.GetManifestResourceNames();
            foreach(string required in new[]{"YiChuang.Form2.resources","YiChuang.Properties.Resources.resources","YiChuang.UserControl配置备份设置.resources"})Check(resources.Contains(required),"Resource missing: "+required);
            Check(App.GetType("YiChuang.Program")!=null && App.GetType("YiChuang.Form2")!=null,"Current namespace absent");
            var resourceType=App.GetType("YiChuang.Properties.Resources");
            Check(resourceType.GetProperty("snap",Hidden).GetValue(null,null)!=null,"Renamed resource manager cannot load OCR icon");
            Data=Path.Combine(output,"damaged");previous=Path.Combine(Data,"Previous-NoLogin-2.9.18B");Directory.CreateDirectory(previous);
            File.WriteAllText(Path.Combine(previous,"setting.txt"),"damaged");File.WriteAllText(Path.Combine(previous,"setting.txt.bak"),"also damaged");
            Check(((string)Call("ClassMain","readJiekou")).Contains("invalid-protected-settings"),"Damaged imported settings did not pause safely");
            Check(File.ReadAllText(Path.Combine(previous,"setting.txt"))=="damaged","Damaged source not preserved");
            Data=Path.Combine(output,"ambiguous");
            foreach(string old in new[]{"First-NoLogin-2.9.18B","Second-NoLogin-2.9.18B"}) {string dir=Path.Combine(Data,old);Directory.CreateDirectory(dir);File.WriteAllText(Path.Combine(dir,"setting.txt"),protectedText);}
            Check(((string)Call("ClassMain","readJiekou")).Contains("invalid-protected-settings"),"Ambiguous configurations silently selected");
            Check(!File.Exists(Path.Combine(Data,"YiChuang","setting.txt")),"Ambiguous configuration published");
            File.WriteAllText(Path.Combine(output,"pass.txt"),"Namespace/resource loading, encrypted settings byte preservation, model/runtime reuse, no overwrite, damaged and ambiguous settings protection: passed.");
            Console.WriteLine("Naming and legacy settings migration regression passed.");return 0;
        }
        catch(Exception ex) {File.WriteAllText(Path.Combine(output,"failure.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
