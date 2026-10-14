using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace wxocr;

internal static class Program
{
	private enum ProcessDpiAwareness
	{
		ProcessDpiUnaware,
		ProcessSystemDpiAware,
		ProcessPerMonitorDpiAware
	}

	[STAThread]
	private static void Main()
	{
		if (Environment.OSVersion.Version.Major >= 6)
		{
			SetProcessDpiAwareness(ProcessDpiAwareness.ProcessPerMonitorDpiAware);
		}
		Application.EnableVisualStyles();
		Application.SetCompatibleTextRenderingDefault(defaultValue: false);
		Form1.RunOcrDirectly();
	}

	[DllImport("shcore.dll")]
	private static extern int SetProcessDpiAwareness(ProcessDpiAwareness value);
}
