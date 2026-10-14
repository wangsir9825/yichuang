using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

namespace YiChuang
{
	internal class MouseHook
	{
		public delegate void MouseMoveHandler(object sender, MouseEventArgs e);

		public delegate void MouseClickHandler(object sender, MouseEventArgs e);

		public delegate void MouseDownHandler(object sender, MouseEventArgs e);

		public delegate void MouseUpHandler(object sender, MouseEventArgs e);

		private Point point;

		private int hHook;

		private static int hMouseHook;

		private const int WM_MOUSEMOVE = 512;

		private const int WM_LBUTTONDOWN = 513;

		private const int WM_RBUTTONDOWN = 516;

		private const int WM_MBUTTONDOWN = 519;

		private const int WM_LBUTTONUP = 514;

		private const int WM_RBUTTONUP = 517;

		private const int WM_MBUTTONUP = 520;

		private const int WM_LBUTTONDBLCLK = 515;

		private const int WM_RBUTTONDBLCLK = 518;

		private const int WM_MBUTTONDBLCLK = 521;

		public const int WH_MOUSE_LL = 14;

		public Win32Api.HookProc hProc;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private MouseMoveHandler m_MouseMoveEvent;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private MouseClickHandler m_MouseClickEvent;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private MouseDownHandler m_MouseDownEvent;

		[CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		private MouseUpHandler m_MouseUpEvent;

		private Point Point
		{
			get
			{
				return point;
			}
			set
			{
				if (((global::_99277E15)_63AF42A1._71AC5D16[298])(point, value))
				{
					point = value;
					if (this.m_MouseMoveEvent != null)
					{
						MouseEventArgs e = ((global::FF888B83)_63AF42A1._71AC5D16[237])(MouseButtons.None, 0, ((global::F82C7088)_63AF42A1._71AC5D16[235])(ref point), ((global::_40146A1C)_63AF42A1._71AC5D16[349])(ref point), 0);
						this.m_MouseMoveEvent(this, e);
					}
				}
			}
		}

		public event MouseMoveHandler MouseMoveEvent
		{
			[CompilerGenerated]
			add
			{
				MouseMoveHandler mouseMoveHandler = this.m_MouseMoveEvent;
				MouseMoveHandler mouseMoveHandler2;
				do
				{
					mouseMoveHandler2 = mouseMoveHandler;
					MouseMoveHandler value2 = (MouseMoveHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseMoveHandler2, value);
					mouseMoveHandler = Interlocked.CompareExchange(ref this.m_MouseMoveEvent, value2, mouseMoveHandler2);
				}
				while ((object)mouseMoveHandler != mouseMoveHandler2);
			}
			[CompilerGenerated]
			remove
			{
				MouseMoveHandler mouseMoveHandler = this.m_MouseMoveEvent;
				MouseMoveHandler mouseMoveHandler2;
				do
				{
					mouseMoveHandler2 = mouseMoveHandler;
					MouseMoveHandler value2 = (MouseMoveHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseMoveHandler2, value);
					mouseMoveHandler = Interlocked.CompareExchange(ref this.m_MouseMoveEvent, value2, mouseMoveHandler2);
				}
				while ((object)mouseMoveHandler != mouseMoveHandler2);
			}
		}

		public event MouseClickHandler MouseClickEvent
		{
			[CompilerGenerated]
			add
			{
				MouseClickHandler mouseClickHandler = this.m_MouseClickEvent;
				MouseClickHandler mouseClickHandler2;
				do
				{
					mouseClickHandler2 = mouseClickHandler;
					MouseClickHandler value2 = (MouseClickHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseClickHandler2, value);
					mouseClickHandler = Interlocked.CompareExchange(ref this.m_MouseClickEvent, value2, mouseClickHandler2);
				}
				while ((object)mouseClickHandler != mouseClickHandler2);
			}
			[CompilerGenerated]
			remove
			{
				MouseClickHandler mouseClickHandler = this.m_MouseClickEvent;
				MouseClickHandler mouseClickHandler2;
				do
				{
					mouseClickHandler2 = mouseClickHandler;
					MouseClickHandler value2 = (MouseClickHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseClickHandler2, value);
					mouseClickHandler = Interlocked.CompareExchange(ref this.m_MouseClickEvent, value2, mouseClickHandler2);
				}
				while ((object)mouseClickHandler != mouseClickHandler2);
			}
		}

		public event MouseDownHandler MouseDownEvent
		{
			[CompilerGenerated]
			add
			{
				MouseDownHandler mouseDownHandler = this.m_MouseDownEvent;
				MouseDownHandler mouseDownHandler2;
				do
				{
					mouseDownHandler2 = mouseDownHandler;
					MouseDownHandler value2 = (MouseDownHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseDownHandler2, value);
					mouseDownHandler = Interlocked.CompareExchange(ref this.m_MouseDownEvent, value2, mouseDownHandler2);
				}
				while ((object)mouseDownHandler != mouseDownHandler2);
			}
			[CompilerGenerated]
			remove
			{
				MouseDownHandler mouseDownHandler = this.m_MouseDownEvent;
				MouseDownHandler mouseDownHandler2;
				do
				{
					mouseDownHandler2 = mouseDownHandler;
					MouseDownHandler value2 = (MouseDownHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseDownHandler2, value);
					mouseDownHandler = Interlocked.CompareExchange(ref this.m_MouseDownEvent, value2, mouseDownHandler2);
				}
				while ((object)mouseDownHandler != mouseDownHandler2);
			}
		}

		public event MouseUpHandler MouseUpEvent
		{
			[CompilerGenerated]
			add
			{
				MouseUpHandler mouseUpHandler = this.m_MouseUpEvent;
				MouseUpHandler mouseUpHandler2;
				do
				{
					mouseUpHandler2 = mouseUpHandler;
					MouseUpHandler value2 = (MouseUpHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseUpHandler2, value);
					mouseUpHandler = Interlocked.CompareExchange(ref this.m_MouseUpEvent, value2, mouseUpHandler2);
				}
				while ((object)mouseUpHandler != mouseUpHandler2);
			}
			[CompilerGenerated]
			remove
			{
				MouseUpHandler mouseUpHandler = this.m_MouseUpEvent;
				MouseUpHandler mouseUpHandler2;
				do
				{
					mouseUpHandler2 = mouseUpHandler;
					MouseUpHandler value2 = (MouseUpHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseUpHandler2, value);
					mouseUpHandler = Interlocked.CompareExchange(ref this.m_MouseUpEvent, value2, mouseUpHandler2);
				}
				while ((object)mouseUpHandler != mouseUpHandler2);
			}
		}

		public MouseHook()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
			Point = default(Point);
		}

