using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Newtonsoft.Json;

namespace YiChuang
{
    public sealed class EngineDefinition
    {
        public int Id;
        public string Name = "", Provider = "openai", Endpoint = "", ApiKey = "", Model = "";
        public string Direction = "auto", Prompt = "generic", SourceLanguage = "", TargetLanguage = "ZH";
        public int TimeoutSeconds = 180;
        public bool Automatic = true;
        public EngineDefinition Copy() { return (EngineDefinition)MemberwiseClone(); }
        public override string ToString() { return EngineLabels.MenuText(Id); }
    }

    // Stable IDs are independent of menu order. The registry is stored inside the existing encrypted settings/backup.
    public static class EngineRegistry
    {
        public static readonly int[] BuiltinIds = {4,5,6,14,16,17,18,19};
        public static readonly int[] DefaultVisibleIds = {4,5,14,18};
        private static readonly object Gate = new object();
        private static List<int> active = new List<int>(BuiltinIds);
        private static HashSet<int> visible = new HashSet<int>(DefaultVisibleIds);
        private static List<EngineDefinition> custom = new List<EngineDefinition>();
        private static int nextId = 1000;
        private static string unreadable;
        public static string LoadError { get; private set; }
        private sealed class State
        {
            public int Version = 2, NextId = 1000;
            public List<int> Active;
            public List<int> Visible;
            public List<EngineDefinition> Custom;
        }
        public static bool IsBuiltin(int id) { return Array.IndexOf(BuiltinIds,id) >= 0; }
        public static int[] ActiveIds { get { lock(Gate) return active.ToArray(); } }
        public static int[] VisibleIds { get { lock(Gate) return active.Where(id=>visible.Contains(id)).ToArray(); } }
        public static bool IsVisible(int id) { lock(Gate) return active.Contains(id) && visible.Contains(id); }
        public static void SetVisible(int id, bool show)
        {
            lock(Gate) { if(!active.Contains(id))return; if(show)visible.Add(id);else visible.Remove(id); unreadable=null;LoadError=null; }
            if(!show && ClassMain.isjiekounum==id)ClassMain.isjiekounum=0;
        }
        public static void Move(int id, int offset)
        {
            lock(Gate) {int from=active.IndexOf(id),to=from+offset;if(from<0||to<0||to>=active.Count)return;active.RemoveAt(from);active.Insert(to,id);unreadable=null;LoadError=null;}
        }
        public static EngineDefinition Get(int id)
        {
            lock(Gate) {
                if(!active.Contains(id)) return null;
                return IsBuiltin(id) ? Builtin(id) : custom.FirstOrDefault(e=>e.Id==id)?.Copy();
            }
        }
        public static EngineDefinition Builtin(int id)
        {
            var e = new EngineDefinition {Id=id,Name=EngineLabels.BuiltinName(id)};
            switch(id) {
                case 4: e.Provider="transmart";break;
                case 5: e.Provider="sogou";break;
                case 6: e.Provider="deepl";e.Endpoint=ClassMain.DeeplInterface=="3"?"https://api.deepl.com":"https://api-free.deepl.com";e.ApiKey=ClassMain.Deeplkey_official;e.Automatic=ClassMain.DeeplLanguage=="1";e.SourceLanguage=ClassMain.DeeplFromLang;e.TargetLanguage=ClassMain.DeeplToLang;break;
                case 14: e.Provider="openai";e.Endpoint="https://api.deepseek.com/v1";e.ApiKey=ClassMain.DeepSeekAI_key;e.Model=ClassMain.DeepSeekAI_model;break;
                case 16: case 17: e.Provider=LocalModelSettings.Backend;e.Endpoint=LocalModelSettings.Endpoint;e.Model=id==16?LocalModelSettings.HunyuanModel:LocalModelSettings.GemmaModel;e.Prompt=id==16?"hy":"gemma";e.Direction=LocalModelSettings.Direction;break;
                case 18: e.Provider="gemini";e.Endpoint="https://generativelanguage.googleapis.com/v1beta";e.ApiKey=ClassMain.GeminiKey;e.Model=ClassMain.GeminiModel;e.Direction=ClassMain.GeminiDirection;break;
                case 19: e.Provider="qwen";e.Endpoint=ClassMain.QwenEndpoint;e.ApiKey=ClassMain.QwenKey;e.Model="qwen-mt-plus";e.Direction=ClassMain.QwenDirection;break;
                default: return null;
            }
            return e;
        }
        public static Uri ValidateApiEndpoint(string value)
        {
            Uri uri; IPAddress ip;
            if(!Uri.TryCreate((value??"").Trim().TrimEnd('/'),UriKind.Absolute,out uri) ||
               !(uri.Scheme=="https" || uri.Scheme=="http" && (uri.Host.Equals("localhost",StringComparison.OrdinalIgnoreCase) || IPAddress.TryParse(uri.Host,out ip)&&IPAddress.IsLoopback(ip))) ||
               uri.UserInfo.Length>0 || uri.Query.Length>0 || uri.Fragment.Length>0)
                throw new InvalidOperationException("接口地址须为 HTTPS；本机服务可用 HTTP。地址中不要包含密钥、查询参数或用户名。");
            return uri;
        }
        public static void Validate(EngineDefinition e)
        {
            if(e==null || string.IsNullOrWhiteSpace(e.Name) || e.Name.Trim().Length>64 || e.Name.Any(char.IsControl)) throw new InvalidOperationException("请填写1至64个字符的引擎名称。");
            if(e.TimeoutSeconds<10 || e.TimeoutSeconds>600) throw new InvalidOperationException("超时应为10至600秒。");
            if(e.Direction!="auto"&&e.Direction!="en-zh"&&e.Direction!="zh-en"&&e.Direction!="to-en"&&e.Direction!="to-zh") throw new InvalidOperationException("翻译方向无效。");
            if((e.ApiKey??"").Any(char.IsControl)) throw new InvalidOperationException("API Key 不能包含换行或控制字符。");
            if(e.Provider=="llamacpp") {
                if(string.IsNullOrWhiteSpace(e.Model)||!System.IO.Path.IsPathRooted(e.Model)||e.Model.Any(char.IsControl))throw new InvalidOperationException("请选择本机 GGUF 模型的完整路径。");
                if(e.Prompt!="generic"&&e.Prompt!="hy"&&e.Prompt!="gemma")throw new InvalidOperationException("本地模型提示模板无效。");
            } else if(e.Provider=="ollama") {
                LocalModelClient.ValidateEndpoint(e.Endpoint);LocalModelClient.ValidateModel(e.Model);
                if(e.Prompt!="generic"&&e.Prompt!="hy"&&e.Prompt!="gemma") throw new InvalidOperationException("本地模型提示模板无效。");
            } else if(e.Provider=="openai") {
                var uri=ValidateApiEndpoint(e.Endpoint);
                if(uri.AbsolutePath.EndsWith("/chat/completions",StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("请填写 API 基础地址，例如 https://服务商/v1，不要包含 /chat/completions。");
            } else if(e.Provider=="gemini") {
                CloudTranslationClient.ValidateGeminiModel(e.Model);
                if(e.Endpoint!="https://generativelanguage.googleapis.com/v1beta")throw new InvalidOperationException("Gemini 专用接口使用 Google 官方地址；兼容服务请选择 OpenAI 兼容接口。");
            } else if(e.Provider=="qwen") CloudTranslationClient.ValidateQwenEndpoint(e.Endpoint);
            else if(e.Provider=="deepl") {
                if(e.Endpoint!="https://api-free.deepl.com"&&e.Endpoint!="https://api.deepl.com")throw new InvalidOperationException("DeepL 请选择官方 Free 或 Pro 接口地址。");
                if(!e.Automatic&&string.IsNullOrWhiteSpace(e.TargetLanguage))throw new InvalidOperationException("请填写译文语言代码。");
            } else throw new InvalidOperationException("不支持的接口类型。");
            if(e.Provider!="deepl"&&e.Provider!="llamacpp"&&(string.IsNullOrWhiteSpace(e.Model)||e.Model.Length>200||e.Model.Any(char.IsControl)))throw new InvalidOperationException("请填写有效的模型名称（不超过200个字符）。");
        }
        public static int Save(EngineDefinition value)
        {
            if(value==null)throw new InvalidOperationException("引擎配置不能为空。");
            var e=value.Copy();e.Name=(e.Name??"").Trim();e.Endpoint=(e.Endpoint??"").Trim().TrimEnd('/');e.Model=(e.Model??"").Trim();e.ApiKey=(e.ApiKey??"").Trim();Validate(e);
            lock(Gate) {
                if(active.Count>=100&&e.Id==0)throw new InvalidOperationException("最多可配置100个引擎。");
                if(nextId>=int.MaxValue-1)throw new InvalidOperationException("引擎编号已达上限，请从配置备份恢复。");
                if(e.Id==0){e.Id=nextId++;custom.Add(e);active.Add(e.Id);visible.Add(e.Id);}
                else {int index=custom.FindIndex(x=>x.Id==e.Id);if(index<0)throw new InvalidOperationException("该引擎已被移除，请重新新增。");custom[index]=e;}
                unreadable=null;LoadError=null;return e.Id;
            }
        }
        public static void Remove(int id)
        {
            lock(Gate){active.Remove(id);visible.Remove(id);custom.RemoveAll(e=>e.Id==id);unreadable=null;LoadError=null;}
            // Do not silently select a different service, especially a cloud service.
            if(ClassMain.isjiekounum==id)ClassMain.isjiekounum=0;
        }
        public static void Restore(int id)
        {
            if(!IsBuiltin(id))throw new InvalidOperationException("只能恢复内置引擎。");
            lock(Gate){if(!active.Contains(id)){if(active.Count>=100)throw new InvalidOperationException("最多可配置100个引擎。");active.Add(id);visible.Add(id);}unreadable=null;LoadError=null;}
        }
        public static string Export()
        {
            lock(Gate)return unreadable??JsonConvert.SerializeObject(new State{NextId=nextId,Active=active,Visible=active.Where(id=>visible.Contains(id)).ToList(),Custom=custom});
        }
        private static State ParseState(string json)
        {
            var s=JsonConvert.DeserializeObject<State>(json);
            if(s==null||(s.Version!=1&&s.Version!=2)||s.Active==null||s.Custom==null||s.NextId>=int.MaxValue-1||s.Active.Count>100||s.Custom.Count>100||s.Active.Distinct().Count()!=s.Active.Count||s.Custom.Any(e=>e==null)||s.Custom.Select(e=>e.Id).Distinct().Count()!=s.Custom.Count||s.Custom.Any(e=>e.Id<1000||e.Id>=int.MaxValue-1))throw new InvalidOperationException("引擎配置格式无效。");
            foreach(var e in s.Custom)Validate(e);
            if(s.Active.Any(id=>!IsBuiltin(id)&&!s.Custom.Any(e=>e.Id==id))||s.Custom.Any(e=>!s.Active.Contains(e.Id)))throw new InvalidOperationException("引擎编号不一致。");
            if(s.Version==2&&(s.Visible==null||s.Visible.Distinct().Count()!=s.Visible.Count||s.Visible.Any(id=>!s.Active.Contains(id))))throw new InvalidOperationException("引擎显示列表无效。");
            return s;
        }
        internal static void ValidateImport(string json) { if(!string.IsNullOrWhiteSpace(json))ParseState(json); }
        public static void Import(string json)
        {
            lock(Gate) {
                if(string.IsNullOrWhiteSpace(json)){active=new List<int>(BuiltinIds);visible=new HashSet<int>(DefaultVisibleIds);custom=new List<EngineDefinition>();nextId=1000;unreadable=null;LoadError=null;return;}
                try {
                    var s=ParseState(json);
                    active=s.Active;visible=new HashSet<int>(s.Version==1?active.Where(id=>DefaultVisibleIds.Contains(id)):s.Visible);custom=s.Custom;nextId=Math.Max(1000,Math.Max(s.NextId,custom.Count==0?1000:custom.Max(e=>e.Id)+1));unreadable=null;LoadError=null;
                } catch {
                    active=new List<int>();visible=new HashSet<int>();custom=new List<EngineDefinition>();unreadable=json;
                    LoadError="引擎配置无法读取，已暂停翻译。可从配置备份恢复，或在引擎管理中重新添加。";
                }
            }
        }
    }
}
