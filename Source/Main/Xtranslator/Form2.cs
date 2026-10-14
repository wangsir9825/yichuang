using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Pipes;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using DSkin.Controls;
using Newtonsoft.Json.Linq;
using Xtranslator.BMemory;
using Xtranslator.Properties;

namespace Xtranslator
{
	public partial class Form2 : Form
	{
		private bool _isInitializing = true;
        private int localRevision;
        private ToolStripButton localCancelButton;
        private ToolStripButton localRetryButton;
        private ToolStripMenuItem localHunyuanMenu;
        private ToolStripMenuItem localGemmaMenu;
        private ToolStripMenuItem geminiMenu, qwenMenu;
        private SentenceHoverController sentenceHover;
        [ThreadStatic] private static string translationSource;
        [ThreadStatic] private static int translationEngine;
        [ThreadStatic] private static int translationRevision;
        [ThreadStatic] private static string translationMode;
        private sealed class LegacyTranslationRequest { public string Text, Mode; public int Engine, Revision; }


        private readonly TranslationScheduler translationScheduler = new TranslationScheduler();
        private OcrPipeReceiver ocrReceiver;
        private Process ocrProcess;
        private RegisteredWaitHandle programStartedWait;
        private bool runtimeDisposed;

		private int _formLocation = 1;

		private int _翻译模式 = 0;

		private string _windowOldStr = "";

		private const int EC_LEFTMARGIN = 1;

		private const int EC_RIGHTMARGIN = 2;

		private const int EM_SETMARGINS = 211;

		private MouseHook mh;

		private string old_ClipboardData = "";


		private IntPtr nextClipboardViewer;


		private StringBuilder clip = ((global::_3C2B76AD)_63AF42A1._71AC5D16[198])();

		private const uint WM_GETTEXTLENGTH = 14u;

		private const uint WM_GETTEXT = 13u;

		private const uint EM_GETSEL = 176u;

		private StringBuilder sb = ((global::BD291E09)_63AF42A1._71AC5D16[680])(256);

		private string title = "";

		private string ClassName = "";

		private string strHwnd = "";

		private const int Guying_HTLEFT = 10;

		private const int Guying_HTRIGHT = 11;

		private const int Guying_HTTOP = 12;

		private const int Guying_HTTOPLEFT = 13;

		private const int Guying_HTTOPRIGHT = 14;

		private const int Guying_HTBOTTOM = 15;

		private const int Guying_HTBOTTOMLEFT = 16;

		private const int Guying_HTBOTTOMRIGHT = 17;

		private IntPtr NextClipHwnd;

		private const int WM_DRAWCLIPBOARD = 776;

		private const int WM_CHANGECBCHAIN = 781;

		private string shangyicifuzhi = "";

		private DateTime dtstart;

		private DateTime dtstop;

		private string 上一次翻译的文本;

		private bool isone;

		private string 上一次获取的译文;

		private const int WM_VSCROLL = 277;

		private const int SB_BOTTOM = 7;

		private IContainer components;

		private ToolStripEx toolStrip1;



		private ToolStripButton tbtntrans;

		private ToolStripButton tbtntop;



		private SplitContainer splitContainer1;

		private ToolStripButton tbtnmodeselect;

		private ToolStripButton tbtnmodectrl;

		private ToolStripButton tbtnmodecopy;

		private ToolStripButton toolStripButton1;

		private ContextMenuStrip contextMenuStrip1;

		private ToolStripMenuItem 复制译文ToolStripMenuItem;

		private ToolStripMenuItem 复制原文ToolStripMenuItem;

		private ToolStripButton toolStripButton2;

		private RichTextBoxEx rtb译文;

		private Panel palMain;

		private DSkinPanel dSkinPanel1;

		private DSkinPanel dSkinPanel2;

		private System.Windows.Forms.Timer timer1;

		private ToolStripButton toolStripButton3;

		private ToolStripButton toolStripButton4;

		private ToolStripDropDownButton toolStripDropDownButton1;




		private ToolStripMenuItem Blind翻译ToolStripMenuItem;

		private ToolStripMenuItem 搜狗翻译引擎ToolStripMenuItem;

		private ToolStripMenuItem deepl翻译引擎ToolStripMenuItem;


		private RichTextBoxEx rtb原文;







		private ToolStripMenuItem eDeepseek官方AI翻译ToolStripMenuItem;


		[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "SendMessageA")]
		private static extern uint SendMessage(int Hdc, int Msg_Const, int wParam, int lParam);

