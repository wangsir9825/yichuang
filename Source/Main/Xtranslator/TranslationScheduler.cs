using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Xtranslator
{
    // Debounce on the UI thread: discarded keystrokes never consume a worker thread.
    internal sealed class TranslationScheduler : IDisposable
    {
        private readonly Timer timer = new Timer();
        private Action pending;
        internal TranslationScheduler() { timer.Tick += Tick; }
        internal void Schedule(Action action, int milliseconds = 350)
        {
            timer.Stop(); pending = action; timer.Interval = milliseconds; timer.Start();
        }
        private void Tick(object sender, EventArgs args)
        {
            timer.Stop(); var action = pending; pending = null;
            if (action != null) Task.Run(action);
        }
        internal void Cancel() { timer.Stop(); pending = null; }
        public void Dispose() { Cancel(); timer.Dispose(); }
    }
}
