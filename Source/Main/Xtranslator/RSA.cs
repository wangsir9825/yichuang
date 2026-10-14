using System;
using System.IO;
using System.Security.Cryptography;

namespace Xtranslator
{
	public class RSA
	{
		public static string RSAEncrypt(string plaintext)
		{
			try
			{
				string publicKey = "<RSAKeyValue><Modulus>5m9m14XH3oqLJ8bNGw9e4rGpXpcktv9MSkHSVFVMjHbfv+SJ5v0ubqQxa5YjLN4vc49z7SVju8s0X4gZ6AzZTn06jzWOgyPRV54Q4I0DCYadWW4Ze3e+BOtwgVU1Og3qHKn8vygoj40J6U85Z/PTJu3hN1m75Zr195ju7g9v4Hk=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>";
				return RSAEncrypt(publicKey, ((global::F0984B9C)_63AF42A1._71AC5D16[36])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), plaintext));
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				return "";
			}
		}

		public static string RSAEncrypt(string publicKey, byte[] plainbytes)
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = ((global::_2A10F737)_63AF42A1._71AC5D16[236])())
			{
				((global::D32C0E1D)_63AF42A1._71AC5D16[780])(rSACryptoServiceProvider, publicKey);
				int num = ((global::_68996615)_63AF42A1._71AC5D16[268])(rSACryptoServiceProvider) / 8 - 11;
				byte[] array = new byte[num];
				using (MemoryStream c0A373B = ((global::C43C66A4)_63AF42A1._71AC5D16[671])(plainbytes))
				{
					using (MemoryStream memoryStream = ((global::_61846897)_63AF42A1._71AC5D16[476])())
					{
						int num2;
						while ((num2 = _1D2D10A3._6D19E215(c0A373B, array, 0, num)) > 0)
						{
							byte[] array2 = new byte[num2];
							((global::B5291E99)_63AF42A1._71AC5D16[463])(array, 0, array2, 0, num2);
							byte[] array3 = ((global::_4F8F032E)_63AF42A1._71AC5D16[386])(rSACryptoServiceProvider, array2, _4385250D: false);
							((global::_343E1C2F)_63AF42A1._71AC5D16[746])(memoryStream, array3, 0, array3.Length);
						}
						byte[] c3801C8E = ((global::CBA73085)_63AF42A1._71AC5D16[139])(memoryStream);
						((global::_27A2759D)_63AF42A1._71AC5D16[469])(rSACryptoServiceProvider);
						return ((global::_0C2B750D)_63AF42A1._71AC5D16[725])(c3801C8E);
					}
				}
			}
		}

		public static string RSADecrypt(string ciphertext)
		{
			try
			{
				string privateKey = "<RSAKeyValue><Modulus>5m9m14XH3oqLJ8bNGw9e4rGpXpcktv9MSkHSVFVMjHbfv+SJ5v0ubqQxa5YjLN4vc49z7SVju8s0X4gZ6AzZTn06jzWOgyPRV54Q4I0DCYadWW4Ze3e+BOtwgVU1Og3qHKn8vygoj40J6U85Z/PTJu3hN1m75Zr195ju7g9v4Hk=</Modulus><Exponent>AQAB</Exponent><P>/hf2dnK7rNfl3lbqghWcpFdu778hUpIEBixCDL5WiBtpkZdpSw90aERmHJYaW2RGvGRi6zSftLh00KHsPcNUMw==</P><Q>6Cn/jOLrPapDTEp1Fkq+uz++1Do0eeX7HYqi9rY29CqShzCeI7LEYOoSwYuAJ3xA/DuCdQENPSoJ9KFbO4Wsow==</Q><DP>ga1rHIJro8e/yhxjrKYo/nqc5ICQGhrpMNlPkD9n3CjZVPOISkWF7FzUHEzDANeJfkZhcZa21z24aG3rKo5Qnw==</DP><DQ>MNGsCB8rYlMsRZ2ek2pyQwO7h/sZT8y5ilO9wu08Dwnot/7UMiOEQfDWstY3w5XQQHnvC9WFyCfP4h4QBissyw==</DQ><InverseQ>EG02S7SADhH1EVT9DD0Z62Y0uY7gIYvxX/uq+IzKSCwB8M2G7Qv9xgZQaQlLpCaeKbux3Y59hHM+KpamGL19Kg==</InverseQ><D>vmaYHEbPAgOJvaEXQl+t8DQKFT1fudEysTy31LTyXjGu6XiltXXHUuZaa2IPyHgBz0Nd7znwsW/S44iql0Fen1kzKioEL3svANui63O3o5xdDeExVM6zOf1wUUh/oldovPweChyoAdMtUzgvCbJk1sYDJf++Nr0FeNW1RB1XG30=</D></RSAKeyValue>";
				return Decrypt(privateKey, ((global::_7B26F80C)_63AF42A1._71AC5D16[232])(ciphertext));
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				return ciphertext;
			}
		}

		public static string Decrypt(string privateKey, byte[] cipherbytes)
		{
			using (RSACryptoServiceProvider rSACryptoServiceProvider = ((global::_2A10F737)_63AF42A1._71AC5D16[236])())
			{
				((global::D32C0E1D)_63AF42A1._71AC5D16[780])(rSACryptoServiceProvider, privateKey);
				int num = ((global::_68996615)_63AF42A1._71AC5D16[268])(rSACryptoServiceProvider) / 8;
				byte[] array = new byte[num];
				using (MemoryStream c0A373B = ((global::C43C66A4)_63AF42A1._71AC5D16[671])(cipherbytes))
				{
					using (MemoryStream memoryStream = ((global::_61846897)_63AF42A1._71AC5D16[476])())
					{
						int num2;
						while ((num2 = _1D2D10A3._6D19E215(c0A373B, array, 0, num)) > 0)
						{
							byte[] array2 = new byte[num2];
							((global::B5291E99)_63AF42A1._71AC5D16[463])(array, 0, array2, 0, num2);
							byte[] array3 = ((global::F30CA117)_63AF42A1._71AC5D16[567])(rSACryptoServiceProvider, array2, D619850C: false);
							((global::_343E1C2F)_63AF42A1._71AC5D16[746])(memoryStream, array3, 0, array3.Length);
						}
						byte[] eA9961AC = ((global::CBA73085)_63AF42A1._71AC5D16[139])(memoryStream);
						((global::_27A2759D)_63AF42A1._71AC5D16[469])(rSACryptoServiceProvider);
						return ((global::_88328A0C)_63AF42A1._71AC5D16[552])(((global::A238FCBC)_63AF42A1._71AC5D16[619])(), eA9961AC);
					}
				}
			}
		}

		public RSA()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
