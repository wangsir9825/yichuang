using System;
using System.IO;
using System.Reflection;
using System.Threading;
class LlamaCpuExitProbe
{
    static string Data;
    public static string Folder(Environment.SpecialFolder f){return Data;}
    const BindingFlags Flags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
    static int Main(string[] args)
    {
        string exe=Path.GetFullPath(args[0]),output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watch=new Timer(s=>Environment.Exit(124),null,240000,Timeout.Infinite))try{
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string p=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(p)?Assembly.LoadFrom(p):null;};var app=Assembly.LoadFrom(exe);
            Data=Path.Combine(output,"isolated-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(LlamaCpuExitProbe).GetMethod("Folder"));
            var options=Activator.CreateInstance(app.GetType("YiChuang.LocalRuntimeOptions"),true);options.GetType().GetField("HyGguf").SetValue(options,Path.GetFullPath(args[2]));options.GetType().GetField("Acceleration").SetValue(options,"cpu");app.GetType("YiChuang.LocalRuntime").GetMethod("Save",Flags).Invoke(null,new[]{options});
            var result=app.GetType("YiChuang.LocalModelClient").GetMethod("Translate").Invoke(null,new object[]{16,"window","en-zh",null});string text=(string)result.GetType().GetField("Text").GetValue(result);if(!text.Contains("窗"))throw new Exception("Unexpected CPU translation: "+text);
            int pid=(int)app.GetType("YiChuang.LlamaRuntime").GetProperty("ProcessId",Flags).GetValue(null,null);if(pid==0)throw new Exception("Runtime did not start");File.WriteAllText(Path.Combine(output,"cpu-result.txt"),text);File.WriteAllText(Path.Combine(output,"child-pid.txt"),pid.ToString());
            // Deliberately bypass FormClosed / Stop / finally to verify the Windows job owns the child.
            Environment.Exit(0);return 0;
        }catch(Exception ex){File.WriteAllText(Path.Combine(output,"error.txt"),ex.ToString());Console.Error.WriteLine(ex);return 1;}
    }
}
