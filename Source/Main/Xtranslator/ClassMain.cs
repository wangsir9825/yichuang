using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Xtranslator
{
	internal class ClassMain
	{
		private struct PARAFORMAT2
		{
			public int cbSize;

			public uint dwMask;

			public short wNumbering;

			public short wReserved;

			public int dxStartIndent;

			public int dxRightIndent;

			public int dxOffset;

			public short wAlignment;

			public short cTabCount;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public int[] rgxTabs;

			public int dySpaceBefore;

			public int dySpaceAfter;

			public int dyLineSpacing;

			public short sStyle;

			public byte bLineSpacingRule;

			public byte bOutlineLevel;

			public short wShadingWeight;

			public short wShadingStyle;

			public short wNumberingStart;

			public short wNumberingStyle;

			public short wNumberingTab;

			public short wBorderSpace;

			public short wBorderWidth;

			public short wBorders;
		}

		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static MatchEvaluator _003C_003E9__109_0;

			public static MatchEvaluator _003C_003E9__123_0;

			public _003C_003Ec()
			{
				((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
			}

			internal string _003CUnicode2String_003Eb__109_0(Match x)
			{
				global::_59951DAD obj = (global::_59951DAD)_63AF42A1._71AC5D16[673];
				string _9D21DE0A = ((global::D1B3723C)_63AF42A1._71AC5D16[75])();
				char _243BA3BA = ((global::_01946E8A)_63AF42A1._71AC5D16[90])(((global::C42DE8A2)_63AF42A1._71AC5D16[270])(((global::B2A33F12)_63AF42A1._71AC5D16[622])(x, "$1"), 16));
				return obj(_9D21DE0A, ((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA));
			}

			internal string _003CStringToUnicode2_003Eb__123_0(Match m)
			{
				global::_59951DAD obj = (global::_59951DAD)_63AF42A1._71AC5D16[673];
				int D89E = ((global::_1F211E96)_63AF42A1._71AC5D16[392])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(m), 0);
				return obj("\\u", ((global::E313AC90)_63AF42A1._71AC5D16[558])(ref D89E, "x4"));
			}
		}


		public static string Backup_Path = ((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.Desktop);

		public static string str_uid = "";

		public static string str_vsen = ProductInfo.Version;

		public static string js1 = "var a=266335226;var b=1757282074;var x= 413817+\".\"+(a+b)";

		public static string js2 = "var b=function(a,b){for(var d=0;d<b.length-2;d+=3){var c=b.charAt(d+2),c=\"a\"<=c?c.charCodeAt(0)-87:Number(c),c=\"+\"==b.charAt(d+1)?a>>>c:a<<c;a=\"+\"==b.charAt(d)?a+c&4294967295:a^c}return a};var tk=function(a,TKK){for(var e=TKK.split(\".\"),h=Number(e[0])||0,g=[],d=0,f=0;f<a.length;f++){var c=a.charCodeAt(f);128>c?g[d++]=c:(2048>c?g[d++]=c>>6|192:(55296==(c&64512)&&f+1<a.length&&56320==(a.charCodeAt(f+1)&64512)?(c=65536+((c&1023)<<10)+(a.charCodeAt(++f)&1023),g[d++]=c>>18|240,g[d++]=c>>12&63|128):g[d++]=c>>12|224,g[d++]=c>>6&63|128),g[d++]=c&63|128)}a=h;for(d=0;d<g.length;d++){a+=g[d],a=b(a,\"+-a^+6\")}a=b(a,\"+-3^+b+-f\");a^=Number(e[1])||0;0>a&&(a=(a&2147483647)+2147483648);a%=1000000;return a.toString()+\".\"+(a^h)};";

		public static string jsbaidu = "function a(r,o){for(var t=0;t<o.length-2;t+=3){var a=o.charAt(t+2);a=a>=\"a\"?a.charCodeAt(0)-87:Number(a),a=\"+\"===o.charAt(t+1)?r>>>a:r<<a,r=\"+\"===o.charAt(t)?r+a&4294967295:r^a}return r}var C=null;var token=function(r,_gtk){var o=r.length;o>30&&(r=\"\"+r.substr(0,10)+r.substr(Math.floor(o/2)-5,10)+r.substring(r.length,r.length-10));var t=void 0,t=null!==C?C:(C=_gtk||\"\")||\"\";for(var e=t.split(\".\"),h=Number(e[0])||0,i=Number(e[1])||0,d=[],f=0,g=0;g<r.length;g++){var m=r.charCodeAt(g);128>m?d[f++]=m:(2048>m?d[f++]=m>>6|192:(55296===(64512&m)&&g+1<r.length&&56320===(64512&r.charCodeAt(g+1))?(m=65536+((1023&m)<<10)+(1023&r.charCodeAt(++g)),d[f++]=m>>18|240,d[f++]=m>>12&63|128):d[f++]=m>>12|224,d[f++]=m>>6&63|128),d[f++]=63&m|128)}for(var S=h,u=\"+-a^+6\",l=\"+-3^+b+-f\",s=0;s<d.length;s++){S+=d[s],S=a(S,u)}return S=a(S,l),S^=i,0>S&&(S=(2147483647&S)+2147483648),S%=1000000,S.toString()+\".\"+(S^h)};";

		public static bool isOpenUpdate = false;

		public static bool isloadApk = false;

		public static string 无效key提示语 = "请到菜单“更多设置-个性化设置”中填写正确的apikey。";

		public static string Master_switch = "0";

		public static string Topping = "0";

		public static string Translation_Model = "1";

		public static string formLocation = "1";


		public static string UrlBaiduWeb = "https://fanyi.baidu.com/?aldtype=16047#en/zh/";

		public static string UrlDeeplWeb = "https://www.deepl.com/translator#en/zh/";

		public static string UrlgugeWeb = "https://translate.google.cn/?sl=auto&tl=zh-CN&text={0}%0A&op=translate";

		public static string niu_apikey = "";

		public static string niu_dictNo = "";

		public static string niu_memoryNo = "";

		public static string ZhiPuAI_key = "";

		public static string ZhiPuAI_key_model = "glm-4-flash";

		public static int ZhiPuAI_duandian = 1;

		public static string TermRepoIDList = "";

		public static string SentRepoIDList = "";

		public static string tengxunSecretId2 = "";

		public static string tengxunSecretKey2 = "";

		public static string SiliconFlowAI_key = "";

		public static string SiliconFlowAI_model = "Qwen/Qwen2.5-7B-Instruct";

		public static string DeepSeekAI_key = "";

		public static string DeepSeekAI_model = "";

        public static string GeminiKey = "";
        public static string GeminiModel = CloudTranslationClient.DefaultGeminiModel;
        public static string GeminiDirection = "auto";
        public static string QwenKey = "";
        public static string QwenEndpoint = CloudTranslationClient.DefaultQwenEndpoint;
        public static string QwenDirection = "auto";

		public static string doubao_key = "";

		public static string doubao_id = "";

		public static int isjiekounum = 14;



		public static string version = "v=V5_0";

		public static bool isVersion = true;

		public static string filePath = "";

		public static bool isDanChuangKou = true;

		public static bool is单词悬浮框 = false;

		public static string baiduAPPid = "";

		public static string baidumiyao = "";

		public static Random rang = ((global::_2CA347A8)_63AF42A1._71AC5D16[87])();

		public static string tengxunSecretId = "";

		public static string tengxunSecretKey = "";

		public static int 上高 = 1;

		public static int 上宽 = 1;

		public static int 下高 = 1;

		public static int 下宽 = 1;

		public static int 左高 = 1;

		public static int 左宽 = 1;

		public static int 右高 = 1;

		public static int 右宽 = 1;

		public static int splitterNum = 100;

		public static float translationFontSize = 11f;

		public static int translationLineSpacing = 350;

		public static string translationFontType = "黑体";

		public static float originalFontSize = 9f;

		public static int originalLineSpacing = 250;

		public static string originalFontType = "宋体";

		public static ArrayList listfont = ((global::CF9E7600)_63AF42A1._71AC5D16[715])();

		public static bool isUpdateSetting = true;

		public static IntPtr FromMainHandle;

		public static string swyxxl = "1";

		public static string 快捷键隐藏显示 = "F6";

		public static string 快捷键顺时针切换窗口 = "Ctrl + Right";

		public static string 快捷键逆时针切换窗口 = "Ctrl + Left";

		public static string 快捷键顺序切换翻译引擎 = "F4";

		public static bool _is自动翻译总开关 = true;

		public static bool _is追加文本 = false;

		public static bool _is最小化 = false;

		public static string updateTime = "2025-01-01";

		public static string userInfo = "";

		public static string baiduqianfan_key = "";

		public static string baiduqianfan_strmodel = "ERNIE-4.5-Turbo-32K";

		public static string chatGPTApiKey = "";

		public static int chatGPTApiModelType = 1;

		public static int chatGPTApiDomainType = 1;

		public static string chatGPTApiDomain2 = "";

		public static string chatGPTApiDomain1 = "";

		public static string tengxun_Token = "";

		public static DateTime tengxun_Token_time = ((global::_4822E694)_63AF42A1._71AC5D16[506])();

		public static string DeeplLanguage = "1";

		public static string DeeplToLang = "zh";

		public static string DeeplFromLang = "en";

		public static string Deeplkey_turn = "";

		public static string Deeplkey_official = "";

		public static string DeeplInterface = "2";

		public static string volcengine_key = "";

		public static string volcengine_id = "";

		public static string googleDomain = "1";

		public const int WM_USER = 1024;

		public const int EM_GETPARAFORMAT = 1085;

		public const int EM_SETPARAFORMAT = 1095;

		public const long MAX_TAB_STOPS = 32L;

		public const uint PFM_LINESPACING = 256u;

		private static classRSA classRsa = new classRSA();

		public static int CountChineseCharacters(string input)
		{
			string c12BCB = "[\\u4e00-\\u9fa5]";
			MatchCollection _8630F = ((global::_338164B6)_63AF42A1._71AC5D16[256])(input, c12BCB);
			return ((global::_2507CF93)_63AF42A1._71AC5D16[689])(_8630F);
		}

		public static double CalculateChineseCharacterPercentage(string input)
		{
			int num = ((global::A11AF20B)_63AF42A1._71AC5D16[217])(input);
			int num2 = CountChineseCharacters(input);
			return (double)num2 / (double)num;
		}

		public static int SubstringCount(string str, string substring)
		{
			if (((global::_20A4ABBF)_63AF42A1._71AC5D16[22])(str, substring))
			{
				string d = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(str, substring, "");
				return (((global::A11AF20B)_63AF42A1._71AC5D16[217])(str) - ((global::A11AF20B)_63AF42A1._71AC5D16[217])(d)) / ((global::A11AF20B)_63AF42A1._71AC5D16[217])(substring);
			}
			return 0;
		}

		public static string readTQwxopenId()
		{
			string result = "";
			try
			{
				string _9D21DE0A = ((global::_4BA88D9A)_63AF42A1._71AC5D16[785])(((global::D78F4A33)_63AF42A1._71AC5D16[667])()) ?? "";
				FileStream fileStream = ((global::_200D2A19)_63AF42A1._71AC5D16[210])(((global::_59951DAD)_63AF42A1._71AC5D16[673])(_9D21DE0A, "\\TQ.txt"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				StreamReader streamReader = ((global::_5DB829A4)_63AF42A1._71AC5D16[149])(fileStream);
				result = ((global::ECBAE137)_63AF42A1._71AC5D16[624])(streamReader);
				((global::_7C90458A)_63AF42A1._71AC5D16[35])(streamReader);
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(fileStream);
			}
			catch (Exception)
			{
			}
			return result;
		}

		public static void RunBat()
		{
			string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::_4BA88D9A)_63AF42A1._71AC5D16[785])(((global::D78F4A33)_63AF42A1._71AC5D16[667])()), "\\data\\npmStart-hide.vbs");
			try
			{
				Process process = ((global::D91F8608)_63AF42A1._71AC5D16[147])();
				FileInfo f785D4B = ((global::ED2DF5B6)_63AF42A1._71AC5D16[126])(text);
				((global::C4372590)_63AF42A1._71AC5D16[212])(((global::F6312DA6)_63AF42A1._71AC5D16[738])(process), ((global::_0B86CC2E)_63AF42A1._71AC5D16[188])(((global::D70043A4)_63AF42A1._71AC5D16[257])(f785D4B)));
				((global::_148CBF98)_63AF42A1._71AC5D16[241])(((global::F6312DA6)_63AF42A1._71AC5D16[738])(process), text);
				((global::_5DB8E2BB)_63AF42A1._71AC5D16[181])(((global::F6312DA6)_63AF42A1._71AC5D16[738])(process), EDA62897: false);
				((global::D92C0985)_63AF42A1._71AC5D16[25])(process);
				((global::_98939F87)_63AF42A1._71AC5D16[553])(process);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		public static string Decode(string strDecode)
		{
			string text = "";
			for (int i = 0; i < ((global::A11AF20B)_63AF42A1._71AC5D16[217])(strDecode) / 4; i++)
			{
				global::_59951DAD obj = (global::_59951DAD)_63AF42A1._71AC5D16[673];
				string _9D21DE0A = text;
				char _243BA3BA = (char)((global::BD0BAAB5)_63AF42A1._71AC5D16[0])(((global::F7065C1C)_63AF42A1._71AC5D16[616])(strDecode, i * 4, 4), NumberStyles.HexNumber);
				text = obj(_9D21DE0A, ((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA));
			}
			return text;
		}

		public static void DeleteFolder()
		{
			try
			{
				string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.ApplicationData), "\\Xtranslator-NoLogin-2.9.18B\\sound");
				if (((global::_6108BC1E)_63AF42A1._71AC5D16[384])(text))
				{
					string[] array = ((global::_36A930B4)_63AF42A1._71AC5D16[621])(text);
					foreach (string text2 in array)
					{
						if (((global::_6108BC1E)_63AF42A1._71AC5D16[384])(text2))
						{
							((global::A5AB939F)_63AF42A1._71AC5D16[333])(text2, _0E9C2F00: true);
						}
						else if (F0BBF48A._0CACA9A7(text2))
						{
							((global::_5A9C3784)_63AF42A1._71AC5D16[440])(text2);
						}
					}
				}
				createDirectoryInfo();
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "ClassMain-DeleteFolder", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		private static void createDirectoryInfo()
		{
			string _6A0A = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.ApplicationData), "\\Xtranslator-NoLogin-2.9.18B\\sound");
			DirectoryInfo directoryInfo = ((global::_79868D8C)_63AF42A1._71AC5D16[57])(_6A0A);
			if (!((global::_433D6B07)_63AF42A1._71AC5D16[347])(directoryInfo))
			{
				((global::_4E9D5591)_63AF42A1._71AC5D16[357])(directoryInfo);
			}
		}

        public static void KillProcess(string processName)
        {
            string expected = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(ClassMain).Assembly.Location), processName + ".exe"));
            foreach (var process in Process.GetProcessesByName(processName))
            using (process) try {
                if (!process.HasExited && string.Equals(Path.GetFullPath(process.MainModule.FileName), expected, StringComparison.OrdinalIgnoreCase)) {
                    process.Kill(); process.WaitForExit(1500);
                }
            } catch (Exception) { /* Never terminate an inaccessible or unrelated same-name process. */ }
        }

		public static string getUA()
		{
			List<string> list = new List<string>();
			list.Add("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.25 Safari/537.36 Core/1.70.3741.400 QQBrowser/10.5.3863.400");
			list.Add("Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.0; Trident/4.0)");
			list.Add("Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0");
			list.Add("Mozilla/5.0 (Windows NT 6.1; rv:2.0.1) Gecko/20100101 Firefox/4.0.1");
			list.Add("Opera/9.80 (Windows NT 6.1; U; en) Presto/2.8.131 Version/11.11");
			list.Add("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1; Trident/4.0; SE 2.X MetaSr 1.0; SE 2.X MetaSr 1.0; .NET CLR 2.0.50727; SE 2.X MetaSr 1.0)");
			list.Add("Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");
			list.Add("Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.110 Safari/537.36");
			int num = 0;
			do
			{
				num = ((global::_5138E009)_63AF42A1._71AC5D16[687])(rang, 0, 8);
			}
			while (list.Count <= num);
			return list[num];
		}

		public static bool HttpDownload(string url, string path)
		{
			string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::EE29FA07)_63AF42A1._71AC5D16[5])(path), "\\temp");
			((global::B2980F37)_63AF42A1._71AC5D16[548])(text);
			string text2 = ((global::CAA00931)_63AF42A1._71AC5D16[629])(text, "\\", ((global::_53ACD81B)_63AF42A1._71AC5D16[704])(path), ".temp");
			if (((global::DD21FB0C)_63AF42A1._71AC5D16[467])(text2))
			{
				((global::_5A9C3784)_63AF42A1._71AC5D16[440])(text2);
			}
			try
			{
				FileStream fileStream = ((global::_200D2A19)_63AF42A1._71AC5D16[210])(text2, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
				HttpWebRequest httpWebRequest = ((global::_5B37291E)_63AF42A1._71AC5D16[678])(url) as HttpWebRequest;
				((global::_8B96D7B3)_63AF42A1._71AC5D16[55])(httpWebRequest, null);
				HttpWebResponse _9EBBE = ((global::_341A0726)_63AF42A1._71AC5D16[387])(httpWebRequest) as HttpWebResponse;
				Stream stream = ((global::C1981588)_63AF42A1._71AC5D16[272])(_9EBBE);
				byte[] array = new byte[1024];
				for (int num = ((global::DD3EEC2E)_63AF42A1._71AC5D16[562])(stream, array, 0, array.Length); num > 0; num = ((global::DD3EEC2E)_63AF42A1._71AC5D16[562])(stream, array, 0, array.Length))
				{
					((global::_343E1C2F)_63AF42A1._71AC5D16[746])(fileStream, array, 0, num);
				}
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(fileStream);
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(stream);
				((global::AD18152E)_63AF42A1._71AC5D16[458])(text2, path);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

        public static string readJiekou() { return SettingsStore.Load(moren); }
        public static string SettingsSaveError;
        public static void writeJiekou(string str) { SettingsStore.Save(str); }

        private static string moren()
		{
			Rectangle _3610C53A = ((global::_86032311)_63AF42A1._71AC5D16[663])(((global::_25250B8F)_63AF42A1._71AC5D16[356])());
			int num = ((global::_4CA32621)_63AF42A1._71AC5D16[541])(ref _3610C53A);
			_3610C53A = ((global::_86032311)_63AF42A1._71AC5D16[663])(((global::_25250B8F)_63AF42A1._71AC5D16[356])());
			int num2 = ((global::C3BBBB98)_63AF42A1._71AC5D16[756])(ref _3610C53A);
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			dictionary.Add("EngineRegistry", EngineRegistry.Export());
                dictionary.Add("Translation_interface", "4");
			dictionary.Add("Master_switch", "1");
			dictionary.Add("Topping", "1");
			double _6307519F = ((global::CA1DA7B9)_63AF42A1._71AC5D16[231])((double)num2 * 0.6 * 0.7);
			dictionary.Add("splitterNum", ((global::ACA24205)_63AF42A1._71AC5D16[448])(ref _6307519F) ?? "");
			dictionary.Add("Translation_Model", "1");
			dictionary.Add("formLocation", "1");
			_6307519F = ((global::CA1DA7B9)_63AF42A1._71AC5D16[231])((double)num2 * 0.5);
			dictionary.Add("formSizeH1", ((global::ACA24205)_63AF42A1._71AC5D16[448])(ref _6307519F) ?? "");
			_6307519F = ((global::CA1DA7B9)_63AF42A1._71AC5D16[231])((double)num * 0.18);
			dictionary.Add("formSizeW1", ((global::ACA24205)_63AF42A1._71AC5D16[448])(ref _6307519F) ?? "");
			dictionary.Add("formSizeH2", "1");
			dictionary.Add("formSizeW2", "1");
			dictionary.Add("formSizeH3", "1");
			dictionary.Add("formSizeW3", "1");
			dictionary.Add("formSizeH4", "1");
			dictionary.Add("formSizeW4", "1");
			dictionary.Add("translationFontSize", ((global::F4224E3B)_63AF42A1._71AC5D16[726])(ref translationFontSize) ?? "");
			dictionary.Add("translationLineSpacing", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref translationLineSpacing) ?? "");
			dictionary.Add("translationFontType", translationFontType ?? "");
			dictionary.Add("originalFontSize", ((global::F4224E3B)_63AF42A1._71AC5D16[726])(ref originalFontSize) ?? "");
			dictionary.Add("originalLineSpacing", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref originalLineSpacing) ?? "");
			dictionary.Add("originalFontType", originalFontType ?? "");
			UiPreferences.Write(dictionary);
            LocalModelSettings.WriteMainSettings(dictionary);
            dictionary.Add("ShortcutkeyHiddenDisplay", 快捷键隐藏显示 ?? "");
			dictionary.Add("ShortcutKeySwitchesTheWindowClockwise", 快捷键顺时针切换窗口 ?? "");
			dictionary.Add("ShortcutKeyToSwitchWindowAnticlockwise", 快捷键逆时针切换窗口 ?? "");
			dictionary.Add("ShortcutkeySequenceSwitchTranslationEngine", 快捷键顺序切换翻译引擎 ?? "");
			dictionary.Add("BaiduAppId", baiduAPPid ?? "");
			dictionary.Add("BaiduAppKey", baidumiyao ?? "");
			dictionary.Add("volcengine_id", volcengine_id);
			dictionary.Add("volcengine_key", volcengine_key);
			dictionary.Add("isTheMouseHoveringDisplay", ((global::E6055B8D)_63AF42A1._71AC5D16[779])(ref is单词悬浮框) ?? "");
			dictionary.Add("uid", str_uid ?? "");
			dictionary.Add("tengxunSecretId", tengxunSecretId ?? "");
			dictionary.Add("tengxunSecretKey", tengxunSecretKey ?? "");
			dictionary.Add("chatGPTApiKey", chatGPTApiKey ?? "");
			dictionary.Add("chatGPTApiDomainType", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref chatGPTApiDomainType) ?? "");
			dictionary.Add("chatGPTApiModelType", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref chatGPTApiModelType) ?? "");
			return DictionaryToJson(dictionary);
		}

		public static string DictionaryToJson(Dictionary<string, string> dicjson)
		{
			try
			{
				JObject jObject = ((global::BBB81587)_63AF42A1._71AC5D16[524])();
				foreach (KeyValuePair<string, string> item in dicjson)
				{
					string _613B = ((global::E83366AE)_63AF42A1._71AC5D16[201])(item.Key);
					string _578A64AA = ((global::E83366AE)_63AF42A1._71AC5D16[201])(item.Value);
					JProperty _279C = ((global::ECA4D828)_63AF42A1._71AC5D16[158])(_613B, _578A64AA);
					((global::C9248033)_63AF42A1._71AC5D16[732])(jObject, _279C);
				}
				return _8C345DB5._6A239107(jObject.ToString());
			}
			catch (Exception)
			{
				return "{}";
			}
		}

		public unsafe static string Unicode2String(string source)
		{
			return _60B3BBBB.CFBF99BE(((global::_6C83F09E)_63AF42A1._71AC5D16[226])("\\\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled), source, _003C_003Ec._003C_003E9__109_0 ?? (_003C_003Ec._003C_003E9__109_0 = new global::System.Text.RegularExpressions.MatchEvaluator(_003C_003Ec._003C_003E9._003CUnicode2String_003Eb__109_0)));
		}

		public static string readbaiduapi()
		{
			string _6D23F = "";
			try
			{
				string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.ApplicationData), "\\Xtranslator-NoLogin-2.9.18B");
				DirectoryInfo directoryInfo = ((global::_79868D8C)_63AF42A1._71AC5D16[57])(text);
				if (!((global::_433D6B07)_63AF42A1._71AC5D16[347])(directoryInfo))
				{
					((global::_4E9D5591)_63AF42A1._71AC5D16[357])(directoryInfo);
					writebaiduapi("|");
				}
				else if (!((global::DD21FB0C)_63AF42A1._71AC5D16[467])(((global::_59951DAD)_63AF42A1._71AC5D16[673])(text, "\\b.txt")))
				{
					writebaiduapi("|");
				}
				FileStream fileStream = _70AB47B4._143EBDA6(_9B8E342D._358A3A88(text, "\\b.txt"), FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);
				StreamReader streamReader = ((global::_5DB829A4)_63AF42A1._71AC5D16[149])(fileStream);
				_6D23F = ((global::ECBAE137)_63AF42A1._71AC5D16[624])(streamReader);
				((global::_7C90458A)_63AF42A1._71AC5D16[35])(streamReader);
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(fileStream);
			}
			catch (Exception)
			{
			}
			return B5A9082C._9C83478A(B5A9082C._9C83478A(_6D23F, "\r", ""), "\n", "");
		}

		public static void writebaiduapi(string str)
		{
			try
			{
				string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.ApplicationData), "\\Xtranslator-NoLogin-2.9.18B");
				DirectoryInfo directoryInfo = ((global::_79868D8C)_63AF42A1._71AC5D16[57])(text);
				if (!((global::_433D6B07)_63AF42A1._71AC5D16[347])(directoryInfo))
				{
					((global::_4E9D5591)_63AF42A1._71AC5D16[357])(directoryInfo);
				}
				FileStream fileStream = _70AB47B4._143EBDA6(_9B8E342D._358A3A88(text, "\\b.txt"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				StreamWriter streamWriter = ((global::_07067E05)_63AF42A1._71AC5D16[406])(fileStream);
				((global::_98B5513F)_63AF42A1._71AC5D16[520])(streamWriter, str);
				((global::_5587C407)_63AF42A1._71AC5D16[600])(streamWriter);
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(fileStream);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		public static bool 判断是不是中译英(string str)
		{
			int num = bytelenght(str);
			int num2 = ((global::A11AF20B)_63AF42A1._71AC5D16[217])(str);
			double num3 = (double)num2 * 0.35;
			if ((double)(num - num2) > num3)
			{
				return true;
			}
			return false;
		}

		private static int bytelenght(string str)
		{
			byte[] array = ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::_1AAD1BBD)_63AF42A1._71AC5D16[93])(), str);
			int num = 0;
			for (int i = 0; i < ((global::_9B27A180)_63AF42A1._71AC5D16[38])(array, 0); i++)
			{
				if (i % 2 == 0)
				{
					num++;
				}
				else if (array[i] > 0)
				{
					num++;
				}
			}
			return num;
		}

		public static bool HasChinese(string str)
		{
			return ((global::D82FE89E)_63AF42A1._71AC5D16[295])(str, "[\\u4e00-\\u9fa5]");
		}

		public static CookieContainer strToCookie(string strCookie, string url)
		{
			string[] array = _16039B03.EA39B9B6(strCookie, new char[1] { ';' });
			CookieContainer cookieContainer = ((global::_442393A6)_63AF42A1._71AC5D16[166])();
			((global::BD3112A1)_63AF42A1._71AC5D16[460])(cookieContainer, 60);
			string[] array2 = array;
			foreach (string text in array2)
			{
				if (((global::BFBCFC0D)_63AF42A1._71AC5D16[396])(text, "expires") <= 0)
				{
					F3012135.DD34E907(cookieContainer, ((global::B2AB5798)_63AF42A1._71AC5D16[590])(url), text);
				}
			}
			return cookieContainer;
		}

		public static void Export_Backup()
		{
			try
			{
				if (!((global::_6108BC1E)_63AF42A1._71AC5D16[384])(Backup_Path))
				{
					((global::D8883001)_63AF42A1._71AC5D16[651])("当前文件夹路径不存在，已为您备份到桌面");
					Backup_Path = ((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.Desktop);
				}
				string text = _9B8E342D._358A3A88(Backup_Path, "\\Xtranslator-NoLogin-2.9.18B备份");
				if (!((global::_6108BC1E)_63AF42A1._71AC5D16[384])(text))
				{
					((global::B2980F37)_63AF42A1._71AC5D16[548])(text);
				}
                jiluset();
                if (SettingsSaveError != null) throw new InvalidOperationException(SettingsSaveError);
				string plaintext = readJiekou();
				DateTime _999FDDAA = ((global::_4822E694)_63AF42A1._71AC5D16[506])();
				string a6A80C8A = ((global::C6B6B733)_63AF42A1._71AC5D16[360])(ref _999FDDAA, "yyyyMMddHHmmssfff");
				string a9B76D0C = ((global::CAA00931)_63AF42A1._71AC5D16[629])(text, "\\xtrans_export_", a6A80C8A, ".json");
				((global::_7E95F689)_63AF42A1._71AC5D16[375])(a9B76D0C, RSA.RSAEncrypt(plaintext));
				OpenFolder(text);
			}
			catch (Exception _2523970E)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(_2523970E));
			}
		}

		public static void jiluset()
		{
			try
			{
				SettingsSaveError=null;
                Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("EngineRegistry", EngineRegistry.Export());
                dictionary.Add("Translation_interface", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref isjiekounum) ?? "");
				dictionary.Add("Master_switch", Master_switch ?? "");
				dictionary.Add("Topping", Topping ?? "");
				dictionary.Add("splitterNum", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref splitterNum) ?? "");
				dictionary.Add("Translation_Model", Translation_Model ?? "");
				dictionary.Add("formLocation", formLocation ?? "");
				dictionary.Add("formSizeH1", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 右高) ?? "");
				dictionary.Add("formSizeW1", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 右宽) ?? "");
				dictionary.Add("formSizeH2", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 下高) ?? "");
				dictionary.Add("formSizeW2", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 下宽) ?? "");
				dictionary.Add("formSizeH3", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 左高) ?? "");
				dictionary.Add("formSizeW3", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 左宽) ?? "");
				dictionary.Add("formSizeH4", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 上高) ?? "");
				dictionary.Add("formSizeW4", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref 上宽) ?? "");
				dictionary.Add("translationFontSize", ((global::F4224E3B)_63AF42A1._71AC5D16[726])(ref translationFontSize) ?? "");
				dictionary.Add("translationLineSpacing", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref translationLineSpacing) ?? "");
				dictionary.Add("translationFontType", translationFontType ?? "");
				dictionary.Add("originalFontSize", ((global::F4224E3B)_63AF42A1._71AC5D16[726])(ref originalFontSize) ?? "");
				dictionary.Add("originalLineSpacing", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref originalLineSpacing) ?? "");
				dictionary.Add("originalFontType", originalFontType ?? "");
				UiPreferences.Write(dictionary);
            LocalModelSettings.WriteMainSettings(dictionary);
            dictionary.Add("ShortcutkeyHiddenDisplay", 快捷键隐藏显示 ?? "");
				dictionary.Add("ShortcutKeySwitchesTheWindowClockwise", 快捷键顺时针切换窗口 ?? "");
				dictionary.Add("ShortcutKeyToSwitchWindowAnticlockwise", 快捷键逆时针切换窗口 ?? "");
				dictionary.Add("ShortcutkeySequenceSwitchTranslationEngine", 快捷键顺序切换翻译引擎 ?? "");
				dictionary.Add("baiduqianfan_model", baiduqianfan_strmodel ?? "");
				dictionary.Add("baiduqianfan_key", baiduqianfan_key);
				dictionary.Add("BaiduAppId", baiduAPPid ?? "");
				dictionary.Add("BaiduAppKey", baidumiyao ?? "");
				dictionary.Add("volcengine_id", volcengine_id);
				dictionary.Add("volcengine_key", volcengine_key);
				dictionary.Add("doubao_id", doubao_id);
				dictionary.Add("doubao_key", doubao_key);
				dictionary.Add("DeepSeekAI_key", DeepSeekAI_key);
				dictionary.Add("DeepSeekAI_model", DeepSeekAI_model);
                dictionary.Add("GeminiKey",GeminiKey);
                dictionary.Add("GeminiModel",GeminiModel);
                dictionary.Add("GeminiDirection",GeminiDirection);
                dictionary.Add("QwenKey",QwenKey);
                dictionary.Add("QwenEndpoint",QwenEndpoint);
                dictionary.Add("QwenDirection",QwenDirection);
				dictionary.Add("SiliconFlowAI_model", SiliconFlowAI_model);
				dictionary.Add("SiliconFlowAI_key", SiliconFlowAI_key);

				dictionary.Add("ZhiPuAI_key", ZhiPuAI_key);
				dictionary.Add("ZhiPuAI_key_model", ZhiPuAI_key_model);
				dictionary.Add("ZhiPuAI_duandian", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref ZhiPuAI_duandian) ?? "");
				dictionary.Add("niu_apikey", niu_apikey);
				dictionary.Add("niu_dictNo", niu_dictNo);
				dictionary.Add("niu_memoryNo", niu_memoryNo);
				dictionary.Add("chatGPTApiKey", chatGPTApiKey ?? "");
				dictionary.Add("chatGPTApiDomainType", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref chatGPTApiDomainType) ?? "");
				dictionary.Add("chatGPTApiModelType", ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref chatGPTApiModelType) ?? "");
				dictionary.Add("isTheMouseHoveringDisplay", ((global::E6055B8D)_63AF42A1._71AC5D16[779])(ref is单词悬浮框) ?? "");
				dictionary.Add("swyxxl", swyxxl);
				dictionary.Add("uid", str_uid);
				dictionary.Add("tengxunSecretId", tengxunSecretId);
				dictionary.Add("tengxunSecretKey", tengxunSecretKey);
				dictionary.Add("DeeplInterface", DeeplInterface);
				dictionary.Add("DeeplLanguage", DeeplLanguage);
				dictionary.Add("DeeplToLang", DeeplToLang);
				dictionary.Add("DeeplFromLang", DeeplFromLang);
				dictionary.Add("Deeplkey_turn", Deeplkey_turn);
				dictionary.Add("Deeplkey_official", Deeplkey_official);
				dictionary.Add("fanyiTokenTime", ((global::C6B6B733)_63AF42A1._71AC5D16[360])(ref tengxun_Token_time, "yyyy-MM-dd"));
				dictionary.Add("fanyiToken", tengxun_Token);
				dictionary.Add("tencentSecretKey2", tengxunSecretKey2);
				dictionary.Add("tencentSecretId2", tengxunSecretId2);
				dictionary.Add("SentRepoIDList", SentRepoIDList);
				dictionary.Add("TermRepoIDList", TermRepoIDList);
				dictionary.Add("googleDomain", googleDomain ?? "");
				dictionary.Add("userInfo", userInfo ?? "");
				dictionary.Add("updateTime", updateTime ?? "");
				dictionary.Add("Backup_Path", Backup_Path);

				string str = DictionaryToJson(dictionary);
				writeJiekou(str);
			}
			catch (Exception ex)
			{
				SettingsSaveError="设置写入失败，请检查配置目录是否可写或磁盘空间是否充足。";
			}
		}

		public static void read_setSetting(string strSetting)
		{
			if (string.IsNullOrWhiteSpace(strSetting)) strSetting = "{}";
			JObject _08A7893D = ((global::C52E42B4)_63AF42A1._71AC5D16[744])(strSetting);
            UiPreferences.Read(_08A7893D);
            LocalModelSettings.ReadMainSettings(_08A7893D);
            EngineRegistry.Import((string)_08A7893D["EngineRegistry"]);
            GeminiKey=(string)_08A7893D["GeminiKey"] ?? "";
            GeminiModel=(string)_08A7893D["GeminiModel"] ?? CloudTranslationClient.DefaultGeminiModel;
            GeminiDirection=(string)_08A7893D["GeminiDirection"] ?? "auto";
            QwenKey=(string)_08A7893D["QwenKey"] ?? "";
            QwenEndpoint=(string)_08A7893D["QwenEndpoint"] ?? CloudTranslationClient.DefaultQwenEndpoint;
            QwenDirection=(string)_08A7893D["QwenDirection"] ?? "auto";
            if(GeminiDirection!="en-zh" && GeminiDirection!="zh-en") GeminiDirection="auto";
            if(QwenDirection!="en-zh" && QwenDirection!="zh-en") QwenDirection="auto";
			try
			{
				string _0D27CCA = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Master_switch")?.ToString() ?? "";
				if (((global::_471F4D82)_63AF42A1._71AC5D16[500])(_0D27CCA, "1"))
				{
					Master_switch = "0";
				}
				else
				{
					Master_switch = "1";
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
			try
			{
				string _1C261D = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Translation_interface")?.ToString() ?? "";
				isjiekounum = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D);
			}
			catch (Exception ex2)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex2.ToString());
			}
			try
			{
				DeeplInterface = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "DeeplInterface")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(DeeplInterface))
				{
					DeeplInterface = "2";
				}
				if (BD0534AB.DB326C83(DeeplInterface, "1"))
				{
					DeeplInterface = "2";
				}
			}
			catch (Exception ex3)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex3.ToString());
			}
			try
			{
				Deeplkey_turn = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Deeplkey_turn")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(Deeplkey_turn))
				{
					Deeplkey_turn = "";
				}
			}
			catch (Exception ex4)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex4.ToString());
			}
			try
			{
				Deeplkey_official = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Deeplkey_official")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(Deeplkey_official))
				{
					Deeplkey_official = "";
				}
			}
			catch (Exception ex5)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex5.ToString());
			}
			try
			{
				DeeplLanguage = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "DeeplLanguage")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(DeeplLanguage))
				{
					DeeplLanguage = "1";
				}
			}
			catch (Exception ex6)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex6.ToString());
			}
			try
			{
				DeeplToLang = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "DeeplToLang")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(DeeplToLang))
				{
					DeeplToLang = "zh";
				}
			}
			catch (Exception ex7)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex7.ToString());
			}
			try
			{
				DeeplFromLang = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "DeeplFromLang")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(DeeplFromLang))
				{
					DeeplFromLang = "en";
				}
			}
			catch (Exception ex8)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex8.ToString());
			}
			try
			{
				string _0D27CCA2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Topping")?.ToString() ?? "";
				if (((global::_471F4D82)_63AF42A1._71AC5D16[500])(_0D27CCA2, "1"))
				{
					Topping = "0";
				}
				else
				{
					Topping = "1";
				}
			}
			catch (Exception ex9)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex9.ToString());
			}
			try
			{
				splitterNum = _9681D68E._528F432E(((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "splitterNum")?.ToString() ?? "");
			}
			catch (Exception ex10)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex10.ToString());
			}
			try
			{
				Translation_Model = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Translation_Model")?.ToString() ?? "";
			}
			catch (Exception ex11)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex11.ToString());
			}
			try
			{
				formLocation = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formLocation")?.ToString() ?? "";
			}
			catch (Exception ex12)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex12.ToString());
			}
			try
			{
				string _1C261D2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeH1")?.ToString() ?? "";
				右高 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D2);
				string _1C261D3 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeW1")?.ToString() ?? "";
				右宽 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D3);
				string _1C261D4 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeH2")?.ToString() ?? "";
				下高 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D4);
				string _1C261D5 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeW2")?.ToString() ?? "";
				下宽 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D5);
				string _1C261D6 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeH3")?.ToString() ?? "";
				左高 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D6);
				string _1C261D7 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeW3")?.ToString() ?? "";
				左宽 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D7);
				string _1C261D8 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeH4")?.ToString() ?? "";
				上高 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D8);
				string _1C261D9 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "formSizeW4")?.ToString() ?? "";
				上宽 = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D9);
			}
			catch (Exception ex13)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex13.ToString());
			}
			try
			{
				string _779F728D = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "translationFontSize")?.ToString() ?? "";
				translationFontSize = ((global::D8AAEE22)_63AF42A1._71AC5D16[641])(_779F728D);
				string _1C261D10 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "translationLineSpacing")?.ToString() ?? "";
				translationLineSpacing = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D10);
				string text = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "translationFontType")?.ToString() ?? "";
				translationFontType = text;
				string _779F728D2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "originalFontSize")?.ToString() ?? "";
				originalFontSize = ((global::D8AAEE22)_63AF42A1._71AC5D16[641])(_779F728D2);
				string _1C261D11 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "originalLineSpacing")?.ToString() ?? "";
				originalLineSpacing = ((global::_07B2CC91)_63AF42A1._71AC5D16[508])(_1C261D11);
				string text2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "originalFontType")?.ToString() ?? "";
				originalFontType = text2;
			}
			catch (Exception ex14)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex14.ToString());
			}
			try
			{
				string text3 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ShortcutkeyHiddenDisplay")?.ToString() ?? "";
				快捷键隐藏显示 = UiPreferences.ReadShowShortcut(_08A7893D);
				string text4 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ShortcutKeySwitchesTheWindowClockwise")?.ToString() ?? "";
				快捷键顺时针切换窗口 = text4;
				string text5 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ShortcutKeyToSwitchWindowAnticlockwise")?.ToString() ?? "";
				快捷键逆时针切换窗口 = text5;
				string text6 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ShortcutkeySequenceSwitchTranslationEngine")?.ToString() ?? "";
				快捷键顺序切换翻译引擎 = text6;
				ClassSetHotKey.xiezaimoren();
				ClassSetHotKey.sethotkey();
			}
			catch (Exception ex15)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex15.ToString());
			}
			try
			{
				string text7 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "BaiduAppId")?.ToString() ?? "";
				baiduAPPid = text7;
				string text8 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "BaiduAppKey")?.ToString() ?? "";
				baidumiyao = text8;
			}
			catch (Exception ex16)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex16.ToString());
			}
			try
			{
				doubao_key = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "doubao_key")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(doubao_key))
				{
					doubao_key = "";
				}
			}
			catch (Exception ex17)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex17.ToString());
			}
			try
			{
				doubao_id = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "doubao_id")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(doubao_id))
				{
					doubao_id = "";
				}
			}
			catch (Exception ex18)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex18.ToString());
			}
			try
			{
				DeepSeekAI_key = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "DeepSeekAI_key")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(DeepSeekAI_key))
				{
					DeepSeekAI_key = "";
				}
			}
			catch (Exception ex19)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex19.ToString());
			}
			try
			{
				DeepSeekAI_model = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "DeepSeekAI_model")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(DeepSeekAI_model))
				{
					DeepSeekAI_model = "deepseek-flash";
				}
			}
			catch (Exception ex20)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex20.ToString());
			}

			try
			{
				SiliconFlowAI_key = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "SiliconFlowAI_key")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(SiliconFlowAI_key))
				{
					SiliconFlowAI_key = "";
				}
			}
			catch (Exception ex22)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex22.ToString());
			}
			try
			{
				TermRepoIDList = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "TermRepoIDList")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(TermRepoIDList))
				{
					TermRepoIDList = "";
				}
			}
			catch (Exception ex23)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex23.ToString());
			}
			try
			{
				SentRepoIDList = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "SentRepoIDList")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(SentRepoIDList))
				{
					SentRepoIDList = "";
				}
			}
			catch (Exception ex24)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex24.ToString());
			}
			try
			{
				tengxunSecretId2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "tencentSecretId2")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(tengxunSecretId2))
				{
					tengxunSecretId2 = "";
				}
			}
			catch (Exception ex25)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex25.ToString());
			}
			try
			{
				tengxunSecretKey2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "tencentSecretKey2")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(tengxunSecretKey2))
				{
					tengxunSecretKey2 = "";
				}
			}
			catch (Exception ex26)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex26.ToString());
			}
			try
			{
				SiliconFlowAI_model = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "SiliconFlowAI_model")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(SiliconFlowAI_model))
				{
					SiliconFlowAI_model = "Qwen/Qwen2.5-7B-Instruct";
				}
			}
			catch (Exception ex27)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex27.ToString());
			}
			try
			{
				ZhiPuAI_key_model = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ZhiPuAI_key_model")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(ZhiPuAI_key_model))
				{
					ZhiPuAI_key_model = "glm-4-flash";
				}
			}
			catch (Exception ex28)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex28.ToString());
			}
			try
			{
				ZhiPuAI_key = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ZhiPuAI_key")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(ZhiPuAI_key))
				{
					ZhiPuAI_key = "";
				}
			}
			catch (Exception ex29)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex29.ToString());
			}
			try
			{
				string text9 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "ZhiPuAI_duandian")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(text9))
				{
					text9 = "1";
				}
				ZhiPuAI_duandian = _9681D68E._528F432E(text9);
			}
			catch (Exception ex30)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex30.ToString());
			}
			try
			{
				baiduqianfan_key = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "baiduqianfan_key")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(baiduqianfan_key))
				{
					baiduqianfan_key = "";
				}
			}
			catch (Exception ex31)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex31.ToString());
			}
			try
			{
				baiduqianfan_strmodel = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "baiduqianfan_strmodel")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(baiduqianfan_strmodel))
				{
					baiduqianfan_strmodel = "ERNIE-4.5-Turbo-32K";
				}
			}
			catch (Exception ex32)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex32.ToString());
			}
			try
			{
				string _442C01BC2 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "updateTime")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(_442C01BC2))
				{
					DateTime _0CA29B = ((global::_4822E694)_63AF42A1._71AC5D16[506])();
					_0CA29B = ((global::_9C8E638C)_63AF42A1._71AC5D16[496])(ref _0CA29B, -2.0);
					updateTime = ((global::C6B6B733)_63AF42A1._71AC5D16[360])(ref _0CA29B, "yyyy-MM-dd");
				}
			}
			catch (Exception ex33)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex33.ToString());
			}
			try
			{
				Backup_Path = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Backup_Path")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(Backup_Path))
				{
					Backup_Path = ((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.Desktop);
				}
			}
			catch (Exception ex34)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex34.ToString());
			}
			try
			{
				userInfo = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "userInfo")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(userInfo))
				{
					userInfo = "";
				}
			}
			catch (Exception ex35)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex35.ToString());
			}
			try
			{
				niu_apikey = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "niu_apikey")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(niu_apikey))
				{
					niu_apikey = "";
				}
			}
			catch (Exception ex36)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex36.ToString());
			}
			try
			{
				niu_dictNo = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "niu_dictNo")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(niu_dictNo))
				{
					niu_dictNo = "";
				}
			}
			catch (Exception ex37)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex37.ToString());
			}
			try
			{
				niu_memoryNo = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "niu_memoryNo")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(niu_memoryNo))
				{
					niu_memoryNo = "";
				}
			}
			catch (Exception ex38)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex38.ToString());
			}
			try
			{
				volcengine_id = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "volcengine_id")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(volcengine_id))
				{
					volcengine_id = "";
				}
			}
			catch (Exception ex39)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex39.ToString());
			}
			try
			{
				volcengine_key = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "volcengine_key")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(volcengine_key))
				{
					volcengine_key = "";
				}
			}
			catch (Exception ex40)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex40.ToString());
			}
			try
			{
				string text10 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "chatGPTApiKey")?.ToString() ?? "";
				chatGPTApiKey = text10;
				int num = ((global::CF25D803)_63AF42A1._71AC5D16[602])(((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "chatGPTApiDomainType"));
				chatGPTApiDomainType = num;
			}
			catch (Exception ex41)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex41.ToString());
			}
			try
			{
				int num2 = ((global::CF25D803)_63AF42A1._71AC5D16[602])(((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "chatGPTApiModelType"));
				chatGPTApiModelType = num2;
			}
			catch (Exception ex42)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex42.ToString());
			}
			try
			{
				string text11 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "tengxunSecretId")?.ToString() ?? "";
				tengxunSecretId = text11;
				string text12 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "tengxunSecretKey")?.ToString() ?? "";
				tengxunSecretKey = text12;
			}
			catch (Exception ex43)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex43.ToString());
			}
			try
			{
				string recoveredLocal = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "isTheMouseHoveringDisplay")?.ToString() ?? "";
				if (((global::_471F4D82)_63AF42A1._71AC5D16[500])(((global::F5871009)_63AF42A1._71AC5D16[274])(recoveredLocal), "true"))
				{
					is单词悬浮框 = true;
				}
				else
				{
					is单词悬浮框 = false;
				}
			}
			catch (Exception ex44)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex44.ToString());
			}
			try
			{
				swyxxl = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "swyxxl")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(swyxxl))
				{
					swyxxl = "1";
				}
			}
			catch (Exception ex45)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex45.ToString());
			}
			try
			{
				tengxun_Token = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "fanyiToken")?.ToString() ?? "";
				if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(tengxun_Token))
				{
					tengxun_Token_time = _2439D33A._85869DBC(((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "fanyiTokenTime")?.ToString() ?? "");
					TimeSpan _6C = ((global::_7A83D990)_63AF42A1._71AC5D16[204])(((global::_4822E694)_63AF42A1._71AC5D16[506])(), tengxun_Token_time);
					if (((global::AA3B1CB4)_63AF42A1._71AC5D16[700])(ref _6C) >= 1)
					{
						tengxun_Token = "";
					}
				}
			}
			catch (Exception ex46)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex46.ToString());
			}
			try
			{
				googleDomain = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "googleDomain")?.ToString() ?? "";
				if (((global::EFADF098)_63AF42A1._71AC5D16[723])(googleDomain))
				{
					googleDomain = "1";
				}
			}
			catch (Exception ex47)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex47.ToString());
			}

		}

		public static void OpenFolder(string folderPath)
		{
			try
			{
				if (((global::_6108BC1E)_63AF42A1._71AC5D16[384])(folderPath))
				{
					((global::_343FE692)_63AF42A1._71AC5D16[793])("explorer.exe", folderPath);
				}
				else
				{
					((global::D8883001)_63AF42A1._71AC5D16[651])("文件夹不存在！");
				}
			}
			catch (Exception _2523970E)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(((global::_59951DAD)_63AF42A1._71AC5D16[673])("打开文件夹时出错: ", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(_2523970E)));
			}
		}

		public static string UrlEncode(string url, bool isUpper = true)
		{
			return UrlEncode(url, ((global::A238FCBC)_63AF42A1._71AC5D16[619])(), isUpper);
		}

		public static string UrlEncode(string url, Encoding encoding, bool isUpper = false)
		{
			string text = ((global::D50D69B5)_63AF42A1._71AC5D16[519])(url, encoding);
			if (!isUpper)
			{
				return text;
			}
			return GetUpperEncode(text);
		}

		private static string GetUpperEncode(string encode)
		{
			StringBuilder stringBuilder = ((global::_3C2B76AD)_63AF42A1._71AC5D16[198])();
			int num = int.MinValue;
			for (int i = 0; i < ((global::A11AF20B)_63AF42A1._71AC5D16[217])(encode); i++)
			{
				char _243BA3BA = ((global::_1F211E96)_63AF42A1._71AC5D16[392])(encode, i);
				string text = ((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA);
				if (((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(text, "%"))
				{
					num = i;
				}
				if (i - num == 1 || i - num == 2)
				{
					text = ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(text);
				}
				_6CA1E428._2087D796(stringBuilder, text);
			}
			return stringBuilder.ToString();
		}

		public unsafe static string StringToUnicode2(string input)
		{
			return _588F170B.AAB63081(input, "[^a-zA-Z0-9 ,.!?']", _003C_003Ec._003C_003E9__123_0 ?? (_003C_003Ec._003C_003E9__123_0 = new global::System.Text.RegularExpressions.MatchEvaluator(_003C_003Ec._003C_003E9._003CStringToUnicode2_003Eb__123_0)));
		}

		public static void DeleteFolder(string dirPath)
		{
			try
			{
				if (!((global::_6108BC1E)_63AF42A1._71AC5D16[384])(dirPath))
				{
					return;
				}
				string[] array = ((global::_36A930B4)_63AF42A1._71AC5D16[621])(dirPath);
				foreach (string text in array)
				{
					if (((global::_6108BC1E)_63AF42A1._71AC5D16[384])(text))
					{
						((global::A5AB939F)_63AF42A1._71AC5D16[333])(text, _0E9C2F00: true);
					}
					else if (F0BBF48A._0CACA9A7(text))
					{
						((global::_5A9C3784)_63AF42A1._71AC5D16[440])(text);
					}
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		public static string GetTimeStamp()
		{
			DateTime time = ((global::_4822E694)_63AF42A1._71AC5D16[506])();
			long _71A63F8F = ConvertDateTimeToInt(time);
			return ((global::_93BB578C)_63AF42A1._71AC5D16[623])(ref _71A63F8F);
		}

		private static long ConvertDateTimeToInt(DateTime time)
		{
			DateTime _3F00B39E = ((global::_333ACE9A)_63AF42A1._71AC5D16[773])(((global::_1619A936)_63AF42A1._71AC5D16[697])(), ((global::D40F28BE)_63AF42A1._71AC5D16[413])(1970, 1, 1, 0, 0, 0, 0));
			return (((global::_9E92CC89)_63AF42A1._71AC5D16[50])(ref time) - ((global::_9E92CC89)_63AF42A1._71AC5D16[50])(ref _3F00B39E)) / 10000;
		}

		public static string EncryptWithMD5(string source)
		{
			byte[] _0CB81C = ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), source);
			MD5 _42301D8D = ((global::_4726CE9B)_63AF42A1._71AC5D16[549])();
			byte[] array = ((global::FB26893E)_63AF42A1._71AC5D16[417])(_42301D8D, _0CB81C);
			StringBuilder stringBuilder = ((global::BD291E09)_63AF42A1._71AC5D16[680])(40);
			for (int i = 0; i < array.Length; i++)
			{
				((global::EBB57480)_63AF42A1._71AC5D16[244])(stringBuilder, ((global::_00BA3109)_63AF42A1._71AC5D16[654])(ref array[i], "x2"));
			}
			return ((global::F5871009)_63AF42A1._71AC5D16[274])(stringBuilder.ToString());
		}

		public static string getIp()
		{
			string text = "10.168.8.63";
			try
			{
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
			return text ?? "";
		}

		[DllImport("user32", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, ref PARAFORMAT2 lParam);

		public static void SetLineSpace(Control ctl, int dyLineSpacing)
		{
			try
			{
				PARAFORMAT2 lParam = default(PARAFORMAT2);
				var rich=ctl as RichTextBox;
				if(rich!=null)using(var document=new NativeTextGeometry(rich.Handle))
					if(document.SetMinimumLineHeight(rich.TextLength,Math.Max(dyLineSpacing/20f,rich.Font.SizeInPoints*1.45f)))return;
				lParam.cbSize = Marshal.SizeOf(lParam);
				// A minimum (not exact) line height lets RichEdit fit larger fonts.
				lParam.bLineSpacingRule = 3;
				lParam.dyLineSpacing = Math.Max(dyLineSpacing,(int)Math.Ceiling(ctl.Font.SizeInPoints*20*1.45));
				lParam.dwMask = 256u;
				SendMessage(((global::_421C8807)_63AF42A1._71AC5D16[193])(ctl, ((global::_1BA5D3BC)_63AF42A1._71AC5D16[171])(ctl)), 1095, 0, ref lParam);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "ClassMain-SetLineSpace", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		public static string WebUrlZhuan(string str, int typenum = 0)
		{
			str = ((global::_8FB766B7)_63AF42A1._71AC5D16[129])(str);
			if (typenum == 1)
			{
				str = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(str, "%2F", "%5C%2F");
			}
			return str;
		}

		public static string GetUid(string id)
		{
			long xx = ((global::_1D161724)_63AF42A1._71AC5D16[319])(id) + 10000000;
			return CodeDecimalConversion.IntToi32(xx);
		}



		public ClassMain()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
