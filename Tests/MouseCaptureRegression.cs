using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

class MouseCaptureRegression
{
    static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    static Assembly App;
    static string Data;
    static List<string> Results = new List<string>();
    static Point LastDown, LastUp;
    static int Downs, Ups;
    public static string Folder(Environment.SpecialFolder folder) { return Data; }
    public static void Down(object sender, MouseEventArgs e) { LastDown = e.Location; Downs++; }
    public static void Up(object sender, MouseEventArgs e) { LastUp = e.Location; Ups++; }
    class NetworkGuard : IWebRequestCreate { public WebRequest Create(Uri uri) { throw new Exception("Unexpected network request: " + uri.Host); } }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static object Call(object obj, string name, params object[] args) { return obj.GetType().GetMethod(name, Hidden).Invoke(obj, args); }
    static object Field(object obj, string name) { return obj.GetType().GetField(name, Hidden).GetValue(obj); }
    static object Gate() { return Activator.CreateInstance(App.GetType("YiChuang.MouseSelectionGate"), true); }
    static void Press(object gate, IntPtr window, int x, int y, long time, bool allowed = true) { Call(gate, "Down", new Point(x, y), window, time, allowed); }
    static object Release(object gate, IntPtr window, int x, int y, long time, bool allowed = true) { return Call(gate, "Up", new Point(x, y), window, time, allowed, 500, new Size(8, 8)); }
    static bool Current(object gate, object ticket, IntPtr window, long time, bool allowed = true) { return (bool)Call(gate, "IsCurrent", ticket, window, time, allowed); }
    static object Drag(object gate, IntPtr window, long time = 0) { Press(gate, window, 100, 100, time); return Release(gate, window, 180, 100, time + 100); }
    static void HookCoordinates()
    {
        var hook = Activator.CreateInstance(App.GetType("YiChuang.MouseHook"), true);
        foreach (string name in new[] { "Down", "Up" })
        {
            var evt = hook.GetType().GetEvent("Mouse" + name + "Event");
            evt.AddEventHandler(hook, Delegate.CreateDelegate(evt.EventHandlerType, typeof(MouseCaptureRegression).GetMethod(name)));
        }
        IntPtr data = Marshal.AllocHGlobal(24);
        try
        {
            for (int i = 0; i < 24; i += 4) Marshal.WriteInt32(data, i, 0);
            Marshal.WriteInt32(data, 0, 111); Marshal.WriteInt32(data, 4, 222);
            Call(hook, "MouseHookProc", 0, new IntPtr(513), data);
            Check(LastDown == new Point(111, 222), "Mouse-down used stale coordinates: " + LastDown);
            Marshal.WriteInt32(data, 0, 333); Marshal.WriteInt32(data, 4, 444);
            Call(hook, "MouseHookProc", 0, new IntPtr(514), data);
            Check(LastUp == new Point(333, 444), "Mouse-up used stale coordinates: " + LastUp);
            Marshal.WriteInt32(data, 12, 1);
            Call(hook, "MouseHookProc", 0, new IntPtr(513), data);
            Call(hook, "MouseHookProc", 0, new IntPtr(514), data);
            Check(Downs == 1 && Ups == 1, "Injected clicks reached physical gesture handlers");
            Call(hook, "MouseHookProc", -1, IntPtr.Zero, IntPtr.Zero);
        }
        finally { Marshal.FreeHGlobal(data); }
        Results.Add("Actual shipping low-level hook: current coordinates, injected events and negative hook codes: passed");
    }
    static void GestureCases()
    {
        IntPtr a = new IntPtr(100), b = new IntPtr(200);
        var gate = Gate();
        Check(Release(gate, a, 100, 100, 0) == null, "Orphan release captured text");
        for (int i = 0; i < 20; i++) { Call(gate, "ObserveWindow", a); Call(gate, "ObserveWindow", b); }
        Check(Release(gate, a, 900, 900, 100) == null, "Window switching created a gesture");
        Press(gate, a, 100, 100, 200); Check(Release(gate, a, 100, 100, 220) == null, "Single click captured");
        Press(gate, a, 900, 900, 250); Check(Release(gate, a, 900, 900, 280) == null, "Two distant clicks counted as a double-click");
        Press(gate, b, 900, 900, 300); Check(Release(gate, b, 900, 900, 320) == null, "Cross-window clicks counted as a double-click");
        Press(gate, b, 901, 901, 360); Check(Release(gate, b, 901, 901, 390) != null, "Real double-click was rejected");
        Press(gate, a, 10, 10, 1000); Check(Release(gate, b, 100, 10, 1100) == null, "Cross-window drag accepted");
        Press(gate, a, 10, 10, 1200); Call(gate, "Reset"); Check(Release(gate, a, 100, 10, 1300) == null, "Hook reset retained stale mouse-down");
        Press(gate, a, 10, 10, 1400, false); Check(Release(gate, a, 100, 10, 1500) == null, "Modified/disabled down accepted");
        Press(gate, a, 10, 10, 1600); Check(Release(gate, a, 100, 10, 1700, false) == null, "Modified/disabled release accepted");
        object ticket = Drag(gate, a, 1800); Check(Current(gate, ticket, a, 1910), "Valid selection rejected");
        Call(gate, "ObserveWindow", b); Call(gate, "ObserveWindow", a);
        Check(!Current(gate, ticket, a, 1920), "Alt+Tab away and back revived an old capture");
        ticket = Drag(gate, a, 2000); Check(!Current(gate, ticket, a, 2110, false), "Modifier/mode change retained capture");
        Check(!Current(gate, ticket, a, 2120), "Releasing modifier revived a cancelled capture");
        ticket = Drag(gate, a, 2200); Check(!Current(gate, ticket, a, 3501), "Expired capture accepted");
        ticket = Drag(gate, a, 3600); Drag(gate, a, 3750); Check(!Current(gate, ticket, a, 3890), "New gesture failed to cancel earlier one");
        Results.Add("Gesture matrix: no gesture, orphan up, single/distant/cross-window clicks, valid double-click/drag, modifiers, reset, expiry, newer gesture, away-and-back: passed");
    }
    static void CopyCases(Form main)
    {
        var run = App.GetType("YiChuang.MouseSelectionCopy").GetMethod("RunAsync", BindingFlags.Static | BindingFlags.NonPublic);
        var source = (RichTextBox)Field(main, "rtb原文");
        foreach (string scenario in new[] { "normal", "same-text-new-copy", "before-copy-switch", "after-copy-switch", "away-back", "modifier", "no-new-clipboard", "clipboard-race", "publish-race", "clipboard-busy" })
        {
            var gate = Gate(); IntPtr a = new IntPtr(100), b = new IntPtr(200), foreground = a;
            long clock = 100; bool allowed = true; object ticket = Drag(gate, a);
            uint sequence = 10; string clipboard = scenario == "same-text-new-copy" ? "Selected sentence." : "Previous clipboard";
            int copies = 0, published = 0, restored = 0, waits = 0, reads = 0;
            source.Text = "Existing original";
            Func<int, Task> delay = ms => {
                waits++; clock += ms;
                if (scenario == "before-copy-switch" && waits == 1 || scenario == "after-copy-switch" && waits == 2) foreground = b;
                if (scenario == "away-back" && waits == 2) { Call(gate, "ObserveWindow", b); Call(gate, "ObserveWindow", a); }
                if (scenario == "modifier" && waits == 2) allowed = false;
                return Task.FromResult(0);
            };
            Func<string> read = () => {
                reads++;
                if (scenario == "clipboard-busy") throw new ExternalException("Busy");
                if (scenario == "clipboard-race" && reads == 2) sequence++;
                return clipboard;
            };
            Action copy = () => { copies++; if (scenario != "no-new-clipboard") { sequence++; clipboard = "Selected sentence."; } };
            Action<string> publish = text => { published++; Call(main, "xianshi", text); if (scenario == "publish-race") sequence++; };
            var task = (Task)run.Invoke(null, new object[] { new Func<bool>(() => Current(gate, ticket, foreground, clock, allowed)), new Func<uint>(() => sequence), read,
                copy, new Action<string>(text => { restored++; clipboard = text; }), publish, delay });
            task.GetAwaiter().GetResult();
            bool success = scenario == "normal" || scenario == "same-text-new-copy" || scenario == "publish-race";
            Check(published == (success ? 1 : 0), scenario + ": incorrect publication count");
            Check(source.Text == (success ? "Selected sentence." : "Existing original"), scenario + ": original pane changed incorrectly");
            Check(restored == (success && scenario != "publish-race" ? 1 : 0), scenario + ": stale clipboard restore");
            if (scenario == "before-copy-switch" || scenario == "clipboard-busy") Check(copies == 0, scenario + ": copied without permission from current gesture");
            Results.Add("Shipping async copy pipeline + actual original pane: " + scenario + ": passed");
        }
    }
    [STAThread] static int Main(string[] args)
    {
        string output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output);
        using (var watchdog = new System.Threading.Timer(s => Environment.Exit(124), null, 30000, Timeout.Infinite))
        try
        {
            string exe = Path.GetFullPath(args[0]); Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve += (s, e) => { string f = Path.Combine(Path.GetDirectoryName(exe), new AssemblyName(e.Name).Name + ".dll"); return File.Exists(f) ? Assembly.LoadFrom(f) : null; };
            App = Assembly.LoadFrom(exe); HookCoordinates();
            if (args.Length > 2 && args[2] == "--hook-only") return 0;
            GestureCases();
            Data = Path.Combine(output, "isolated-data-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(Data);
            var proxies = (object[])App.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318] = Delegate.CreateDelegate(proxies[318].GetType(), typeof(MouseCaptureRegression).GetMethod("Folder"));
            App.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null, new EventWaitHandle(false, EventResetMode.AutoReset));
            App.GetType("RebuildInfo").GetField("DiagnosticOutput", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, output);
            WebRequest.RegisterPrefix("http://", new NetworkGuard()); WebRequest.RegisterPrefix("https://", new NetworkGuard()); Application.EnableVisualStyles();
            using (var main = (Form)Activator.CreateInstance(App.GetType("YiChuang.Form2")))
            {
                main.Opacity = 0; main.Show(); Application.DoEvents(); main.GetType().GetField("_isInitializing", Hidden).SetValue(main, true);
                Check((IntPtr)Field(main, "captureForegroundHook") != IntPtr.Zero, "Foreground event hook failed to install");
                CopyCases(main);
                Call(main, "SetCaptureMode", 1, true);
                var gate = Field(main, "mouseSelection"); var ticket = Drag(gate, new IntPtr(100));
                main.GetType().GetField("activeMouseCaptures", Hidden).SetValue(main, 1);
                Call(main, "timer1_Tick", null, EventArgs.Empty);
                Check(Current(gate, ticket, new IntPtr(100), 120), "Periodic hook refresh cancelled an active copy");
                main.GetType().GetField("activeMouseCaptures", Hidden).SetValue(main, 0);
                var source = (RichTextBox)Field(main, "rtb原文"); source.Text = "Keep original on clipboard notification";
                Call(main, "WndProc", Message.Create(main.Handle, 0x308, IntPtr.Zero, IntPtr.Zero));
                Check(source.Text == "Keep original on clipboard notification", "Clipboard notification alone triggered selection capture");
                Call(main, "SetCaptureMode", 2, true);
                Check(!Current(gate, ticket, new IntPtr(100), 150), "Changing capture mode retained pending gesture");
                main.Close();
                Check((IntPtr)Field(main, "captureForegroundHook") == IntPtr.Zero, "Foreground hook leaked after close");
            }
            Results.Add("Actual form: mode change cancels capture; hook refresh preserves active copy; clipboard notification alone does not change original; foreground hook install/disposal: passed");
            File.WriteAllLines(Path.Combine(output, "mouse-capture-regression.txt"), Results); Console.WriteLine("Mouse capture regression passed (hook, gesture matrix, 10 async pipeline cases, form lifecycle)."); return 0;
        }
        catch (Exception ex) { Results.Add(ex.ToString()); File.WriteAllLines(Path.Combine(output, "mouse-capture-regression.txt"), Results); Console.Error.WriteLine(ex); return 1; }
    }
}
