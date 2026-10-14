using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

public abstract class GlobalHook
{
	[StructLayout(LayoutKind.Sequential)]
	protected class POINT
	{
		public int x;

		public int y;

		public POINT()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	protected class MouseHookStruct
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

	[StructLayout(LayoutKind.Sequential)]
	protected class MouseLLHookStruct
	{
		public POINT pt;

		public int mouseData;

		public int flags;

		public int time;

		public int dwExtraInfo;

		public MouseLLHookStruct()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	protected class KeyboardHookStruct
	{
		public int vkCode;

		public int scanCode;

		public int flags;

		public int time;

		public int dwExtraInfo;

		public KeyboardHookStruct()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}

	protected delegate int HookProc(int nCode, int wParam, IntPtr lParam);

	protected const int WH_MOUSE_LL = 14;

	protected const int WH_KEYBOARD_LL = 13;

	protected const int WH_MOUSE = 7;

	protected const int WH_KEYBOARD = 2;

	protected const int WM_MOUSEMOVE = 512;

	protected const int WM_LBUTTONDOWN = 513;

	protected const int WM_RBUTTONDOWN = 516;

	protected const int WM_MBUTTONDOWN = 519;

	protected const int WM_LBUTTONUP = 514;

	protected const int WM_RBUTTONUP = 517;

	protected const int WM_MBUTTONUP = 520;

	protected const int WM_LBUTTONDBLCLK = 515;

	protected const int WM_RBUTTONDBLCLK = 518;

	protected const int WM_MBUTTONDBLCLK = 521;

	protected const int WM_MOUSEWHEEL = 522;

	protected const int WM_KEYDOWN = 256;

	protected const int WM_KEYUP = 257;

	protected const int WM_SYSKEYDOWN = 260;

	protected const int WM_SYSKEYUP = 261;

	protected const byte VK_SHIFT = 16;

	protected const byte VK_CAPITAL = 20;

	protected const byte VK_NUMLOCK = 144;

	protected const byte VK_LSHIFT = 160;

	protected const byte VK_RSHIFT = 161;

	protected const byte VK_LCONTROL = 162;

	protected const byte VK_RCONTROL = 3;

	protected const byte VK_LALT = 164;

	protected const byte VK_RALT = 165;

	protected const byte LLKHF_ALTDOWN = 32;

	protected int _hookType;

	protected int _handleToHook;

	public bool _isStarted;

	protected HookProc _hookCallback;

	public bool IsStarted => _isStarted;

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
	protected static extern int SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, int dwThreadId);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr GetModuleHandle(string modulename);

	[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern IntPtr SetWindowsHookExW(int idHook, HookProc lpfn, IntPtr hmod, uint dwThreadID);

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
	protected static extern int UnhookWindowsHookEx(int idHook);

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	protected static extern int CallNextHookEx(int idHook, int nCode, int wParam, IntPtr lParam);

	[DllImport("user32")]
	protected static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpwTransKey, int fuState);

	[DllImport("user32")]
	protected static extern int GetKeyboardState(byte[] pbKeyState);

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto)]
	protected static extern short GetKeyState(int vKey);

	public unsafe GlobalHook()
	{
		((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		((global::_4A2C25BE)_63AF42A1._71AC5D16[749])(new global::System.EventHandler(this.Application_ApplicationExit));
	}

	public void Start()
	{
		if (_isStarted || _hookType == 0)
		{
			return;
		}
		_hookCallback = HookCallbackProcedure;
		using (Process _1F98A = ((global::DD06B908)_63AF42A1._71AC5D16[370])())
		{
			using (ProcessModule aE0BAF = ((global::C18F2D08)_63AF42A1._71AC5D16[260])(_1F98A))
			{
				_handleToHook = ((global::_958D2B04)_63AF42A1._71AC5D16[136])(SetWindowsHookExW(_hookType, _hookCallback, GetModuleHandle(((global::_37998B9B)_63AF42A1._71AC5D16[635])(aE0BAF)), 0u));
			}
		}
		if (_handleToHook != 0)
		{
			_isStarted = true;
		}
	}

	public void Stop()
	{
		if (_isStarted)
		{
			UnhookWindowsHookEx(_handleToHook);
			_isStarted = false;
		}
	}

	protected virtual int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
	{
		return 0;
	}

	protected void Application_ApplicationExit(object sender, EventArgs e)
	{
		if (_isStarted)
		{
			Stop();
		}
	}
}
