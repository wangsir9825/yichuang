using System;
using System.Runtime.InteropServices;

namespace YiChuang
{
	internal class Win32Api
	{
		[StructLayout(LayoutKind.Sequential)]
		public class POINT
		{
			public int x;

			public int y;

			public POINT()
			{
				((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public class MouseHookStruct
		{
			public POINT pt;

			public int hwnd;

			public int wHitTestCode;

			public int dwExtraInfo;

			public MouseHookStruct()
			{
				((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
			}
		}

		public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern bool UnhookWindowsHookEx(int idHook);

		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
		public static extern int CallNextHookEx(int idHook, int nCode, IntPtr wParam, IntPtr lParam);

		public Win32Api()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
