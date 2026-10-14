using System;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace YiChuang
{
    internal sealed class RequestLifetime : IDisposable
    {
        private readonly Func<bool> current;
        private readonly Stopwatch elapsed = Stopwatch.StartNew();
        private readonly Timer timer;
        private readonly int timeout;
        private volatile HttpWebRequest request;
        internal RequestLifetime(Func<bool> current, int timeout)
        {
            this.current=current??(()=>true);this.timeout=timeout;
            timer=new Timer(s=>{try{if(!this.current()||elapsed.ElapsedMilliseconds>=timeout)request?.Abort();}catch{}},null,75,75);
        }
        internal void Attach(HttpWebRequest value)
        {
            request=value;value.Timeout=timeout;value.ReadWriteTimeout=timeout;value.AllowAutoRedirect=false;
            Check();
        }
        internal void Check()
        {
            if(!current())throw new OperationCanceledException();
            if(elapsed.ElapsedMilliseconds>=timeout)throw new InvalidOperationException("翻译请求超时，请稍后重试。");
        }
        public void Dispose(){timer.Dispose();}
    }
}
