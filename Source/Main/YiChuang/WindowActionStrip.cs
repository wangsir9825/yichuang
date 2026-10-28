using System;
using System.Windows.Forms;

namespace YiChuang
{
    // These are window controls: activating an inactive window must not consume
    // the user's close/minimize/settings click like an ordinary menu toolbar.
    internal sealed class WindowActionStrip : ToolStrip
    {
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            const int WmMouseActivate = 0x0021;
            const int ActivateAndEat = 2;
            const int Activate = 1;
            if (message.Msg == WmMouseActivate && message.Result == (IntPtr)ActivateAndEat)
                message.Result = (IntPtr)Activate;
        }
    }
}
