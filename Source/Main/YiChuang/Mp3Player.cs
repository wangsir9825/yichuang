using System.Runtime.InteropServices;

namespace YiChuang
{
	internal class Mp3Player
	{
		public enum State
		{
			mPlaying = 1,
			mPuase,
			mStop
		}

		public struct structMCI
		{
			public bool bMut;

			public int iDur;

			public int iPos;

			public int iVol;

			public int iBal;

			public string iName;

			public State state;
		}

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
		private string Name = "";

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		private string durLength = "";

		[MarshalAs(UnmanagedType.LPTStr)]
		private string TemStr = "";

		private int ilong;

		public structMCI mc = default(structMCI);

		public string FileName
		{
			get
			{
				return mc.iName;
			}
			set
			{
				try
				{
					TemStr = "";
					TemStr = ((global::CD84F691)_63AF42A1._71AC5D16[13])(TemStr, 127, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
					Name = ((global::CD84F691)_63AF42A1._71AC5D16[13])(Name, 260, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
					mc.iName = value;
					ilong = APIClass.GetShortPathName(mc.iName, Name, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(Name));
					Name = GetCurrPath(Name);
					string[] obj = new string[5] { "open ", null, null, null, null };
					char _243BA3BA = ((global::_59362E3A)_63AF42A1._71AC5D16[164])(34);
					obj[1] = ((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA);
					obj[2] = Name;
					_243BA3BA = ((global::_59362E3A)_63AF42A1._71AC5D16[164])(34);
					obj[3] = ((global::B61F3510)_63AF42A1._71AC5D16[419])(ref _243BA3BA);
					obj[4] = " alias media";
					Name = _0B90492C._2004AE2C(obj);
					ilong = APIClass.mciSendString("close all", TemStr, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(TemStr), 0);
					ilong = APIClass.mciSendString(Name, TemStr, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(TemStr), 0);
					ilong = APIClass.mciSendString("set media time format milliseconds", TemStr, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(TemStr), 0);
					mc.state = State.mStop;
				}
				catch
				{
				}
			}
		}

		public int Duration
		{
			get
			{
				durLength = "";
				durLength = ((global::CD84F691)_63AF42A1._71AC5D16[13])(durLength, 128, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
				APIClass.mciSendString("status media length", durLength, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(durLength), 0);
				durLength = ((global::E83366AE)_63AF42A1._71AC5D16[201])(durLength);
				if (((global::_1E0C2AA7)_63AF42A1._71AC5D16[575])(durLength, ""))
				{
					return 0;
				}
				return (int)(_992A3B93.E99418B8(durLength) / 1000.0);
			}
		}

		public int CurrentPosition
		{
			get
			{
				durLength = "";
				durLength = ((global::CD84F691)_63AF42A1._71AC5D16[13])(durLength, 128, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
				APIClass.mciSendString("status media position", durLength, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(durLength), 0);
				mc.iPos = (int)(((global::_4186978D)_63AF42A1._71AC5D16[643])(durLength) / 1000.0);
				return mc.iPos;
			}
		}

		public void play()
		{
			TemStr = "";
			TemStr = ((global::CD84F691)_63AF42A1._71AC5D16[13])(TemStr, 127, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
			APIClass.mciSendString("play media", TemStr, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(TemStr), 0);
			mc.state = State.mPlaying;
		}

		public void StopT()
		{
			TemStr = "";
			TemStr = ((global::CD84F691)_63AF42A1._71AC5D16[13])(TemStr, 128, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
			ilong = APIClass.mciSendString("close media", TemStr, 128, 0);
			ilong = APIClass.mciSendString("close all", TemStr, 128, 0);
			mc.state = State.mStop;
		}

		public void Puase()
		{
			TemStr = "";
			TemStr = ((global::CD84F691)_63AF42A1._71AC5D16[13])(TemStr, 128, ((global::_19BC7A93)_63AF42A1._71AC5D16[293])(" "));
			ilong = APIClass.mciSendString("pause media", TemStr, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(TemStr), 0);
			mc.state = State.mPuase;
		}

		private string GetCurrPath(string name)
		{
			if (((global::A11AF20B)_63AF42A1._71AC5D16[217])(name) < 1)
			{
				return "";
			}
			name = _8C345DB5._6A239107(name);
			name = ((global::F7065C1C)_63AF42A1._71AC5D16[616])(name, 0, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(name) - 1);
			return name;
		}

		public Mp3Player()
		{
			((global::_7D392405)_63AF42A1._71AC5D16[73])(this);
		}
	}
}
