using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YiChuang
{
    internal sealed class OllamaSetupWizard : Form
    {
        private readonly LocalRuntimeOptions options;
        private readonly TextBox executable=new TextBox(),modelsDirectory=new TextBox(),endpoint=new TextBox(),status=new TextBox();
        private readonly Button detect=new Button(),install=new Button(),cancel=new Button(),done=new Button(),browseExe=new Button(),browseModels=new Button();
        private CancellationTokenSource operation;
        private bool serviceReady,ownsHotkeySuspension;
        private readonly Font titleFont;
        private readonly float dpiScale;
        private int Px(int value){return (int)Math.Round(value*dpiScale);}
        internal OllamaSetupWizard()
        {
            options=LocalRuntime.Load();Text="译窗 · 本地模型配置";StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;
            using(var graphics=Graphics.FromHwnd(IntPtr.Zero))dpiScale=graphics.DpiX/96f;
            Font=new Font("Microsoft YaHei UI",10);titleFont=new Font(Font.FontFamily,17,FontStyle.Bold);BackColor=Color.White;ForeColor=UiTheme.Ink;AutoScaleMode=AutoScaleMode.None;
            var area=Screen.FromControl(this).WorkingArea;
            ClientSize=new Size(Math.Min(Px(740),area.Width-Px(30)),Math.Min(Px(720),area.Height-Px(65)));MinimumSize=new Size(Math.Min(Px(600),Width),Math.Min(Px(540),Height));Icon=Icon.ExtractAssociatedIcon(typeof(Form2).Assembly.Location);
            var body=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(Px(24))};
            body.Controls.Add(new Label{Text="先翻译，再按需添加本地模型",AutoSize=true,Font=titleFont,Margin=new Padding(0,0,0,Px(12))});
            body.Controls.Add(new Label{Text="网页翻译可直接使用；在线 AI 需要你自己的密钥。本地翻译是可选项。\n内置中英文离线 OCR，无微信时也能截图取字。",AutoSize=true,MaximumSize=new Size(Px(680),0),Margin=new Padding(0,0,0,Px(12))});
            AddField(body,"Ollama 程序（自动查找，也可选择已安装的 ollama.exe）",executable,browseExe);
            AddField(body,"模型存放目录（仅用于译窗新启动的服务）",modelsDirectory,browseModels);
            AddField(body,"本地服务地址",endpoint,null);
            executable.Text=LocalRuntime.FindExecutable(options.Executable);modelsDirectory.Text=string.IsNullOrWhiteSpace(options.ModelsDirectory)?LocalRuntime.DefaultModelsDirectory():options.ModelsDirectory;endpoint.Text=LocalModelSettings.Endpoint;
            browseExe.Text="选择程序";browseExe.Click+=(s,e)=>{using(var dialog=new OpenFileDialog{Filter="Ollama 程序|ollama.exe",CheckFileExists=true})if(dialog.ShowDialog(this)==DialogResult.OK)executable.Text=dialog.FileName;};
            browseModels.Text="选择目录";browseModels.Click+=(s,e)=>{using(var dialog=new FolderBrowserDialog{Description="选择模型存放目录",SelectedPath=modelsDirectory.Text})if(dialog.ShowDialog(this)==DialogResult.OK)modelsDirectory.Text=dialog.SelectedPath;};
            var serviceButtons=new FlowLayoutPanel{AutoSize=true,Margin=new Padding(0,Px(4),0,Px(12))};
            ButtonStyle(detect,"检查并启动服务");ButtonStyle(install,"下载并安装 Ollama");serviceButtons.Controls.Add(detect);serviceButtons.Controls.Add(install);
            var website=new LinkLabel{Text="Ollama 官网",AutoSize=true,Margin=new Padding(Px(10),Px(10),0,0)};website.LinkClicked+=(s,e)=>System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://ollama.com/download/windows"){UseShellExecute=true});serviceButtons.Controls.Add(website);body.Controls.Add(serviceButtons);
            body.Controls.Add(new Label{Text="服务连接后，在‘我的本地模型’添加模型并读取已安装列表。运行设备由 Ollama 管理。",AutoSize=true,MaximumSize=new Size(Px(680),0)});
            var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=Px(42),WrapContents=false};ButtonStyle(cancel,"取消当前操作");ButtonStyle(done,"保存并关闭");cancel.Enabled=false;actions.Controls.Add(cancel);actions.Controls.Add(done);
            status.Multiline=true;status.ReadOnly=true;status.ScrollBars=ScrollBars.Vertical;status.Dock=DockStyle.Top;status.Height=Px(76);status.BorderStyle=BorderStyle.FixedSingle;status.BackColor=UiTheme.Background;status.Text="可先跳过；以后在设置 → 翻译引擎中打开“本地模型配置向导”。";
            var footer=new Panel{Dock=DockStyle.Bottom,Height=Px(144),Padding=new Padding(Px(24),Px(10),Px(24),Px(10)),BackColor=UiTheme.Background};footer.Controls.Add(status);footer.Controls.Add(actions);Controls.Add(body);Controls.Add(footer);
            body.SizeChanged+=(s,e)=>{
                int width=Math.Max(Px(350),body.ClientSize.Width-body.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-Px(4));
                foreach(Control item in body.Controls){if(item is Label||item is CheckBox)item.MaximumSize=new Size(width,0);else if(item is Panel)item.Width=width;}
            };
            detect.Click+=async(s,e)=>await Run(async(token,progress)=>{
                string address=endpoint.Text;LocalModelClient.ValidateEndpoint(address);options.Executable=executable.Text.Trim();options.ModelsDirectory=modelsDirectory.Text.Trim();
                bool started=await Task.Run(()=>LocalRuntime.EnsureService(options,address,progress,token));
                string[] names=await Task.Run(()=>LocalRuntime.Models(address,token));serviceReady=true;executable.Text=LocalRuntime.FindExecutable(options.Executable);
                modelsDirectory.ReadOnly=!started;browseModels.Enabled=started;
                progress("已连接，实际发现 "+names.Length+" 个本地模型。\r\n"+string.Join("、",names)+(started?"":"\r\n沿用现有服务的模型目录，不结束其他程序。"));
            });
            install.Click+=async(s,e)=>await Run((token,progress)=>Task.Run(()=>LocalRuntime.InstallOllama(progress,token)));
            cancel.Click+=(s,e)=>{operation?.Cancel();status.Text="正在取消…已下载数据由 Ollama 保留，稍后可重试。";};
            done.Click+=(s,e)=>{try{LocalModelClient.ValidateEndpoint(endpoint.Text.Trim());options.Executable=executable.Text.Trim();options.ModelsDirectory=modelsDirectory.Text.Trim();LocalRuntime.Save(options);Close();}catch(Exception ex){status.Text="设置未保存："+ex.Message;}};
            endpoint.TextChanged+=(s,e)=>serviceReady=false;
            FormClosing+=(s,e)=>{if(operation!=null){operation.Cancel();e.Cancel=true;status.Text="正在停止当前操作，请稍后再关闭。";}};
            Shown+=(s,e)=>{if(!ClassSetHotKey.Suspended){ClassSetHotKey.Suspend();ownsHotkeySuspension=true;}};
            FormClosed+=(s,e)=>ResumeHotkeys();
        }
        private void AddField(FlowLayoutPanel body,string label,TextBox box,Button browse)
        {
            body.Controls.Add(new Label{Text=label,AutoSize=true,Margin=new Padding(0,Px(5),0,Px(4))});
            var row=new Panel{Width=Px(670),Height=Px(36),Margin=Padding.Empty};box.Dock=DockStyle.Fill;row.Controls.Add(box);
            if(browse!=null){browse.Dock=DockStyle.Right;browse.Width=Px(106);var spacer=new Panel{Dock=DockStyle.Right,Width=Px(8)};row.Controls.Add(spacer);row.Controls.Add(browse);}body.Controls.Add(row);
        }
        private void ButtonStyle(Button b,string text){b.Text=text;b.AutoSize=true;b.MinimumSize=new Size(Px(130),Px(34));b.FlatStyle=FlatStyle.Flat;b.ForeColor=UiTheme.Accent;b.Margin=new Padding(0,0,Px(10),0);}
        private Task Run(Func<CancellationToken,Action<string>,Task> work)
        {
            // Establish the UI dispatcher before entering the async state machine.
            // Embedders/tests may invoke a button without an installed UI context.
            if(!(SynchronizationContext.Current is WindowsFormsSynchronizationContext))
                SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
            return RunOnUiThread(work);
        }
        private async Task RunOnUiThread(Func<CancellationToken,Action<string>,Task> work)
        {
            if(operation!=null)return;operation=new CancellationTokenSource();
            foreach(var b in new[]{detect,install,done,browseExe,browseModels})b.Enabled=false;executable.Enabled=modelsDirectory.Enabled=endpoint.Enabled=false;cancel.Enabled=true;
            Action<string> progress=value=>{
                if(IsDisposed||Disposing||!IsHandleCreated)return;
                try { BeginInvoke((Action)(()=>{if(!IsDisposed&&!Disposing)status.Text=value;})); }
                catch(InvalidOperationException) { /* The window closed while dispatching. */ }
            };
            try{await work(operation.Token,progress);}catch(OperationCanceledException){status.Text="已取消。已下载模型不会被删除；再次准备可复用已有数据。";}catch(Exception ex){status.Text=ex.Message;}
            finally {operation.Dispose();operation=null;if(!IsDisposed){foreach(var b in new[]{detect,install,done,browseExe,browseModels})b.Enabled=true;executable.Enabled=modelsDirectory.Enabled=endpoint.Enabled=true;browseModels.Enabled=!modelsDirectory.ReadOnly;cancel.Enabled=false;}}
        }
        private void ResumeHotkeys(){if(ownsHotkeySuspension){ownsHotkeySuspension=false;ClassSetHotKey.Resume();}}
        protected override void OnLoad(EventArgs e){TopMost=Owner!=null&&Owner.TopMost;base.OnLoad(e);}
        protected override void Dispose(bool disposing){if(disposing){operation?.Cancel();ResumeHotkeys();titleFont.Dispose();Icon?.Dispose();}base.Dispose(disposing);}
    }
}
