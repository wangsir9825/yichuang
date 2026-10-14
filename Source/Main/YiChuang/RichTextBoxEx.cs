using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YiChuang
{
	public class RichTextBoxEx : RichTextBox
	{
		private struct CHARFORMAT2_STRUCT
		{
			public uint cbSize;

			public uint dwMask;

			public uint dwEffects;

			public int yHeight;

			public int yOffset;

			public int crTextColor;

			public byte bCharSet;

			public byte bPitchAndFamily;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public char[] szFaceName;

			public ushort wWeight;

			public ushort sSpacing;

			public int crBackColor;

			public int lcid;

			public int dwReserved;

			public short sStyle;

			public short wKerning;

			public byte bUnderlineType;

			public byte bAnimation;

			public byte bRevAuthor;

			public byte bReserved1;
		}

		private const string DLL_RICHEDIT = "msftedit.dll";

		private const string WC_RICHEDITW = "RICHEDIT50W";

		private IntPtr moduleHandle;

		private bool attemptedLoad;

		private const int WM_USER = 1024;

		private const int EM_GETCHARFORMAT = 1082;

		private const int EM_SETCHARFORMAT = 1092;

		private const int SCF_SELECTION = 1;

		private const int SCF_WORD = 2;

		private const int SCF_ALL = 4;

		private const uint CFE_BOLD = 1u;

		private const uint CFE_ITALIC = 2u;

		private const uint CFE_UNDERLINE = 4u;

		private const uint CFE_STRIKEOUT = 8u;

		private const uint CFE_PROTECTED = 16u;

		private const uint CFE_LINK = 32u;

		private const uint CFE_AUTOCOLOR = 1073741824u;

		private const uint CFE_SUBSCRIPT = 65536u;

		private const uint CFE_SUPERSCRIPT = 131072u;

		private const int CFM_SMALLCAPS = 64;

		private const int CFM_ALLCAPS = 128;

		private const int CFM_HIDDEN = 256;

		private const int CFM_OUTLINE = 512;

		private const int CFM_SHADOW = 1024;

		private const int CFM_EMBOSS = 2048;

		private const int CFM_IMPRINT = 4096;

		private const int CFM_DISABLED = 8192;

		private const int CFM_REVISED = 16384;

		private const int CFM_BACKCOLOR = 67108864;

		private const int CFM_LCID = 33554432;

		private const int CFM_UNDERLINETYPE = 8388608;

		private const int CFM_WEIGHT = 4194304;

		private const int CFM_SPACING = 2097152;

		private const int CFM_KERNING = 1048576;

		private const int CFM_STYLE = 524288;

		private const int CFM_ANIMATION = 262144;

		private const int CFM_REVAUTHOR = 32768;

		private const uint CFM_BOLD = 1u;

		private const uint CFM_ITALIC = 2u;

		private const uint CFM_UNDERLINE = 4u;

		private const uint CFM_STRIKEOUT = 8u;

		private const uint CFM_PROTECTED = 16u;

		private const uint CFM_LINK = 32u;

		private const uint CFM_SIZE = 2147483648u;

		private const uint CFM_COLOR = 1073741824u;

		private const uint CFM_FACE = 536870912u;

		private const uint CFM_OFFSET = 268435456u;

		private const uint CFM_CHARSET = 134217728u;

		private const uint CFM_SUBSCRIPT = 196608u;

		private const uint CFM_SUPERSCRIPT = 196608u;

		private const byte CFU_UNDERLINENONE = 0;

		private const byte CFU_UNDERLINE = 1;

		private const byte CFU_UNDERLINEWORD = 2;

		private const byte CFU_UNDERLINEDOUBLE = 3;

		private const byte CFU_UNDERLINEDOTTED = 4;

		private const byte CFU_UNDERLINEDASH = 5;

		private const byte CFU_UNDERLINEDASHDOT = 6;

		private const byte CFU_UNDERLINEDASHDOTDOT = 7;

		private const byte CFU_UNDERLINEWAVE = 8;

		private const byte CFU_UNDERLINETHICK = 9;

		private const byte CFU_UNDERLINEHAIRLINE = 10;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = ((global::_56848C97)_63AF42A1._71AC5D16[287])(this);
				AttemptToLoadNewRichEdit();
				if (((global::_00940505)_63AF42A1._71AC5D16[276])(moduleHandle, ((global::_4481691B)_63AF42A1._71AC5D16[561])()))
				{
					((global::C839F718)_63AF42A1._71AC5D16[599])(createParams, "RICHEDIT50W");
				}
				return createParams;
			}
		}

		[DefaultValue(false)]
		public new bool DetectUrls
		{
			get
			{
				return ((global::_0D897291)_63AF42A1._71AC5D16[320])(this);
			}
			set
			{
				((global::_4A08EE21)_63AF42A1._71AC5D16[160])(this, value);
			}
		}

		[DllImport("Kernel32.dll")]
		internal static extern IntPtr LoadLibrary(string libname);

		private void AttemptToLoadNewRichEdit()
		{
			if (!attemptedLoad)
			{
				attemptedLoad = true;
				string libname = ((global::_88866E02)_63AF42A1._71AC5D16[29])(((global::F88D7A97)_63AF42A1._71AC5D16[337])(), "msftedit.dll");
				moduleHandle = LoadLibrary(libname);
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

		public RichTextBoxEx()
		{
			((global::_6F2BA500)_63AF42A1._71AC5D16[302])(this);
			DetectUrls = false;
			moduleHandle = ((global::_4481691B)_63AF42A1._71AC5D16[561])();
			attemptedLoad = false;
		}

		public void InsertLink(string text)
		{
			InsertLink(text, ((global::F90843AB)_63AF42A1._71AC5D16[518])(this));
		}

		public void InsertLink(string text, int position)
		{
			if (position < 0 || position > ((global::A11AF20B)_63AF42A1._71AC5D16[217])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(this)))
			{
				throw ((global::_9B3DC794)_63AF42A1._71AC5D16[211])("position");
			}
			_7321E49C._7E0B283B(this, position);
			((global::_248E8B02)_63AF42A1._71AC5D16[402])(this, text);
			((global::_9F28F90B)_63AF42A1._71AC5D16[328])(this, position, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(text));
			SetSelectionLink(link: true);
			((global::_9F28F90B)_63AF42A1._71AC5D16[328])(this, position + ((global::A11AF20B)_63AF42A1._71AC5D16[217])(text), 0);
		}

		public void InsertLink(string text, string hyperlink)
		{
			InsertLink(text, hyperlink, ((global::F90843AB)_63AF42A1._71AC5D16[518])(this));
		}

		public void InsertLink(string text, string hyperlink, int position)
		{
			if (position < 0 || position > ((global::A11AF20B)_63AF42A1._71AC5D16[217])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(this)))
			{
				throw ((global::_9B3DC794)_63AF42A1._71AC5D16[211])("position");
			}
			_7321E49C._7E0B283B(this, position);
			_1A12A938.CAAC1683(this, _0B90492C._2004AE2C(new string[5] { "{\\rtf1\\ansi ", text, "\\v #", hyperlink, "\\v0}" }));
			((global::_9F28F90B)_63AF42A1._71AC5D16[328])(this, position, ((global::A11AF20B)_63AF42A1._71AC5D16[217])(text) + ((global::A11AF20B)_63AF42A1._71AC5D16[217])(hyperlink) + 1);
			SetSelectionLink(link: true);
			((global::_9F28F90B)_63AF42A1._71AC5D16[328])(this, position + ((global::A11AF20B)_63AF42A1._71AC5D16[217])(text) + ((global::A11AF20B)_63AF42A1._71AC5D16[217])(hyperlink) + 1, 0);
		}

		public void SetSelectionLink(bool link)
		{
			SetSelectionStyle(32u, link ? 32u : 0u);
		}

		public int GetSelectionLink()
		{
			return GetSelectionStyle(32u, 32u);
		}

		private void SetSelectionStyle(uint mask, uint effect)
		{
			CHARFORMAT2_STRUCT structure = default(CHARFORMAT2_STRUCT);
			structure.cbSize = (uint)Marshal.SizeOf(structure);
			structure.dwMask = mask;
			structure.dwEffects = effect;
			IntPtr _4A = default(IntPtr);
			((global::_3A89F0A3)_63AF42A1._71AC5D16[215])(ref _4A, 1);
			IntPtr intPtr = ((global::_6F90FBB8)_63AF42A1._71AC5D16[555])(Marshal.SizeOf(structure));
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
			IntPtr intPtr2 = SendMessage(((global::_4DB657B2)_63AF42A1._71AC5D16[172])(this), 1092, _4A, intPtr);
			((global::_81020403)_63AF42A1._71AC5D16[54])(intPtr);
		}

		private int GetSelectionStyle(uint mask, uint effect)
		{
			CHARFORMAT2_STRUCT structure = default(CHARFORMAT2_STRUCT);
			structure.cbSize = (uint)Marshal.SizeOf(structure);
			structure.szFaceName = new char[32];
			IntPtr _4A = default(IntPtr);
			((global::_3A89F0A3)_63AF42A1._71AC5D16[215])(ref _4A, 1);
			IntPtr intPtr = ((global::_6F90FBB8)_63AF42A1._71AC5D16[555])(Marshal.SizeOf(structure));
			Marshal.StructureToPtr(structure, intPtr, fDeleteOld: false);
			IntPtr intPtr2 = SendMessage(((global::_4DB657B2)_63AF42A1._71AC5D16[172])(this), 1082, _4A, intPtr);
			structure = (CHARFORMAT2_STRUCT)((global::_2BA3698F)_63AF42A1._71AC5D16[313])(intPtr, ((global::_743E8F3F)_63AF42A1._71AC5D16[254])(typeof(CHARFORMAT2_STRUCT).TypeHandle));
			int result = (((structure.dwMask & mask) != mask) ? (-1) : (((structure.dwEffects & effect) == effect) ? 1 : 0));
			_1F8B3C35._84291D37(intPtr);
			return result;
		}

		protected override void WndProc(ref Message m)
		{
			if (((global::_96163720)_63AF42A1._71AC5D16[706])(ref m) == 33 && ((global::_7B3D17BA)_63AF42A1._71AC5D16[786])(this) && !((global::BE00278A)_63AF42A1._71AC5D16[481])(this))
			{
				((global::_44023A3B)_63AF42A1._71AC5D16[593])(this);
			}
			_45B3C702._7406D102(this, ref m);
		}
	}
}
