using System;
using System.Runtime.InteropServices;

namespace YiChuang
{
	internal class TaskBarUtil
	{
		private struct RECT
		{
			public int left;

			public int top;

			public int right;

			public int bottom;
		}

		public static void RefreshNotification()
		{
			IntPtr notifyAreaHandle = GetNotifyAreaHandle();
			if (((global::_00940505)_63AF42A1._71AC5D16[276])(notifyAreaHandle, ((global::_4481691B)_63AF42A1._71AC5D16[561])()))
			{
				RefreshWindow(notifyAreaHandle);
			}
			IntPtr notifyOverHandle = GetNotifyOverHandle();
			if (((global::_00940505)_63AF42A1._71AC5D16[276])(notifyOverHandle, ((global::_4481691B)_63AF42A1._71AC5D16[561])()))
			{
				RefreshWindow(notifyOverHandle);
			}
		}

		private static void RefreshWindow(IntPtr windowHandle)
		{
			GetClientRect(windowHandle, out var rect);
			for (int i = 0; i < rect.right; i += 5)
			{
				for (int j = 0; j < rect.bottom; j += 5)
				{
					SendMessage(windowHandle, 512u, 0, (j << 16) + i);
				}
			}
		}

		private static IntPtr GetNotifyAreaHandle()
		{
			IntPtr parentHandle = FindWindowEx(((global::_4481691B)_63AF42A1._71AC5D16[561])(), ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "Shell_TrayWnd", ((global::D1B3723C)_63AF42A1._71AC5D16[75])());
			IntPtr parentHandle2 = FindWindowEx(parentHandle, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "TrayNotifyWnd", ((global::D1B3723C)_63AF42A1._71AC5D16[75])());
			IntPtr parentHandle3 = FindWindowEx(parentHandle2, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "SysPager", ((global::D1B3723C)_63AF42A1._71AC5D16[75])());
			return FindWindowEx(parentHandle3, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "ToolbarWindow32", "用户提示通知区域");
		}

		private static IntPtr GetNotifyOverHandle()
		{
			IntPtr parentHandle = FindWindowEx(((global::_4481691B)_63AF42A1._71AC5D16[561])(), ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "NotifyIconOverflowWindow", ((global::D1B3723C)_63AF42A1._71AC5D16[75])());
			return FindWindowEx(parentHandle, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "ToolbarWindow32", "溢出通知区域");
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr handle, out RECT rect);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr handle, uint message, int wParam, int lParam);

		public TaskBarUtil()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
