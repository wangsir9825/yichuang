using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
    public static class DeepLClient
    {
        public static string Translate(string text, string host, string key, bool automatic, string source, string target)
        {
            return TranslateCancellable(text,host,key,automatic,source,target,()=>true,20000);
        }
        public static string TranslateCancellable(string text, string host, string key, bool automatic, string source, string target, Func<bool> current, int timeout)
        {
            if(!current())throw new OperationCanceledException();
            if (string.IsNullOrWhiteSpace(key))
                throw new InvalidOperationException("请在 DeepL 引擎设置中填写 API Key，并点击保存并返回。");
            key = key.Trim();
            if (key.Contains("*"))
                throw new InvalidOperationException("API Key 包含星号。请在 DeepL 网页点击 Copy key，复制完整密钥。");
            if (host != "api-free.deepl.com" && host != "api.deepl.com")
                throw new InvalidOperationException("DeepL 接口地址无效。");
            if (key.EndsWith(":fx", StringComparison.Ordinal) && host != "api-free.deepl.com")
                throw new InvalidOperationException("这枚密钥使用免费接口，请将账号类型改为 API Free / Developer。");
            if (string.IsNullOrWhiteSpace(text)) return "";
            var payload = new JObject { ["text"] = new JArray(text) };
            if (automatic)
                target = ClassMain.CalculateChineseCharacterPercentage(text) >= 0.35 ? "EN" : "ZH";
            else
            {
                if (string.IsNullOrWhiteSpace(target))
                    throw new InvalidOperationException("请填写译文语言代码，例如 ZH 或 EN。");
                if (!string.IsNullOrWhiteSpace(source) && !source.Equals("auto", StringComparison.OrdinalIgnoreCase))
                    payload["source_lang"] = source.Trim().ToUpperInvariant();
            }
            payload["target_lang"] = target.Trim().ToUpperInvariant();
            // Use the global network route while preserving certificate validation.
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            var request = NetworkProxy.Create("https://" + host + "/v2/translate");
            request.Method = "POST";
            request.ContentType = "application/json; charset=utf-8";
            request.Accept = "application/json";
            request.Headers["Authorization"] = "DeepL-Auth-Key " + key;
            request.Timeout = timeout;
            request.ReadWriteTimeout = timeout;
            request.AllowAutoRedirect = false;
            byte[] bytes = Encoding.UTF8.GetBytes(payload.ToString(Formatting.None));
            request.ContentLength = bytes.Length;
            DateTime deadline=DateTime.UtcNow.AddMilliseconds(timeout);
            using(var abort=new System.Threading.Timer(s=>{if(!current()||DateTime.UtcNow>=deadline)try{request.Abort();}catch{}},null,75,75))
            try
            {
                using (var stream = request.GetRequestStream()) stream.Write(bytes, 0, bytes.Length);
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    if((int)response.StatusCode>=300)throw new InvalidOperationException("DeepL 接口发生重定向，已停止请求。");
                    var json = JObject.Parse(ResponseReader.Read(reader));
                    if(!current())throw new OperationCanceledException();
                    var translations = json["translations"] as JArray;
                    if (translations == null || translations.Count == 0)
                        throw new InvalidOperationException("DeepL 没有返回译文，请重新尝试。");
                    var result = new StringBuilder();
                    foreach (JToken translation in translations)
                    {
                        string translated = (string)translation["text"];
                        if (string.IsNullOrWhiteSpace(translated))
                            throw new InvalidOperationException("DeepL 返回了空译文，请重新尝试。");
                        if (result.Length > 0) result.AppendLine();
                        result.Append(translated);
                    }
                    return result.ToString();
                }
            }
            catch (WebException ex)
            {
                if(!current())throw new OperationCanceledException();
                var response = ex.Response as HttpWebResponse;
                if (response != null)
                {
                    int status = (int)response.StatusCode;
                    response.Dispose();
                    switch (status)
                    {
                        case 400: throw new InvalidOperationException("DeepL 请求参数有误（HTTP 400），请检查语言代码。");
                        case 401:
                        case 403: throw new InvalidOperationException("DeepL API 认证失败（HTTP " + status + "），请检查完整密钥和账号类型。");
                        case 407: throw new InvalidOperationException(NetworkProxy.FailureHint(ex,status));
                        case 429: throw new InvalidOperationException("DeepL 请求过于频繁（HTTP 429），请稍后重试。");
                        case 456: throw new InvalidOperationException("DeepL 翻译额度已用完（HTTP 456），请查看账号 API usage。");
                        default: throw new InvalidOperationException("DeepL 服务请求失败（HTTP " + status + "），请稍后重试。");
                    }
                }
                if (ex.Status == WebExceptionStatus.Timeout || DateTime.UtcNow>=deadline)
                    throw new InvalidOperationException("DeepL 连接超时，请检查网络连接后重试。");
                if (ex.Status == WebExceptionStatus.NameResolutionFailure || ex.Status == WebExceptionStatus.ProxyNameResolutionFailure)
                    throw new InvalidOperationException("无法解析 DeepL 接口地址，请检查网络或代理连接。");
                throw new InvalidOperationException("无法连接 DeepL 接口（" + ex.Status + "），请检查网络或代理连接。");
            }
            catch (JsonException)
            {
                throw new InvalidOperationException("DeepL 返回了无法识别的响应，请稍后重试。");
            }
        }
    }
}
