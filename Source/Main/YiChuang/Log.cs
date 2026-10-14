namespace YiChuang
{
	public class Log
	{
		public static void WriteTextLog(LogType type, string action, string strMessage)
		{
			RebuildInfo.Log(new System.Exception(action + ": " + strMessage));
		}

		public Log()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
