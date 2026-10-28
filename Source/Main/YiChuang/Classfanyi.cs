using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
    internal class Classfanyi
    {
        public static string path_美 = "";
        private const string UserAgent="Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36";
        private static HttpWebRequest Request(string url,RequestLifetime lifetime,CookieContainer cookies=null)
        {
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var request=NetworkProxy.Create(url);lifetime.Attach(request);
            request.UserAgent=UserAgent;request.CookieContainer=cookies;request.ServicePoint.Expect100Continue=false;
            return request;
        }
        private static JObject Post(HttpWebRequest request,JObject payload,RequestLifetime lifetime)
        {
            request.Method="POST";request.ContentType="application/json; charset=utf-8";request.Accept="application/json";
            byte[] bytes=Encoding.UTF8.GetBytes(payload.ToString(Formatting.None));request.ContentLength=bytes.Length;
            using(var stream=request.GetRequestStream())stream.Write(bytes,0,bytes.Length);
            using(var response=(HttpWebResponse)request.GetResponse()) {
                if((int)response.StatusCode<200||(int)response.StatusCode>=300)throw new InvalidOperationException("网页翻译接口发生重定向，已停止请求。");
                using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8)) {
                    string text=ResponseReader.Read(reader);lifetime.Check();return JObject.Parse(text);
                }
            }
        }
        private static string Translate(Func<RequestLifetime,string> action,Func<bool> current)
        {
            using(var lifetime=new RequestLifetime(current,20000))
            try { lifetime.Check();return action(lifetime); }
            catch(WebException ex) { int status=0;using(var response=ex.Response) {if(response is HttpWebResponse)status=(int)((HttpWebResponse)response).StatusCode;}lifetime.Check();throw new InvalidOperationException("网页翻译请求失败"+(status>0?"（HTTP "+status+"）":"")+"。"+NetworkProxy.FailureHint(ex,status)); }
            catch(JsonException) {throw new InvalidOperationException("网页翻译返回格式不正确，接口可能已变化。");}
            catch(IOException) {lifetime.Check();throw new InvalidOperationException("网页翻译连接中断，请重试。");}
        }
        public static string getSouGou(string text,string targetLanguage=null,Func<bool> current=null)
        {
            return Translate(lifetime=>{
                var cookies=new CookieContainer();
                cookies.Add(new Uri("https://fanyi.sogou.com/"),new Cookie("FUV","0daa4bca33bc3b80b1cfc869e7bdbb40"));
                cookies.Add(new Uri("https://fanyi.sogou.com/"),new Cookie("SNUID","B6C3A812CDC81294CBC95D0DCEE38232"));
                var preflight=Request("https://fanyi.sogou.com/",lifetime,cookies);
                using(var response=(HttpWebResponse)preflight.GetResponse()) {
                    if((int)response.StatusCode>=300)throw new InvalidOperationException("搜狗网页入口发生重定向，请稍后重试。");
                    using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8))ResponseReader.Read(reader);
                }
                lifetime.Check();string target=(targetLanguage??TranslationDirection.Target(text,"auto"))=="EN"?"en":"zh-CHS";
                var payload=new JObject {["from"]="auto",["to"]=target,["text"]=text,["client"]="pc",["fr"]="browser_pc",["needQc"]=1,
                    ["s"]=ClassMain.EncryptWithMD5("auto"+target+text+"109984457"),["uuid"]=Guid.NewGuid().ToString("D"),["exchange"]=false};
                var request=Request("https://fanyi.sogou.com/api/transpc/text/result",lifetime,cookies);request.Referer="https://fanyi.sogou.com/";request.Headers["x-requested-with"]="XMLHttpRequest";
                return (string)Post(request,payload,lifetime)["data"]?["translate"]?["dit"]??"";
            },current);
        }
        public static string getTransmart(string text,string targetLanguage=null,Func<bool> current=null)
        {
            return Translate(lifetime=>{
                bool chinese=ClassMain.判断是不是中译英(text);string target=(targetLanguage??(chinese?"EN":"ZH"))=="EN"?"en":"zh";
                var payload=new JObject {
                    ["header"]=new JObject {["fn"]="auto_translation",["client_key"]="browser-chrome-113.0.0-Windows 10-"+Guid.NewGuid()+"-"+ClassMain.GetTimeStamp()},
                    ["type"]="plain",["model_category"]="normal",["text_domain"]="general",
                    ["source"]=new JObject {["lang"]=chinese?"zh":"en",["text_list"]=new JArray(text.Split(new[]{"\n"},StringSplitOptions.None))},
                    ["target"]=new JObject {["lang"]=target}};
                var request=Request("https://transmart.qq.com/api/imt",lifetime);request.Referer="https://transmart.qq.com/zh-CN/index";
                var parts=Post(request,payload,lifetime)["auto_translation"] as JArray;
                if(parts==null)throw new InvalidOperationException("Transmart未返回译文，请稍后重试。");
                var output=new StringBuilder();foreach(var part in parts)output.AppendLine((string)part);return output.ToString();
            },current);
        }
    }
}
