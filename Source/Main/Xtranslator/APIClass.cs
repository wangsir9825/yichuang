using System.Runtime.InteropServices;

namespace Xtranslator
{
	public class APIClass
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		public static extern int GetShortPathName(string lpszLongPath, string shortFile, int cchBuffer);

		[DllImport("winmm.dll", CharSet = CharSet.Auto)]
		public static extern int mciSendString(string lpstrCommand, string lpstrReturnString, int uReturnLength, int hwndCallback);

		public APIClass()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
