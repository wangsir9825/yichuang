using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
    internal sealed class LocalRuntimeOptions
    {
        public string Executable = "", ModelsDirectory = "";
        public string GgufDirectory = "", HyGguf = "", GemmaGguf = "", Acceleration = "auto";
        public bool WizardCompleted;
    }

    internal static class LocalRuntime
    {
        internal const string HyModel = "yichuang-hy-mt2:7b-q6", GemmaModel = "translategemma:12b";
        internal static string OptionsPath { get { return Path.Combine(SettingsStore.Folder,"local-runtime.json"); } }
        internal static LocalRuntimeOptions Load()
        {
            try { return JsonConvert.DeserializeObject<LocalRuntimeOptions>(File.ReadAllText(OptionsPath)) ?? new LocalRuntimeOptions(); }
            catch { return new LocalRuntimeOptions(); }
        }
        internal static void Save(LocalRuntimeOptions options)
        {
            Directory.CreateDirectory(SettingsStore.Folder);
            string temp=OptionsPath+"."+Guid.NewGuid().ToString("N")+".tmp";
            try { File.WriteAllText(temp,JsonConvert.SerializeObject(options,Formatting.Indented),Encoding.UTF8);if(File.Exists(OptionsPath))File.Replace(temp,OptionsPath,null);else File.Move(temp,OptionsPath); }
            finally { if(File.Exists(temp))File.Delete(temp); }
        }
        internal static string DefaultModelsDirectory()
        {
            string configured=Environment.GetEnvironmentVariable("OLLAMA_MODELS");
            if(string.IsNullOrWhiteSpace(configured))configured=Environment.GetEnvironmentVariable("OLLAMA_MODELS",EnvironmentVariableTarget.User);
            if(string.IsNullOrWhiteSpace(configured))configured=Environment.GetEnvironmentVariable("OLLAMA_MODELS",EnvironmentVariableTarget.Machine);
            return string.IsNullOrWhiteSpace(configured)?Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".ollama","models"):Environment.ExpandEnvironmentVariables(configured);
        }
        internal static string FindExecutable(string preferred)
        {
            var paths=new List<string>();if(!string.IsNullOrWhiteSpace(preferred))paths.Add(preferred);
            foreach(string folder in (Environment.GetEnvironmentVariable("PATH")??"").Split(Path.PathSeparator))if(!string.IsNullOrWhiteSpace(folder))try{paths.Add(Path.Combine(folder.Trim('"'),"ollama.exe"));}catch{}
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Programs","Ollama","ollama.exe"));
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Ollama","ollama.exe"));
            paths.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Ollama","ollama.exe"));
            foreach(string name in new[]{"ollama","ollama app"})foreach(var process in Process.GetProcessesByName(name))using(process)try {
                string exe=process.MainModule.FileName;paths.Add(Path.Combine(Path.GetDirectoryName(exe),"ollama.exe"));
            } catch {}
            foreach(var hive in new[]{RegistryHive.CurrentUser,RegistryHive.LocalMachine})foreach(var view in new[]{RegistryView.Registry64,RegistryView.Registry32})try {
                using(var root=RegistryKey.OpenBaseKey(hive,view))using(var uninstall=root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall"))if(uninstall!=null)foreach(string name in uninstall.GetSubKeyNames())using(var item=uninstall.OpenSubKey(name)) {
                    if(((string)item?.GetValue("DisplayName")??"").StartsWith("Ollama",StringComparison.OrdinalIgnoreCase)) {
                        string folder=(string)item.GetValue("InstallLocation");if(!string.IsNullOrWhiteSpace(folder))paths.Add(Path.Combine(folder,"ollama.exe"));
                    }
                }
            } catch {}
            return paths.FirstOrDefault(p=>File.Exists(p)&&string.Equals(Path.GetFileName(p),"ollama.exe",StringComparison.OrdinalIgnoreCase))??"";
        }
        internal static string[] Models(string address,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            return LocalModelClient.ListInstalled(address,()=>!token.IsCancellationRequested);
        }
        internal static bool TryModels(string address,CancellationToken token,out string[] names)
        {
            try { names=Models(address,token);return true; }
            catch(InvalidOperationException ex) {
                token.ThrowIfCancellationRequested();var network=ex.InnerException as WebException;
                if(network==null||(network.Status!=WebExceptionStatus.ConnectFailure&&network.Status!=WebExceptionStatus.NameResolutionFailure))throw;
                names=new string[0];return false;
            }
        }
        // Start only when the chosen endpoint is offline. Never stop another Ollama process.
        internal static bool EnsureService(LocalRuntimeOptions options,string address,Action<string> progress,CancellationToken token)
        {
            var endpoint=LocalModelClient.ValidateEndpoint(address);string[] names;
            if(TryModels(address,token,out names)){progress?.Invoke("已连接现有服务，发现 "+names.Length+" 个本地模型；沿用该服务的模型目录。");return false;}
            string executable=FindExecutable(options.Executable);
            if(executable.Length==0)throw new InvalidOperationException("尚未找到 Ollama。请点击“下载并安装 Ollama”，安装后重新检查。");
            string folder=string.IsNullOrWhiteSpace(options.ModelsDirectory)?DefaultModelsDirectory():Path.GetFullPath(options.ModelsDirectory);
            Directory.CreateDirectory(folder);token.ThrowIfCancellationRequested();
            // Recheck immediately before launching, in case Ollama was started by its tray app.
            if(TryModels(address,token,out names))return false;
            var start=new ProcessStartInfo(executable,"serve") {UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden,WorkingDirectory=Path.GetDirectoryName(executable)};
            start.EnvironmentVariables["OLLAMA_MODELS"]=folder;
            start.EnvironmentVariables["OLLAMA_HOST"]=endpoint.HostNameType==UriHostNameType.IPv6?"["+endpoint.Host.Trim('[',']')+"]:"+endpoint.Port:endpoint.Host+":"+endpoint.Port;
            start.EnvironmentVariables["OLLAMA_NO_CLOUD"]="1";
            progress?.Invoke("正在启动本地服务…");
            using(var owned=Process.Start(start)) {
                var watch=Stopwatch.StartNew();
                while(watch.ElapsedMilliseconds<20000) {
                    token.ThrowIfCancellationRequested();
                    if(TryModels(address,token,out names)){options.Executable=executable;options.ModelsDirectory=folder;Save(options);progress?.Invoke("服务已启动，实际模型列表包含 "+names.Length+" 项。");return true;}
                    if(owned.HasExited)throw new InvalidOperationException("Ollama 启动后退出。请检查所选程序、模型目录权限及端口是否被占用。");
                    if(token.WaitHandle.WaitOne(250))token.ThrowIfCancellationRequested();
                }
            }
            throw new InvalidOperationException("Ollama 启动等待超时。可以打开 Ollama 后点击“重新检查”；已下载模型不会删除。");
        }
        internal static void ReadProgress(TextReader reader,Action<string> progress,CancellationToken token)
        {
            string line;bool success=false;var throttle=Stopwatch.StartNew();
            while((line=ResponseReader.Line(reader))!=null) {
                token.ThrowIfCancellationRequested();if(string.IsNullOrWhiteSpace(line))continue;
                var state=JObject.Parse(line);if(state["error"]!=null)throw new InvalidOperationException("模型准备失败："+((string)state["error"]??"未知错误"));
                string status=(string)state["status"]??"正在处理";
                long total=(long?)state["total"]??0,completed=(long?)state["completed"]??0;
                if(total>0)status="正在下载模型："+Math.Min(100,Math.Max(0,100.0*completed/total)).ToString("F1")+"%（"+(completed/1048576)+" / "+(total/1048576)+" MB）";
                if(throttle.ElapsedMilliseconds>150||status=="success"){progress?.Invoke(status=="success"?"模型准备完成":status);throttle.Restart();}
                if((string)state["status"]=="success")success=true;
            }
            token.ThrowIfCancellationRequested();if(!success)throw new IOException("下载连接提前结束，模型尚未确认完整。请重试，Ollama 会复用已有下载数据。");
        }
        private static void PostProgress(string address,string path,JObject payload,Action<string> progress,CancellationToken token)
        {
            var request=(HttpWebRequest)WebRequest.Create(new Uri(LocalModelClient.ValidateEndpoint(address),path));
            request.Proxy=null;request.AllowAutoRedirect=false;request.Method="POST";request.ContentType="application/json; charset=utf-8";request.Timeout=30000;request.ReadWriteTimeout=180000;
            using(var limit=CancellationTokenSource.CreateLinkedTokenSource(token)) {
                limit.CancelAfter(TimeSpan.FromMinutes(60));
                using(limit.Token.Register(()=>request.Abort()))try {
                    byte[] bytes=Encoding.UTF8.GetBytes(payload.ToString(Formatting.None));request.ContentLength=bytes.Length;
                    using(var stream=request.GetRequestStream())stream.Write(bytes,0,bytes.Length);
                    using(var response=(HttpWebResponse)request.GetResponse())using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8))ReadProgress(reader,progress,limit.Token);
                } catch(WebException ex) { ex.Response?.Dispose();token.ThrowIfCancellationRequested();if(limit.IsCancellationRequested)throw new InvalidOperationException("模型下载超过一小时，请重试以继续下载。");throw new InvalidOperationException("模型下载连接失败，请检查网络和 Ollama 服务后重试。"); }
                catch(IOException) {token.ThrowIfCancellationRequested();if(limit.IsCancellationRequested)throw new InvalidOperationException("模型下载超过一小时，请重试以继续下载。");throw;}
            }
        }
        internal static JObject HyDefinition(string baseModel = "kaelri/hy-mt2:7b")
        {
            return new JObject { ["model"]=HyModel,["from"]=baseModel,["stream"]=true,
                ["template"]="{{ if .System }}<|startoftext|>{{ .System }}<|extra_4|>{{ .Prompt }}<|extra_0|>{{ else }}<|startoftext|>{{ .Prompt }}<|extra_0|>{{ end }}",
                ["parameters"]=new JObject { ["stop"]=new JArray("<|eos|>","<|extra_5|>"),["temperature"]=0.7,["top_p"]=0.6,["top_k"]=20,["repeat_penalty"]=1.05,["num_ctx"]=8192,["num_predict"]=4096 }};
        }
        internal static void PrepareModels(string address,bool hy,bool gemma,Action<string> progress,CancellationToken token)
        {
            if(!hy&&!gemma)throw new InvalidOperationException("请先勾选至少一款模型。");
            var installed=Models(address,token);
            if(hy&&!installed.Contains(HyModel)) {
                string existing = installed.FirstOrDefault(name=>name.EndsWith("-hy-mt2:7b-q6",StringComparison.OrdinalIgnoreCase)) ?? "kaelri/hy-mt2:7b";
                if(!installed.Contains(existing))PostProgress(address,"api/pull",new JObject{["model"]=existing,["stream"]=true},progress,token);
                progress?.Invoke("正在配置腾讯翻译模板（复用已下载权重）…");PostProgress(address,"api/create",HyDefinition(existing),progress,token);
            }
            if(gemma&&!installed.Contains(GemmaModel))PostProgress(address,"api/pull",new JObject{["model"]=GemmaModel,["stream"]=true},progress,token);
            installed=Models(address,token);
            if(hy&&!installed.Contains(HyModel)||gemma&&!installed.Contains(GemmaModel))throw new InvalidOperationException("服务尚未列出选中的完整模型，请重新检查。译窗不会将未完成的下载当作可用。");
            progress?.Invoke("已核对实际模型列表，选中的模型均已就绪。");
        }
        internal static void InstallOllama(Action<string> progress,CancellationToken token)
        {
            string dir=Path.Combine(SettingsStore.Folder,"Downloads");Directory.CreateDirectory(dir);
            string file=Path.Combine(dir,"OllamaSetup-"+Guid.NewGuid().ToString("N")+".exe");
            var request=(HttpWebRequest)WebRequest.Create("https://ollama.com/download/OllamaSetup.exe");request.Timeout=30000;request.ReadWriteTimeout=60000;
            ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
            try {
                using(token.Register(()=>request.Abort()))using(var response=(HttpWebResponse)request.GetResponse())using(var input=response.GetResponseStream())using(var output=File.Create(file)) {
                    byte[] buffer=new byte[65536];int read;long done=0;var tick=Stopwatch.StartNew();
                    while((read=input.Read(buffer,0,buffer.Length))>0) {token.ThrowIfCancellationRequested();done+=read;if(done>4L*1024*1024*1024)throw new IOException("安装程序大小异常。");output.Write(buffer,0,read);if(tick.ElapsedMilliseconds>200){progress("下载 Ollama 安装程序："+(done/1048576)+" MB"+(response.ContentLength>0?" / "+(response.ContentLength/1048576)+" MB":""));tick.Restart();}}
                }
                token.ThrowIfCancellationRequested();progress("正在检查 Ollama 安装程序签名…");
                // PowerShell validates Authenticode trust, including the file digest, before execution.
                string escaped=file.Replace("'","''");
                string script="$s=Get-AuthenticodeSignature -LiteralPath '"+escaped+"'; if($s.Status -ne 'Valid' -or $s.SignerCertificate.Subject -notmatch 'CN=Ollama Inc\\.') {exit 9}; exit 0";
                var check=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),@"WindowsPowerShell\v1.0\powershell.exe"),"-NoProfile -NonInteractive -EncodedCommand "+Convert.ToBase64String(Encoding.Unicode.GetBytes(script))) {UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};
                using(var process=Process.Start(check)){if(!process.WaitForExit(60000)){try{process.Kill();}catch{}throw new IOException("安装程序签名检查超时。");}if(process.ExitCode!=0)throw new IOException("安装程序签名未通过检查，已停止安装。请从 Ollama 官网下载。");}
                token.ThrowIfCancellationRequested();progress("正在打开 Ollama 安装程序。请在安装窗口完成安装，然后回到这里重新检查。");
                // Interactive installer is intentional; never force an unattended system change.
                using(var process=Process.Start(new ProcessStartInfo(file){UseShellExecute=true}))while(!process.WaitForExit(250))token.ThrowIfCancellationRequested();
                progress("Ollama 安装窗口已关闭。请点击“检查并启动服务”。");
            } catch(WebException) {token.ThrowIfCancellationRequested();throw new IOException("无法下载 Ollama 安装程序，请检查网络后重试，或使用官网安装入口。");}
            finally { try{if(File.Exists(file))File.Delete(file);}catch{} }
        }
    }
}
