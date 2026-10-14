using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Xtranslator.BMemory
{
	public class ComMain
	{



		public const int WM_SYSCOMMAND = 274;

		public const int SC_MOVE = 61456;

		public const int HTCAPTION = 2;





		public static RichTextBox SetFontColor(RichTextBoxEx textBox)
		{
			((global::EA9AC1BD)_63AF42A1._71AC5D16[486])(textBox, ((global::_0D024B07)_63AF42A1._71AC5D16[67])(((global::_92B6CB19)_63AF42A1._71AC5D16[522])(textBox), ((global::_8D044F02)_63AF42A1._71AC5D16[698])(((global::_92B6CB19)_63AF42A1._71AC5D16[522])(textBox)) | FontStyle.Regular));
			List<string> list = new List<string>();
			list.Add("【美式发音】");
			string item = "";
			string[] a59E718F = new string[1] { list[0] };
			string[] array = ((global::_5B09BFA4)_63AF42A1._71AC5D16[696])(((global::F5871009)_63AF42A1._71AC5D16[274])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(textBox)), a59E718F, StringSplitOptions.RemoveEmptyEntries);
			if (array.Length > 1)
			{
				string text = ((global::E83366AE)_63AF42A1._71AC5D16[201])(array[1]);
				if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(text))
				{
					item = _9B8E342D._358A3A88(_16039B03.EA39B9B6(text, new char[1] { ']' })[0], "]");
				}
			}
			list.Clear();
			list.Add(item);
			for (int i = 0; i < list.Count; i++)
			{
				try
				{
					if (((global::_20A4ABBF)_63AF42A1._71AC5D16[22])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(textBox), list[i]))
					{
						string[] a59E718F2 = new string[1] { list[i] };
						string[] array2 = ((global::_5B09BFA4)_63AF42A1._71AC5D16[696])(((global::F5871009)_63AF42A1._71AC5D16[274])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(textBox)), a59E718F2, StringSplitOptions.RemoveEmptyEntries);
						int num = 0;
						for (int j = 0; j < array2.Length; j++)
						{
							num += ((global::A11AF20B)_63AF42A1._71AC5D16[217])(array2[j]);
							((global::_3F83D38A)_63AF42A1._71AC5D16[327])(textBox, num, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(list[i]));
							textBox.SetSelectionLink(link: true);
							((global::_24381E37)_63AF42A1._71AC5D16[523])(textBox, ((global::_6504E5B9)_63AF42A1._71AC5D16[720])("宋体", ClassMain.translationFontSize, FontStyle.Regular));
							num += ((global::A11AF20B)_63AF42A1._71AC5D16[217])(list[i]);
						}
					}
				}
				catch (Exception ex)
				{
					((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
					Log.WriteTextLog(LogType.主进程, "ComMain-SetFontColor", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
				}
			}
			return textBox;
		}

		public static void playMp3(string path)
		{
			try
			{
				Mp3Player mp3Player = new Mp3Player
				{
					FileName = path
				};
				mp3Player.play();
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "ComMain-playMp3", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}







		[DllImport("user32.dll")]
		public static extern bool ReleaseCapture();

		[DllImport("user32.dll")]
		public static extern bool SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

		public static void TCapture(IntPtr hwnd)
		{
			try
			{
				ReleaseCapture();
				SendMessage(hwnd, 274, 61458, 0);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "无法拖动窗体", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}



		public static int MatchesCount(string str1, string str2)
		{
			int result = 0;
			string[] array = _16039B03.EA39B9B6(((global::E3051AA0)_63AF42A1._71AC5D16[794])(str1, str2, "艹"), new char[1] { '艹' });
			if (array.Length != 0)
			{
				return array.Length - 1;
			}
			return result;
		}



		public ComMain()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
