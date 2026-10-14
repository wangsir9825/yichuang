using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Xtranslator
{
    // Clipboard updates are accepted only after an explicit shortcut/mouse-combination request.
    internal sealed class ExplicitCaptureGate
    {
        private IntPtr target;
        private uint before;
        private long deadline;
        internal void Arm(IntPtr window, uint sequence, long now) { target=window;before=sequence;deadline=now+5000; }
        internal void Cancel() { target=IntPtr.Zero; }
        internal bool WindowChanged(IntPtr window) { if(target==IntPtr.Zero||target==window)return false;Cancel();return true; }
        internal bool Accept(IntPtr window,uint sequence,long now,bool enabled)
        {
            if(!enabled||now>deadline||WindowChanged(window)){Cancel();return false;}
            return target!=IntPtr.Zero&&window==target&&sequence!=before;
        }
    }
    public partial class Form2
    {
        private readonly ExplicitCaptureGate explicitCapture = new ExplicitCaptureGate();
        [DllImport("user32.dll")] private static extern bool AddClipboardFormatListener(IntPtr handle);
        [DllImport("user32.dll")] private static extern bool RemoveClipboardFormatListener(IntPtr handle);
        private void CancelExplicitCapture()
        {
            explicitCapture.Cancel();
            if(keyboardCopy!=null) { try { if(!keyboardCopy.HasExited)keyboardCopy.Kill(); } catch(InvalidOperationException){} catch(System.ComponentModel.Win32Exception){} keyboardCopy.Dispose();keyboardCopy=null; }
        }
        private void ExplicitForegroundChanged(IntPtr window)
        {
            if(explicitCapture.WindowChanged(window))CancelExplicitCapture();
        }
        private void CaptureExplicitSelection(int mode,IntPtr target)
        {
            if(!ClassMain._is自动翻译总开关||_翻译模式!=mode||ClassSetHotKey.Suspended||target!=GetForegroundWindow())return;
            if(IsTranslatorWindow(target)){if(mode==2)QueueSelectedTranslation();return;}
            CancelExplicitCapture();
            explicitCapture.Arm(target,GetClipboardSequenceNumber(),mouseCaptureClock.ElapsedMilliseconds);
            try {
                keyboardCopy=Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(typeof(Form2).Assembly.Location),"XtransNoLogin-F1-mode.exe"),"capture "+target.ToInt64()) {UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden});
            } catch(Exception) { CancelExplicitCapture();ClassSetHotKey.LastError="取词未能启动，请检查程序文件是否完整。";RefreshModernStatus(); }
        }
        private void ReadExplicitClipboard()
        {
            bool allowed=ClassMain._is自动翻译总开关&&(_翻译模式==2||_翻译模式==3)&&!ClassSetHotKey.Suspended;
            if(!explicitCapture.Accept(GetForegroundWindow(),GetClipboardSequenceNumber(),mouseCaptureClock.ElapsedMilliseconds,allowed))return;
            try {
                uint sequence=GetClipboardSequenceNumber();
                string text=Clipboard.ContainsText()?Clipboard.GetText():null;
                if(!explicitCapture.Accept(GetForegroundWindow(),GetClipboardSequenceNumber(),mouseCaptureClock.ElapsedMilliseconds,allowed)||sequence!=GetClipboardSequenceNumber())return;
                if(string.IsNullOrWhiteSpace(text))return;
                explicitCapture.Cancel();shangyicifuzhi=text;xianshi(text);
            } catch(ExternalException) { /* Keep the short request window open for the next clipboard notification. */ }
        }
    }
}
