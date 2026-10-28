using System;
using System.Reflection;
class RuntimeCheckLogRegression
{
    static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
    static int Main(string[] args)
    {
        try
        {
            var type=Assembly.LoadFrom(args[0]).GetType("RuntimeCheck");
            var flags=BindingFlags.NonPublic|BindingFlags.Static;
            var extract=type.GetMethod("MicrosoftLogExcerpt",flags);
            var redact=type.GetMethod("Redact",flags);
            string fixture="Microsoft Visual C++ v14\nUnrelated line that should be omitted\n"+
                "[1]i100: Detect begin, 3 packages\n"+
                "[1]i200: Plan begin, action: Uninstall\n"+
                "[1]w327: Will not uninstall package: vcRuntimeMinimum_x64, found dependents: 1\n"+
                "[1]i201: Planned package: vcRuntimeMinimum_x64, state: Present, execute: None\n"+
                "[1]i300: Apply begin\n"+
                "[1]i399: Apply complete, result: 0x0, restart: None\n";
            string result=string.Join("\n",(string[])extract.Invoke(null,new object[]{fixture}));
            Check(result.Contains("found dependents: 1")&&result.Contains("execute: None")&&result.Contains("result: 0x0"),"Dependency retention evidence was lost.");
            Check(!result.Contains("Unrelated line"),"Irrelevant log text retained.");
            string msi="Microsoft Visual C++ v14\nMSI file: C:\\Users\\Test Person\\Downloads\\vc.msi\nError: example failure\nAction ended: InstallFinalize. Return value 3.\nMainEngineThread is returning 1603\n";
            result=string.Join("\n",(string[])extract.Invoke(null,new object[]{msi}));
            Check(result.Contains("example failure")&&result.Contains("returning 1603"),"MSI failure context lost.");
            Check(!result.Contains("Test Person")&&result.Contains("C:\\Users\\[用户]\\Downloads"),"User path was not redacted.");
            Check((string)redact.Invoke(null,new object[]{"C:\\Users\\Jane\\a.txt and D:\\Users\\John Doe\\b.txt"})=="C:\\Users\\[用户]\\a.txt and D:\\Users\\[用户]\\b.txt","Multiple profiles not redacted.");
            Check(((string[])extract.Invoke(null,new object[]{""})).Length==0,"Empty log produced evidence.");
            Console.WriteLine("PASS: retained package decisions, uninstall result, MSI failure context, irrelevant-line exclusion and profile-path redaction.");
            return 0;
        }
        catch(Exception ex){Console.Error.WriteLine(ex);return 1;}
    }
}
