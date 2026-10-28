using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Microsoft.Win32;

// Read-only evidence collection: no application settings, model files or API keys.
internal static class RuntimeCheck
{
    [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
    static extern IntPtr LoadLibraryEx(string file, IntPtr reserved, uint flags);
    [DllImport("kernel32.dll")] static extern bool FreeLibrary(IntPtr handle);
    [DllImport("msi.dll", CharSet=CharSet.Unicode)] static extern int MsiQueryProductState(string productCode);
    static readonly StringBuilder Report = new StringBuilder();
    static readonly Dictionary<string,string> ProductNames=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
    static void Line(string text) { Report.AppendLine(text); }
    static string Value(RegistryKey key, string name) { return key == null ? "未找到" : Convert.ToString(key.GetValue(name, "未找到")); }
    static int Number(RegistryKey key, string name) { int n; return int.TryParse(Value(key,name),out n) ? n : -1; }

    static void RegistryEvidence()
    {
        foreach (RegistryView view in new[]{RegistryView.Registry64,RegistryView.Registry32})
        {
            using (var root=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,view))
            using (var key=root.OpenSubKey(@"SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64"))
            {
                Line(view+" 的 x64 运行库记录：");
                foreach(string name in new[]{"Installed","Major","Minor","Bld","Rbld","Version"}) Line("  "+name+"="+Value(key,name));
                if(view==RegistryView.Registry64)
                {
                    int major=Number(key,"Major"), minor=Number(key,"Minor");
                    bool skip=Number(key,"Installed")==1&&(major>14||(major==14&&minor>=29));
                    Line("  译窗 1.5.6 按此记录的判断："+(skip?"跳过 Visual C++ 补装":"应启动 Visual C++ 补装"));
                }
            }
        }
        Line("");Line("程序和功能中的相关记录（仅名称和版本）：");
        var seen=new HashSet<string>();
        foreach(var hive in new[]{RegistryHive.LocalMachine,RegistryHive.CurrentUser})
        foreach(var view in new[]{RegistryView.Registry64,RegistryView.Registry32})
        using(var root=RegistryKey.OpenBaseKey(hive,view))
        using(var uninstall=root.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
        {
            if(uninstall==null)continue;
            foreach(string sub in uninstall.GetSubKeyNames())
            using(var key=uninstall.OpenSubKey(sub))
            {
                string name=Value(key,"DisplayName"),displayVersion=Value(key,"DisplayVersion");
                Version version;bool v14=Version.TryParse(displayVersion.TrimStart('v','V'),out version)&&version.Major==14;
                if(name=="译窗"||name.StartsWith("译窗 ",StringComparison.Ordinal)||
                   (name.StartsWith("Microsoft Visual C++",StringComparison.OrdinalIgnoreCase)&&v14))
                {
                    ProductNames[sub]=name;
                    if(seen.Add(name+" | "+displayVersion))Line("  "+name+" | "+displayVersion);
                    if(v14&&name.StartsWith("Microsoft Visual C++",StringComparison.OrdinalIgnoreCase)&&name.IndexOf("x64",StringComparison.OrdinalIgnoreCase)>=0)
                    {
                        Line("    登记位置："+hive+" / "+view+" / "+sub);
                        Line("    SystemComponent="+Value(key,"SystemComponent")+"；WindowsInstaller="+Value(key,"WindowsInstaller"));
                        if(Number(key,"WindowsInstaller")==1&&Regex.IsMatch(sub,@"^\{[0-9a-fA-F-]{36}\}$"))
                            Line("    MSI 当前状态="+MsiQueryProductState(sub)+"（5=已安装，2=未安装，-1=未知）");
                    }
                }
            }
        }
    }

    static void DependencyEvidence()
    {
        Line("");Line("v14 x64 运行组件的依赖记录（只读，不删除引用）：");
        int found=0;
        foreach(var view in new[]{RegistryView.Registry64,RegistryView.Registry32})
        using(var root=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,view))
        using(var dependencies=root.OpenSubKey(@"SOFTWARE\Classes\Installer\Dependencies"))
        {
            if(dependencies==null)continue;
            foreach(string name in dependencies.GetSubKeyNames().Where(n=>Regex.IsMatch(n,@"VC_Runtime.*amd64,v14$",RegexOptions.IgnoreCase)))
            using(var provider=dependencies.OpenSubKey(name))
            {
                found++;Line("  "+view+" / "+name+"；版本="+Value(provider,"Version"));
                using(var dependents=provider==null?null:provider.OpenSubKey("Dependents"))
                {
                    string[] names=dependents==null?new string[0]:dependents.GetSubKeyNames();
                    Line("    引用数量="+names.Length);
                    foreach(string dependent in names.Take(30))
                    {
                        string display;Line("    引用方="+dependent+(ProductNames.TryGetValue(dependent,out display)?" / "+display:" / 名称未在上述运行库卸载记录中找到"));
                    }
                }
            }
        }
        if(found==0)Line("  未找到该路径中的 v14 x64 引用记录；不能据此断定不存在其他共享依赖。");
    }

