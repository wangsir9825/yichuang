using System;
using System.Security.Cryptography;

namespace Xtranslator
{
	public class AesUtil
	{
		public static string Decrypt(string data)
		{
			try
			{
				string f412F = "ydsecret://query/key/B*RGygVywfNBwpmBaZg*WT7SIOUP2T0C9WHMZN39j^DAdaZhAnxvGcCY6VYFwnHl";
				string f412F2 = "ydsecret://query/iv/C@lZe2YzHtZ2CYgaXKSVfsb7Y4QWHjITPPZ0nQp87fBeJ!Iv6v^6fvi2WN@bYpJ4";
				MD5 _42301D8D = ((global::_4726CE9B)_63AF42A1._71AC5D16[549])();
				byte[] _18B2AB = ((global::FB26893E)_63AF42A1._71AC5D16[417])(_42301D8D, ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), f412F));
				byte[] _33B = ((global::FB26893E)_63AF42A1._71AC5D16[417])(_42301D8D, ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), f412F2));
				AesManaged aesManaged = ((global::F434340D)_63AF42A1._71AC5D16[493])();
				_18A8EA20._840A27A4(aesManaged, 128);
				FB24CF22._5DA32A24(aesManaged, 128);
				B6380AB6._03875003(aesManaged, CipherMode.CBC);
				_3E30E389._2A067E17(aesManaged, PaddingMode.PKCS7);
				AesManaged aesManaged2 = aesManaged;
				((global::_5798B326)_63AF42A1._71AC5D16[679])(aesManaged2, _18B2AB);
				((global::_3484ED39)_63AF42A1._71AC5D16[596])(aesManaged2, _33B);
				ICryptoTransform _43BB7B = ((global::_3A0580A1)_63AF42A1._71AC5D16[186])(aesManaged2, ((global::E799368F)_63AF42A1._71AC5D16[459])(aesManaged2), ((global::_9921538A)_63AF42A1._71AC5D16[27])(aesManaged2));
				byte[] array = ((global::_7B26F80C)_63AF42A1._71AC5D16[232])(data);
				byte[] eA9961AC = ((global::F1AB3238)_63AF42A1._71AC5D16[727])(_43BB7B, array, 0, array.Length);
				return ((global::_88328A0C)_63AF42A1._71AC5D16[552])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), eA9961AC);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
			return "";
		}

		public AesUtil()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
