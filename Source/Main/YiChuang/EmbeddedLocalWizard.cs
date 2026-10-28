using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace YiChuang
{
    // Opened on demand. Models are user supplied; no hardware-specific presets.
    internal sealed class LocalSetupWizard : Form
    {
        private readonly ComboBox acceleration=new ComboBox();
        private readonly TextBox status=new TextBox();
        private readonly Button done=new Button();
        private readonly TabControl pages=new TabControl();
        private readonly EngineManagerControl manager;
        private readonly string originalRegistry,originalAcceleration;
        private readonly int originalSelection;
        private bool suspended,saved;
        private readonly float scale;
        internal Func<DialogResult> ChangesPrompt;
        private int Px(int x){return (int)Math.Round(x*scale);}
        internal LocalSetupWizard()
        {
            var options=LocalRuntime.Load();originalAcceleration=options.Acceleration;
            originalRegistry=EngineRegistry.Export();originalSelection=ClassMain.isjiekounum;
            using(var g=Graphics.FromHwnd(IntPtr.Zero))scale=g.DpiX/96f;
            Text="译窗 · 本地模型配置";Font=new Font("Microsoft YaHei UI",10);BackColor=Color.White;ForeColor=UiTheme.Ink;
            AutoScaleMode=AutoScaleMode.None;StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;
            Icon=Icon.ExtractAssociatedIcon(typeof(LocalSetupWizard).Assembly.Location);
            var area=Screen.FromControl(this).WorkingArea;
            ClientSize=new Size(Math.Min(Px(800),area.Width-Px(28)),Math.Min(Px(650),area.Height-Px(65)));
            MinimumSize=new Size(Math.Min(Px(640),Width),Math.Min(Px(500),Height));
            pages.Name="LocalModelPages";pages.Dock=DockStyle.Fill;pages.Padding=new Point(Px(14),Px(8));
            var models=new TabPage("我的本地模型"){BackColor=Color.White,AutoScroll=true};
            var runtime=new TabPage("运行设置"){BackColor=Color.White};pages.TabPages.AddRange(new[]{models,runtime});
            manager=new EngineManagerControl(null,true);manager.Scale(new SizeF(scale,scale));manager.Location=new Point(Px(24),Px(24));models.Controls.Add(manager);
            var body=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(Px(24))};runtime.Controls.Add(body);
            body.Controls.Add(Label("内置 llama.cpp 运行设备",15));
            body.Controls.Add(Label("默认仅 CPU；有兼容显卡时，可自行开启自动显卡加速。此选项应用于你添加的 GGUF 模型。"));
            acceleration.Name="LocalAcceleration";acceleration.DropDownStyle=ComboBoxStyle.DropDownList;acceleration.Width=Px(600);
            acceleration.Items.AddRange(new object[]{"自动显卡加速（Vulkan）","仅 CPU"});acceleration.SelectedIndex=options.Acceleration=="auto"?0:1;body.Controls.Add(acceleration);
            body.Controls.Add(Label("模型仍需足够的内存；请根据电脑配置选择模型大小。切换模型时自动释放上一款。Ollama 模型的运行设备由 Ollama 管理。"));
            body.Controls.Add(Label("使用 Ollama（可选）",15));
            var configure=new Button{Name="ConfigureOllama",Text="Ollama 服务设置",AutoSize=true,MinimumSize=new Size(Px(180),Px(36)),FlatStyle=FlatStyle.Flat};
            configure.Click+=(s,e)=>{using(var wizard=new OllamaSetupWizard())wizard.ShowDialog(this);status.Text="添加 Ollama 模型时，在模型编辑窗口填写服务地址并读取已安装模型。";};body.Controls.Add(configure);
            body.SizeChanged+=(s,e)=>{int width=Math.Max(Px(300),body.ClientSize.Width-body.Padding.Horizontal-SystemInformation.VerticalScrollBarWidth-Px(8));acceleration.Width=width;foreach(Control c in body.Controls)if(c is Label)c.MaximumSize=new Size(width,0);};
            var footer=new Panel{Dock=DockStyle.Bottom,Height=Px(130),Padding=new Padding(Px(20),Px(8),Px(20),Px(8)),BackColor=UiTheme.Background};
            status.Name="LocalSetupStatus";status.Multiline=true;status.ReadOnly=true;status.ScrollBars=ScrollBars.Vertical;status.BackColor=UiTheme.Background;status.Dock=DockStyle.Top;status.Height=Px(62);
            status.Text="按需添加本机 GGUF 文件或 Ollama 模型；保存后在主界面选择。移除条目不会删除磁盘模型文件。";
            var actions=new FlowLayoutPanel{Dock=DockStyle.Bottom,Height=Px(42),WrapContents=false};
            done.Name="SaveLocalSetup";done.Text="保存并关闭";done.MinimumSize=new Size(Px(130),Px(36));done.AutoSize=true;done.FlatStyle=FlatStyle.Flat;done.ForeColor=UiTheme.Accent;actions.Controls.Add(done);
            footer.Controls.Add(status);footer.Controls.Add(actions);Controls.Add(pages);Controls.Add(footer);
            done.Click+=(s,e)=>{if(Save())Close();};
            FormClosing+=(s,e)=>{
                if(saved)return;
                bool changed=originalRegistry!=EngineRegistry.Export()||originalAcceleration!=(acceleration.SelectedIndex==0?"auto":"cpu");
                if(!changed)return;
                var answer=ChangesPrompt!=null?ChangesPrompt():MessageBox.Show(this,"本地模型配置已修改，是否保存？","保存配置修改",MessageBoxButtons.YesNoCancel,MessageBoxIcon.Question);
                if(answer==DialogResult.Cancel){e.Cancel=true;return;}
                if(answer==DialogResult.Yes){e.Cancel=!Save();return;}
                EngineRegistry.Import(originalRegistry);ClassMain.isjiekounum=originalSelection;
            };
            Shown+=(s,e)=>{if(!ClassSetHotKey.Suspended){ClassSetHotKey.Suspend();suspended=true;}};FormClosed+=(s,e)=>Resume();
        }
        private Label Label(string text,int size=10){return new Label{Text=text,AutoSize=true,MaximumSize=new Size(Px(700),0),Font=new Font("Microsoft YaHei UI",size,size>10?FontStyle.Bold:FontStyle.Regular),Margin=new Padding(0,Px(5),0,Px(14))};}
        private bool Save()
        {
            // Do not overwrite service fields saved independently by the Ollama dialog.
            var latest=LocalRuntime.Load();
            byte[] previous=null;
            latest.Acceleration=acceleration.SelectedIndex==0?"auto":"cpu";latest.WizardCompleted=true;
            bool runtimeSaved=false;
            try{previous=File.Exists(LocalRuntime.OptionsPath)?File.ReadAllBytes(LocalRuntime.OptionsPath):null;LocalRuntime.Save(latest);runtimeSaved=true;ClassMain.jiluset();if(ClassMain.SettingsSaveError!=null)throw new IOException(ClassMain.SettingsSaveError);saved=true;return true;}
            catch(Exception ex){
                string message=ex.Message;
                if(runtimeSaved)try{
                    if(previous==null)File.Delete(LocalRuntime.OptionsPath);
                    else {
                        string temp=Path.Combine(Path.GetDirectoryName(LocalRuntime.OptionsPath),Path.GetRandomFileName());
                        try{using(var output=new FileStream(temp,FileMode.CreateNew,FileAccess.Write,FileShare.None))output.Write(previous,0,previous.Length);File.Replace(temp,LocalRuntime.OptionsPath,null);}
                        finally{if(File.Exists(temp))File.Delete(temp);}
                    }
                }catch{message+="；运行设备设置未能恢复，请重试保存。";}
                status.Text="未保存："+message;return false;
            }
        }
        private void Resume(){if(suspended){suspended=false;ClassSetHotKey.Resume();}}
        protected override void OnLoad(EventArgs e){TopMost=Owner!=null&&Owner.TopMost;base.OnLoad(e);}
        protected override void Dispose(bool disposing){if(disposing){Resume();Icon?.Dispose();}base.Dispose(disposing);}
    }
}
