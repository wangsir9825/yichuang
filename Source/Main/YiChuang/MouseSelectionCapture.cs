using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YiChuang
{
    public partial class Form2
    {
        private readonly MouseSelectionGate mouseSelection = new MouseSelectionGate();
        private readonly Stopwatch mouseCaptureClock = Stopwatch.StartNew();
        private IntPtr captureForegroundHook;
        private int activeMouseCaptures;
        private CaptureWinEvent captureForegroundCallback;
        private delegate void CaptureWinEvent(IntPtr hook, uint evt, IntPtr window, int objectId, int childId, uint thread, uint time);
        [DllImport("user32.dll")] private static extern IntPtr SetWinEventHook(uint first, uint last, IntPtr module, CaptureWinEvent callback, uint process, uint thread, uint flags);
        [DllImport("user32.dll")] private static extern bool UnhookWinEvent(IntPtr hook);
        [DllImport("user32.dll")] private static extern short GetAsyncKeyState(int key);
        [DllImport("user32.dll")] private static extern uint GetClipboardSequenceNumber();
        [DllImport("user32.dll")] private static extern IntPtr GetAncestor(IntPtr window, uint flags);
        [DllImport("user32.dll")] private static extern bool GetClientRect(IntPtr window, out Rectangle rectangle);
        [DllImport("user32.dll")] private static extern bool ScreenToClient(IntPtr window, ref Point point);

        private void InitializeMouseCapture()
        {
            mouseSelection.ObserveWindow(GetForegroundWindow());
            captureForegroundCallback = (hook, evt, window, obj, child, thread, time) => { mouseSelection.ObserveWindow(window); ExplicitForegroundChanged(window); };
            captureForegroundHook = SetWinEventHook(3, 3, IntPtr.Zero, captureForegroundCallback, 0, 0, 0);
        }
        private void DisposeMouseCapture()
        {
            mouseSelection.Reset();
            if (captureForegroundHook != IntPtr.Zero) { UnhookWinEvent(captureForegroundHook); captureForegroundHook = IntPtr.Zero; }
        }
        private bool MouseCaptureAllowed(IntPtr window)
        {
            if (IsDisposed || Disposing || !ClassMain._is自动翻译总开关 || _翻译模式 != 1 ||
                ClassSetHotKey.Suspended || ClassMain._is最小化 || IsTranslatorWindow(window)) return false;
            foreach (int key in new[] { 0x10, 0x11, 0x12, 0x09, 0x5B, 0x5C })
                if ((GetAsyncKeyState(key) & 0x8000) != 0) return false;
            var className = new StringBuilder(256);
            GetClassName(window, className, className.Capacity);
            switch (className.ToString())
            {
                case "CabinetWClass": case "Progman": case "WorkerW": case "XLMAIN":
                case "TscShellContainerClass": case "SDL_app": case "H-SMILE-FRAME": return false;
            }
            return true;
        }
        private bool CapturePointInWindow(Point point, IntPtr window)
        {
            Rectangle client;
            return GetAncestor(WindowFromPoint(point), 2) == window && GetClientRect(window, out client) &&
                ScreenToClient(window, ref point) && client.Contains(point);
        }
        private void mh_MouseDownEvent(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { mouseSelection.Reset(); return; }
            // A real drag may start with a click that activates an inactive window.
            IntPtr window = GetAncestor(WindowFromPoint(e.Location), 2);
            mouseSelection.Down(e.Location, window, mouseCaptureClock.ElapsedMilliseconds,
                MouseCaptureAllowed(window) && CapturePointInWindow(e.Location, window));
        }
        private void mh_MouseUpEvent(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) { mouseSelection.Reset(); return; }
            IntPtr window = GetForegroundWindow();
            var ticket = mouseSelection.Up(e.Location, window, mouseCaptureClock.ElapsedMilliseconds,
                MouseCaptureAllowed(window) && CapturePointInWindow(e.Location, window), SystemInformation.DoubleClickTime, SystemInformation.DoubleClickSize);
            if (ticket != null) CaptureMouseSelection(ticket);
        }
        private async void CaptureMouseSelection(MouseSelectionGate.Ticket ticket)
        {
            activeMouseCaptures++;
            try
            {
                await MouseSelectionCopy.RunAsync(
                    () => { var window = GetForegroundWindow(); return mouseSelection.IsCurrent(ticket, window, mouseCaptureClock.ElapsedMilliseconds, MouseCaptureAllowed(window)); },
                    GetClipboardSequenceNumber,
                    () => Clipboard.ContainsText() ? Clipboard.GetText() : null,
                    () => {
                        keybd_event(Keys.RControlKey, 0, 0, 0); keybd_event(Keys.C, 0, 0, 0);
                        keybd_event(Keys.C, 0, 2, 0); keybd_event(Keys.RControlKey, 0, 2, 0);
                    },
                    text => { if (text.Length > 0) Clipboard.SetText(text); else Clipboard.Clear(); },
                    text => { shangyicifuzhi = text; xianshi(text); }, Task.Delay);
            }
            catch (Exception ex) { Debug.WriteLine("Mouse selection capture: " + ex.GetType().Name); }
            finally { activeMouseCaptures--; }
        }
    }
}
