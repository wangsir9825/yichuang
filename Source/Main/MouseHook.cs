using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;

public class MouseHook : GlobalHook
{
	private enum MouseEventType
	{
		None,
		MouseDown,
		MouseUp,
		DoubleClick,
		MouseWheel,
		MouseMove
	}

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private MouseEventHandler m_MouseDown;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private MouseEventHandler m_MouseUp;

	[CompilerGenerated]
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private MouseEventHandler m_MouseMove;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private MouseEventHandler m_MouseWheel;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private EventHandler m_Click;

	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	[CompilerGenerated]
	private EventHandler m_DoubleClick;

	public event MouseEventHandler MouseDown
	{
		[CompilerGenerated]
		add
		{
			MouseEventHandler mouseEventHandler = this.m_MouseDown;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseDown, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			MouseEventHandler mouseEventHandler = this.m_MouseDown;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseDown, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
	}

	public event MouseEventHandler MouseUp
	{
		[CompilerGenerated]
		add
		{
			MouseEventHandler mouseEventHandler = this.m_MouseUp;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseUp, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			MouseEventHandler mouseEventHandler = this.m_MouseUp;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseUp, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
	}

	public event MouseEventHandler MouseMove
	{
		[CompilerGenerated]
		add
		{
			MouseEventHandler mouseEventHandler = this.m_MouseMove;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseMove, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			MouseEventHandler mouseEventHandler = this.m_MouseMove;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseMove, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
	}

	public event MouseEventHandler MouseWheel
	{
		[CompilerGenerated]
		add
		{
			MouseEventHandler mouseEventHandler = this.m_MouseWheel;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseWheel, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			MouseEventHandler mouseEventHandler = this.m_MouseWheel;
			MouseEventHandler mouseEventHandler2;
			do
			{
				mouseEventHandler2 = mouseEventHandler;
				MouseEventHandler value2 = (MouseEventHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(mouseEventHandler2, value);
				mouseEventHandler = Interlocked.CompareExchange(ref this.m_MouseWheel, value2, mouseEventHandler2);
			}
			while ((object)mouseEventHandler != mouseEventHandler2);
		}
	}

	public event EventHandler Click
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = this.m_Click;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref this.m_Click, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = this.m_Click;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref this.m_Click, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public event EventHandler DoubleClick
	{
		[CompilerGenerated]
		add
		{
			EventHandler eventHandler = this.m_DoubleClick;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)((global::_8207F12E)_63AF42A1._71AC5D16[253])(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref this.m_DoubleClick, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
		[CompilerGenerated]
		remove
		{
			EventHandler eventHandler = this.m_DoubleClick;
			EventHandler eventHandler2;
			do
			{
				eventHandler2 = eventHandler;
				EventHandler value2 = (EventHandler)((global::_7E8D4D06)_63AF42A1._71AC5D16[447])(eventHandler2, value);
				eventHandler = Interlocked.CompareExchange(ref this.m_DoubleClick, value2, eventHandler2);
			}
			while ((object)eventHandler != eventHandler2);
		}
	}

	public MouseHook()
	{
		_hookType = 14;
	}

	protected override int HookCallbackProcedure(int nCode, int wParam, IntPtr lParam)
	{
		if (nCode > -1 && (this.m_MouseDown != null || this.m_MouseUp != null || this.m_MouseMove != null))
		{
			MouseLLHookStruct mouseLLHookStruct = (MouseLLHookStruct)((global::_2BA3698F)_63AF42A1._71AC5D16[313])(lParam, ((global::_743E8F3F)_63AF42A1._71AC5D16[254])(typeof(MouseLLHookStruct).TypeHandle));
			MouseButtons button = GetButton(wParam);
			MouseEventType mouseEventType = GetEventType(wParam);
			MouseEventArgs _5A = D4A9BE03._2738BAA6(button, (mouseEventType != MouseEventType.DoubleClick) ? 1 : 2, mouseLLHookStruct.pt.x, mouseLLHookStruct.pt.y, (mouseEventType == MouseEventType.MouseWheel) ? ((short)((mouseLLHookStruct.mouseData >> 16) & 0xFFFF)) : 0);
			if (button == MouseButtons.Right && mouseLLHookStruct.flags != 0)
			{
				mouseEventType = MouseEventType.None;
			}
			switch (mouseEventType)
			{
			case MouseEventType.MouseDown:
				if (this.m_MouseDown != null)
				{
					((global::C52A002E)_63AF42A1._71AC5D16[62])(this.m_MouseDown, this, _5A);
				}
				break;
			case MouseEventType.MouseUp:
				if (this.m_Click != null)
				{
					((global::_0631029B)_63AF42A1._71AC5D16[96])(this.m_Click, this, ((global::B7A6FA2F)_63AF42A1._71AC5D16[331])());
				}
				if (this.m_MouseUp != null)
				{
					((global::C52A002E)_63AF42A1._71AC5D16[62])(this.m_MouseUp, this, _5A);
				}
				break;
			case MouseEventType.DoubleClick:
				if (this.m_DoubleClick != null)
				{
					((global::_0631029B)_63AF42A1._71AC5D16[96])(this.m_DoubleClick, this, ((global::B7A6FA2F)_63AF42A1._71AC5D16[331])());
				}
				break;
			case MouseEventType.MouseWheel:
				if (this.m_MouseWheel != null)
				{
					((global::C52A002E)_63AF42A1._71AC5D16[62])(this.m_MouseWheel, this, _5A);
				}
				break;
			case MouseEventType.MouseMove:
				if (this.m_MouseMove != null)
				{
					((global::C52A002E)_63AF42A1._71AC5D16[62])(this.m_MouseMove, this, _5A);
				}
				break;
			}
		}
		return GlobalHook.CallNextHookEx(_handleToHook, nCode, wParam, lParam);
	}

	private MouseButtons GetButton(int wParam)
	{
		switch (wParam)
		{
		case 513:
		case 514:
		case 515:
			return MouseButtons.Left;
		case 516:
		case 517:
		case 518:
			return MouseButtons.Right;
		case 519:
		case 520:
		case 521:
			return MouseButtons.Middle;
		default:
			return MouseButtons.None;
		}
	}

	private MouseEventType GetEventType(int wParam)
	{
		switch (wParam)
		{
		case 513:
		case 516:
		case 519:
			return MouseEventType.MouseDown;
		case 514:
		case 517:
		case 520:
			return MouseEventType.MouseUp;
		case 515:
		case 518:
		case 521:
			return MouseEventType.DoubleClick;
		case 522:
			return MouseEventType.MouseWheel;
		case 512:
			return MouseEventType.MouseMove;
		default:
			return MouseEventType.None;
		}
	}
}
