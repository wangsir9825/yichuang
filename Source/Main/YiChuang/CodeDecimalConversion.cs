using System.Collections.Generic;
using System.Linq;

namespace YiChuang
{
	internal class CodeDecimalConversion
	{
		public static Dictionary<int, string> Base64Code = new Dictionary<int, string>
		{
			{ 0, "z" },
			{ 1, "1" },
			{ 2, "2" },
			{ 3, "3" },
			{ 4, "4" },
			{ 5, "5" },
			{ 6, "6" },
			{ 7, "7" },
			{ 8, "8" },
			{ 9, "9" },
			{ 10, "a" },
			{ 11, "b" },
			{ 12, "c" },
			{ 13, "d" },
			{ 14, "e" },
			{ 15, "f" },
			{ 16, "g" },
			{ 17, "h" },
			{ 18, "i" },
			{ 19, "j" },
			{ 20, "k" },
			{ 21, "x" },
			{ 22, "m" },
			{ 23, "n" },
			{ 24, "y" },
			{ 25, "p" },
			{ 26, "q" },
			{ 27, "r" },
			{ 28, "s" },
			{ 29, "t" },
			{ 30, "u" },
			{ 31, "v" },
			{ 32, "w" },
			{ 33, "x" },
			{ 34, "y" },
			{ 35, "z" },
			{ 36, "A" },
			{ 37, "B" },
			{ 38, "C" },
			{ 39, "D" },
			{ 40, "E" },
			{ 41, "F" },
			{ 42, "G" },
			{ 43, "H" },
			{ 44, "I" },
			{ 45, "J" },
			{ 46, "K" },
			{ 47, "L" },
			{ 48, "M" },
			{ 49, "N" },
			{ 50, "O" },
			{ 51, "P" },
			{ 52, "Q" },
			{ 53, "R" },
			{ 54, "S" },
			{ 55, "T" },
			{ 56, "U" },
			{ 57, "V" },
			{ 58, "W" },
			{ 59, "X" },
			{ 60, "Y" },
			{ 61, "Z" },
			{ 62, "-" },
			{ 63, "_" }
		};

		public static Dictionary<string, int> _Base64Code => ((global::_9209EBAA)_63AF42A1._71AC5D16[657])(0, Base64Code.Count()).ToDictionary((int i) => Base64Code[i], (int i) => i);

		public static string IntToi32(long xx)
		{
			string text = "";
			while (xx >= 1)
			{
				int key = ((global::_778975A6)_63AF42A1._71AC5D16[322])(xx - xx / 32 * 32);
				text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(Base64Code[key], text);
				xx /= 32;
			}
			return text;
		}

		public static long i32ToInt(string xx)
		{
			long num = 0L;
			int num2 = ((global::A11AF20B)_63AF42A1._71AC5D16[217])(xx) - 1;
			for (int i = 0; i <= num2; i++)
			{
				long num3 = num;
				Dictionary<string, int> base64Code = _Base64Code;
				char _243BA3BA = ((global::_1F211E96)_63AF42A1._71AC5D16[392])(xx, num2 - i);
				num = num3 + base64Code[((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA)] * ((global::D2134822)_63AF42A1._71AC5D16[584])(((global::_8084E910)_63AF42A1._71AC5D16[563])(32.0, i));
			}
			return num;
		}

		public static string IntToi64(long xx)
		{
			string text = "";
			while (xx >= 1)
			{
				int key = ((global::_778975A6)_63AF42A1._71AC5D16[322])(xx - xx / 64 * 64);
				text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(Base64Code[key], text);
				xx /= 64;
			}
			return text;
		}

		public static long i64ToInt(string xx)
		{
			long num = 0L;
			int num2 = ((global::A11AF20B)_63AF42A1._71AC5D16[217])(xx) - 1;
			for (int i = 0; i <= num2; i++)
			{
				long num3 = num;
				Dictionary<string, int> base64Code = _Base64Code;
				char _243BA3BA = ((global::_1F211E96)_63AF42A1._71AC5D16[392])(xx, num2 - i);
				num = num3 + base64Code[((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA)] * ((global::D2134822)_63AF42A1._71AC5D16[584])(((global::_8084E910)_63AF42A1._71AC5D16[563])(64.0, i));
			}
			return num;
		}

		public CodeDecimalConversion()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
