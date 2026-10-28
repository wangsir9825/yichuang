using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Globalization;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
    internal static class SettingsStore
    {
        private const string Prefix = "YICHUANG-DPAPI-1:";
        private static readonly object Gate = new object();
        internal static string LoadError;
        private static bool blocked;
        internal static string Folder { get { return Path.Combine(((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.ApplicationData), "YiChuang"); } }
        internal static void ImportPreviousSettings()
        {
            lock (Gate)
            {
                string destination = Folder;
                if (File.Exists(Path.Combine(destination,"setting.txt")) || File.Exists(Path.Combine(destination,"setting.txt.bak"))) return;
                string parent = Path.GetDirectoryName(destination);
                if (!Directory.Exists(parent)) return;
                // The former portable builds used this versioned suffix. Match their
                // layout without tying the current product to a former namespace.
                var candidates = Directory.GetDirectories(parent,"*-NoLogin-2.9.18B")
                    .Where(p => (File.GetAttributes(p) & FileAttributes.ReparsePoint) == 0)
                    .Where(p => File.Exists(Path.Combine(p,"setting.txt")) || File.Exists(Path.Combine(p,"setting.txt.bak"))).ToArray();
                if (candidates.Length == 0) return;
                if (candidates.Length != 1) throw new InvalidDataException("找到多份旧版配置，请通过配置备份导入需要使用的一份。旧文件已保留。");
                Directory.CreateDirectory(destination);
                string stage = Path.Combine(destination,".upgrade-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(stage);
                try
                {
                    // Copy bytes, including encrypted keys and a potentially damaged
                    // primary. Load performs normal recovery; the original stays intact.
                    string[] names = {"local-models.json","local-runtime.json","setting.txt.bak","setting.txt"};
                    foreach (string name in names)
                    {
                        string source = Path.Combine(candidates[0],name);
                        if (!File.Exists(source) || File.Exists(Path.Combine(destination,name))) continue;
                        if ((File.GetAttributes(source) & FileAttributes.ReparsePoint) != 0 || new FileInfo(source).Length > 4*1024*1024)
                            throw new InvalidDataException("旧配置文件不符合导入要求。请在配置备份中手动恢复，旧文件已保留。");
                        File.Copy(source,Path.Combine(stage,name),false);
                    }
                    foreach (string name in names)
                    {
                        string staged = Path.Combine(stage,name), target = Path.Combine(destination,name);
                        if (File.Exists(staged) && !File.Exists(target)) File.Move(staged,target);
                    }
                }
                finally
                {
                    foreach(string file in Directory.GetFiles(stage)) File.Delete(file);
                    Directory.Delete(stage);
                }
            }
        }
        private static string Decode(string value)
        {
            if(!value.StartsWith(Prefix,StringComparison.Ordinal) && !value.TrimStart().StartsWith("{",StringComparison.Ordinal))Convert.FromBase64String(value);
            string json = value.StartsWith(Prefix, StringComparison.Ordinal) ? Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value.Substring(Prefix.Length)), null, DataProtectionScope.CurrentUser)) : value.TrimStart().StartsWith("{",StringComparison.Ordinal) ? value : RSA.RSADecrypt(value);
            var parsed=JObject.Parse(json);
            if (parsed.Count == 0 || parsed.Properties().Any(p=>p.Value is JContainer)) throw new InvalidDataException("Invalid settings structure");
            return json;
        }
        private static string ReadFile(string path)
        {
            if (new FileInfo(path).Length > 4 * 1024 * 1024) throw new InvalidDataException("Settings too large");
            return Decode(File.ReadAllText(path, Encoding.UTF8));
        }
        internal static string Load(Func<string> defaults)
        {
            lock (Gate)
            {
                blocked = false; LoadError = null;
                try { ImportPreviousSettings(); }
                catch(Exception ex) { blocked=true;LoadError="旧版设置未能自动导入："+ex.Message;return "{\"Master_switch\":\"0\",\"Translation_interface\":\"0\",\"EngineRegistry\":\"invalid-protected-settings\"}"; }
                Directory.CreateDirectory(Folder);
                string path = Path.Combine(Folder, "setting.txt");
                if (!File.Exists(path) && !File.Exists(path + ".bak")) return defaults();
                try { return ReadFile(path); }
                catch
                {
                    try { string json = ReadFile(path + ".bak"); LoadError = "主设置文件无法读取，已使用上一份有效备份。原文件会保留，请检查设置。"; return json; }
                    catch { blocked = true; LoadError = "设置和备份均无法读取，已暂停取词并保护原文件。请在配置备份中导入有效备份。"; return "{\"Master_switch\":\"0\",\"Translation_interface\":\"0\",\"EngineRegistry\":\"invalid-protected-settings\"}"; }
                }
            }
        }
        internal static void AllowRecovery(string json) { if (JObject.Parse(json).Count == 0) throw new InvalidDataException("备份内容为空。"); lock (Gate) { blocked = false; LoadError = null; } }
        internal static void ValidateBackup(string json)
        {
            var parsed=JObject.Parse(json);
            if(parsed["Master_switch"]==null||parsed["Translation_interface"]==null||parsed.Properties().Any(p=>p.Value is JContainer))throw new InvalidDataException("这不是完整的译窗配置备份。");
            EngineRegistry.ValidateImport((string)parsed["EngineRegistry"]);
            NetworkProxy.Parse(parsed).Validate();
            if(parsed["LocalBackend"]!=null&&(string)parsed["LocalBackend"]!="ollama"&&(string)parsed["LocalBackend"]!="llamacpp")throw new InvalidDataException("备份中的本地模型运行方式无效。");
            foreach(string name in new[]{"translationFontSize","originalFontSize"})if(parsed[name]!=null) {
                float value;
                if(!float.TryParse((string)parsed[name],NumberStyles.Float,CultureInfo.CurrentCulture,out value)||float.IsNaN(value)||float.IsInfinity(value)||value<6||value>96)throw new InvalidDataException("备份中的字号无效。");
            }
            var local=new[]{"LocalEndpoint","HunyuanModel","GemmaModel"};
            if(local.Any(name=>parsed[name]!=null)) {
                LocalModelClient.ValidateEndpoint((string)parsed[local[0]]);
                LocalModelClient.ValidateModel((string)parsed[local[1]]);
                LocalModelClient.ValidateModel((string)parsed[local[2]]);
            }
        }
        internal static void ImportBackup(string json)
        {
            ValidateBackup(json);
            lock(Gate) {
                bool wasBlocked=blocked;
                try { blocked=false;Save(json);LoadError=null; }
                catch { blocked=wasBlocked;throw; }
            }
        }
        internal static void Save(string json)
        {
            if(JObject.Parse(json).Count==0)throw new InvalidDataException("设置内容为空。");
            lock (Gate)
            {
                if (blocked) throw new InvalidOperationException(LoadError);
                Directory.CreateDirectory(Folder);
                string path = Path.Combine(Folder, "setting.txt"), temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                string backup = path + ".bak";
                if (File.Exists(path))
                {
                    try { ReadFile(path); }
                    catch { File.Copy(path, path + ".corrupt-" + Guid.NewGuid().ToString("N"), false); backup = null; }
                }
                try
                {
                    string protectedText = Prefix + Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser));
                    File.WriteAllText(temporary, protectedText, Encoding.UTF8);
                    if (File.Exists(path)) File.Replace(temporary, path, backup); else File.Move(temporary, path);
                }
                finally { if (File.Exists(temporary)) File.Delete(temporary); }
            }
        }
    }
}
