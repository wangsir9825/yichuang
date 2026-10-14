using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YiChuang
{
    internal sealed class OcrPipeReceiver : IDisposable
    {
        internal const int MaximumBytes = 256 * 1024;
        private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
        private readonly object gate = new object();
        private NamedPipeServerStream active;
        private bool disposed;
        internal Task Completion { get; private set; }
        internal OcrPipeReceiver(Action<string> receive, Action<string> error)
        {
            Completion = Run(receive, error);
        }
        private async Task Run(Action<string> receive, Action<string> error)
        {
            while (!shutdown.IsCancellationRequested)
            {
                try
                {
                    var security = new PipeSecurity();
                    security.SetAccessRuleProtection(true, false);
                    security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null), PipeAccessRights.FullControl, AccessControlType.Deny));
                    using (var identity = WindowsIdentity.GetCurrent())
                        security.AddAccessRule(new PipeAccessRule(identity.User, PipeAccessRights.FullControl, AccessControlType.Allow));
                    using (var pipe = new NamedPipeServerStream("YiChuangOcr2918", PipeDirection.InOut, 1,
                        PipeTransmissionMode.Byte, PipeOptions.Asynchronous, 4096, 4096, security))
                    {
                        lock (gate) { if (disposed) return; active = pipe; }
                        await pipe.WaitForConnectionAsync(shutdown.Token).ConfigureAwait(false);
                        using (var timeout = new CancellationTokenSource(5000))
                        using (var abort = timeout.Token.Register(() => { try { pipe.Dispose(); } catch { } }))
                        using (var data = new MemoryStream())
                        {
                            byte[] buffer = new byte[4096]; int count;
                            while ((count = await pipe.ReadAsync(buffer, 0, buffer.Length, shutdown.Token).ConfigureAwait(false)) > 0)
                            {
                                if (data.Length + count > MaximumBytes) throw new InvalidDataException("OCR文字过长，请缩小截图范围。");
                                data.Write(buffer, 0, count);
                            }
                            timeout.Token.ThrowIfCancellationRequested();
                            // Decode once after EOF: UTF-8 characters may span pipe reads.
                            string text = new UTF8Encoding(false, true).GetString(data.ToArray());
                            if (!shutdown.IsCancellationRequested && !string.IsNullOrWhiteSpace(text)) receive(text);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (shutdown.IsCancellationRequested) break;
                    error(ex is InvalidDataException ? ex.Message : "OCR文字接收失败或超时，请重新截图。");
                    try { await Task.Delay(100, shutdown.Token).ConfigureAwait(false); } catch (OperationCanceledException) { break; }
                }
                finally { lock (gate) active = null; }
            }
        }
        public void Dispose()
        {
            NamedPipeServerStream pipe;
            lock (gate) { if (disposed) return; disposed = true; pipe = active; }
            // Cancellation can synchronously wait for Run finally to acquire gate.
            // Never hold that lock while cancelling or disposing pending pipe IO.
            shutdown.Cancel(); pipe?.Dispose();
            // The loop still observes the token while its pending async read unwinds.
            Completion.ContinueWith(t => shutdown.Dispose(), TaskScheduler.Default);
        }
    }
}
