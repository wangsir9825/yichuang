using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace YiChuang
{
	internal class VolcEngineExampleCV
	{
		private static readonly HashSet<char> URLENCODER;

		private static readonly string CONST_ENCODE;

		private static readonly Encoding UTF_8;

		private readonly string region;

		private readonly string service;

		private readonly string schema;

		private readonly string host;

		private readonly string path;

		private readonly string ak;

		private readonly string sk;

		static VolcEngineExampleCV()
		{
			URLENCODER = new HashSet<char>();
			CONST_ENCODE = "0123456789ABCDEF";
			UTF_8 = ((global::A238FCBC)_63AF42A1._71AC5D16[619])();
			for (char c = 'a'; c <= 'z'; c = (char)(c + 1))
			{
				URLENCODER.Add(c);
			}
			for (char c2 = 'A'; c2 <= 'Z'; c2 = (char)(c2 + 1))
			{
				URLENCODER.Add(c2);
			}
			for (char c3 = '0'; c3 <= '9'; c3 = (char)(c3 + 1))
			{
				URLENCODER.Add(c3);
			}
			URLENCODER.Add('-');
			URLENCODER.Add('_');
			URLENCODER.Add('.');
			URLENCODER.Add('~');
		}

		public VolcEngineExampleCV(string region, string service, string schema, string host, string path, string ak, string sk)
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
			this.region = region;
			this.service = service;
			this.host = host;
			this.schema = schema;
			this.path = path;
			this.ak = ak;
			this.sk = sk;
		}

		public string DoRequest(string method, Dictionary<string, string> queryList, byte[] body, DateTime date, string action, string version)
		{
			string result = "";
			try
			{
				if (body == null)
				{
					body = new byte[0];
				}
				string text = HashSHA256(body);
				string text2 = ((global::C6B6B733)_63AF42A1._71AC5D16[360])(ref date, "yyyyMMdd'T'HHmmss'Z'");
				string text3 = ((global::F7065C1C)_63AF42A1._71AC5D16[616])(text2, 0, 8);
				string text4 = "application/json";
				string text5 = "host;x-date;x-content-sha256;content-type";
				SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>(queryList);
				sortedDictionary.Add("Action", action);
				sortedDictionary.Add("Version", version);
				StringBuilder stringBuilder = ((global::_3C2B76AD)_63AF42A1._71AC5D16[198])();
				foreach (KeyValuePair<string, string> item in sortedDictionary)
				{
					((global::EBB57480)_63AF42A1._71AC5D16[244])(((global::EBB57480)_63AF42A1._71AC5D16[244])(((global::EBB57480)_63AF42A1._71AC5D16[244])(((global::EBB57480)_63AF42A1._71AC5D16[244])(stringBuilder, SignStringEncoder(item.Key)), "="), SignStringEncoder(item.Value)), "&");
				}
				int num = _79809F8E._188EEF95(stringBuilder);
				_8BB0B48F._763DC500(stringBuilder, num - 1);
				string text6 = _0B90492C._2004AE2C(new string[17]
				{
					method,
					"\n",
					path,
					"\n",
					stringBuilder?.ToString(),
					"\nhost:",
					host,
					"\nx-date:",
					text2,
					"\nx-content-sha256:",
					text,
					"\ncontent-type:",
					text4,
					"\n\n",
					text5,
					"\n",
					text
				});
				((global::_9C345083)_63AF42A1._71AC5D16[8])(text6);
				string text7 = HashSHA256(((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), text6));
				string text8 = _0B90492C._2004AE2C(new string[6] { text3, "/", region, "/", service, "/request" });
				string content = _0B90492C._2004AE2C(new string[6] { "HMAC-SHA256\n", text2, "\n", text8, "\n", text7 });
				byte[] key = GenSigningSecretKeyV4(sk, text3, region, service);
				string text9 = ((global::F5871009)_63AF42A1._71AC5D16[274])(((global::E3051AA0)_63AF42A1._71AC5D16[794])(((global::D88C550C)_63AF42A1._71AC5D16[531])(HmacSHA256(key, content)), "-", ""));
				Uri _55BEEC3E = B31298BD.E19625B3(C3A60D32.F8BD5BAC("{0}://{1}{2}?{3}", new object[4] { schema, host, path, stringBuilder }));
				HttpWebRequest httpWebRequest = (HttpWebRequest)((global::CB8B3B9C)_63AF42A1._71AC5D16[315])(_55BEEC3E);
				((global::_2798B585)_63AF42A1._71AC5D16[99])(httpWebRequest, method);
				((global::_5AB7C29C)_63AF42A1._71AC5D16[345])(httpWebRequest, host);
				((global::E9BD31B0)_63AF42A1._71AC5D16[514])(httpWebRequest, text4);
				((global::_77121629)_63AF42A1._71AC5D16[105])(((global::C92ED4B4)_63AF42A1._71AC5D16[195])(httpWebRequest), "X-Date", text2);
				((global::_77121629)_63AF42A1._71AC5D16[105])(((global::C92ED4B4)_63AF42A1._71AC5D16[195])(httpWebRequest), "X-Content-Sha256", text);
				_3D1540BC._8A05D226(((global::C92ED4B4)_63AF42A1._71AC5D16[195])(httpWebRequest), "Authorization", _0B90492C._2004AE2C(new string[8] { "HMAC-SHA256 Credential=", ak, "/", text8, ", SignedHeaders=", text5, ", Signature=", text9 }));
				if (!((global::_38BD863E)_63AF42A1._71AC5D16[40])(((global::E501DFB5)_63AF42A1._71AC5D16[258])(httpWebRequest), "GET", StringComparison.OrdinalIgnoreCase))
				{
					((global::E9BD31B0)_63AF42A1._71AC5D16[514])(httpWebRequest, text4);
					using (Stream f52CD38D = ((global::F22D77AA)_63AF42A1._71AC5D16[677])(httpWebRequest))
					{
						((global::_343E1C2F)_63AF42A1._71AC5D16[746])(f52CD38D, body, 0, body.Length);
					}
				}
				HttpWebResponse httpWebResponse = (HttpWebResponse)_2AA5761A.FB9E40AC(httpWebRequest);
				string text10;
				using (StreamReader _3890D8BF = ((global::_5DB829A4)_63AF42A1._71AC5D16[149])(((global::C1981588)_63AF42A1._71AC5D16[272])(httpWebResponse)))
				{
					text10 = ((global::ECBAE137)_63AF42A1._71AC5D16[624])(_3890D8BF);
				}
				DD89C22A.EE08D396((int)A6B60A9E._7C091B32(httpWebResponse));
				((global::_9C345083)_63AF42A1._71AC5D16[8])(text10);
				JObject _08A7893D = _29263820.F126C729(text10 ?? "");
				string _6FA369B = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "TranslationList")?.ToString() ?? "";
				JArray _823E = ((global::C887B4B0)_63AF42A1._71AC5D16[379])(_6FA369B);
				using (IEnumerator<JToken> enumerator2 = ((global::D4267B11)_63AF42A1._71AC5D16[470])(_823E))
				{
					if (_9DB6399B._3AAECF2C(enumerator2))
					{
						JObject _08A7893D2 = _29263820.F126C729(enumerator2.Current?.ToString() ?? "");
						string text11 = ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D2, "Translation")?.ToString() ?? "";
						StringBuilder stringBuilder2 = ((global::_3C2B76AD)_63AF42A1._71AC5D16[198])();
						string[] array = ((global::_1BB28611)_63AF42A1._71AC5D16[441])(text11.ToString(), "\n");
						string[] array2 = array;
						foreach (string b7A93D9D in array2)
						{
							((global::EBB57480)_63AF42A1._71AC5D16[244])(stringBuilder2, ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::_37B6D32C)_63AF42A1._71AC5D16[48])(b7A93D9D), "\r\n"));
						}
						result = stringBuilder2.ToString();
					}
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
			return result;
		}

		private static string SignStringEncoder(string source)
		{
			if (source == null)
			{
				return null;
			}
			StringBuilder stringBuilder = _8E8F38BD.AB9D7F90(_2DA00A0A._7B08FC2E(source));
			byte[] array = ((global::F0984B9C)_63AF42A1._71AC5D16[36])(UTF_8, source);
			foreach (byte b in array)
			{
				if (URLENCODER.Contains((char)b))
				{
					((global::F28493BB)_63AF42A1._71AC5D16[594])(stringBuilder, (char)b);
				}
				else if (b == 32)
				{
					((global::EBB57480)_63AF42A1._71AC5D16[244])(stringBuilder, "%20");
				}
				else
				{
					((global::F28493BB)_63AF42A1._71AC5D16[594])(((global::F28493BB)_63AF42A1._71AC5D16[594])(((global::F28493BB)_63AF42A1._71AC5D16[594])(stringBuilder, '%'), ((global::_1F211E96)_63AF42A1._71AC5D16[392])(CONST_ENCODE, b >> 4)), ((global::_1F211E96)_63AF42A1._71AC5D16[392])(CONST_ENCODE, b & 0xF));
				}
			}
			return stringBuilder.ToString();
		}

		private static string HashSHA256(byte[] content)
		{
			using (SHA256 _42301D8D = ((global::DBB10FB2)_63AF42A1._71AC5D16[504])())
			{
				return ((global::F5871009)_63AF42A1._71AC5D16[274])(((global::E3051AA0)_63AF42A1._71AC5D16[794])(((global::D88C550C)_63AF42A1._71AC5D16[531])(((global::FB26893E)_63AF42A1._71AC5D16[417])(_42301D8D, content)), "-", ""));
			}
		}

		private static byte[] HmacSHA256(byte[] key, string content)
		{
			using (HMACSHA256 _42301D8D = ((global::AF12E39D)_63AF42A1._71AC5D16[757])(key))
			{
				return ((global::FB26893E)_63AF42A1._71AC5D16[417])(_42301D8D, ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), content));
			}
		}

		private byte[] GenSigningSecretKeyV4(string secretKey, string date, string region, string service)
		{
			byte[] key = HmacSHA256(((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), secretKey), date);
			byte[] key2 = HmacSHA256(key, region);
			byte[] key3 = HmacSHA256(key2, service);
			return HmacSHA256(key3, "request");
		}
	}
}