		public int SetHook()
		{
			hProc = MouseHookProc;
			hHook = Win32Api.SetWindowsHookEx(14, hProc, ((global::_4481691B)_63AF42A1._71AC5D16[561])(), 0);
			return hHook;
		}

		public void UnHook()
		{
			Win32Api.UnhookWindowsHookEx(hHook);
		}

        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        private struct LowLevelMouseData
        {
            public Point Position;
            public uint MouseData, Flags, Time;
            public IntPtr ExtraInfo;
        }
        private int MouseHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0) return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            var data = (LowLevelMouseData)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(LowLevelMouseData));
            // Injected releases (including helpers) are not a fresh physical selection.
            if ((data.Flags & 3) != 0) return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            Point = data.Position; // Deliver this event's coordinates, not the preceding event's.
            MouseButtons button;
            bool down;
            switch (wParam.ToInt32())
            {
                case 513: button = MouseButtons.Left; down = true; break;
                case 516: button = MouseButtons.Right; down = true; break;
                case 519: button = MouseButtons.Middle; down = true; break;
                case 514: button = MouseButtons.Left; down = false; break;
                case 517: button = MouseButtons.Right; down = false; break;
                case 520: button = MouseButtons.Middle; down = false; break;
                default: return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
            }
            var args = new MouseEventArgs(button, 1, data.Position.X, data.Position.Y, 0);
            if (down) m_MouseDownEvent?.Invoke(this, args); else m_MouseUpEvent?.Invoke(this, args);
            return Win32Api.CallNextHookEx(hHook, nCode, wParam, lParam);
        }
    }
}