    internal static string Redact(string value)
    {
        return Regex.Replace(value,@"(?i)([A-Z]:\\Users\\)[^\\\r\n]+(?=\\)",m=>m.Groups[1].Value+"[用户]");
    }
    internal static string[] MicrosoftLogExcerpt(string text)
    {
        var lines=text.Split(new[]{"\r\n","\n","\r"},StringSplitOptions.None);
        var selected=new SortedSet<int>();
        for(int i=0;i<lines.Length;i++)
        {
            bool relevant=Regex.IsMatch(lines[i],@"(?i)(Detect begin|Detect complete|Plan begin|Plan complete|Apply begin|Apply complete|Detected package:|Planned package:|Will not uninstall|found dependents|dependency provider|dependent:|Applying execute package:|Applied execute package:|MainEngineThread is returning|Removal (completed|failed)|Installation (completed|failed)|Return value 3|\be[0-9]{3}:|\bw[0-9]{3}:|Error 0x|Final Result:)");
            if(!relevant)continue;
            int context=lines[i].IndexOf("Return value 3",StringComparison.OrdinalIgnoreCase)>=0?8:0;
            for(int n=Math.Max(0,i-context);n<=i;n++)selected.Add(n);
        }
        return selected.Take(180).Select(i=>Redact(lines[i])).ToArray();
    }

    static void MicrosoftInstallerEvidence()
    {
        Line("");Line("微软 Visual C++ 安装/卸载日志摘要（最多最近8份，用户名已遮盖）：");
        var files=new List<FileInfo>();
        foreach(string folder in new[]{Path.GetTempPath(),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"Temp")}.Distinct(StringComparer.OrdinalIgnoreCase))
        try
        {
            files.AddRange(new DirectoryInfo(folder).EnumerateFiles("*.log").Where(f=>Regex.IsMatch(f.Name,@"(?i)(vcredist|vc_redist|vcRuntime|visual.?c\+\+|vc14)")));
        }
        catch(UnauthorizedAccessException){Line("  某个系统临时日志目录无读取权限；不要求提权，继续检查当前用户日志。");}
        catch(IOException){Line("  某个临时日志目录暂不可读。");}
        int found=0;
        foreach(var file in files.OrderByDescending(f=>f.LastWriteTimeUtc).Take(24))
        {
            if(file.Length>16*1024*1024){Line("  "+file.Name+" 超过16MB，未读取。");continue;}
            string content;
            try{content=File.ReadAllText(file.FullName);}catch(Exception ex){if(!(ex is IOException)&&!(ex is UnauthorizedAccessException))throw;continue;}
            if(!Regex.IsMatch(content,@"(?i)(Visual C\+\+|VC_redist|vcredist|vcRuntimeMinimum|vcRuntimeAdditional)"))continue;
            Line("  日志："+file.Name+"；修改时间="+file.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
            var excerpt=MicrosoftLogExcerpt(content);
            foreach(string line in excerpt)Line("    "+line);
            if(excerpt.Length==0)Line("    未找到可识别的安装/卸载决策行。");
            if(++found==8)break;
        }
        if(found==0)Line("  未找到微软运行库日志。可能被清理或使用自定义日志路径；不要为了采集重复卸载。");
    }

    static void Section(string name,Action action)
    {
        try{action();}catch(Exception ex){Line("["+name+"] 未完成："+ex.GetType().Name+" / "+Redact(ex.Message));}
    }

    static void FileEvidence()
    {
        Line("");Line("64 位系统运行库文件（实际文件与加载检查）：");
        foreach(string name in new[]{"msvcp140.dll","vcruntime140.dll","vcruntime140_1.dll"})
        {
            string path=Path.Combine(Environment.SystemDirectory,name);
            if(!File.Exists(path)){Line("  "+name+"：缺失");continue;}
            string version=FileVersionInfo.GetVersionInfo(path).FileVersion;
            // Constrain dependencies to System32 as well; never use a DLL beside this tool.
            IntPtr module=LoadLibraryEx(path,IntPtr.Zero,0x800);
            int error=module==IntPtr.Zero?Marshal.GetLastWin32Error():0;
            if(module!=IntPtr.Zero)FreeLibrary(module);
            Line("  "+name+"：版本 "+version+"；"+(error==0?"系统加载成功":"系统加载失败，错误码 "+error));
        }
    }

