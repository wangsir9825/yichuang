using System;
using System.Threading;

namespace YiChuang
{
	internal static class Program
	{
		public static EventWaitHandle ProgramStarted;

		private static string lib;

		private static string browser;

		private static string locales;

		private static string res;

		private static void cefInitialize()
		{
			string text = ((global::_59951DAD)_63AF42A1._71AC5D16[673])(((global::D8B105B1)_63AF42A1._71AC5D16[318])(Environment.SpecialFolder.ApplicationData), "\\YiChuang");
		}

		[STAThread]
		private static void Main(string[] args)
		{
			RebuildInfo.Initialize();
			if (args.Length == 2 && args[0] == "--startup-check")
			{
				RebuildInfo.DiagnosticOutput = System.IO.Path.GetFullPath(args[1]);
				System.IO.Directory.CreateDirectory(RebuildInfo.DiagnosticOutput);
			}
			bool _13282AA = default(bool);
			ProgramStarted = ((global::_45A0FDA7)_63AF42A1._71AC5D16[15])(_7401F88C: false, EventResetMode.AutoReset, "YiChuang.SingleInstance", ref _13282AA);
			if (!_13282AA)
			{
				((global::D227FABF)_63AF42A1._71AC5D16[300])(ProgramStarted);
				return;
			}
			((global::_9C0D35A9)_63AF42A1._71AC5D16[761])();
			((global::FA181004)_63AF42A1._71AC5D16[537])(A72B0D1D: false);
			try { SettingsStore.ImportPreviousSettings(); } catch(Exception ex) { RebuildInfo.Log(ex); }
			Form2 form = new Form2();
			if(RebuildInfo.DiagnosticOutput==null)form.Shown+=async(s,e)=>{
                if(Array.IndexOf(args,"--setup")>=0){using(var wizard=new LocalSetupWizard())wizard.ShowDialog(form);form.RefreshAfterSetup();}
                else if(EngineRegistry.Get(ClassMain.isjiekounum)?.Provider=="ollama") {
                    using(var cancel=new CancellationTokenSource()) {
                        System.Windows.Forms.FormClosedEventHandler closed=(sender,eventArgs)=>cancel.Cancel();form.FormClosed+=closed;
                        string address=EngineRegistry.Get(ClassMain.isjiekounum)?.Endpoint??LocalModelSettings.Endpoint;
                        try{await System.Threading.Tasks.Task.Run(()=>LocalRuntime.EnsureService(LocalRuntime.Load(),address,null,cancel.Token));}
                        catch(OperationCanceledException){}
                        catch(Exception ex){if(!form.IsDisposed)form.ShowRuntimeNotice(ex.Message+" 可在设置 → 翻译引擎打开本地模型配置向导。");}
                        finally{form.FormClosed-=closed;}
                    }
                }
            };
			if (RebuildInfo.DiagnosticOutput != null) RebuildInfo.PrepareStartupCheck(form);
            try { ((global::BD190319)_63AF42A1._71AC5D16[730])(form); }
            finally { LlamaRuntime.Stop(); }
		}
	}
}
