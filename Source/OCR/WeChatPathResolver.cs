using System;
using System.IO;
using Microsoft.Win32;

public class WeChatPathResolver
{
	public static (string OcrBinaryPath, string RuntimePath, string Version) GetWeChatPaths()
	{
		string item = null;
		string text = null;
		string text2 = null;
		try
		{
			using (RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Tencent\\Weixin"))
			{
				if (registryKey != null)
				{
					string text3 = registryKey.GetValue("InstallPath") as string;
					if (!string.IsNullOrEmpty(text3))
					{
						string[] directories = Directory.GetDirectories(text3, "4.*");
						if (directories.Length != 0)
						{
							text = directories[0];
							text2 = "4.x";
						}
					}
				}
			}
			if (text2 == "4.x")
			{
				string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
				string path = Path.Combine(folderPath, "AppData\\Roaming\\Tencent\\xwechat\\XPlugin\\Plugins\\WeChatOcr");
				if (Directory.Exists(path))
				{
					string[] directories2 = Directory.GetDirectories(path);
					string[] array = directories2;
					foreach (string path2 in array)
					{
						string text4 = Path.Combine(path2, "extracted");
						if (Directory.Exists(text4))
						{
							string text5 = Path.Combine(text4, "wxocr.dll");
							if (File.Exists(text5))
							{
								item = text5;
								break;
							}
						}
					}
				}
			}
			if (text2 == null)
			{
				using (RegistryKey registryKey2 = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Tencent\\WeChat"))
				{
					if (registryKey2 != null)
					{
						string text6 = registryKey2.GetValue("InstallPath") as string;
						if (!string.IsNullOrEmpty(text6))
						{
							string[] directories3 = Directory.GetDirectories(text6, "3.*");
							if (directories3.Length != 0)
							{
								text = directories3[0];
								text2 = "3.x";
							}
						}
					}
				}
				if (text2 == "3.x")
				{
					string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
					string path3 = Path.Combine(folderPath2, "AppData\\Roaming\\Tencent\\WeChat\\XPlugin\\Plugins\\WeChatOCR");
					if (Directory.Exists(path3))
					{
						string[] directories4 = Directory.GetDirectories(path3);
						string[] array2 = directories4;
						foreach (string path4 in array2)
						{
							string text7 = Path.Combine(path4, "extracted");
							if (Directory.Exists(text7))
							{
								string text8 = Path.Combine(text7, "WeChatOCR.exe");
								if (File.Exists(text8))
								{
									item = text8;
									break;
								}
							}
						}
					}
				}
			}
			if (string.IsNullOrEmpty(text) && text2 == "4.x")
			{
				string[] array3 = new string[3] { "C:\\Program Files\\Tencent\\Weixin", "C:\\Program Files (x86)\\Tencent\\Weixin", "D:\\Program Files\\Weixin" };
				string[] array4 = array3;
				foreach (string path5 in array4)
				{
					if (Directory.Exists(path5))
					{
						string[] directories5 = Directory.GetDirectories(path5, "4.*");
						if (directories5.Length != 0)
						{
							text = directories5[0];
							break;
						}
					}
				}
			}
			else if (string.IsNullOrEmpty(text) && text2 == "3.x")
			{
				string[] array5 = new string[3] { "C:\\Program Files\\Tencent\\WeChat", "C:\\Program Files (x86)\\Tencent\\WeChat", "E:\\Program Files\\WeChat" };
				string[] array6 = array5;
				foreach (string path6 in array6)
				{
					if (Directory.Exists(path6))
					{
						string[] directories6 = Directory.GetDirectories(path6, "3.*");
						if (directories6.Length != 0)
						{
							text = directories6[0];
							break;
						}
					}
				}
			}
			return (OcrBinaryPath: item, RuntimePath: text, Version: text2);
		}
		catch (Exception ex)
		{
			Console.WriteLine("Error resolving WeChat paths: " + ex.Message);
			return (OcrBinaryPath: null, RuntimePath: null, Version: null);
		}
	}
}