    static void InstallerEvidence()
    {
        Line("");Line("最近安装日志的运行库相关记录：");
        int found=0;
        foreach(var file in new DirectoryInfo(Path.GetTempPath()).GetFiles("Setup Log*.txt").OrderByDescending(f=>f.LastWriteTimeUtc).Take(40))
        {
            if(file.Length>8*1024*1024)continue;
            string text;
            try{text=File.ReadAllText(file.FullName);}catch(IOException){continue;}
            if(text.IndexOf("YiChuang",StringComparison.OrdinalIgnoreCase)<0)continue;
            Line("  日志："+file.Name);
            var package=Regex.Match(text,@"YiChuang-Setup-[0-9A-Za-z.\-]+\.exe",RegexOptions.IgnoreCase);
            if(package.Success)Line("  安装包："+package.Value);
            foreach(string line in text.Split(new[]{'\r','\n'},StringSplitOptions.RemoveEmptyEntries))
                if(line.Contains("Prerequisite ")||line.Contains("Installing prerequisite:")||line.Contains("Setup exit code:")||line.Contains("Need to restart Windows?"))Line("  "+line.Trim());
            if(++found==5)break;
        }
        if(found==0)Line("  未找到当前用户的译窗安装日志；可能已清理或由其他用户安装。");
    }

    static void Collect()
    {
        Line("译窗运行库检查 V2（只读）");Line("时间："+DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        using(var root=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64))
        using(var key=root.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
            Line("Windows："+Value(key,"DisplayVersion")+"，内部版本 "+Value(key,"CurrentBuildNumber")+"."+Value(key,"UBR")+"；64 位系统="+Environment.Is64BitOperatingSystem+"；64 位检查进程="+Environment.Is64BitProcess);
        Line("本工具不会安装、卸载或修复组件；不会读取翻译内容、API Key 或个人配置。");Line("");
        if(!Environment.Is64BitProcess)throw new InvalidOperationException("请使用本工具的 x64 版本。");
        Section("注册记录",RegistryEvidence);Section("文件加载",FileEvidence);Section("组件引用",DependencyEvidence);
        Section("译窗日志",InstallerEvidence);Section("微软日志",MicrosoftInstallerEvidence);
        Line("");Line("说明：文件可加载不代表所有 OCR/模型功能已验证。请把此报告发回，结合安装日志判断是否发生误判。");
    }

    [STAThread] static int Main(string[] args)
    {
        int exit=0;
        try{Collect();}catch(Exception ex){Line("检查未完成："+ex.GetType().Name+" / "+ex.Message);exit=1;}
        string path=args.Length==2&&args[0]=="--report"?Path.GetFullPath(args[1]):Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"YiChuang-RuntimeReport-v2-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".txt");
        try{File.WriteAllText(path,Report.ToString(),new UTF8Encoding(true));}
        catch(Exception){path=Path.Combine(Path.GetTempPath(),"YiChuang-RuntimeReport-v2-"+Guid.NewGuid().ToString("N")+".txt");File.WriteAllText(path,Report.ToString(),new UTF8Encoding(true));}
        if(args.Length==2&&args[0]=="--report")return exit;
        Application.EnableVisualStyles();
        using(var form=new Form{Text="译窗 · 运行库检查 V2",StartPosition=FormStartPosition.CenterScreen,Size=new Size(860,650),MinimumSize=new Size(600,400),Font=new Font("Microsoft YaHei UI",10)})
        using(var text=new TextBox{Multiline=true,ReadOnly=true,ScrollBars=ScrollBars.Both,WordWrap=false,Dock=DockStyle.Fill,Text=Report.ToString()})
        using(var footer=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=60,Padding=new Padding(8),FlowDirection=FlowDirection.LeftToRight})
        {
            var copy=new Button{Text="复制检查结果",AutoSize=true,Height=36};copy.Click+=(s,e)=>{try{Clipboard.SetText(Report.ToString());}catch{MessageBox.Show(form,"无法复制，请使用报告文件。",form.Text);}};
            var open=new Button{Text="打开报告文件",AutoSize=true,Height=36};open.Click+=(s,e)=>Process.Start(new ProcessStartInfo("notepad.exe","\""+path+"\""){UseShellExecute=true});
            footer.Controls.Add(copy);footer.Controls.Add(open);form.Controls.Add(text);form.Controls.Add(footer);Application.Run(form);
        }
        return exit;
    }
}
