using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YiChuang
{
    public sealed class UserControlLocalModels : UserControl
    {
        public UserControlLocalModels()
        {
            Size = new Size(490, 550);
            AddLabel("本地翻译模型（离线）", 8);
            AddLabel("本地服务地址", 48);
            var endpoint = AddText("LocalEndpoint", LocalModelSettings.Endpoint, 72);
            endpoint.TextChanged += (s,e) => LocalModelSettings.Endpoint = endpoint.Text.Trim();
            AddLabel("腾讯 Hy-MT2-7B 模型名", 110);
            var hy = AddText("HunyuanModel", LocalModelSettings.HunyuanModel, 134);
            hy.TextChanged += (s,e) => LocalModelSettings.HunyuanModel = hy.Text.Trim();
            AddLabel("Google TranslateGemma-12B 模型名", 172);
            var gemma = AddText("GemmaModel", LocalModelSettings.GemmaModel, 196);
            gemma.TextChanged += (s,e) => LocalModelSettings.GemmaModel = gemma.Text.Trim();
            var check = new Button { Name = "CheckLocalModels", Text = "检查服务与模型", Location = new Point(8,240), Width = 160, Height = 30 };
            var status = new Label { Name = "LocalStatus", Location = new Point(8,281), Width = 460, Height = 75 };
            check.Click += async (s,e) => { check.Enabled = false; status.Text = "正在检查…";
                try { string result=await Task.Run(() => LocalModelClient.CheckService()); if(!IsDisposed)status.Text=result; }
                catch (Exception ex) { if(!IsDisposed)status.Text = ex.Message; }
                finally { if (!IsDisposed) check.Enabled = true; } };
            Controls.Add(check); Controls.Add(status);
            foreach(Control c in Controls)if(c.Top>8)c.Top+=64;
            var legacy=new System.Collections.Generic.List<Control>();foreach(Control c in Controls)if(c.Top>=112&&c.Top<304)legacy.Add(c);
            var backend=new ComboBox{Name="LocalBackend",DropDownStyle=ComboBoxStyle.DropDownList,Location=new Point(8,64),Width=452};backend.Items.AddRange(new object[]{"内置 llama.cpp（默认，无需另装软件）","Ollama（独立本地服务）"});
            Controls.Add(new Label{Text="本地模型运行方式",Location=new Point(8,40),AutoSize=true});Controls.Add(backend);
            var embeddedHint=new Label{Location=new Point(8,114),Size=new Size(460,150),Text="模型由译窗自动加载和释放，无需手动启动服务。\r\n\r\n点击下方‘下载与配置向导’可复用已有权重、选择 GGUF 文件、下载模型或设置 CPU / 显卡加速。\r\n\r\n翻译方向仍在主界面原文区上方选择。"};Controls.Add(embeddedHint);
            backend.SelectedIndexChanged+=(s,e)=>{LocalModelSettings.Backend=backend.SelectedIndex==1?"ollama":"llamacpp";bool local=backend.SelectedIndex==0;foreach(Control c in legacy)c.Visible=!local;embeddedHint.Visible=local;};backend.SelectedIndex=LocalModelSettings.Backend=="ollama"?1:0;
            var setup=new Button{Name="OpenLocalSetup",Text="下载与配置向导",Location=new Point(188,304),Size=new Size(160,30)};
            setup.Click+=(s,e)=>{using(var wizard=new LocalSetupWizard())wizard.ShowDialog(FindForm());backend.SelectedIndex=LocalModelSettings.Backend=="ollama"?1:0;endpoint.Text=LocalModelSettings.Endpoint;hy.Text=LocalModelSettings.HunyuanModel;gemma.Text=LocalModelSettings.GemmaModel;};Controls.Add(setup);
            Controls.Add(new Label { Location = new Point(8,445), Width = 460, Height = 65,
                Text = "在主界面的引擎菜单选择本地腾讯或本地 Google。\r\n翻译只连接本机；两个模型轮流使用显卡。" });
        }
        private void AddLabel(string text, int y) { Controls.Add(new Label { Text = text, Location = new Point(8,y), AutoSize = true }); }
        private TextBox AddText(string name, string value, int y)
        {
            var box = new TextBox { Name = name, Text = value, Location = new Point(8,y), Width = 452 };
            Controls.Add(box); return box;
        }
    }


}
