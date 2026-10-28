using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

// Compile with /platform:x86. Exercises the shipping preflight without creating
// a form, loading a model, or starting an inference service.
internal static class LlamaOfflinePreflightRegression
{
    const BindingFlags F = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static Assembly App;
    static string Data;
    static int AllowedPort, ExternalRequests;
    static readonly List<string> Log = new List<string>();
    public static string Folder(Environment.SpecialFolder _) { return Data; }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static Exception Unwrap(Exception error)
    {
        while (error is TargetInvocationException && error.InnerException != null) error = error.InnerException;
        return error;
    }
    static Exception InvokeRelease(Func<bool> current)
    {
        try { App.GetType("YiChuang.LocalModelClient").GetMethod("ReleaseForLlama", F).Invoke(null, new object[] { current, null }); return null; }
        catch (Exception error) { return Unwrap(error); }
    }
    static void Endpoint(int port)
    {
        AllowedPort = port;
        var settings = App.GetType("YiChuang.LocalModelSettings");
        settings.GetField("Endpoint", F).SetValue(null, "http://127.0.0.1:" + port);
        settings.GetField("Backend", F).SetValue(null, "llamacpp");
        var client = App.GetType("YiChuang.LocalModelClient");
        client.GetField("lastEndpoint", F).SetValue(null, null);
        client.GetField("lastModel", F).SetValue(null, null);
        object used = client.GetField("UsedModels", F).GetValue(null);
        used.GetType().GetMethod("Clear").Invoke(used, null);
    }
    sealed class Guard : IWebRequestCreate
    {
        public WebRequest Create(Uri uri)
        {
            if (uri.Scheme != "http" || uri.Host != "127.0.0.1" || uri.Port != AllowedPort)
            {
                Interlocked.Increment(ref ExternalRequests);
                throw new InvalidOperationException("Unexpected network request in isolated preflight test.");
            }
            return WebRequest.CreateDefault(uri);
        }
    }
    // A tiny loopback fixture, not an Ollama/model service. The TCP probe may
    // connect and immediately disconnect before the separate HTTP request.
    sealed class Server : IDisposable
    {
        readonly TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
        readonly ManualResetEvent stop = new ManualResetEvent(false);
        readonly List<TcpClient> clients = new List<TcpClient>();
        readonly List<Thread> workers = new List<Thread>();
        readonly Thread accept;
        readonly bool blackhole;
        readonly List<string> paths = new List<string>();
        public readonly ManualResetEvent RequestReceived = new ManualResetEvent(false);
        public readonly int Port;
        public Exception Failure;
        public Server(bool swallowRequests)
        {
            blackhole = swallowRequests;
            listener.Start();
            Port = ((IPEndPoint)listener.LocalEndpoint).Port;
            accept = new Thread(Accept) { IsBackground = true };
            accept.Start();
        }
        void Accept()
        {
            while (!stop.WaitOne(0))
            {
                TcpClient socket;
                try { socket = listener.AcceptTcpClient(); }
                catch (SocketException) { return; }
                catch (ObjectDisposedException) { return; }
                lock (clients) clients.Add(socket);
                var worker = new Thread(() => Handle(socket)) { IsBackground = true };
                lock (workers) workers.Add(worker);
                worker.Start();
            }
        }
        void Handle(TcpClient socket)
        {
            try
            {
                using (socket) using (var stream = socket.GetStream())
                {
                    stream.ReadTimeout = 1500;
                    var bytes = new List<byte>(); int value;
                    while ((value = stream.ReadByte()) >= 0)
                    {
                        bytes.Add((byte)value); int n = bytes.Count;
                        if (n >= 4 && bytes[n - 4] == 13 && bytes[n - 3] == 10 && bytes[n - 2] == 13 && bytes[n - 1] == 10) break;
                        if (n > 16384) throw new InvalidDataException("Oversized test HTTP header.");
                    }
                    if (bytes.Count == 0) return; // Connection-only preflight.
                    string[] first = Encoding.ASCII.GetString(bytes.ToArray()).Split(new[] { "\r\n" }, StringSplitOptions.None)[0].Split(' ');
                    Check(first.Length >= 2 && first[0] == "GET", "Unexpected preflight HTTP method.");
                    lock (paths) paths.Add(first[1]);
                    RequestReceived.Set();
                    Check(first[1] == "/api/ps", "Preflight called an endpoint other than /api/ps.");
                    if (blackhole) { stop.WaitOne(10000); return; }
                    byte[] body = Encoding.UTF8.GetBytes("{\"models\":[]}");
                    byte[] head = Encoding.ASCII.GetBytes("HTTP/1.1 200 OK\r\nContent-Type: application/json\r\nContent-Length: " + body.Length + "\r\nConnection: close\r\n\r\n");
                    stream.Write(head, 0, head.Length); stream.Write(body, 0, body.Length);
                }
            }
            catch (IOException) { } // Timed out/aborted test requests close their socket.
            catch (ObjectDisposedException) { }
            catch (SocketException) { }
            catch (Exception error) { Failure = error; }
        }
        public int Requests { get { lock (paths) return paths.Count; } }
        public void Dispose()
        {
            stop.Set(); listener.Stop(); accept.Join(1000);
            lock (clients) foreach (var socket in clients) socket.Close();
            Thread[] pending; lock (workers) pending = workers.ToArray();
            foreach (var worker in pending) worker.Join(1000);
            stop.Dispose(); RequestReceived.Dispose();
        }
    }
    static int FreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start(); int port = ((IPEndPoint)listener.LocalEndpoint).Port; listener.Stop(); return port;
    }
    static void RunCases()
    {
        Endpoint(FreePort());
        var watch = Stopwatch.StartNew(); Exception error = InvokeRelease(null);
        Check(error == null, "An absent Ollama blocked llama.cpp: " + error);
        Check(watch.ElapsedMilliseconds < 5000, "Offline preflight did not finish promptly.");
        Log.Add("PASS: refused loopback connection permits llama.cpp; " + watch.ElapsedMilliseconds + " ms.");

        using (var server = new Server(false))
        {
            Endpoint(server.Port); watch.Restart(); error = InvokeRelease(null);
            Check(error == null, "Responsive empty Ollama fixture failed: " + error);
            Check(server.Failure == null && server.Requests == 1, "Connected Ollama did not perform exactly one valid /api/ps check: " + server.Failure);
            Check(watch.ElapsedMilliseconds < 5000, "Responsive preflight did not finish promptly.");
            Log.Add("PASS: connected HTTP endpoint is checked via /api/ps; empty model list permits llama.cpp.");
        }
        using (var server = new Server(true))
        {
            Endpoint(server.Port); watch.Restart(); error = InvokeRelease(null);
            Check(server.Failure == null && server.Requests == 1, "Unresponsive fixture did not receive /api/ps: " + server.Failure);
            Check(error is InvalidOperationException && !(error is OperationCanceledException), "A listening but unresponsive endpoint was silently skipped: " + error);
            Check(watch.ElapsedMilliseconds >= 500 && watch.ElapsedMilliseconds < 6500, "Unresponsive endpoint did not fail within a bounded request timeout.");
            Log.Add("PASS: listening but unresponsive endpoint remains an error; " + watch.ElapsedMilliseconds + " ms.");
        }
        using (var server = new Server(true)) using (var cancellation = new ManualResetEvent(false))
        {
            Endpoint(server.Port);
            var cancel = new Thread(() => { if (server.RequestReceived.WaitOne(3000)) { Thread.Sleep(100); cancellation.Set(); } }) { IsBackground = true };
            cancel.Start(); watch.Restart(); error = InvokeRelease(() => !cancellation.WaitOne(0)); cancel.Join(3500);
            Check(server.Failure == null && server.Requests == 1 && cancellation.WaitOne(0), "Cancellation did not occur while /api/ps was pending.");
            Check(error is OperationCanceledException, "Cancellation was lost or converted to an ordinary failure: " + error);
            Check(watch.ElapsedMilliseconds < 1800, "Cancellation waited for the full HTTP timeout.");
            Log.Add("PASS: cancellation during pending /api/ps propagates OperationCanceledException; " + watch.ElapsedMilliseconds + " ms.");
        }
        Check(ExternalRequests == 0, "Test attempted a non-fixture HTTP request.");
        Check(Directory.GetFiles(Data, "*", SearchOption.AllDirectories).Length == 0, "Preflight unexpectedly wrote configuration.");
        Log.Add("PASS: no model loads, GUI, real user settings, or external HTTP requests were used.");
    }
    static int Main(string[] args)
    {
        if (args.Length != 2) { Console.Error.WriteLine("Usage: LlamaOfflinePreflightRegression.exe <YiChuang.exe> <isolated-output>"); return 2; }
        string output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output);
        using (var watchdog = new Timer(_ => Environment.Exit(124), null, 25000, Timeout.Infinite))
        try
        {
            Check(!Environment.Is64BitProcess, "This shipping assembly requires the x86 test build.");
            string executable = Path.GetFullPath(args[0]), installed = Path.GetDirectoryName(executable);
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) => { string path = Path.Combine(installed, new AssemblyName(e.Name).Name + ".dll"); return File.Exists(path) ? Assembly.LoadFrom(path) : null; };
            App = Assembly.LoadFrom(executable);
            Data = Path.Combine(output, "isolated-data-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Data);
            var proxies = (object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318] = Delegate.CreateDelegate(proxies[318].GetType(), typeof(LlamaOfflinePreflightRegression).GetMethod("Folder"));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput", F).SetValue(null, output);
            Check(WebRequest.RegisterPrefix("http://", new Guard()) && WebRequest.RegisterPrefix("https://", new Guard()), "Could not install isolated HTTP guard.");
            RunCases();
            File.WriteAllLines(Path.Combine(output, "llama-offline-preflight-regression.txt"), Log, Encoding.UTF8);
            Console.WriteLine(string.Join(Environment.NewLine, Log)); return 0;
        }
        catch (Exception error)
        {
            Log.Add("FAIL: " + Unwrap(error)); File.WriteAllLines(Path.Combine(output, "llama-offline-preflight-regression.txt"), Log, Encoding.UTF8);
            Console.Error.WriteLine(string.Join(Environment.NewLine, Log)); return 1;
        }
    }
}
