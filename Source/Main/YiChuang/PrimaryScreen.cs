using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace YiChuang
{
	internal class PrimaryScreen
	{
		private const int HORZRES = 8;

		private const int VERTRES = 10;

		private const int LOGPIXELSX = 88;

		private const int LOGPIXELSY = 90;

		private const int DESKTOPVERTRES = 117;

		private const int DESKTOPHORZRES = 118;

		public static Size WorkingArea
		{
			get
			{
				IntPtr dC = GetDC(((global::_4481691B)_63AF42A1._71AC5D16[561])());
				Size _043238AB = default(Size);
				((global::_413E77BE)_63AF42A1._71AC5D16[52])(ref _043238AB, GetDeviceCaps(dC, 8));
				((global::_1480BE92)_63AF42A1._71AC5D16[626])(ref _043238AB, GetDeviceCaps(dC, 10));
				ReleaseDC(((global::_4481691B)_63AF42A1._71AC5D16[561])(), dC);
				return _043238AB;
			}
		}

		public static int DpiX
		{
			get
			{
				IntPtr dC = GetDC(((global::_4481691B)_63AF42A1._71AC5D16[561])());
				int deviceCaps = GetDeviceCaps(dC, 88);
				ReleaseDC(((global::_4481691B)_63AF42A1._71AC5D16[561])(), dC);
				return deviceCaps;
			}
		}

		public static int DpiY
		{
			get
			{
				IntPtr dC = GetDC(((global::_4481691B)_63AF42A1._71AC5D16[561])());
				int deviceCaps = GetDeviceCaps(dC, 90);
				ReleaseDC(((global::_4481691B)_63AF42A1._71AC5D16[561])(), dC);
				return deviceCaps;
			}
		}

		public static Size DESKTOP
		{
			get
			{
				IntPtr dC = GetDC(((global::_4481691B)_63AF42A1._71AC5D16[561])());
				Size _043238AB = default(Size);
				((global::_413E77BE)_63AF42A1._71AC5D16[52])(ref _043238AB, GetDeviceCaps(dC, 118));
				((global::_1480BE92)_63AF42A1._71AC5D16[626])(ref _043238AB, GetDeviceCaps(dC, 117));
				ReleaseDC(((global::_4481691B)_63AF42A1._71AC5D16[561])(), dC);
				return _043238AB;
			}
		}

		public static float ScaleX
		{
			get
			{
				IntPtr dC = GetDC(((global::_4481691B)_63AF42A1._71AC5D16[561])());
				int deviceCaps = GetDeviceCaps(dC, 118);
				int deviceCaps2 = GetDeviceCaps(dC, 8);
				float result = (float)GetDeviceCaps(dC, 118) / (float)GetDeviceCaps(dC, 8);
				ReleaseDC(((global::_4481691B)_63AF42A1._71AC5D16[561])(), dC);
				return result;
			}
		}

		public static float ScaleY
		{
			get
			{
				IntPtr dC = GetDC(((global::_4481691B)_63AF42A1._71AC5D16[561])());
				float result = (float)GetDeviceCaps(dC, 117) / (float)GetDeviceCaps(dC, 10);
				ReleaseDC(((global::_4481691B)_63AF42A1._71AC5D16[561])(), dC);
				return result;
			}
		}

		[DllImport("user32.dll")]
		private static extern IntPtr GetDC(IntPtr ptr);

		[DllImport("gdi32.dll")]
		private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

		[DllImport("user32.dll")]
		private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDc);

		public PrimaryScreen()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
