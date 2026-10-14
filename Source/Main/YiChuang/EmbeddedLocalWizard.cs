using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YiChuang
{
    internal sealed class LocalSetupWizard : Form
    {
        private readonly LocalRuntimeOptions options;
        private readonly ComboBox backend=new ComboBox(),acceleration=new ComboBox();
        private readonly TextBox directory=new TextBox(),hyFile=new TextBox(),gemmaFile=new TextBox(),status=new TextBox();
        private readonly CheckBox hy=new CheckBox(),gemma=new CheckBox();
        private readonly Button inspect=new Button(),prepare=new Button(),cancel=new Button(),done=new Button();
        private readonly FlowLayoutPanel body=new FlowLayoutPanel(),llamaPanel=new FlowLayoutPanel(),ollamaPanel=new FlowLayoutPanel();
        private readonly TabControl pages=new TabControl();
        private readonly TabPage myModels=new TabPage("我的本地模型"),presets=new TabPage("推荐模型与运行设置");
        private readonly EngineManagerControl manager;
        private CancellationTokenSource operation;private bool suspended;private readonly float scale;
        private int Px(int x){return (int)Math.Round(x*scale);}
        internal LocalSetupWizard()
        {
            options=LocalRuntime.Load();using(var g=Graphics.FromHwnd(IntPtr.Zero))scale=g.DpiX/96f;
            Text="译窗 · 本地模型配置";Font=new Font("Microsoft YaHei UI",10);BackColor=Color.White;ForeColor=UiTheme.Ink;AutoScaleMode=AutoScaleMode.None;StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;Icon=Icon.ExtractAssociatedIcon(typeof(LocalSetupWizard).Assembly.Location);
            var area=Screen.FromControl(this).WorkingArea;ClientSize=new Size(Math.Min(Px(800),area.Width-Px(28)),Math.Min(Px(720),area.Height-Px(65)));MinimumSize=new Size(Math.Min(Px(640),Width),Math.Min(Px(540),Height));
            body.Dock=DockStyle.Fill;body.Padding=new Padding(Px(24));body.AutoScroll=true;body.FlowDirection=FlowDirection.TopDown;body.WrapContents=false;
            body.Controls.Add(Label("准备推荐模型",15));
            body.Controls.Add(Label("腾讯 / Google 是两款预设。其他模型请到‘我的本地模型’添加；无需另外安装模型软件。"));
            backend.Name="LocalBackend";backend.DropDownStyle=ComboBoxStyle.DropDownList;backend.Items.AddRange(new object[]{"内置 llama.cpp（推荐）","Ollama（使用独立服务）"});backend.Width=Px(680);backend.SelectedIndex=LocalModelSettings.Backend=="ollama"?1:0;body.Controls.Add(Label("运行方式（应用于内置腾讯 / Google 模型）"));body.Controls.Add(backend);
            foreach(var panel in new[]{llamaPanel,ollamaPanel}){panel.FlowDirection=FlowDirection.TopDown;panel.WrapContents=false;panel.AutoSize=true;panel.Width=Px(710);panel.Margin=new Padding(0,Px(8),0,0);body.Controls.Add(panel);}
            directory.Name="GgufDirectory";directory.Text=GgufModels.DirectoryFor(options);hyFile.Name="HyGguf";hyFile.Text=GgufModels.Resolve(options,true);gemmaFile.Name="GemmaGguf";gemmaFile.Text=GgufModels.Resolve(options,false);
            AddPath("新下载 / 兼容副本的保存目录",directory,true);AddPath("腾讯 GGUF 文件（留空可准备或下载）",hyFile,false);AddPath("Google GGUF 文件（留空可准备或下载）",gemmaFile,false);
            acceleration.Name="LocalAcceleration";acceleration.DropDownStyle=ComboBoxStyle.DropDownList;acceleration.Width=Px(680);acceleration.Items.AddRange(new object[]{"自动显卡加速（Vulkan；不需 CUDA 工具包）","仅 CPU（速度较慢）"});acceleration.SelectedIndex=options.Acceleration=="cpu"?1:0;llamaPanel.Controls.Add(Label("运行设备"));llamaPanel.Controls.Add(acceleration);
            hy.Name="PrepareHy";hy.Text="腾讯 Hy-MT2-7B · 下载约 6.2 GB";gemma.Name="PrepareGemma";gemma.Text="Google TranslateGemma-12B · 下载约 8.1 GB";
            foreach(var box in new[]{hy,gemma}){box.AutoSize=true;box.Margin=new Padding(0,Px(8),0,0);llamaPanel.Controls.Add(box);}
            llamaPanel.Controls.Add(Label("只准备勾选项，已有模型优先复用。旧版 Google 权重会生成约 7.4 GB 兼容副本，原文件保留。\n切换模型时释放上一款；取消、退出时关闭本次运行进程。"));
            var links=new FlowLayoutPanel{AutoSize=true};foreach(var pair in new[]{new[]{"腾讯模型许可","https://huggingface.co/tencent/HY-MT2-7B"},new[]{"Google 模型许可","https://ai.google.dev/gemma/terms"}}){string url=pair[1];var link=new LinkLabel{Text=pair[0],AutoSize=true,Margin=new Padding(0,Px(5),Px(18),0)};link.LinkClicked+=(s,e)=>System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url){UseShellExecute=true});links.Controls.Add(link);}llamaPanel.Controls.Add(links);
            ollamaPanel.Controls.Add(Label("保留原有 Ollama 模型名称和服务地址。使用前可检查实际模型列表、配置路径或准备模型。"));
            var configure=new Button{Name="ConfigureOllama",Text="配置 Ollama 服务与模型",AutoSize=true,Height=Px(36)};configure.Click+=(s,e)=>{using(var wizard=new OllamaSetupWizard())wizard.ShowDialog(this);var saved=LocalRuntime.Load();options.Executable=saved.Executable;options.ModelsDirectory=saved.ModelsDirectory;status.Text="Ollama 配置窗口已关闭。点击‘保存并关闭’启用此运行方式。";};ollamaPanel.Controls.Add(configure);
            var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=Px(44),WrapContents=false};Style(inspect,"检查本地文件");Style(prepare,"准备选中的模型");Style(cancel,"取消操作");Style(done,"保存并关闭");cancel.Enabled=false;actions.Controls.AddRange(new Control[]{inspect,prepare,cancel,done});
            status.Name="LocalSetupStatus";status.Multiline=true;status.ReadOnly=true;status.ScrollBars=ScrollBars.Vertical;status.BackColor=UiTheme.Background;status.Dock=DockStyle.Top;status.Height=Px(70);status.Text="添加、编辑、显示勾选与排序统一在这里管理；保存并关闭后生效。移除条目不删除磁盘模型。自定义模型各自选择运行方式。";
            pages.Name="LocalModelPages";pages.Dock=DockStyle.Fill;pages.Padding=new Point(Px(14),Px(8));pages.TabPages.AddRange(new[]{myModels,presets});
            foreach(var page in new[]{myModels,presets}){page.BackColor=Color.White;page.UseVisualStyleBackColor=false;}
            myModels.AutoScroll=true;myModels.Padding=new Padding(Px(24));presets.Controls.Add(body);
            manager=new EngineManagerControl(id=>{pages.SelectedTab=presets;status.Text="这是一款预设模型，在此调整其文件或运行方式。自定义模型可在‘我的本地模型’中直接编辑。";},true);
            manager.Scale(new SizeF(scale,scale));manager.Location=new Point(Px(24),Px(24));myModels.Controls.Add(manager);
            var footer=new Panel{Dock=DockStyle.Bottom,Height=Px(146),Padding=new Padding(Px(20),Px(8),Px(20),Px(8)),BackColor=UiTheme.Background};footer.Controls.Add(status);footer.Controls.Add(actions);Controls.Add(pages);Controls.Add(footer);
            pages.SelectedIndexChanged+=(s,e)=>{manager.RefreshList(ClassMain.isjiekounum);UpdateBackend();};backend.SelectedIndexChanged+=(s,e)=>UpdateBackend();UpdateBackend();
            body.SizeChanged+=(s,e)=>{int width=Math.Max(Px(460),body.ClientSize.Width-body.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-Px(8));backend.Width=width;foreach(Control p in body.Controls){if(p is Label)p.MaximumSize=new Size(width,0);}foreach(var panel in new[]{llamaPanel,ollamaPanel}){panel.MaximumSize=new Size(width,0);panel.Width=width;foreach(Control c in panel.Controls){if(c is Label)c.MaximumSize=new Size(width,0);else if(c is Panel||c is ComboBox)c.Width=width;}}};
            inspect.Click+=async(s,e)=>{string h=hyFile.Text,g=gemmaFile.Text;await Run((token,progress)=>Task.Run(()=>{LlamaRuntime.CheckInstalled();token.ThrowIfCancellationRequested();progress("内置 llama.cpp 组件已找到。腾讯："+Describe(h)+"；Google："+Describe(g)+"。翻译时自动启动，不需要另开服务。");}));};
            prepare.Click+=async(s,e)=>await Run(async(token,progress)=>{
                bool wantHy=hy.Checked,wantGemma=gemma.Checked;if(!wantHy&&!wantGemma)throw new InvalidOperationException("请先勾选至少一款要准备的模型。");ReadOptions();LlamaRuntime.CheckInstalled();
                if(wantHy){string path=await Task.Run(()=>GgufModels.Prepare(options,true,progress,token));options.HyGguf=path;hyFile.Text=path;}
                if(wantGemma){string path=await Task.Run(()=>GgufModels.Prepare(options,false,progress,token));options.GemmaGguf=path;gemmaFile.Text=path;}
                token.ThrowIfCancellationRequested();Save();if(wantHy){EngineRegistry.Restore(16);EngineRegistry.SetVisible(16,true);}if(wantGemma){EngineRegistry.Restore(17);EngineRegistry.SetVisible(17,true);}ClassMain.isjiekounum=wantHy?16:17;ClassMain.jiluset();if(ClassMain.SettingsSaveError!=null)throw new IOException(ClassMain.SettingsSaveError);
                manager.RefreshList(ClassMain.isjiekounum);progress("模型已就绪。可到‘我的本地模型’管理显示与排序；保存并关闭后，在译文区上方选择模型。");
            });
            cancel.Click+=(s,e)=>{operation?.Cancel();status.Text="正在取消…已下载部分和原模型会保留。";};done.Click+=(s,e)=>{try{Save();Close();}catch(Exception ex){status.Text="未保存："+ex.Message;}};
            FormClosing+=(s,e)=>{if(operation!=null){operation.Cancel();e.Cancel=true;status.Text="正在停止当前操作，请稍后再关闭。";}};
            Shown+=(s,e)=>{if(!ClassSetHotKey.Suspended){ClassSetHotKey.Suspend();suspended=true;}};FormClosed+=(s,e)=>Resume();
        }
        private string Describe(string path){if(string.IsNullOrWhiteSpace(path))return "尚未选择";GgufModels.ValidateFile(path);return "已找到 GGUF";}
        private Label Label(string text,int size=10){return new Label{Text=text,AutoSize=true,MaximumSize=new Size(Px(710),0),Font=new Font("Microsoft YaHei UI",size,size>10?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(0,Px(5),0,Px(7))};}
        private void Style(Button b,string text){b.Name=text=="保存并关闭"?"SaveLocalSetup":text=="准备选中的模型"?"PrepareLocalModels":text=="取消操作"?"CancelLocalSetup":"InspectLocalModels";b.Text=text;b.AutoSize=true;b.MinimumSize=new Size(Px(116),Px(34));b.FlatStyle=FlatStyle.Flat;b.ForeColor=UiTheme.Accent;b.Margin=new Padding(0,0,Px(10),0);}
        private void AddPath(string label,TextBox box,bool folder)
        {
            llamaPanel.Controls.Add(Label(label));var row=new Panel{Width=Px(710),Height=Px(34),Margin=Padding.Empty};box.Dock=DockStyle.Fill;var browse=new Button{Text="选择…",Width=Px(78),Dock=DockStyle.Right};row.Controls.Add(box);row.Controls.Add(browse);llamaPanel.Controls.Add(row);
            browse.Click+=(s,e)=>{if(folder){using(var dialog=new FolderBrowserDialog{SelectedPath=box.Text,Description="选择模型目录"})if(dialog.ShowDialog(this)==DialogResult.OK)box.Text=dialog.SelectedPath;}else{using(var dialog=new OpenFileDialog{Filter="GGUF 模型|*.gguf|全部文件|*.*",CheckFileExists=true})if(dialog.ShowDialog(this)==DialogResult.OK)box.Text=dialog.FileName;}};
        }
        private void UpdateBackend(){bool local=backend.SelectedIndex==0;llamaPanel.Visible=local;ollamaPanel.Visible=!local;inspect.Visible=prepare.Visible=local&&pages.SelectedTab==presets;}
        private void ReadOptions(){options.GgufDirectory=Path.GetFullPath(directory.Text.Trim());options.HyGguf=hyFile.Text.Trim();options.GemmaGguf=gemmaFile.Text.Trim();options.Acceleration=acceleration.SelectedIndex==1?"cpu":"auto";}
        private void Save(){ReadOptions();options.WizardCompleted=true;LocalRuntime.Save(options);LocalModelSettings.Backend=backend.SelectedIndex==1?"ollama":"llamacpp";ClassMain.jiluset();if(ClassMain.SettingsSaveError!=null)throw new IOException(ClassMain.SettingsSaveError);}
        private async Task Run(Func<CancellationToken,Action<string>,Task> work)
        {
            if(operation!=null)return;operation=new CancellationTokenSource();pages.Enabled=body.Enabled=inspect.Enabled=prepare.Enabled=done.Enabled=false;cancel.Enabled=true;var p=new Progress<string>(v=>{if(!IsDisposed)status.Text=v;});
            try{await work(operation.Token,v=>((IProgress<string>)p).Report(v));}catch(OperationCanceledException){status.Text="已取消。已下载部分、已有模型和原文件均保留；稍后可重试。";}catch(Exception ex){status.Text=ex.Message;}
            finally{operation.Dispose();operation=null;if(!IsDisposed){pages.Enabled=body.Enabled=inspect.Enabled=prepare.Enabled=done.Enabled=true;cancel.Enabled=false;}}
        }
        private void Resume(){if(suspended){suspended=false;ClassSetHotKey.Resume();}}
        protected override void OnLoad(EventArgs e){TopMost=Owner!=null&&Owner.TopMost;base.OnLoad(e);}
        protected override void Dispose(bool disposing){if(disposing){operation?.Cancel();Resume();Icon?.Dispose();}base.Dispose(disposing);}
    }
}
