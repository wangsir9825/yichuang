using System;
using System.IO;
using System.Net;

namespace YiChuang
{
	public class HttpGetPost
	{
		public static string HttpGet(string Url, string postDataStr)
		{
			HttpWebRequest httpWebRequest = (HttpWebRequest)_643EB937._59A5F7A8(_61043631._37273A2B(Url, ((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(postDataStr, "") ? "" : "?", postDataStr));
			((global::_2798B585)_63AF42A1._71AC5D16[99])(httpWebRequest, "GET");
			((global::E9BD31B0)_63AF42A1._71AC5D16[514])(httpWebRequest, "text/html;charset=UTF-8");
			NetworkProxy.Apply(httpWebRequest);
			HttpWebResponse _9EBBE = (HttpWebResponse)((global::_341A0726)_63AF42A1._71AC5D16[387])(httpWebRequest);
			Stream stream = ((global::C1981588)_63AF42A1._71AC5D16[272])(_9EBBE);
			StreamReader streamReader = ((global::_69A30324)_63AF42A1._71AC5D16[41])(stream, ((global::A238FCBC)_63AF42A1._71AC5D16[619])());
			string result = ((global::ECBAE137)_63AF42A1._71AC5D16[624])(streamReader);
			((global::_7C90458A)_63AF42A1._71AC5D16[35])(streamReader);
			((global::F32F78AB)_63AF42A1._71AC5D16[497])(stream);
			return result;
		}

		public static string HttpPost(string Url, string postDataStr)
		{
			try
			{
				byte[] array = ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), postDataStr);
				HttpWebRequest httpWebRequest = (HttpWebRequest)((global::_5B37291E)_63AF42A1._71AC5D16[678])(Url);
				((global::_2798B585)_63AF42A1._71AC5D16[99])(httpWebRequest, "POST");
				NetworkProxy.Apply(httpWebRequest);
				((global::_2CB738A4)_63AF42A1._71AC5D16[709])(httpWebRequest, 10000);
				((global::E9BD31B0)_63AF42A1._71AC5D16[514])(httpWebRequest, "application/x-www-form-urlencoded");
				((global::_3F080F24)_63AF42A1._71AC5D16[675])(httpWebRequest, array.Length);
				Stream stream = ((global::F22D77AA)_63AF42A1._71AC5D16[677])(httpWebRequest);
				((global::_343E1C2F)_63AF42A1._71AC5D16[746])(stream, array, 0, array.Length);
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(stream);
				HttpWebResponse _9EBBE = (HttpWebResponse)((global::_341A0726)_63AF42A1._71AC5D16[387])(httpWebRequest);
				StreamReader _3890D8BF = ((global::_69A30324)_63AF42A1._71AC5D16[41])(((global::C1981588)_63AF42A1._71AC5D16[272])(_9EBBE), ((global::A238FCBC)_63AF42A1._71AC5D16[619])());
				return ((global::ECBAE137)_63AF42A1._71AC5D16[624])(_3890D8BF);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				return "";
			}
		}

		public static string HttpPostversion(string Url, string postDataStr)
		{
			try
			{
				byte[] array = ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), postDataStr);
				HttpWebRequest httpWebRequest = (HttpWebRequest)((global::_5B37291E)_63AF42A1._71AC5D16[678])(Url);
				((global::_2798B585)_63AF42A1._71AC5D16[99])(httpWebRequest, "POST");
				NetworkProxy.Apply(httpWebRequest);
				((global::E9BD31B0)_63AF42A1._71AC5D16[514])(httpWebRequest, "application/x-www-form-urlencoded");
				((global::_3F080F24)_63AF42A1._71AC5D16[675])(httpWebRequest, array.Length);
				Stream stream = ((global::F22D77AA)_63AF42A1._71AC5D16[677])(httpWebRequest);
				((global::_343E1C2F)_63AF42A1._71AC5D16[746])(stream, array, 0, array.Length);
				((global::F32F78AB)_63AF42A1._71AC5D16[497])(stream);
				HttpWebResponse _9EBBE = (HttpWebResponse)((global::_341A0726)_63AF42A1._71AC5D16[387])(httpWebRequest);
				StreamReader _3890D8BF = ((global::_69A30324)_63AF42A1._71AC5D16[41])(((global::C1981588)_63AF42A1._71AC5D16[272])(_9EBBE), ((global::A238FCBC)_63AF42A1._71AC5D16[619])());
				return ((global::ECBAE137)_63AF42A1._71AC5D16[624])(_3890D8BF);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				return "1";
			}
		}

		public HttpGetPost()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
