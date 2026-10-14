using System;
using System.Drawing;
using System.Threading.Tasks;

namespace YiChuang
{
    // One ticket belongs to one physical gesture and one foreground window.
    internal sealed class MouseSelectionGate
    {
        internal sealed class Ticket
        {
            internal int Revision;
            internal IntPtr Window;
            internal long Created;
        }
        private int revision;
        private IntPtr foreground, downWindow, clickWindow;
        private Point downPoint, clickPoint;
        private long downTime, clickTime;
        private bool pressed, hasClick;

        internal void Reset()
        {
            revision++;
            pressed = hasClick = false;
        }
        internal void ObserveWindow(IntPtr window)
        {
            if (foreground != window) { Reset(); foreground = window; }
        }
        internal void Down(Point point, IntPtr window, long now, bool allowed)
        {
            ObserveWindow(window);
            if (!allowed || window == IntPtr.Zero) { Reset(); return; }
            revision++;
            pressed = true; downPoint = point; downWindow = window; downTime = now;
        }
        internal Ticket Up(Point point, IntPtr window, long now, bool allowed, int doubleClickTime, Size doubleClickSize)
        {
            ObserveWindow(window);
            if (!pressed || !allowed || window != downWindow) { Reset(); return null; }
            pressed = false;
            long dx = point.X - downPoint.X, dy = point.Y - downPoint.Y;
            bool dragged = dx * dx + dy * dy > 25;
            bool doubleClick = !dragged && hasClick && window == clickWindow &&
                downTime >= clickTime && downTime - clickTime <= doubleClickTime &&
                Math.Abs(downPoint.X - clickPoint.X) <= doubleClickSize.Width / 2 &&
                Math.Abs(downPoint.Y - clickPoint.Y) <= doubleClickSize.Height / 2;
            if (dragged || doubleClick)
            {
                hasClick = false;
                return new Ticket { Revision = revision, Window = window, Created = now };
            }
            hasClick = true; clickWindow = window; clickPoint = downPoint; clickTime = downTime;
            return null;
        }
        internal bool IsCurrent(Ticket ticket, IntPtr window, long now, bool allowed)
        {
            ObserveWindow(window);
            if (!allowed) Reset();
            return ticket != null && allowed && ticket.Revision == revision && ticket.Window == window &&
                now >= ticket.Created && now - ticket.Created <= 1200;
        }
    }

    internal static class MouseSelectionCopy
    {
        // Continuations run on the UI thread. Never publish or restore a stale capture.
        internal static async Task RunAsync(Func<bool> current, Func<uint> sequence, Func<string> read,
            Action copy, Action<string> restore, Action<string> publish, Func<int, Task> delay)
        {
            try
            {
                await delay(40); // Let the source application finish processing mouse-up.
                if (!current()) return;
                uint before = sequence();
                string previous = read();
                if (!current() || sequence() != before) return;
                copy();
                for (int attempt = 0; attempt < 20; attempt++)
                {
                    await delay(25);
                    if (!current()) return;
                    uint copied = sequence();
                    if (copied == before) continue; // Existing clipboard text is not a new selection.
                    string text = read();
                    if (!current() || sequence() != copied || string.IsNullOrWhiteSpace(text)) return;
                    publish(text);
                    if (previous != null && current() && sequence() == copied) restore(previous);
                    return;
                }
            }
            catch (System.Runtime.InteropServices.ExternalException) { /* Clipboard temporarily owned by another app. */ }
        }
    }
}
