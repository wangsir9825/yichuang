using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
    // Immutable snapshot: an in-flight request keeps its route; edits affect only
    // requests started after a successful Save/Apply. Never change Windows proxy.
    internal sealed class NetworkProxyOptions
    {
        internal readonly string Mode, Address;
        internal NetworkProxyOptions(string mode, string address) { Mode=mode; Address=(address??"").Trim(); }
        internal void Validate()
        {
            if(Mode!="system" && Mode!="direct" && Mode!="manual")
                throw new InvalidOperationException("请选择网络连接方式：跟随系统代理、直连或指定代理。");
            if(Mode=="manual") ValidateAddress(Address);
        }
        internal static Uri ValidateAddress(string value)
        {
            Uri uri;
            if(!Uri.TryCreate(value,UriKind.Absolute,out uri) || uri.Scheme!="http" ||
                string.IsNullOrWhiteSpace(uri.Host) || uri.Port<1 || uri.Port>65535 ||
                uri.UserInfo.Length!=0 || uri.Query.Length!=0 || uri.Fragment.Length!=0 || uri.AbsolutePath!="/" ||
                value.IndexOfAny(new[]{'\r','\n','\t',' '})>=0)
                throw new InvalidOperationException("请填写有效的 HTTP 代理地址，例如 http://127.0.0.1:10808。不能填写订阅链接或 SOCKS 地址，也不能在地址中包含账号密码。");
            return uri;
        }
    }
    internal static class NetworkProxy
    {
        internal static volatile NetworkProxyOptions Current=new NetworkProxyOptions("system","");
        internal static Func<IWebProxy> SystemProxyFactory=WebRequest.GetSystemWebProxy;
        internal static NetworkProxyOptions Parse(JObject json)
        { return new NetworkProxyOptions((string)json["NetworkProxyMode"]??"system",(string)json["NetworkProxyAddress"]??""); }
        internal static void Read(JObject json) { Current=Parse(json); }
        internal static void Write(Dictionary<string,string> data, NetworkProxyOptions options=null)
        {
            var value=options??Current;
            data["NetworkProxyMode"]=value.Mode;data["NetworkProxyAddress"]=value.Address;
        }
        internal static HttpWebRequest Create(string url) { return Create(new Uri(url)); }
        internal static HttpWebRequest Create(Uri url)
        {
            var request=(HttpWebRequest)WebRequest.Create(url);Apply(request);return request;
        }
        internal static void Apply(HttpWebRequest request)
        {
            // Local inference works offline; even a broken manual proxy must not
            // intercept loopback (including custom OpenAI-compatible local servers).
            if(request.RequestUri.IsLoopback) { request.Proxy=null;return; }
            var options=Current;options.Validate();
            if(options.Mode=="direct")request.Proxy=null;
            else if(options.Mode=="manual")request.Proxy=new WebProxy(NetworkProxyOptions.ValidateAddress(options.Address),false);
            else request.Proxy=SystemProxyFactory();
        }
        internal static string FailureHint(WebException error,int httpStatus=0)
        {
            if(httpStatus==407)return "代理要求身份验证（HTTP 407）；请使用无需身份验证的 HTTP/混合代理端口，或在代理客户端完成认证。";
            if(error.Status==WebExceptionStatus.ProxyNameResolutionFailure)return "无法解析代理地址，请检查“设置 → 网络代理”。";
            return "请检查网络及“设置 → 网络代理”；使用局域网代理时，请确认代理电脑的 IP、端口和允许局域网连接设置。";
        }
    }
}
