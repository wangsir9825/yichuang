using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
    internal class ChatGPT
    {
        public delegate void MyDelegate(string message);
        public static event MyDelegate MyChatGPTEvent;
        public static void TriggerEvent(string message) { MyChatGPTEvent?.Invoke(message); }
        public static string LoadSourceCode4(object msg, string domain, string apiKey, string model, Func<bool> current = null)
        {
            if (domain != "api.deepseek.com") throw new InvalidOperationException("DeepSeek接口地址无效。");
            if (string.IsNullOrWhiteSpace(apiKey) || apiKey.IndexOfAny(new[] {'\r','\n'}) >= 0) throw new InvalidOperationException("请填写完整有效的DeepSeek API Key。");
            if (string.IsNullOrWhiteSpace(model)) throw new InvalidOperationException("请填写DeepSeek模型名称。");
            var payload = new JObject {
                ["model"]=model, ["stream"]=true, ["temperature"]=0.5, ["frequency_penalty"]=0,
                ["thinking_budget"]=1, ["thinking"]=new JObject {["type"]="disabled"}, ["enable_thinking"]=false,
                ["messages"]=new JArray(new JObject {["role"]="user",["content"]=Convert.ToString(msg).Trim()})
            };
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            using(var lifetime = new RequestLifetime(current,60000))
            try {
                var request=(HttpWebRequest)WebRequest.Create("https://api.deepseek.com/v1/chat/completions");
                lifetime.Attach(request);request.Method="POST";request.ContentType="application/json; charset=utf-8";
                request.Headers["Authorization"]="Bearer "+apiKey.Trim();request.ServicePoint.Expect100Continue=false;
                byte[] bytes=Encoding.UTF8.GetBytes(payload.ToString(Formatting.None));request.ContentLength=bytes.Length;
                using(var stream=request.GetRequestStream())stream.Write(bytes,0,bytes.Length);
                using(var response=(HttpWebResponse)request.GetResponse()) {
                    if((int)response.StatusCode<200||(int)response.StatusCode>=300)throw new InvalidOperationException("DeepSeek接口重定向，已停止请求。");
                    using(var reader=new StreamReader(response.GetResponseStream(),Encoding.UTF8))return ReadTranslation(reader,lifetime.Check);
                }
            } catch(WebException ex) {
                int status=0;if(ex.Response!=null){using(var response=ex.Response) { var http=response as HttpWebResponse;if(http!=null)status=(int)http.StatusCode; }}
                lifetime.Check();
                if(status==401||status==403)throw new InvalidOperationException("API 认证失败（HTTP "+status+"），请检查完整的DeepSeek API Key。");
                string hint=status==401||status==403?"请检查API Key。":status==402?"账号额度不足。":status==429?"请求频繁或额度不足，请稍后重试。":status==400||status==404?"请检查模型名称和参数。":"请检查网络或稍后重试。";
                throw new InvalidOperationException("DeepSeek请求失败"+(status>0?"（HTTP "+status+"）":"")+"，"+hint);
            } catch(IOException) { lifetime.Check();throw new InvalidOperationException("DeepSeek连接中断，译文未完成，请重试。"); }
        }
        internal static string ReadTranslation(TextReader reader,Action check)
        {
            var result=new StringBuilder();bool finished=false;string line;int received=0;
            while((line=ResponseReader.Line(reader))!=null) {
                check();received+=line.Length;
                if(received>ResponseReader.MaximumCharacters)throw new InvalidOperationException("流式响应过长，已停止读取。");
                if(!line.StartsWith("data:",StringComparison.Ordinal))continue;
                string data=line.Substring(5).Trim();if(data=="[DONE]"){finished=true;break;}
                JObject json;
                try { json=JObject.Parse(data); } catch(JsonException) { throw new InvalidOperationException("DeepSeek流式响应格式错误，译文未完成，请重试。"); }
                if(json["error"]!=null)throw new InvalidOperationException("DeepSeek在翻译过程中返回错误，请检查模型、密钥和额度。");
                var choices=json["choices"] as JArray;if(choices==null||choices.Count==0)continue;
                var choice=choices[0];string delta=(string)choice["delta"]?["content"];
                if(!string.IsNullOrEmpty(delta)){result.Append(delta);TriggerEvent(delta);}
                string finish=(string)choice["finish_reason"];
                if(finish=="length")throw new InvalidOperationException("DeepSeek译文达到长度限制，请缩短原文后重试。");
                if(!string.IsNullOrEmpty(finish)&&finish!="stop")throw new InvalidOperationException("DeepSeek未正常完成翻译，请检查输入后重试。");
                if(finish=="stop"){finished=true;break;}
            }
            check();
            if(!finished)throw new InvalidOperationException("DeepSeek连接提前结束，译文未完成，请重试。");
            if(string.IsNullOrWhiteSpace(result.ToString()))throw new InvalidOperationException("DeepSeek没有返回译文，请重试。");
            return result.ToString();
        }
    }
}
