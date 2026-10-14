using System;
using System.Runtime.InteropServices;

namespace YiChuang
{
	internal static class RemoveIconsHelper
	{
		public struct TBBUTTONINFO
		{
			public uint cbSize;

			public uint dwMask;

			public int idCommand;

			public int iImage;

			public byte fsState;

			public byte fsStyle;

			public short cx;

			public IntPtr lParam;

			[MarshalAs(UnmanagedType.LPTStr)]
			public string lpszText;

			public int cchText;
		}

		public const int WM_USER = 1024;

		public const int TB_HIDEBUTTON = 1028;

		private const int TB_DELETEBUTTON = 1046;

		public const int TB_BUTTONCOUNT = 1048;

		public const int TB_GETBUTTONINFO = 1087;

		public const int TBIF_IMAGE = 1;

		public const int TBIF_TEXT = 2;

		public const int TBIF_STATE = 4;

		public const int TBIF_STYLE = 8;

		public const int TBIF_LPARAM = 16;

		public const int TBIF_COMMAND = 32;

		public const int TBIF_SIZE = 64;

		public const uint TBIF_BYINDEX = 2147483648u;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, bool lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, ref TBBUTTONINFO lParam);

		public static void ShowTaskbarButtons(bool showButtons)
		{
			IntPtr notifyWindow = GetNotifyWindow();
			int num = ((global::_958D2B04)_63AF42A1._71AC5D16[136])(SendMessage(notifyWindow, 1048u, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), ((global::_4481691B)_63AF42A1._71AC5D16[561])()));
			for (int i = 0; i < num; i++)
			{
				SendMessage(notifyWindow, 1028u, i, showButtons);
			}
		}

		[DllImport("user32.dll", SetLastError = true)]
		private static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);

		public static void DeleteAllTaskbarButtons()
		{
		}

		public static void ShowAllTaskbarButtons()
		{
			IntPtr notifyWindow = GetNotifyWindow();
			SendMessage(notifyWindow, 1087u, 0, 1);
		}

		private static IntPtr GetNotifyWindow()
		{
			IntPtr hwndParent = FindWindow("Shell_TrayWnd", null);
			IntPtr hwndParent2 = FindWindowEx(hwndParent, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "TrayNotifyWnd", null);
			IntPtr hwndParent3 = FindWindowEx(hwndParent2, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "SysPager", null);
			return FindWindowEx(hwndParent3, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), "ToolbarWindow32", null);
		}
	}
}
