using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YiChuang
{
	internal class HtmlProcess
	{
		public static string GetElementById(string html, string id)
		{
			string _94863F = "<([a-z]+)(?:(?!id)[^<>])*id=([\"']?){0}\\2[^>]*>(?>(?<o><\\1[^>]*>)|(?<-o></\\1>)|(?:(?!</?\\1).))*(?(o)(?!))</\\1>";
			_94863F = ((global::_879AE88F)_63AF42A1._71AC5D16[572])(_94863F, ((global::C99F6B04)_63AF42A1._71AC5D16[128])(id));
			Match match = ((global::C4067339)_63AF42A1._71AC5D16[784])(html, _94863F, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			return ((global::_7A195AAD)_63AF42A1._71AC5D16[85])(match) ? _9F2A1202.CCA9F682(match) : "";
		}

		public static string[] GetElementsByClass(string html, string className)
		{
			return GetElements(html, "", className);
		}

		public static string[] GetElementsByTagName(string html, string tagName)
		{
			return GetElements(html, tagName, "");
		}

		public static string[] GetElementsByTagAndClass(string html, string tagName, string className)
		{
			return GetElements(html, tagName, className);
		}

		public static List<string> GetHtmlallyao(string html, string tag, string attr)
		{
			Regex d49FF = ((global::_64A8B9B4)_63AF42A1._71AC5D16[10])(((global::_9B817F36)_63AF42A1._71AC5D16[169])("(<", tag, "[\\w\\W].+?>)"));
			MatchCollection matchCollection = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF, html);
			List<string> list = new List<string>();
			Regex d49FF2 = ((global::_6C83F09E)_63AF42A1._71AC5D16[226])("([a-zA-Z1-9_-]+)\\s*=\\s*(\\x27|\\x22)([^\\x27\\x22]*)(\\x27|\\x22)", RegexOptions.IgnoreCase);
			for (int i = 0; i < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection); i++)
			{
				MatchCollection matchCollection2 = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF2, ((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection, i).ToString());
				for (int j = 0; j < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection2); j++)
				{
					GroupCollection _2099540C = ((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection2, j));
					if (((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(attr), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						list.Add(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3)));
						break;
					}
				}
			}
			return list;
		}

		public static Dictionary<string, string> GetHtmlallyao(string html, string tag, string attr1, string attr2)
		{
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Regex d49FF = ((global::_64A8B9B4)_63AF42A1._71AC5D16[10])(((global::_9B817F36)_63AF42A1._71AC5D16[169])("(<", tag, "[\\w\\W].+?>)"));
			MatchCollection matchCollection = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF, html);
			Regex d49FF2 = ((global::_6C83F09E)_63AF42A1._71AC5D16[226])("([a-zA-Z1-9_-]+)\\s*=\\s*(\\x27|\\x22)([^\\x27\\x22]*)(\\x27|\\x22)", RegexOptions.IgnoreCase);
			for (int i = 0; i < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection); i++)
			{
				MatchCollection matchCollection2 = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF2, ((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection, i).ToString());
				bool flag = false;
				bool flag2 = false;
				string text = "";
				string value = "";
				for (int j = 0; j < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection2); j++)
				{
					GroupCollection _2099540C = ((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection2, j));
					if (((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(attr1), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						flag = true;
						text = ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3));
						if (!((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(text, "cartSeq"))
						{
						}
					}
					if (BD0534AB.DB326C83(DB23AE31._34B863B2(attr2), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						flag2 = true;
						value = ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3));
					}
					if (!flag || !flag2)
					{
						continue;
					}
					if (dictionary.ContainsKey(text))
					{
						if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(dictionary[text].ToString()))
						{
							break;
						}
						dictionary.Remove(text);
					}
					dictionary.Add(text, value);
					break;
				}
			}
			return dictionary;
		}

		private static string[] GetElements(string html, string tagName, string className)
		{
			string text = "";
			if (((global::C6233EA3)_63AF42A1._71AC5D16[662])(tagName, "") && ((global::C6233EA3)_63AF42A1._71AC5D16[662])(className, ""))
			{
				text = "<({0})(?:(?!class)[^<>])*class=([\"']?){1}\\2[^>]*>(?>(?<o><\\1[^>]*>)|(?<-o></\\1>)|(?:(?!</?\\1).))*(?(o)(?!))</\\1>";
				text = ((global::F7813B2F)_63AF42A1._71AC5D16[89])(text, ((global::C99F6B04)_63AF42A1._71AC5D16[128])(tagName), ((global::C99F6B04)_63AF42A1._71AC5D16[128])(className));
			}
			else if (_843343AB._96292B01(tagName, ""))
			{
				text = "<({0})(?:[^<>])*>(?>(?<o><\\1[^>]*>)|(?<-o></\\1>)|(?:(?!</?\\1).))*(?(o)(?!))</\\1>";
				text = ((global::_879AE88F)_63AF42A1._71AC5D16[572])(text, ((global::C99F6B04)_63AF42A1._71AC5D16[128])(tagName));
			}
			else if (_843343AB._96292B01(className, ""))
			{
				text = "<([a-z]+)(?:(?!class)[^<>])*class=([\"']?){0}\\2[^>]*>(?>(?<o><\\1[^>]*>)|(?<-o></\\1>)|(?:(?!</?\\1).))*(?(o)(?!))</\\1>";
				text = ((global::_879AE88F)_63AF42A1._71AC5D16[572])(text, ((global::C99F6B04)_63AF42A1._71AC5D16[128])(className));
			}
			if (BD0534AB.DB326C83(text, ""))
			{
				return new string[0];
			}
			List<string> list = new List<string>();
			Regex regex = ((global::_6C83F09E)_63AF42A1._71AC5D16[226])(text, RegexOptions.IgnoreCase | RegexOptions.Singleline);
			Match match = ((global::_038ED9A6)_63AF42A1._71AC5D16[768])(regex, html);
			while (_2D8B5918.BF20B08D(match))
			{
				list.Add(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(match));
				match = ((global::D136B3A0)_63AF42A1._71AC5D16[294])(regex, html, ((global::_58A67D34)_63AF42A1._71AC5D16[368])(match) + ((global::_0625258F)_63AF42A1._71AC5D16[453])(match));
			}
			return list.ToArray();
		}

		public static string GetTitle(string HtmlResponse)
		{
			Match _68810C = ((global::C4067339)_63AF42A1._71AC5D16[784])(HtmlResponse, "<title>([^<]*)</title>", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			return ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(_68810C), 1));
		}

		public static string GetMeta(string HtmlResponse)
		{
			Match _68810C = ((global::C4067339)_63AF42A1._71AC5D16[784])(HtmlResponse, "<meta name=\"DESCRIPTION\" content=\"([^<]*)\">", RegexOptions.IgnoreCase | RegexOptions.Multiline);
			return ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(_68810C), 1));
		}

		public static List<string> PickupImgUrl(string html)
		{
			Regex d49FF = ((global::_6C83F09E)_63AF42A1._71AC5D16[226])("<img\\b[^<>]*?\\bsrc[\\s\\t\\r\\n]*=[\\s\\t\\r\\n]*[\"']?[\\s\\t\\r\\n]*(?<imgUrl>[^\\s\\t\\r\\n\"'<>]*)[^<>]*?/?[\\s\\t\\r\\n]*>", RegexOptions.IgnoreCase);
			MatchCollection e02E3E = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF, html);
			List<string> list = new List<string>();
			IEnumerator enumerator = ((global::_2B2087B2)_63AF42A1._71AC5D16[515])(e02E3E);
			try
			{
				while (_9DB6399B._3AAECF2C(enumerator))
				{
					Match _68810C = (Match)F8115528.A522B594(enumerator);
					list.Add(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_85990C8A)_63AF42A1._71AC5D16[649])(((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(_68810C), "imgUrl")));
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return list;
		}

		public static string PickupImgUrlFirst(string html)
		{
			List<string> list = PickupImgUrl(html);
			return (list.Count == 0) ? _17ADF890._48ADE80A() : list[0];
		}

		public static string StripHtml(string strHtml)
		{
			Regex e20C0A = ((global::_64A8B9B4)_63AF42A1._71AC5D16[10])("<(.|\n)+?>");
			string bCB88C0A = ((global::BEAB559D)_63AF42A1._71AC5D16[699])(e20C0A, strHtml, "");
			bCB88C0A = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "<", "<");
			return ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, ">", ">");
		}

		public static string DelHTML(string Htmlstring)
		{
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "<(.[^>]*)>", "", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "([\\r\\n])[\\s]+", "", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "-->", "", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "<!--.*", "", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(quot|#34);", "\"", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(amp|#38);", "&", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(lt|#60);", "<", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(gt|#62);", ">", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(nbsp|#160);", " ", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(iexcl|#161);", "¡", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(cent|#162);", "¢", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(pound|#163);", "£", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&(copy|#169);", "©", RegexOptions.IgnoreCase);
			Htmlstring = ((global::_0D2D6084)_63AF42A1._71AC5D16[98])(Htmlstring, "&#(\\d+);", "", RegexOptions.IgnoreCase);
			((global::E3051AA0)_63AF42A1._71AC5D16[794])(Htmlstring, "<", "");
			((global::E3051AA0)_63AF42A1._71AC5D16[794])(Htmlstring, ">", "");
			((global::E3051AA0)_63AF42A1._71AC5D16[794])(Htmlstring, "\r\n", "");
			return Htmlstring;
		}

		public static List<string> GetHtmlallyao乐天(string html, string tag, string attr1, string attr2)
		{
			List<string> list = new List<string>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Regex d49FF = ((global::_64A8B9B4)_63AF42A1._71AC5D16[10])(((global::_9B817F36)_63AF42A1._71AC5D16[169])("(<", tag, "[\\w\\W].+?>)"));
			MatchCollection matchCollection = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF, html);
			Regex d49FF2 = ((global::_6C83F09E)_63AF42A1._71AC5D16[226])("([a-zA-Z1-9_-]+)\\s*=\\s*(\\x27|\\x22)([^\\x27\\x22]*)(\\x27|\\x22)", RegexOptions.IgnoreCase);
			for (int i = 0; i < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection); i++)
			{
				MatchCollection matchCollection2 = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF2, ((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection, i).ToString());
				bool flag = false;
				bool flag2 = false;
				string text = "";
				string text2 = "";
				for (int j = 0; j < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection2); j++)
				{
					GroupCollection _2099540C = ((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection2, j));
					if (((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(attr1), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						flag = true;
						text = ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3));
						if (!((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(text, "cartSeq"))
						{
						}
					}
					if (BD0534AB.DB326C83(DB23AE31._34B863B2(attr2), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						flag2 = true;
						text2 = ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3));
					}
					if (!flag || !flag2)
					{
						continue;
					}
					if (((global::_471F4D82)_63AF42A1._71AC5D16[500])(text, "cartPrdSeq"))
					{
						list.Add(text2);
					}
					if (dictionary.ContainsKey(text))
					{
						if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(dictionary[text].ToString()))
						{
							break;
						}
						dictionary.Remove(text);
					}
					dictionary.Add(text, text2);
					break;
				}
			}
			IEnumerable<string> source = list.Distinct();
			return source.ToList();
		}

		public static List<string> GetHtmlallyao乐天获取id(string html, string tag, string attr1, string attr2)
		{
			List<string> list = new List<string>();
			Dictionary<string, string> dictionary = new Dictionary<string, string>();
			Regex d49FF = ((global::_64A8B9B4)_63AF42A1._71AC5D16[10])(((global::_9B817F36)_63AF42A1._71AC5D16[169])("(<", tag, "[\\w\\W].+?>)"));
			MatchCollection matchCollection = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF, html);
			Regex d49FF2 = ((global::_6C83F09E)_63AF42A1._71AC5D16[226])("([a-zA-Z1-9_-]+)\\s*=\\s*(\\x27|\\x22)([^\\x27\\x22]*)(\\x27|\\x22)", RegexOptions.IgnoreCase);
			for (int i = 0; i < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection); i++)
			{
				MatchCollection matchCollection2 = ((global::_122EF4B3)_63AF42A1._71AC5D16[620])(d49FF2, ((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection, i).ToString());
				bool flag = false;
				bool flag2 = false;
				string text = "";
				string text2 = "";
				for (int j = 0; j < ((global::_2507CF93)_63AF42A1._71AC5D16[689])(matchCollection2); j++)
				{
					GroupCollection _2099540C = ((global::_0AA8C93B)_63AF42A1._71AC5D16[542])(((global::_4B3E9F96)_63AF42A1._71AC5D16[617])(matchCollection2, j));
					if (((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(attr1), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						flag = true;
						text = ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3));
						if (!((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(text, "cartSeq"))
						{
						}
					}
					if (BD0534AB.DB326C83(DB23AE31._34B863B2(attr2), ((global::_28AF7F8A)_63AF42A1._71AC5D16[633])(((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 1)))))
					{
						flag2 = true;
						text2 = ((global::DBB5AC03)_63AF42A1._71AC5D16[282])(((global::_20A2928E)_63AF42A1._71AC5D16[176])(_2099540C, 3));
					}
					if (!flag || !flag2)
					{
						continue;
					}
					if (((global::_471F4D82)_63AF42A1._71AC5D16[500])(text, "cartNo"))
					{
						list.Add(text2);
					}
					if (dictionary.ContainsKey(text))
					{
						if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(dictionary[text].ToString()))
						{
							break;
						}
						dictionary.Remove(text);
					}
					dictionary.Add(text, text2);
					break;
				}
			}
			IEnumerable<string> source = list.Distinct();
			return source.ToList();
		}

		public HtmlProcess()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