		private void timer1_Tick(object sender, EventArgs e)
		{
			if(!ClassMain._is自动翻译总开关||_翻译模式!=1||ClassSetHotKey.Suspended||Control.MouseButtons!=MouseButtons.None||activeMouseCaptures>0)return;
            // Do not interrupt an in-progress drag when refreshing the low-level hook.
			try
			{
				clearHook();
				HookEvents();
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		private void HookEvents()
		{
            if (mh != null) mh.UnHook();
            mouseSelection.Reset();
            mh = new MouseHook();
            mh.MouseDownEvent += mh_MouseDownEvent;
            mh.MouseUpEvent += mh_MouseUpEvent;
            mh.SetHook();
		}

		private void clearHook()
		{
            mouseSelection.Reset();
            if (mh != null) mh.UnHook();
		}

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void keybd_event(Keys bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern bool SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, StringBuilder lParam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wparam, IntPtr lparam);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		private static extern int SendMessage(IntPtr hWnd, uint Msg, out int wParam, out int lParam);

		[DllImport("user32.dll", ExactSpelling = true)]
		private static extern IntPtr GetFocus();

		[DllImport("user32.dll", SetLastError = true)]
		private static extern int GetWindowThreadProcessId(int handle, out int processId);

		[DllImport("user32", ExactSpelling = true, SetLastError = true)]
		private static extern int AttachThreadInput(int idAttach, int idAttachTo, bool fAttach);

		[DllImport("kernel32.dll")]
		private static extern int GetCurrentThreadId();

		[DllImport("User32.dll")]
		private static extern IntPtr WindowFromPoint(Point p);

		[DllImport("User32.dll")]
		private static extern void GetClassName(IntPtr hwnd, StringBuilder sb, int nMaxCount);

		[DllImport("User32.dll")]
		private static extern void GetWindowText(IntPtr handle, StringBuilder text, int MaxLen);

		[DllImport("user32.dll ")]
		private static extern IntPtr GetActiveWindow();

		public Form2()
		{
			DateTime _438F = ((global::_4822E694)_63AF42A1._71AC5D16[506])();
			dtstart = ((global::_22AE913E)_63AF42A1._71AC5D16[26])(ref _438F, -5.0);
			dtstop = ((global::_4822E694)_63AF42A1._71AC5D16[506])();
			上一次翻译的文本 = "";
			isone = true;
			上一次获取的译文 = "";
			components = null;
			((global::_87A40116)_63AF42A1._71AC5D16[72])(this);
			InitializeComponent();
			sentenceHover = new SentenceHoverController((HoverRichTextBox)rtb原文, (HoverRichTextBox)rtb译文);
			ConfigureNoLoginMenus();
            InitializeModernUi();
            InitializeMouseCapture();
			InitializeData();
			HookEvents();
		}

		private unsafe void InitializeData()
		{
			programStartedWait = ThreadPool.RegisterWaitForSingleObject(Program.ProgramStarted, OnProgramStarted, null, Timeout.Infinite, false);
			((global::_76048C0C)_63AF42A1._71AC5D16[702])(this, ControlStyles.UserPaint, _39386B2D: true);
			((global::_76048C0C)_63AF42A1._71AC5D16[702])(this, ControlStyles.AllPaintingInWmPaint, _39386B2D: true);
			((global::_76048C0C)_63AF42A1._71AC5D16[702])(this, ControlStyles.DoubleBuffer, _39386B2D: true);
			Control.CheckForIllegalCrossThreadCalls = true;
			((global::_1E0745A0)_63AF42A1._71AC5D16[23])(521);
			((global::_20B45000)_63AF42A1._71AC5D16[648])();
			((global::_60B9EEB9)_63AF42A1._71AC5D16[361])(toolStrip1, new global::System.Windows.Forms.MouseEventHandler(this.GPanelTitleBack_MouseDown));
			ChatGPT.MyChatGPTEvent += ChatGPT_MyChatGPTEvent;
		}

		private unsafe void LoadData()
		{
			RebuildInfo.Stage("LoadData begins");
			ClassMain.FromMainHandle = ((global::_4DB657B2)_63AF42A1._71AC5D16[172])(this);
			if (!ClassMain.isVersion)
			{
				((global::_329CCB02)_63AF42A1._71AC5D16[296])("请关闭软件再打开，连续重复提示则请安装新版！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				((global::A1366A8E)_63AF42A1._71AC5D16[564])();
				return;
			}
			ClassMain.DeleteFolder();
			RebuildInfo.Stage("Sound folder ready");
			try
			{
				int num = ((global::F896E08C)_63AF42A1._71AC5D16[456])(toolStripDropDownButton1);
				IEnumerator enumerator = ((global::_6BA5AB93)_63AF42A1._71AC5D16[228])(((global::_8B371BAA)_63AF42A1._71AC5D16[271])(toolStrip1));
				try
				{
					while (_9DB6399B._3AAECF2C(enumerator))
					{
						object obj = F8115528.A522B594(enumerator);
						if (((global::D1AAA02F)_63AF42A1._71AC5D16[184])(((global::_6E8D7B14)_63AF42A1._71AC5D16[557])(obj), ((global::_743E8F3F)_63AF42A1._71AC5D16[254])(typeof(ToolStripButton).TypeHandle)))
						{
							((global::_748A38A5)_63AF42A1._71AC5D16[639])((ToolStripButton)obj, E81979B6: false);
							((global::B3886690)_63AF42A1._71AC5D16[560])((ToolStripButton)obj, num);
							((global::_83240E9D)_63AF42A1._71AC5D16[83])((ToolStripButton)obj, num);
						}
						else if (C92C1933._2239CC8E(_0F2F2894.A9A8BD2D(obj), ((global::_743E8F3F)_63AF42A1._71AC5D16[254])(typeof(ToolStripDropDownButton).TypeHandle)))
						{
							((global::_748A38A5)_63AF42A1._71AC5D16[639])((ToolStripDropDownButton)obj, E81979B6: false);
							((global::B3886690)_63AF42A1._71AC5D16[560])((ToolStripDropDownButton)obj, num);
							((global::_83240E9D)_63AF42A1._71AC5D16[83])((ToolStripDropDownButton)obj, num);
						}
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
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
			toolStrip1.ImageScalingSize = new Size(20,20);
			readSetting();
			RebuildInfo.Stage("Settings loaded");
			NoLoginBuild.NormalizeSettings();
            F_Event_SetTranslationSizeType();
            F_Event_SetYuanSizeType();
            F_Event_SetTranslationData();
            F_Event_SetYuanData();
			RebuildInfo.Stage("Fonts complete; engine=" + ClassMain.isjiekounum);
            RebuildEngineMenu();
            SelectManagedEngine(ClassMain.isjiekounum);
			int num2 = 4;
			int num3 = 2;
			IntPtr _0702C = ((global::_1BA5D3BC)_63AF42A1._71AC5D16[171])(rtb原文);
			SendMessage(((global::DCA98F89)_63AF42A1._71AC5D16[777])(ref _0702C), 211, 3, num2 | num3);
			_0702C = ((global::_1BA5D3BC)_63AF42A1._71AC5D16[171])(rtb译文);
			SendMessage(((global::DCA98F89)_63AF42A1._71AC5D16[777])(ref _0702C), 211, 3, num2 | num3);
			((global::_00113993)_63AF42A1._71AC5D16[14])(toolStrip1, D9084A09: true);
            RestoreWindowLayout();
            Text = ProductInfo.Title;
			Text = ProductInfo.Title;
			old_ClipboardData = "";
			AddClipboardFormatListener(ClassMain.FromMainHandle);
			ApplyModernLayout(false);
		}

		private void read_setSetting(string strSetting)
		{
			ClassMain.read_setSetting(strSetting);
            try { if(!LocalModelSettings.MainSettingsPresent)LocalModelSettings.Load(); }
            catch (Exception ex) { RebuildInfo.Stage("Local model settings: " + ex.Message); }
			((global::_9299521D)_63AF42A1._71AC5D16[144])(tbtntrans, ClassMain.Master_switch);
			tntrans(0);
			((global::_9299521D)_63AF42A1._71AC5D16[144])(tbtntop, ClassMain.Topping);
			istop();
            int savedMode; if(!int.TryParse(ClassMain.Translation_Model,out savedMode))savedMode=1;
            SetCaptureMode(savedMode,ClassMain._is自动翻译总开关);
			_formLocation = 1;
		}

		private void readSetting()
		{
			string strSetting = ClassMain.readJiekou();
			read_setSetting(strSetting);
		}

		private void Form2_Load(object sender, EventArgs e)
		{
			try
			{
				getData();
				LoadData();
				_isInitializing = false;
                if(SettingsStore.LoadError!=null)ShowTranslationStatus(SettingsStore.LoadError);
				RebuildInfo.Stage("Form load completed");
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "frmMain-LoadData", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		private void showInTaskbar()
		{
			((global::_98936226)_63AF42A1._71AC5D16[653])(500);
			((global::_2A0C612A)_63AF42A1._71AC5D16[91])(this, FD928DA8: true);
		}

		private void GPanelTitleBack_MouseDown(object sender, MouseEventArgs e)
		{
			ComMain.TCapture(((global::_4DB657B2)_63AF42A1._71AC5D16[172])(this));
		}

		private unsafe void OnProgramStarted(object state, bool timeout)
		{
			try
			{
				if (((global::_541FE098)_63AF42A1._71AC5D16[281])(this))
				{
					_9BB7291A._7E1C8B92(this, new global::System.Threading.WaitOrTimerCallback(this.OnProgramStarted), new object[2] { state, timeout });
				}
				else
				{
					((global::_691D9D2C)_63AF42A1._71AC5D16[547])(this, new global::System.Windows.Forms.MethodInvoker(this._003COnProgramStarted_003Eb__65_0));
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "frmMain-OnProgramStarted", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		[DllImport("user32")]
		private static extern IntPtr SetClipboardViewer(IntPtr hwnd);

		[DllImport("user32")]
		private static extern IntPtr ChangeClipboardChain(IntPtr hwnd, IntPtr hWndNext);

		[DllImport("user32")]
		private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("User32.dll", EntryPoint = "SendMessage")]
		private static extern int SendMessage2(IntPtr hWnd, uint Msg, int wParam, int lParam);

		private bool boolcharNum(string str)
		{
			try
			{
				int num = 0;
				char[] array = ((global::_573181A9)_63AF42A1._71AC5D16[321])(str);
				foreach (char c in array)
				{
					num = ((c < '一' || c > '龥') ? (num + 1) : (num + 2));
				}
				if (num <= 5000)
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
			return false;
		}

		protected override void WndProc(ref Message m)
		{
            // Let DWM retain its frame/shadow while our controls own the full client area.
            if(m.Msg==0x83 && m.WParam!=IntPtr.Zero && FormBorderStyle==FormBorderStyle.None){m.Result=IntPtr.Zero;return;}
            if(m.Msg==0x31E)ConfigureModernFrame();
			try
			{
				switch (((global::_96163720)_63AF42A1._71AC5D16[706])(ref m))
				{
                case 0x031D: ReadExplicitClipboard(); break;
                case 0x83C0: if(m.WParam.ToInt32()==3)CaptureExplicitSelection(3,m.LParam); break;
				case 786:
				{
					IntPtr _0702C = EF90A50A.E5B7D713(ref m);
					switch (((global::DCA98F89)_63AF42A1._71AC5D16[777])(ref _0702C))
					{
					case 105:
                        CaptureSelectedTranslation();
                        break;
                    case 100:
						if (_56205C25._44B80321(this) == FormWindowState.Minimized)
						{
							((global::_8630FF21)_63AF42A1._71AC5D16[140])(this, FormWindowState.Normal);
						}
						else
						{
							((global::_8630FF21)_63AF42A1._71AC5D16[140])(this, FormWindowState.Minimized);
						}
						break;
					}
					break;
				}
				case 132:
				{
					_30A23B88.A6BB5823(this, ref m);
					Point _32244F = default(Point);
					((global::BA8A722E)_63AF42A1._71AC5D16[151])(ref _32244F, (short)(((global::_958D2B04)_63AF42A1._71AC5D16[136])(((global::_9E98132A)_63AF42A1._71AC5D16[492])(ref m)) & 0xFFFF), (short)((((global::_958D2B04)_63AF42A1._71AC5D16[136])(((global::_9E98132A)_63AF42A1._71AC5D16[492])(ref m)) >> 16) & 0xFFFF));
					_32244F = ((global::_761E0C11)_63AF42A1._71AC5D16[789])(this, _32244F);
					Size _2AAD;
					if (((global::F82C7088)_63AF42A1._71AC5D16[235])(ref _32244F) <= 5)
					{
						if (((global::_40146A1C)_63AF42A1._71AC5D16[349])(ref _32244F) <= 5)
						{
							((global::C2857D0D)_63AF42A1._71AC5D16[442])(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(13));
							break;
						}
						int num = _11A3C00C._9E359A85(ref _32244F);
						_2AAD = ((global::C511068F)_63AF42A1._71AC5D16[521])(this);
						if (num >= ((global::_42067127)_63AF42A1._71AC5D16[408])(ref _2AAD) - 5)
						{
							((global::C2857D0D)_63AF42A1._71AC5D16[442])(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(16));
						}
						else
						{
							ECA5EC1E.D9B60D89(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(10));
						}
						break;
					}
					int num2 = _1BADC0B8._1E81FBA3(ref _32244F);
					_2AAD = ((global::C511068F)_63AF42A1._71AC5D16[521])(this);
					if (num2 >= ((global::_34221E0A)_63AF42A1._71AC5D16[795])(ref _2AAD) - 5)
					{
						if (((global::_40146A1C)_63AF42A1._71AC5D16[349])(ref _32244F) <= 5)
						{
							((global::C2857D0D)_63AF42A1._71AC5D16[442])(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(14));
							break;
						}
						int num3 = _11A3C00C._9E359A85(ref _32244F);
						_2AAD = ((global::C511068F)_63AF42A1._71AC5D16[521])(this);
						if (num3 >= ((global::_42067127)_63AF42A1._71AC5D16[408])(ref _2AAD) - 5)
						{
							((global::C2857D0D)_63AF42A1._71AC5D16[442])(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(17));
						}
						else
						{
							ECA5EC1E.D9B60D89(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(11));
						}
					}
					else if (_11A3C00C._9E359A85(ref _32244F) <= 5)
					{
						((global::C2857D0D)_63AF42A1._71AC5D16[442])(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(12));
					}
					else
					{
						int num4 = _11A3C00C._9E359A85(ref _32244F);
						_2AAD = ((global::C511068F)_63AF42A1._71AC5D16[521])(this);
						if (num4 >= ((global::_42067127)_63AF42A1._71AC5D16[408])(ref _2AAD) - 5)
						{
							((global::C2857D0D)_63AF42A1._71AC5D16[442])(ref m, ((global::_811739A3)_63AF42A1._71AC5D16[2])(15));
						}
					}
					break;
				}
				case 513:
					_2F2BDB8D.E1AF2006(ref m, 161);
					((global::D62D1E26)_63AF42A1._71AC5D16[539])(ref m, ((global::_4481691B)_63AF42A1._71AC5D16[561])());
					((global::_9F246D37)_63AF42A1._71AC5D16[630])(ref m, ((global::F286DA0F)_63AF42A1._71AC5D16[214])(2));
					((global::_45A9B73E)_63AF42A1._71AC5D16[527])(this, ref m);
					break;
				default:
					_30A23B88.A6BB5823(this, ref m);
					break;
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}



		private void tbtntrans_Click(object sender, EventArgs e)
		{
			tntrans(1);
		}

		private void tntrans(int starting) {
            if(starting==0){ClassMain._is自动翻译总开关=Convert.ToString(tbtntrans.Tag)!="1";tbtntrans.Tag=ClassMain._is自动翻译总开关?"1":"0";return;}
            SetCaptureMode(_翻译模式,!ClassMain._is自动翻译总开关);
        }

        private void tbtntop_Click(object sender, EventArgs e)
		{
			istop();
		}

		private void istop()
		{
			if (((global::_249B95AA)_63AF42A1._71AC5D16[316])(((global::DB39BFBA)_63AF42A1._71AC5D16[736])(tbtntop), "1"))
			{
				((global::_4A384EBF)_63AF42A1._71AC5D16[455])(tbtntop, Resources.top_off);
				((global::_9299521D)_63AF42A1._71AC5D16[144])(tbtntop, "0");
				((global::AC8B0B34)_63AF42A1._71AC5D16[143])(this);
				((global::F38CAA03)_63AF42A1._71AC5D16[603])(this, _5837E520: false);
			}
			else
			{
				((global::_4A384EBF)_63AF42A1._71AC5D16[455])(tbtntop, Resources.top_on);
				((global::_9299521D)_63AF42A1._71AC5D16[144])(tbtntop, "1");
				((global::AC8B0B34)_63AF42A1._71AC5D16[143])(this);
				((global::_7B18C311)_63AF42A1._71AC5D16[477])(this);
				((global::F38CAA03)_63AF42A1._71AC5D16[603])(this, _5837E520: true);
			}
		}

		private void setIMG(ToolStripMenuItem btn)
        {
            foreach(ToolStripItem item in toolStripDropDownButton1.DropDownItems) {
                var menu = item as ToolStripMenuItem;
                if(menu != null) {menu.Image = null;menu.Checked = false;}
            }
            btn.Image = Resources.对号;
        }

        private void tbtnmodeselect_Click(object sender, EventArgs e) { SetCaptureMode(1,true); }

		private void qidong1()
		{
			guanbi2();
			guanbi3();
			TaskBarUtil.RefreshNotification();
			if (ClassMain._is自动翻译总开关)
			{
				clearHook();
				HookEvents();
				((global::B13D54AA)_63AF42A1._71AC5D16[751])(timer1);
			}
		}

		private void qidong2()
		{
            timer1.Stop();clearHook();guanbi1();guanbi3();
            ClassSetHotKey.SetTranslateEnabled(ClassMain._is自动翻译总开关);
            RefreshModernStatus();
        }

        private void qidong3()
        {
            timer1.Stop();clearHook();guanbi1();guanbi2();
            if(!ClassMain._is自动翻译总开关)return;
            try { Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(typeof(Form2).Assembly.Location),"XtransNoLogin-doubleR-mode.exe"),Handle.ToInt64().ToString()) {UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden})?.Dispose(); }
            catch(Exception) { ClassSetHotKey.LastError="鼠标组合取词未能启动，请检查辅助程序是否完整。";RefreshModernStatus(); }
        }

		private void guanbi1()
		{
			ClassMain.KillProcess("XtransNoLogin-select-mode");
		}

		private void guanbi2()
		{
			ClassSetHotKey.SetTranslateEnabled(false);
            ClassMain.KillProcess("XtransNoLogin-F1-mode");
		}

		private void guanbi3()
		{
			ClassMain.KillProcess("XtransNoLogin-doubleR-mode");
		}

		private void tbtnmodectrl_Click(object sender, EventArgs e) { SetCaptureMode(2,true); }

		private void tbtnmodecopy_Click(object sender, EventArgs e) { SetCaptureMode(3,true); }

        private string ClipboardGetText() { try { return Clipboard.ContainsText() ? Clipboard.GetText() : ""; } catch (ExternalException) { return ""; } }

		private void ClipboardSetDataObject(string res)
		{
			((global::_98936226)_63AF42A1._71AC5D16[653])(50);
			try
			{
				((global::A01198B2)_63AF42A1._71AC5D16[194])(res, DA051F8F: true);
			}
			catch (Exception ex)
			{
				((global::_12A3E807)_63AF42A1._71AC5D16[695])();
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				ClipboardSetDataObject(res);
			}
		}

		private void xianshi(string Clipboardstr)
		{
			try
			{
                if (LocalModelClient.IsLocalEngine(ClassMain.isjiekounum) || (CloudTranslationClient.IsEngine(ClassMain.isjiekounum) || CustomTranslationClient.IsEngine(ClassMain.isjiekounum))) { xianshiyuanwen(Clipboardstr ?? ""); return; }
				string bCB88C0A = _8C345DB5._6A239107(Clipboardstr ?? "");
				bCB88C0A = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, ".\r\n", "@zhw@");
				bCB88C0A = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "-\r\n", "");
				bCB88C0A = ((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "\r\n", " "));
				bCB88C0A = ((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::E3051AA0)_63AF42A1._71AC5D16[794])(((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "\n", " "), "\r", " "));
				bCB88C0A = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "\u00a0", " ");
				bCB88C0A = ((global::E83366AE)_63AF42A1._71AC5D16[201])(bCB88C0A);
				bCB88C0A = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "@zhw@", ".\r\n");
				bCB88C0A = ((global::E3051AA0)_63AF42A1._71AC5D16[794])(bCB88C0A, "\u00ad ", "");
				xianshiyuanwen(((global::E83366AE)_63AF42A1._71AC5D16[201])(bCB88C0A));
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

        private void xianshiyuanwen(string str)
        {
            if (ClassMain._is追加文本) { str=上一次翻译的文本+" "+str; zengliang(); }

            str=(str ?? "").Replace("\u00a0"," ").Replace("\n ","\n");
            string captured=str;
            Action apply=()=>{ rtb原文.Text=captured; F_Event_SetYuanData(); };
            if(InvokeRequired) BeginInvoke(apply); else apply();
        }

		private void RestoreWindowLayout()
		{ ApplyModernLayout(true); }









        private void getOldStr() { _windowOldStr = ClipboardGetText(); }

		private void toolStrip1_MouseEnter(object sender, EventArgs e)
		{
		}

		private void Form2_FormClosed(object sender, FormClosedEventArgs e)
		{
            CaptureModernGeometry();
			Interlocked.Increment(ref localRevision);
			if (sentenceHover != null) sentenceHover.Dispose();
			jiluset();
			guanbi1();
			guanbi2();
			guanbi3();
			TaskBarUtil.RefreshNotification();
			clearHook();
		}

		private void jiluset()
		{
			try
			{
				ClassMain.Master_switch = ((global::DB39BFBA)_63AF42A1._71AC5D16[736])(tbtntrans)?.ToString() ?? "";
				ClassMain.Topping = ((global::DB39BFBA)_63AF42A1._71AC5D16[736])(tbtntop)?.ToString() ?? "";
				ClassMain.splitterNum = ((global::F1AA1A14)_63AF42A1._71AC5D16[81])(splitContainer1);
				ClassMain.Translation_Model = ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref _翻译模式) ?? "";
				ClassMain.formLocation = ((global::_898B04B1)_63AF42A1._71AC5D16[352])(ref _formLocation) ?? "";
				ClassMain.jiluset();
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		private unsafe void rtb原文_TextChanged(object sender, EventArgs e)
		{
			if (_isInitializing) return;
			string recoveredLocal = ((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb原文);
            if(!NoLoginBuild.IsAllowedEngine(ClassMain.isjiekounum)){Interlocked.Increment(ref localRevision);ShowTranslationStatus(EngineRegistry.LoadError ?? "请先在引擎菜单选择一个翻译引擎；可在设置中新增或恢复。");return;}
            if (LocalModelClient.IsLocalEngine(ClassMain.isjiekounum)) {
                QueueLocalTranslation(recoveredLocal,ClassMain.isjiekounum); return;
            }
            if (CustomTranslationClient.IsEngine(ClassMain.isjiekounum)) { QueueCustomTranslation(recoveredLocal,ClassMain.isjiekounum); return; }
            if (CloudTranslationClient.IsEngine(ClassMain.isjiekounum)) {
                QueueCloudTranslation(recoveredLocal,ClassMain.isjiekounum); return;
            }
            int revision=Interlocked.Increment(ref localRevision);
            translationScheduler.Cancel();
            if (recoveredLocal.Length > 12000) { localCancelButton.Enabled=false; ShowTranslationStatus("原文过长，请分段翻译，每次不超过12000字符。"); return; }
            localCancelButton.Enabled=!string.IsNullOrWhiteSpace(recoveredLocal);
            if(string.IsNullOrWhiteSpace(recoveredLocal)){YiStr("");return;}
            var request=new LegacyTranslationRequest{Text=recoveredLocal,Mode=TranslationDirection.Mode,Engine=ClassMain.isjiekounum,Revision=revision};
            ShowTranslationStatus("正在翻译…");
            translationScheduler.Schedule(() => 翻译(request));
        }

        private void 翻译(object str)
        {
            var request=str as LegacyTranslationRequest;
            if(request==null)return;
            translationSource=request.Text;translationEngine=request.Engine;translationRevision=request.Revision;translationMode=request.Mode;
            Func<bool> current=()=>request.Revision==Volatile.Read(ref localRevision)&&ClassMain.isjiekounum==request.Engine&&!IsDisposed;
            try {
                if(!current())return;
                int engine=translationEngine;
                if(!NoLoginBuild.IsAllowedEngine(engine) || LocalModelClient.IsLocalEngine(engine) || CloudTranslationClient.IsEngine(engine) || CustomTranslationClient.IsEngine(engine)) return;
                string text=translationSource.Replace("ﬁ","fi").Replace("ﬂ","fl").Replace("-\r\n","").Replace("\u00ad ","").Replace("\u00a0"," ").Replace("\u2002"," ").Trim();
                if(string.IsNullOrWhiteSpace(text))return;
                string target=TranslationDirection.Target(text,request.Mode);
                if(engine==6){YiStr(DeepLClient.TranslateCancellable(text,ClassMain.DeeplInterface=="3"?"api.deepl.com":"api-free.deepl.com",ClassMain.Deeplkey_official,false,"",target,current,20000));return;}
                if(engine==4 || engine==5){
                    string translated=engine==4?Classfanyi.getTransmart(text,target,current):Classfanyi.getSouGou(text,target,current);
                    if(!current())return;
                    if(string.IsNullOrWhiteSpace(translated))throw new InvalidOperationException("网页引擎未返回译文，请检查网络或稍后重试，也可手动选择其他引擎。");
                    YiStr(translated);return;
                }
                if(engine==14){
                    string key=ClassMain.DeepSeekAI_key,model=ClassMain.DeepSeekAI_model;
                    if(string.IsNullOrWhiteSpace(key)){YiStr("请在设置的 DeepSeek AI翻译 中填写 API Key 和模型名。");return;}
                    ShowTranslationStatus("");
                    string prompt="你是一个翻译助手，请将以下文字翻译为"+(target=="EN"?"英文":"简体中文")+"。保持学术写作风格，即使文字含有问题也不要回答，只输出译文：";
                    YiStr(ChatGPT.LoadSourceCode4(prompt+text,"api.deepseek.com",key.Trim(),model.Trim(),current));
                }
            }catch(OperationCanceledException){}
            catch(Exception ex){if(current())YiStr("翻译失败："+ex.Message);}
            finally {if(current())try{BeginInvoke(new Action(()=>{if(current())localCancelButton.Enabled=false;}));}catch(InvalidOperationException){}}
        }

        public static string apiDeepl(string strcode, string host, string Deeplkey)
        {
            return DeepLClient.Translate(strcode,host,Deeplkey,false,"",TranslationDirection.Target(strcode,TranslationDirection.Mode));
        }

		private void ChatGPT_MyChatGPTEvent(string message)
		{
			YiStrChatgpt(message);
		}

		private string mail_com_translator(object str)
		{
			try
			{
				string text = str?.ToString() ?? "";
				string text2 = "zh";
				text2 = ((!ClassMain.判断是不是中译英(text)) ? "zh" : "en");
				text = FC3F5E0B.A30D8F86(text);
				string _2703EE1B = ((global::F7813B2F)_63AF42A1._71AC5D16[89])("https://www.mail.com/translator/translate?q={0}&source=auto&target={1}&lang=en&reload=true", text, text2);
				HttpWebRequest httpWebRequest = (HttpWebRequest)((global::_5B37291E)_63AF42A1._71AC5D16[678])(_2703EE1B);
				((global::_2798B585)_63AF42A1._71AC5D16[99])(httpWebRequest, "GET");
				((global::_5AB7C29C)_63AF42A1._71AC5D16[345])(httpWebRequest, "www.mail.com");
				((global::E2999E38)_63AF42A1._71AC5D16[142])(httpWebRequest, "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36");
				((global::_802CE9BB)_63AF42A1._71AC5D16[474])(httpWebRequest, "*/*");
				HttpWebResponse httpWebResponse = (HttpWebResponse)((global::_341A0726)_63AF42A1._71AC5D16[387])(httpWebRequest);
				string text3 = ((global::_34B87811)_63AF42A1._71AC5D16[86])(((global::B1B0B29D)_63AF42A1._71AC5D16[571])(httpWebResponse), "Set-Cookie");
				string _709E5D9E;
				using (Stream _43A = ((global::C1981588)_63AF42A1._71AC5D16[272])(httpWebResponse))
				{
					using (StreamReader _3890D8BF = ((global::_69A30324)_63AF42A1._71AC5D16[41])(_43A, ((global::A238FCBC)_63AF42A1._71AC5D16[619])()))
					{
						_709E5D9E = ((global::ECBAE137)_63AF42A1._71AC5D16[624])(_3890D8BF);
					}
				}
				JObject _08A7893D = _29263820.F126C729(_709E5D9E);
				return ((global::_48A6CDA4)_63AF42A1._71AC5D16[264])(_08A7893D, "Translation")?.ToString() ?? "";
			}
			catch (Exception)
			{
				((global::A0050104)_63AF42A1._71AC5D16[122])();
			}
			return "";
		}

		private void YiStr(string fanyistr)
		{
            if(IsDisposed || Disposing || !IsHandleCreated)return;
			if (InvokeRequired) {
                string source = translationSource; int engine = translationEngine; int revision=translationRevision;
                try { BeginInvoke(new Action(() => { if (!IsDisposed && !Disposing && (source == null || (revision==Volatile.Read(ref localRevision) && rtb原文.Text == source && ClassMain.isjiekounum == engine))) YiStr(fanyistr); })); } catch(InvalidOperationException){} return;
            }
			try
			{
				
				((global::B23A488F)_63AF42A1._71AC5D16[183])(rtb译文, fanyistr);
				F_Event_SetTranslationData();
				if (((global::_20A4ABBF)_63AF42A1._71AC5D16[22])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb译文), "【美式发音】"))
				{
					ComMain.SetFontColor(rtb译文);
					((global::_0381CB0E)_63AF42A1._71AC5D16[592])(rtb译文);
					((global::_3F83D38A)_63AF42A1._71AC5D16[327])(rtb译文, 0, 0);
					((global::_8C22D737)_63AF42A1._71AC5D16[284])(rtb译文);
				}
                sentenceHover.SetResultReady(!string.IsNullOrWhiteSpace(fanyistr) &&
                    !fanyistr.StartsWith("翻译失败：") && !fanyistr.StartsWith("请在更多设置") &&
                    !fanyistr.Contains("【美式发音】"));
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "frmMain-YiStr", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		private void YiStrChatgpt(string fanyistr)
		{
            if(IsDisposed || Disposing || !IsHandleCreated)return;
			if (InvokeRequired) {
                string source = translationSource; int engine = translationEngine; int revision=translationRevision;
                try { BeginInvoke(new Action(() => { if (!IsDisposed && !Disposing && (source == null || (revision==Volatile.Read(ref localRevision) && rtb原文.Text == source && ClassMain.isjiekounum == engine))) YiStrChatgpt(fanyistr); })); } catch(InvalidOperationException){} return;
            }
			try
			{
				sentenceHover.Reset();
				
				((global::_68B7F236)_63AF42A1._71AC5D16[589])(rtb译文, fanyistr);
				F_Event_SetTranslationData();
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "frmMain-YiStrChatgpt", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		private void rtb译文_MouseEnter(object sender, EventArgs e) { }

		private void rtb译文_MouseLeave(object sender, EventArgs e) { }

		private void rtb原文_MouseEnter(object sender, EventArgs e) { }

		private void rtb原文_MouseLeave(object sender, EventArgs e) { }

		private void 复制译文ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb译文))))
			{
				ClipboardSetDataObject(((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb译文)));
				((global::_98936226)_63AF42A1._71AC5D16[653])(200);
			}
		}

		private void 复制原文ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (!((global::EFADF098)_63AF42A1._71AC5D16[723])(((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb原文))))
			{
				ClipboardSetDataObject(((global::E83366AE)_63AF42A1._71AC5D16[201])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb原文)));
				((global::_98936226)_63AF42A1._71AC5D16[653])(200);
			}
		}

        private async void toolStripButton4_Click(object sender, EventArgs e)
        {
            if(ocrProcess!=null)return;toolStripButton4.Enabled=false;
            Process process=null;
            try {
                process=Process.Start(new ProcessStartInfo(Path.Combine(Path.GetDirectoryName(typeof(Form2).Assembly.Location),"wxocr.exe")){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden});
                ocrProcess=process;await System.Threading.Tasks.Task.Run(()=>process.WaitForExit());
            } catch(Exception) { if(!IsDisposed&&!Disposing)ShowTranslationStatus("截图识别未能完成，请检查OCR程序与组件后重试。"); }
            finally { ocrProcess=null;process?.Dispose();if(!IsDisposed&&!Disposing)toolStripButton4.Enabled=true; }
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {
            ClassMain._is最小化=WindowState == FormWindowState.Minimized;
            if(ClassMain._is最小化) return;
            shangyicifuzhi="";
            if(isone) {isone=false;return;}
            if(WindowState==FormWindowState.Normal) {ClassMain.右宽=Width;ClassMain.右高=Height;}
        }

		private void toolStripButton1_Click(object sender, EventArgs e)
		{
			Interlocked.Increment(ref localRevision);localCancelButton.Enabled=false;
            Frmsetting frmsetting = new Frmsetting();
			frmsetting.Event_SetTranslationData += F_Event_SetTranslationData;
			frmsetting.Event_SetTranslationSizeType += F_Event_SetTranslationSizeType;
			frmsetting.Event_SetYuanData += F_Event_SetYuanData;
			frmsetting.Event_SetYuanSizeType += F_Event_SetYuanSizeType;
            frmsetting.TopMost = TopMost;
            using (frmsetting) frmsetting.ShowDialog(this);
            RebuildEngineMenu();
            if(!NoLoginBuild.IsAllowedEngine(ClassMain.isjiekounum))ShowTranslationStatus("请在引擎菜单选择一个翻译引擎。");
            RefreshModernToolbar();
            jiluset();
		}

		private void F_Event_SetYuanSizeType()
		{
			try
			{
				((global::EA9AC1BD)_63AF42A1._71AC5D16[486])(rtb原文, ((global::_6504E5B9)_63AF42A1._71AC5D16[720])(ClassMain.originalFontType, ClassMain.originalFontSize, FontStyle.Regular));
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "frmMain-F_Event_SetYuanSizeType", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		private void F_Event_SetTranslationSizeType()
		{
			try
			{
				((global::EA9AC1BD)_63AF42A1._71AC5D16[486])(rtb译文, ((global::_6504E5B9)_63AF42A1._71AC5D16[720])(ClassMain.translationFontType, ClassMain.translationFontSize, FontStyle.Regular));
				if (((global::_20A4ABBF)_63AF42A1._71AC5D16[22])(((global::_4185CE09)_63AF42A1._71AC5D16[661])(rtb译文), "发音】"))
				{
					ComMain.SetFontColor(rtb译文);
				}
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
				Log.WriteTextLog(LogType.主进程, "frmMain-F_Event_SetTranslationSizeType", ((global::_79A2BF2C)_63AF42A1._71AC5D16[18])(ex));
			}
		}

		private void F_Event_SetYuanData()
		{
			try
			{
				ClassMain.SetLineSpace(rtb原文, ClassMain.originalLineSpacing);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		private void F_Event_SetTranslationData()
		{
			try
			{
				ClassMain.SetLineSpace(rtb译文, ClassMain.translationLineSpacing);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		private void toolStripButton2_Click(object sender, EventArgs e)
		{
			zengliang();
		}

		private void zengliang()
		{
			if (((global::_249B95AA)_63AF42A1._71AC5D16[316])(((global::DB39BFBA)_63AF42A1._71AC5D16[736])(toolStripButton2), "1"))
			{
				((global::_4A384EBF)_63AF42A1._71AC5D16[455])(toolStripButton2, Resources.add_off);
				((global::_9299521D)_63AF42A1._71AC5D16[144])(toolStripButton2, "0");
				ClassMain._is追加文本 = false;
			}
			else
			{
				((global::_4A384EBF)_63AF42A1._71AC5D16[455])(toolStripButton2, Resources.add_on);
				((global::_9299521D)_63AF42A1._71AC5D16[144])(toolStripButton2, "1");
				ClassMain._is追加文本 = true;
			}
		}

		private void rtb译文_LinkClicked(object sender, LinkClickedEventArgs e)
		{
			ComMain.playMp3(Classfanyi.path_美);
		}

		private void palMIn_MouseEnter(object sender, EventArgs e)
		{
		}

		private void palMIn_MouseLeave(object sender, EventArgs e)
		{
		}

		private void palClose_MouseEnter(object sender, EventArgs e)
		{
		}

		private void palClose_MouseLeave(object sender, EventArgs e)
		{
		}

		private void palMIn_MouseClick(object sender, MouseEventArgs e)
		{
			((global::_8630FF21)_63AF42A1._71AC5D16[140])(this, FormWindowState.Minimized);
		}

		private void palClose_MouseClick(object sender, MouseEventArgs e)
		{
			((global::_5A0013BF)_63AF42A1._71AC5D16[410])(this);
		}

		private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
		{
		}

		private void toolStripButton3_Click(object sender, EventArgs e)
		{
			((global::_5A0013BF)_63AF42A1._71AC5D16[410])(this);
		}

		private void rtb译文_TextChanged(object sender, EventArgs e)
		{
			ScrollToBottom(rtb译文);
		}


        private void ConfigureNoLoginMenus()
        {
            Blind翻译ToolStripMenuItem.Text=EngineLabels.MenuText(4);
            搜狗翻译引擎ToolStripMenuItem.Text=EngineLabels.MenuText(5);
            deepl翻译引擎ToolStripMenuItem.Text=EngineLabels.MenuText(6);
            eDeepseek官方AI翻译ToolStripMenuItem.Text=EngineLabels.MenuText(14);
            localHunyuanMenu=new ToolStripMenuItem(EngineLabels.MenuText(16)){Name="LocalHunyuanEngine"};
            localGemmaMenu=new ToolStripMenuItem(EngineLabels.MenuText(17)){Name="LocalGemmaEngine"};
            localHunyuanMenu.Click+=(s,e)=>SelectLocalEngine(16);localGemmaMenu.Click+=(s,e)=>SelectLocalEngine(17);
            geminiMenu=new ToolStripMenuItem(EngineLabels.MenuText(18)){Name="GeminiEngine"};
            qwenMenu=new ToolStripMenuItem(EngineLabels.MenuText(19)){Name="QwenMtEngine"};
            geminiMenu.Click+=(s,e)=>SelectCloudEngine(18);qwenMenu.Click+=(s,e)=>SelectCloudEngine(19);
            toolStripDropDownButton1.DropDownItems.Add(new ToolStripSeparator());
            toolStripDropDownButton1.DropDownItems.Add(localHunyuanMenu);toolStripDropDownButton1.DropDownItems.Add(localGemmaMenu);
            toolStripDropDownButton1.DropDownItems.Add(new ToolStripSeparator());
            toolStripDropDownButton1.DropDownItems.Add(geminiMenu);toolStripDropDownButton1.DropDownItems.Add(qwenMenu);
            localCancelButton=new ToolStripButton("停"){Name="CancelLocalTranslation",ToolTipText="取消当前翻译请求",AccessibleName="取消翻译",Visible=false,Enabled=false};
            localRetryButton=new ToolStripButton("译"){Name="RetryLocalTranslation",ToolTipText="用当前引擎重新翻译",AccessibleName="重试翻译",Visible=false};
            localCancelButton.Click+=(s,e)=>{Interlocked.Increment(ref localRevision);translationScheduler.Cancel();LlamaRuntime.CancelIdle();localCancelButton.Enabled=false;上一次翻译的文本="";ShowTranslationStatus("翻译已取消，原文已保留。点击重新翻译可重试。");};
            localRetryButton.Click+=(s,e)=>QueueSelectedTranslation();
            toolStrip1.Items.Add(localCancelButton);toolStrip1.Items.Add(localRetryButton);
        }

        private void SelectLocalEngine(int engine) { if(LocalModelClient.IsLocalEngine(engine)) SelectManagedEngine(engine); }

        private void QueueLocalTranslation(string text,int engine)
        {
            int revision = Interlocked.Increment(ref localRevision);
            localCancelButton.Enabled = !string.IsNullOrWhiteSpace(text);
            if (string.IsNullOrWhiteSpace(text)) { YiStr(""); return; }
            上一次翻译的文本 = text;
            var configured=TranslationDirection.Snapshot(EngineRegistry.Get(engine),text);
            if(configured==null){localCancelButton.Enabled=false;return;}
            ShowTranslationStatus("本地模型正在翻译…首次加载会稍慢。");
            translationScheduler.Schedule(() => {
                Func<bool> current = () => revision == Volatile.Read(ref localRevision) && ClassMain.isjiekounum == engine && !IsDisposed;
                if (!current()) return;
                try {
                    var result = LocalModelClient.TranslateConfigured(configured,text,current,
                        message => PublishLocalTranslation(message,revision,engine,false));
                    if (result != null) PublishLocalTranslation(result.Text,revision,engine);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) { PublishLocalTranslation("翻译失败：" + ex.Message,revision,engine); }
            });
        }

        private void PublishLocalTranslation(string text,int revision,int engine,bool completed = true)
        {
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke(new Action(() => {
                if (!IsDisposed && revision == Volatile.Read(ref localRevision) && ClassMain.isjiekounum == engine) {
                    if (completed) { YiStr(text); localCancelButton.Enabled = false; }
                    else ShowTranslationStatus(text);
                }
            })); } catch (InvalidOperationException) { }
        }

        private void ShowTranslationStatus(string text) {
            if (InvokeRequired) { YiStr(text); return; }
            YiStr(text); sentenceHover.Reset();
        }

        private void QueueSelectedTranslation()
        {
            int engine=ClassMain.isjiekounum;
            if(!NoLoginBuild.IsAllowedEngine(engine)){Interlocked.Increment(ref localRevision);localCancelButton.Enabled=false;ShowTranslationStatus("请在译文区上方选择引擎；可在设置中勾选要显示的引擎。");return;}
            if(LocalModelClient.IsLocalEngine(engine)) QueueLocalTranslation(rtb原文.Text,engine);
            else if(CustomTranslationClient.IsEngine(engine)) QueueCustomTranslation(rtb原文.Text,engine);
            else if(CloudTranslationClient.IsEngine(engine)) QueueCloudTranslation(rtb原文.Text,engine);
            else { 上一次翻译的文本=""; rtb原文_TextChanged(rtb原文,EventArgs.Empty); }
        }
        private void SelectCloudEngine(int engine) { if(CloudTranslationClient.IsEngine(engine)) SelectManagedEngine(engine); }
        private void QueueCloudTranslation(string text,int engine)
        {
            int revision=Interlocked.Increment(ref localRevision);
            localCancelButton.Enabled=!string.IsNullOrWhiteSpace(text);
            if(string.IsNullOrWhiteSpace(text)) {YiStr("");return;}
            ShowTranslationStatus(CloudTranslationClient.Name(engine)+" 正在翻译…");
            var configured=TranslationDirection.Snapshot(EngineRegistry.Get(engine),text);
            translationScheduler.Schedule(()=>{
                Func<bool> current=()=>revision==Volatile.Read(ref localRevision)&&ClassMain.isjiekounum==engine&&!IsDisposed;
                if(!current())return;
                try {PublishLocalTranslation(CloudTranslationClient.TranslateConfigured(configured,text,current),revision,engine);}
                catch(OperationCanceledException){}
                catch(Exception ex){PublishLocalTranslation("翻译失败："+ex.Message,revision,engine);}
            });
        }







		private void Blind翻译ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(toolStripDropDownButton1, Resources.engine_4_on);
			translate(sender, 4);
		}

		private void 搜狗翻译引擎ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(toolStripDropDownButton1, Resources.engine_5_on);
			translate(sender, 5);
		}

		private void deepl翻译引擎ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(toolStripDropDownButton1, Resources._6_on);
			translate(sender, 6);
		}





		private unsafe void translate(object sender, int jiekou) { SelectManagedEngine(jiekou); }

        private void eDeepseek官方AI翻译ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			toolStripDropDownButton1.Image = Resources.e;
			translate(sender, 14);
		}

        private void getData()
        {
            if (ocrReceiver != null) return;
            ocrReceiver = new OcrPipeReceiver(text => PostOcrResult(text, false), error => PostOcrResult(error, true));
        }
        private void PostOcrResult(string text, bool error)
        {
            if (IsDisposed || Disposing || !IsHandleCreated) return;
            try { BeginInvoke(new Action(() => { if (IsDisposed || Disposing) return; if (error) ShowTranslationStatus(text); else xianshi(text); })); }
            catch (InvalidOperationException) { }
        }
        private void DisposeRuntime()
        {
            if (runtimeDisposed) return; runtimeDisposed = true;
            Interlocked.Increment(ref localRevision);
            LlamaRuntime.Stop();
            translationScheduler.Dispose(); ocrReceiver?.Dispose(); programStartedWait?.Unregister(null);
            if(ocrProcess!=null)try{if(!ocrProcess.HasExited)ocrProcess.Kill();}catch(Exception){}
            DisposeModernImages();
            ChatGPT.MyChatGPTEvent -= ChatGPT_MyChatGPTEvent;
            DisposeMouseCapture(); clearHook();
            if (IsHandleCreated) RemoveClipboardFormatListener(Handle);
            CancelExplicitCapture();
        }

		[DllImport("user32.dll")]
		private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		public void ScrollToBottom(RichTextBox richTextBox)
		{
			try
			{
				SendMessage(((global::_1BA5D3BC)_63AF42A1._71AC5D16[171])(richTextBox), 277, 7, 0);
			}
			catch (Exception ex)
			{
				((global::_9C345083)_63AF42A1._71AC5D16[8])(ex.ToString());
			}
		}

		protected override void Dispose(bool disposing)
		{
            if (disposing) DisposeRuntime();
			if (disposing && components != null)
			{
				components.Dispose();
			}
			_028405B9._54A2560E(this, disposing);
		}

		private unsafe void InitializeComponent()
		{
			this.components = ((global::_42A0288E)_63AF42A1._71AC5D16[666])();
			System.ComponentModel.ComponentResourceManager resources = ((global::DD1F222D)_63AF42A1._71AC5D16[24])(((global::_743E8F3F)_63AF42A1._71AC5D16[254])(typeof(Xtranslator.Form2).TypeHandle));
			this.splitContainer1 = ((global::_1B3F789B)_63AF42A1._71AC5D16[348])();
			this.dSkinPanel1 = ((global::_673077B6)_63AF42A1._71AC5D16[218])();
			this.rtb译文 = new Xtranslator.HoverRichTextBox();
			this.contextMenuStrip1 = ((global::_493B7632)_63AF42A1._71AC5D16[353])(this.components);
			this.复制译文ToolStripMenuItem = ((global::_308D359E)_63AF42A1._71AC5D16[535])();
			this.复制原文ToolStripMenuItem = ((global::_308D359E)_63AF42A1._71AC5D16[535])();
			this.dSkinPanel2 = ((global::_673077B6)_63AF42A1._71AC5D16[218])();
			this.rtb原文 = new Xtranslator.HoverRichTextBox();
			this.palMain = ((global::B39D98B4)_63AF42A1._71AC5D16[199])();
			this.toolStrip1 = new Xtranslator.ToolStripEx();
			this.tbtntrans = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.tbtntop = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.toolStripButton4 = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.toolStripButton2 = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.toolStripDropDownButton1 = ((global::FC0E3989)_63AF42A1._71AC5D16[112])();
			this.Blind翻译ToolStripMenuItem = ((global::_308D359E)_63AF42A1._71AC5D16[535])();
			this.搜狗翻译引擎ToolStripMenuItem = ((global::_308D359E)_63AF42A1._71AC5D16[535])();
			this.deepl翻译引擎ToolStripMenuItem = ((global::_308D359E)_63AF42A1._71AC5D16[535])();
			this.eDeepseek官方AI翻译ToolStripMenuItem = ((global::_308D359E)_63AF42A1._71AC5D16[535])();
			this.tbtnmodeselect = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.tbtnmodecopy = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.tbtnmodectrl = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.toolStripButton1 = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.toolStripButton3 = ((global::_25851011)_63AF42A1._71AC5D16[162])();
			this.timer1 = ((global::A209D8B8)_63AF42A1._71AC5D16[721])(this.components);
			((global::_361979B7)_63AF42A1._71AC5D16[165])(this.splitContainer1);
			((global::_6B903684)_63AF42A1._71AC5D16[645])(((global::_2A00C91B)_63AF42A1._71AC5D16[153])(this.splitContainer1));
			((global::_6B903684)_63AF42A1._71AC5D16[645])(((global::_5828F3AD)_63AF42A1._71AC5D16[499])(this.splitContainer1));
			((global::_6B903684)_63AF42A1._71AC5D16[645])(this.splitContainer1);
			((global::_6B903684)_63AF42A1._71AC5D16[645])(this.dSkinPanel1);
			((global::_6B903684)_63AF42A1._71AC5D16[645])(this.contextMenuStrip1);
			((global::_6B903684)_63AF42A1._71AC5D16[645])(this.dSkinPanel2);
			((global::_6B903684)_63AF42A1._71AC5D16[645])(this.palMain);
			((global::_6B903684)_63AF42A1._71AC5D16[645])(this.toolStrip1);
			((global::_4409D2A0)_63AF42A1._71AC5D16[646])(this);
			((global::B811DF17)_63AF42A1._71AC5D16[739])(this.splitContainer1, ((global::D61AD715)_63AF42A1._71AC5D16[759])());
			((global::_5B811C0C)_63AF42A1._71AC5D16[31])(this.splitContainer1, System.Windows.Forms.DockStyle.Fill);
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.splitContainer1, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(25, 0));
			((global::_111D7900)_63AF42A1._71AC5D16[556])(this.splitContainer1, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.splitContainer1, "splitContainer1");
			((global::_2EA6F8BC)_63AF42A1._71AC5D16[421])(this.splitContainer1, System.Windows.Forms.Orientation.Horizontal);
			((global::B811DF17)_63AF42A1._71AC5D16[739])(((global::_2A00C91B)_63AF42A1._71AC5D16[153])(this.splitContainer1), ((global::B9101B3D)_63AF42A1._71AC5D16[787])());
			((global::_631E68BD)_63AF42A1._71AC5D16[45])(((global::_2A00C91B)_63AF42A1._71AC5D16[153])(this.splitContainer1)).Add(this.dSkinPanel1);
			((global::_472203A5)_63AF42A1._71AC5D16[529])(this.splitContainer1, 1);
			((global::B811DF17)_63AF42A1._71AC5D16[739])(((global::_5828F3AD)_63AF42A1._71AC5D16[499])(this.splitContainer1), ((global::B9101B3D)_63AF42A1._71AC5D16[787])());
			((global::_631E68BD)_63AF42A1._71AC5D16[45])(((global::_5828F3AD)_63AF42A1._71AC5D16[499])(this.splitContainer1)).Add(this.dSkinPanel2);
			((global::_3592BB96)_63AF42A1._71AC5D16[141])(this.splitContainer1, 1);
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.splitContainer1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(238, 555));
			((global::_9A916F0E)_63AF42A1._71AC5D16[44])(this.splitContainer1, 525);
			((global::_9AB0ADB6)_63AF42A1._71AC5D16[580])(this.splitContainer1, 3);
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.splitContainer1, 1);
			((global::B811DF17)_63AF42A1._71AC5D16[739])(this.dSkinPanel1, ((global::D61AD715)_63AF42A1._71AC5D16[759])());
			((global::_87A069AC)_63AF42A1._71AC5D16[376])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel1), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_74A70105)_63AF42A1._71AC5D16[485])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel1), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_853C5836)_63AF42A1._71AC5D16[385])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel1), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_478468B3)_63AF42A1._71AC5D16[409])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel1), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_51AFA5A5)_63AF42A1._71AC5D16[513])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel1), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_631E68BD)_63AF42A1._71AC5D16[45])(this.dSkinPanel1).Add(this.rtb译文);
			((global::_70147E92)_63AF42A1._71AC5D16[380])(this.dSkinPanel1, System.Windows.Forms.DockStyle.Fill);
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.dSkinPanel1, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(0, 0));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.dSkinPanel1, "dSkinPanel1");
			((global::_5D9DC604)_63AF42A1._71AC5D16[364])(this.dSkinPanel1, ((global::_3608A386)_63AF42A1._71AC5D16[114])(1));
			((global::FBB70A24)_63AF42A1._71AC5D16[669])(this.dSkinPanel1, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "dSkinPanel1.RightBottom"));
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.dSkinPanel1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(238, 525));
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.dSkinPanel1, 0);
			((global::B23A488F)_63AF42A1._71AC5D16[183])(this.dSkinPanel1, "dSkinPanel1");
			((global::B811DF17)_63AF42A1._71AC5D16[739])(this.rtb译文, ((global::B9101B3D)_63AF42A1._71AC5D16[787])());
			((global::_70302A05)_63AF42A1._71AC5D16[607])(this.rtb译文, System.Windows.Forms.BorderStyle.None);
			((global::_142A682C)_63AF42A1._71AC5D16[118])(this.rtb译文, this.contextMenuStrip1);
			((global::_70147E92)_63AF42A1._71AC5D16[380])(this.rtb译文, System.Windows.Forms.DockStyle.Fill);
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.rtb译文, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(1, 1));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.rtb译文, "rtb译文");
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.rtb译文, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(236, 523));
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.rtb译文, 0);
			((global::B23A488F)_63AF42A1._71AC5D16[183])(this.rtb译文, "");
			((global::_471A67B2)_63AF42A1._71AC5D16[468])(this.rtb译文, new global::System.Windows.Forms.LinkClickedEventHandler(this.rtb译文_LinkClicked));
			((global::A426CF14)_63AF42A1._71AC5D16[277])(this.rtb译文, new global::System.EventHandler(this.rtb译文_TextChanged));
			((global::_8E08590C)_63AF42A1._71AC5D16[748])(this.rtb译文, new global::System.EventHandler(this.rtb译文_MouseEnter));
			((global::_87A50338)_63AF42A1._71AC5D16[664])(this.rtb译文, new global::System.EventHandler(this.rtb译文_MouseLeave));
			((global::A30904A3)_63AF42A1._71AC5D16[587])(this.contextMenuStrip1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 24));
			D337FCBD._278B1EA2(((global::_8B371BAA)_63AF42A1._71AC5D16[271])(this.contextMenuStrip1), new System.Windows.Forms.ToolStripItem[2] { this.复制译文ToolStripMenuItem, this.复制原文ToolStripMenuItem });
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.contextMenuStrip1, "contextMenuStrip1");
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.contextMenuStrip1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(125, 48));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.复制译文ToolStripMenuItem, "复制译文ToolStripMenuItem");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.复制译文ToolStripMenuItem, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(124, 22));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.复制译文ToolStripMenuItem, "复制译文");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.复制译文ToolStripMenuItem, new global::System.EventHandler(this.复制译文ToolStripMenuItem_Click));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.复制原文ToolStripMenuItem, "复制原文ToolStripMenuItem");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.复制原文ToolStripMenuItem, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(124, 22));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.复制原文ToolStripMenuItem, "复制原文");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.复制原文ToolStripMenuItem, new global::System.EventHandler(this.复制原文ToolStripMenuItem_Click));
			((global::B811DF17)_63AF42A1._71AC5D16[739])(this.dSkinPanel2, ((global::D61AD715)_63AF42A1._71AC5D16[759])());
			((global::_87A069AC)_63AF42A1._71AC5D16[376])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel2), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_74A70105)_63AF42A1._71AC5D16[485])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel2), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_853C5836)_63AF42A1._71AC5D16[385])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel2), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_478468B3)_63AF42A1._71AC5D16[409])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel2), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_51AFA5A5)_63AF42A1._71AC5D16[513])(((global::_5AB42384)_63AF42A1._71AC5D16[509])(this.dSkinPanel2), ((global::CE1A2521)_63AF42A1._71AC5D16[772])());
			((global::_631E68BD)_63AF42A1._71AC5D16[45])(this.dSkinPanel2).Add(this.rtb原文);
			((global::_70147E92)_63AF42A1._71AC5D16[380])(this.dSkinPanel2, System.Windows.Forms.DockStyle.Fill);
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.dSkinPanel2, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(0, 0));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.dSkinPanel2, "dSkinPanel2");
			((global::_5D9DC604)_63AF42A1._71AC5D16[364])(this.dSkinPanel2, ((global::_3608A386)_63AF42A1._71AC5D16[114])(1));
			((global::FBB70A24)_63AF42A1._71AC5D16[669])(this.dSkinPanel2, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "dSkinPanel2.RightBottom"));
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.dSkinPanel2, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(238, 27));
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.dSkinPanel2, 0);
			((global::B23A488F)_63AF42A1._71AC5D16[183])(this.dSkinPanel2, "dSkinPanel2");
			((global::B811DF17)_63AF42A1._71AC5D16[739])(this.rtb原文, ((global::B9101B3D)_63AF42A1._71AC5D16[787])());
			((global::_70302A05)_63AF42A1._71AC5D16[607])(this.rtb原文, System.Windows.Forms.BorderStyle.None);
			((global::_142A682C)_63AF42A1._71AC5D16[118])(this.rtb原文, this.contextMenuStrip1);
			((global::_70147E92)_63AF42A1._71AC5D16[380])(this.rtb原文, System.Windows.Forms.DockStyle.Fill);
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.rtb原文, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(1, 1));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.rtb原文, "rtb原文");
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.rtb原文, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(236, 25));
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.rtb原文, 0);
			((global::B23A488F)_63AF42A1._71AC5D16[183])(this.rtb原文, "");
			((global::A426CF14)_63AF42A1._71AC5D16[277])(this.rtb原文, new global::System.EventHandler(this.rtb原文_TextChanged));
			((global::_8E08590C)_63AF42A1._71AC5D16[748])(this.rtb原文, new global::System.EventHandler(this.rtb原文_MouseEnter));
			((global::_87A50338)_63AF42A1._71AC5D16[664])(this.rtb原文, new global::System.EventHandler(this.rtb原文_MouseLeave));
			((global::_631E68BD)_63AF42A1._71AC5D16[45])(this.palMain).Add(this.splitContainer1);
			((global::_631E68BD)_63AF42A1._71AC5D16[45])(this.palMain).Add(this.toolStrip1);
			((global::_70147E92)_63AF42A1._71AC5D16[380])(this.palMain, System.Windows.Forms.DockStyle.Fill);
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.palMain, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(0, 0));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.palMain, "palMain");
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.palMain, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(263, 555));
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.palMain, 2);
			((global::_00113993)_63AF42A1._71AC5D16[14])(this.toolStrip1, false);
			((global::_0E856009)_63AF42A1._71AC5D16[670])(this.toolStrip1, ((global::D61AD715)_63AF42A1._71AC5D16[759])());
			((global::_70147E92)_63AF42A1._71AC5D16[380])(this.toolStrip1, System.Windows.Forms.DockStyle.Left);
			((global::_3BB82EBA)_63AF42A1._71AC5D16[170])(this.toolStrip1, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::F7B38E83)_63AF42A1._71AC5D16[373])(this.toolStrip1, System.Windows.Forms.ToolStripGripStyle.Hidden);
			((global::A30904A3)_63AF42A1._71AC5D16[587])(this.toolStrip1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(26, 21));
            this.toolStrip1.Items.AddRange(new ToolStripItem[] {this.tbtntrans,this.tbtntop,this.toolStripButton4,this.toolStripButton2,this.toolStripDropDownButton1,this.tbtnmodeselect,this.tbtnmodecopy,this.tbtnmodectrl,this.toolStripButton1,this.toolStripButton3});
			((global::F937B4BD)_63AF42A1._71AC5D16[234])(this.toolStrip1, ((global::_5606D72D)_63AF42A1._71AC5D16[150])(0, 0));
			((global::_18830825)_63AF42A1._71AC5D16[655])(this.toolStrip1, "toolStrip1");
			((global::_5D9DC604)_63AF42A1._71AC5D16[364])(this.toolStrip1, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::_1D8218A3)_63AF42A1._71AC5D16[338])(this.toolStrip1, System.Windows.Forms.ToolStripRenderMode.System);
			((global::_4B0F752A)_63AF42A1._71AC5D16[68])(this.toolStrip1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(25, 555));
			((global::FF83831D)_63AF42A1._71AC5D16[47])(this.toolStrip1, 0);
			((global::B23A488F)_63AF42A1._71AC5D16[183])(this.toolStrip1, "toolStrip1");
			((global::_8E08590C)_63AF42A1._71AC5D16[748])(this.toolStrip1, new global::System.EventHandler(this.toolStrip1_MouseEnter));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.tbtntrans, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.tbtntrans, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.tbtntrans, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "tbtntrans.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.tbtntrans, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.tbtntrans, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.tbtntrans, "tbtntrans");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.tbtntrans, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_9299521D)_63AF42A1._71AC5D16[144])(this.tbtntrans, "1");
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.tbtntrans, "总开关");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.tbtntrans, new global::System.EventHandler(this.tbtntrans_Click));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.tbtntop, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.tbtntop, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.tbtntop, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "tbtntop.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.tbtntop, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.tbtntop, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.tbtntop, "tbtntop");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.tbtntop, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_9299521D)_63AF42A1._71AC5D16[144])(this.tbtntop, "1");
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.tbtntop, "窗口置顶");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.tbtntop, new global::System.EventHandler(this.tbtntop_Click));
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.toolStripButton4, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.toolStripButton4, Xtranslator.Properties.Resources.snap);
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.toolStripButton4, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.toolStripButton4, "toolStripButton4");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.toolStripButton4, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.toolStripButton4, "截图翻译");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.toolStripButton4, new global::System.EventHandler(this.toolStripButton4_Click));
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.toolStripButton2, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.toolStripButton2, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "toolStripButton2.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.toolStripButton2, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.toolStripButton2, "toolStripButton2");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.toolStripButton2, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_9299521D)_63AF42A1._71AC5D16[144])(this.toolStripButton2, "0");
			((global::_95802736)_63AF42A1._71AC5D16[185])(this.toolStripButton2, "跨页文字翻译，请官网帮助文档中查看使用方法");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.toolStripButton2, new global::System.EventHandler(this.toolStripButton2_Click));
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.toolStripDropDownButton1, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
            this.toolStripDropDownButton1.DropDownItems.AddRange(new ToolStripItem[] {this.Blind翻译ToolStripMenuItem,this.搜狗翻译引擎ToolStripMenuItem,this.deepl翻译引擎ToolStripMenuItem,this.eDeepseek官方AI翻译ToolStripMenuItem});
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.toolStripDropDownButton1, Xtranslator.Properties.Resources.engine_1_off);
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.toolStripDropDownButton1, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.toolStripDropDownButton1, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.toolStripDropDownButton1, "toolStripDropDownButton1");
			((global::_0A31E3B3)_63AF42A1._71AC5D16[683])(this.toolStripDropDownButton1, false);
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.toolStripDropDownButton1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.toolStripDropDownButton1, "选择引擎");
			((global::_95802736)_63AF42A1._71AC5D16[185])(this.toolStripDropDownButton1, "选择引擎");
			((global::_2C1099B0)_63AF42A1._71AC5D16[307])(this.Blind翻译ToolStripMenuItem, System.Windows.Forms.ToolStripItemImageScaling.None);
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.Blind翻译ToolStripMenuItem, "Blind翻译ToolStripMenuItem");
			((global::EBB6B716)_63AF42A1._71AC5D16[660])(this.Blind翻译ToolStripMenuItem, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.Blind翻译ToolStripMenuItem, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(252, 20));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.Blind翻译ToolStripMenuItem, "4：Blind翻译(免key)");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.Blind翻译ToolStripMenuItem, new global::System.EventHandler(this.Blind翻译ToolStripMenuItem_Click));
			((global::_2C1099B0)_63AF42A1._71AC5D16[307])(this.搜狗翻译引擎ToolStripMenuItem, System.Windows.Forms.ToolStripItemImageScaling.None);
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.搜狗翻译引擎ToolStripMenuItem, "搜狗翻译引擎ToolStripMenuItem");
			((global::EBB6B716)_63AF42A1._71AC5D16[660])(this.搜狗翻译引擎ToolStripMenuItem, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.搜狗翻译引擎ToolStripMenuItem, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(252, 20));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.搜狗翻译引擎ToolStripMenuItem, "5：搜狗翻译(免key)");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.搜狗翻译引擎ToolStripMenuItem, new global::System.EventHandler(this.搜狗翻译引擎ToolStripMenuItem_Click));
			((global::_2C1099B0)_63AF42A1._71AC5D16[307])(this.deepl翻译引擎ToolStripMenuItem, System.Windows.Forms.ToolStripItemImageScaling.None);
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.deepl翻译引擎ToolStripMenuItem, "deepl翻译引擎ToolStripMenuItem");
			((global::EBB6B716)_63AF42A1._71AC5D16[660])(this.deepl翻译引擎ToolStripMenuItem, ((global::_3608A386)_63AF42A1._71AC5D16[114])(0));
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.deepl翻译引擎ToolStripMenuItem, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(252, 20));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.deepl翻译引擎ToolStripMenuItem, "6：Deepl翻译");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.deepl翻译引擎ToolStripMenuItem, new global::System.EventHandler(this.deepl翻译引擎ToolStripMenuItem_Click));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.eDeepseek官方AI翻译ToolStripMenuItem, "eDeepseek官方AI翻译ToolStripMenuItem");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.eDeepseek官方AI翻译ToolStripMenuItem, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(252, 22));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.eDeepseek官方AI翻译ToolStripMenuItem, "e：Deepseek官方AI翻译");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.eDeepseek官方AI翻译ToolStripMenuItem, new global::System.EventHandler(this.eDeepseek官方AI翻译ToolStripMenuItem_Click));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.tbtnmodeselect, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.tbtnmodeselect, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.tbtnmodeselect, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "tbtnmodeselect.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.tbtnmodeselect, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.tbtnmodeselect, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.tbtnmodeselect, "tbtnmodeselect");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.tbtnmodeselect, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_95802736)_63AF42A1._71AC5D16[185])(this.tbtnmodeselect, "划选模式：选中即翻译，不要在编辑文档时使用");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.tbtnmodeselect, new global::System.EventHandler(this.tbtnmodeselect_Click));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.tbtnmodecopy, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.tbtnmodecopy, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.tbtnmodecopy, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "tbtnmodecopy.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.tbtnmodecopy, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.tbtnmodecopy, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.tbtnmodecopy, "tbtnmodecopy");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.tbtnmodecopy, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_95802736)_63AF42A1._71AC5D16[185])(this.tbtnmodecopy, "右键模式：选中段落、鼠标尚未松开时点右键触发翻译");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.tbtnmodecopy, new global::System.EventHandler(this.tbtnmodecopy_Click));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.tbtnmodectrl, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.tbtnmodectrl, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.tbtnmodectrl, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "tbtnmodectrl.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.tbtnmodectrl, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.tbtnmodectrl, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.tbtnmodectrl, "tbtnmodectrl");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.tbtnmodectrl, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_95802736)_63AF42A1._71AC5D16[185])(this.tbtnmodectrl, "键盘模式：选中文字后按自定义快捷键触发翻译");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.tbtnmodectrl, new global::System.EventHandler(this.tbtnmodectrl_Click));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.toolStripButton1, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.toolStripButton1, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.toolStripButton1, (System.Drawing.Image)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "toolStripButton1.Image"));
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.toolStripButton1, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.toolStripButton1, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.toolStripButton1, "toolStripButton1");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.toolStripButton1, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.toolStripButton1, "更多设置");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.toolStripButton1, new global::System.EventHandler(this.toolStripButton1_Click));
			((global::_0B313598)_63AF42A1._71AC5D16[770])(this.toolStripButton3, System.Windows.Forms.ImageLayout.Center);
			((global::_5D38C319)_63AF42A1._71AC5D16[586])(this.toolStripButton3, System.Windows.Forms.ToolStripItemDisplayStyle.Image);
			((global::_4A384EBF)_63AF42A1._71AC5D16[455])(this.toolStripButton3, Xtranslator.Properties.Resources.close1);
			((global::A89057B8)_63AF42A1._71AC5D16[102])(this.toolStripButton3, ((global::_03A9423A)_63AF42A1._71AC5D16[765])());
			((global::AFB66F88)_63AF42A1._71AC5D16[238])(this.toolStripButton3, ((global::_97BA772E)_63AF42A1._71AC5D16[598])(0, 1, 0, 0));
			((global::_9E1E3DA2)_63AF42A1._71AC5D16[505])(this.toolStripButton3, "toolStripButton3");
			((global::_812B46B8)_63AF42A1._71AC5D16[511])(this.toolStripButton3, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(24, 25));
			((global::_4DB6F621)_63AF42A1._71AC5D16[377])(this.toolStripButton3, "退出程序");
			((global::DA97DEA2)_63AF42A1._71AC5D16[206])(this.toolStripButton3, new global::System.EventHandler(this.toolStripButton3_Click));
			((global::_2581E0AC)_63AF42A1._71AC5D16[34])(this.timer1, 5000);
			((global::F090D0B0)_63AF42A1._71AC5D16[420])(this.timer1, new global::System.EventHandler(this.timer1_Tick));
			((global::F9307D14)_63AF42A1._71AC5D16[197])(this, ((global::_589CB328)_63AF42A1._71AC5D16[156])(6f, 12f));
			((global::_26AE419E)_63AF42A1._71AC5D16[187])(this, System.Windows.Forms.AutoScaleMode.Font);
			((global::B811DF17)_63AF42A1._71AC5D16[739])(this, ((global::B20F7630)_63AF42A1._71AC5D16[778])());
			((global::B203CF9F)_63AF42A1._71AC5D16[691])(this, ((global::CB1D2E28)_63AF42A1._71AC5D16[637])(263, 555));
			((global::BD0F1513)_63AF42A1._71AC5D16[46])(this).Add(this.palMain);
			((global::_492B5F9E)_63AF42A1._71AC5D16[334])(this, true);
			((global::_98B7DB97)_63AF42A1._71AC5D16[498])(this, (System.Drawing.Icon)((global::_7C073EB4)_63AF42A1._71AC5D16[741])(resources, "$this.Icon"));
			((global::DD1CEC13)_63AF42A1._71AC5D16[656])(this, "Form2");
			((global::_85A86B06)_63AF42A1._71AC5D16[343])(this, System.Windows.Forms.FormStartPosition.CenterScreen);
			((global::B23A488F)_63AF42A1._71AC5D16[183])(this, "1");
			((global::F38CAA03)_63AF42A1._71AC5D16[603])(this, true);
			((global::_5C1AF218)_63AF42A1._71AC5D16[769])(this, new global::System.Windows.Forms.FormClosedEventHandler(this.Form2_FormClosed));
			((global::_7A953A37)_63AF42A1._71AC5D16[135])(this, new global::System.EventHandler(this.Form2_Load));
			((global::_63A9AFA5)_63AF42A1._71AC5D16[418])(this, new global::System.EventHandler(this.Form2_SizeChanged));
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(((global::_2A00C91B)_63AF42A1._71AC5D16[153])(this.splitContainer1), false);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(((global::_5828F3AD)_63AF42A1._71AC5D16[499])(this.splitContainer1), false);
			((global::_7528181E)_63AF42A1._71AC5D16[245])(this.splitContainer1);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(this.splitContainer1, false);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(this.dSkinPanel1, false);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(this.contextMenuStrip1, false);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(this.dSkinPanel2, false);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(this.palMain, false);
			((global::B4B69CA5)_63AF42A1._71AC5D16[435])(this.toolStrip1, false);
			((global::_69B4170F)_63AF42A1._71AC5D16[130])(this.toolStrip1);
			((global::_188B601C)_63AF42A1._71AC5D16[436])(this, false);
		}

		[CompilerGenerated]
		private void _003COnProgramStarted_003Eb__65_0()
		{
			((global::AC8B0B34)_63AF42A1._71AC5D16[143])(this);
			((global::_98936226)_63AF42A1._71AC5D16[653])(50);
			((global::_629D772C)_63AF42A1._71AC5D16[464])(this);
			((global::_98936226)_63AF42A1._71AC5D16[653])(50);
			((global::_7B18C311)_63AF42A1._71AC5D16[477])(this);
			((global::_98936226)_63AF42A1._71AC5D16[653])(50);
			SendMessage2(((global::_4DB657B2)_63AF42A1._71AC5D16[172])(this), 274u, 61728, 0);
		}
	}
}
