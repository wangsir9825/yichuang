using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

class WindowActivationRegression
{
    static readonly List<string> Log = new List<string>();
    [DllImport("user32.dll")] static extern int GetMessagePos();
    static void Check(bool condition, string reason) { if (!condition) throw new Exception(reason); }
    static IntPtr Dispatch(Control control, int id, IntPtr wparam, IntPtr lparam)
    {
        object[] args = { Message.Create(control.Handle, id, wparam, lparam) };
        control.GetType().GetMethod("WndProc", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(control, args);
        return ((Message)args[0]).Result;
    }
    static void Mouse(ToolStrip strip, ToolStripItem item)
    {
        var point = new Point(item.Bounds.Left + item.Width / 2, item.Bounds.Top + item.Height / 2);
        var position = (IntPtr)((point.Y << 16) | point.X);
        Dispatch(strip, 0x200, IntPtr.Zero, position);
        Dispatch(strip, 0x201, (IntPtr)1, position);
        Dispatch(strip, 0x202, IntPtr.Zero, position);
    }
    static void Exercise(Type type, bool expectedClick)
    {
        using (var window = new Form())
        using (var other = new Form())
        using (var strip = (ToolStrip)Activator.CreateInstance(type, true))
        {
            window.Opacity = other.Opacity = 0;
            window.Controls.Add(strip);
            int clicks = 0;
            var action = new ToolStripButton("Close"); action.Click += (s,e) => clicks++;
            strip.Items.Add(action);
            window.Show(); other.Show(); other.Activate(); Application.DoEvents();
            Check(Form.ActiveForm != window, "Fixture must have an inactive target");
            // ToolStrip hit testing uses the last queued message's screen point,
            // not WM_MOUSEACTIVATE.lParam (which contains message/hit-test codes).
            // Place this invisible test fixture there without moving the cursor.
            int packed = GetMessagePos();
            var lastPoint = new Point((short)packed, (short)(packed >> 16));
            var hitPoint = strip.PointToScreen(new Point(action.Bounds.Left + action.Width / 2, action.Bounds.Top + action.Height / 2));
            window.Location = new Point(window.Left + lastPoint.X - hitPoint.X, window.Top + lastPoint.Y - hitPoint.Y);
            // Reproduce the native WM_MOUSEACTIVATE contract. Windows delivers
            // the initiating mouse click only if the response does not eat it.
            int response = Dispatch(strip, 0x21, window.Handle, (IntPtr)((0x201 << 16) | 1)).ToInt32();
            if (response == 1) { window.Activate(); Mouse(strip, action); }
            Check(clicks == (expectedClick ? 1 : 0), "Inactive first click has the wrong result: " + type.Name + ", response=" + response);
            Check(response == (expectedClick ? 1 : 2), "Unexpected activation response");
            if (!expectedClick) { window.Activate(); Mouse(strip, action); Check(clicks == 1, "Old toolbar second click should execute"); }
            else
            {
                Check(Dispatch(strip, 0x21, window.Handle, (IntPtr)((0x201 << 16) | 1)) == (IntPtr)3, "Active toolbar must retain native no-activation behavior");
                Mouse(strip, action); Check(clicks == 2, "Active-window click must execute exactly once");
                action.Enabled = false; Mouse(strip, action); Check(clicks == 2, "Disabled action must remain disabled");
            }
            Log.Add(type.Name + ": inactive WM_MOUSEACTIVATE=" + response + "; click checks passed" + (expectedClick ? "; active no-activation and disabled button preserved" : ""));
        }
    }
    [STAThread] static int Main(string[] args)
    {
        string output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output);
        try
        {
            Application.EnableVisualStyles();
            var app = Assembly.LoadFrom(Path.GetFullPath(args[0]));
            Exercise(typeof(ToolStrip), false);
            Exercise(app.GetType("YiChuang.WindowActionStrip", true), true);
            File.WriteAllLines(Path.Combine(output,"activation.txt"), Log);
            Console.WriteLine(string.Join(Environment.NewLine, Log)); return 0;
        }
        catch(Exception error) { Log.Add(error.ToString()); File.WriteAllLines(Path.Combine(output,"activation.txt"), Log); Console.Error.WriteLine(error); return 1; }
    }
}
