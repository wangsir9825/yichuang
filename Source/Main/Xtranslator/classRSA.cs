using System.Security.Cryptography;

namespace Xtranslator
{
	internal class classRSA
	{
		private string publicKey = "<RSAKeyValue><Modulus>xitE89Au1niOcLXTxtrdRxz9Q5rd3xczS83aSkmncJcRTj8jGa8HUi+cpAsh80qv9ayLAQF3zJgP6C4nGGJIqauguCDdONjUQHtrgyct/Gr1tL3q3ay/awJD6tujfnh0kOr57sxvo+yvHW+fXni7DUvGzJfZM1tlIC1avUy+oIU=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";

		private string privateKey = "<RSAKeyValue><Modulus>xitE89Au1niOcLXTxtrdRxz9Q5rd3xczS83aSkmncJcRTj8jGa8HUi+cpAsh80qv9ayLAQF3zJgP6C4nGGJIqauguCDdONjUQHtrgyct/Gr1tL3q3ay/awJD6tujfnh0kOr57sxvo+yvHW+fXni7DUvGzJfZM1tlIC1avUy+oIU=</Modulus><Exponent>AQAB</Exponent><P>xsN952COqpnuGSAW1GYQeH+Oqy8y0z5xu+HYQxmikfMonHdmymEk0TCTa4pjwg6i+n9uHf4vkwzym4hMWJN22w==</P><Q>/zvxeW+7Px0PIcMKvryw3eONiOaqUQXZaHNEmeJpt/Qj5XQBTGmgE1ATe/O+Bf8j2ri2pNf1aZwPEzDhPZh0Hw==</Q><DP>uSxnn6fVQhsmN0IuArMTpS8xO0o9j5AdZWyCjgDT9CUVMLAMk37tTfZzYT0Msz3vmHAfptGogNcPLtdMSTkZAQ==</DP><DQ>/bcdrGSIEw4U66/Rr5csjfXUzQDWz2HsIytqRzpNcwIq6L1vSQE6kdgo9i9udtGQV60ESksu9oOebKpLTQqOaw==</DQ><InverseQ>DkUyMqq8MQEeW4vHsnMgttENTBGKzJDjvU/C9s7tPM4i82ls/Dh3NvSF4liYVnvsvt17asL9pqZQHPvTIQqBCg==</InverseQ><D>Qj9y1KMzFUXGMI3Il4DRWpWpYTFjnNHvtxbBM/Z9gFkwUt0uicaGhwYdsoV6OxAr1NByw8pO11m6b58GONOagG52xZBWgoXzXWk0bpKEUqLlAxOI3p3Bs3yI+dp49eX0GfuY/JW64Uil58BIhDZ4Jsc5l4fRdrta3kLtFdMuJn0=</D></RSAKeyValue>";

		public string RsaEncrypt(string value, bool usePkcs8 = false)
		{
			if (((global::EFADF098)_63AF42A1._71AC5D16[723])(value))
			{
				return value;
			}
			RSACryptoServiceProvider rSACryptoServiceProvider = A511CE25._3C398914(1024);
			((global::D32C0E1D)_63AF42A1._71AC5D16[780])(rSACryptoServiceProvider, publicKey);
			byte[] _2FA = ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), value);
			_2FA = ((global::_4F8F032E)_63AF42A1._71AC5D16[386])(rSACryptoServiceProvider, _2FA, _4385250D: false);
			return ((global::_0C2B750D)_63AF42A1._71AC5D16[725])(_2FA);
		}

		public string RsaDecrypt(string value, bool usePkcs8 = false)
		{
			if (((global::EFADF098)_63AF42A1._71AC5D16[723])(value))
			{
				return value;
			}
			RSACryptoServiceProvider rSACryptoServiceProvider = A511CE25._3C398914(1024);
			((global::D32C0E1D)_63AF42A1._71AC5D16[780])(rSACryptoServiceProvider, privateKey);
			byte[] eA9961AC = ((global::F30CA117)_63AF42A1._71AC5D16[567])(rSACryptoServiceProvider, ((global::_7B26F80C)_63AF42A1._71AC5D16[232])(value), D619850C: false);
			return ((global::_88328A0C)_63AF42A1._71AC5D16[552])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), eA9961AC);
		}

		public classRSA()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
