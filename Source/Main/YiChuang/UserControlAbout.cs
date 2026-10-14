using System.Drawing;
using System.Windows.Forms;

namespace YiChuang
{
    public sealed class UserControlAbout : UserControl
    {
        public UserControlAbout()
        {
            Size = new Size(540, 370);
            var content=new FlowLayoutPanel {Dock=DockStyle.Top, AutoSize=true, FlowDirection=FlowDirection.TopDown, WrapContents=false, Padding=new Padding(8)};
            Controls.Add(content);
            content.Controls.Add(new Label {AutoSize=true, Margin=new Padding(0,0,0,20),
                Text=ProductInfo.Title, Font=new Font("Microsoft YaHei UI",14,FontStyle.Bold)});
            content.Controls.Add(new Label {AutoSize=true, MaximumSize=new Size(510,0), Margin=new Padding(0,0,0,24),
                Text="桌面取词与翻译工具\r\n\r\n支持八款翻译引擎、中英翻译、截图文字识别、\r\n划词 / 快捷键（默认 F5）/ 鼠标组合取词，以及原文与译文句子联动高亮。\r\n\r\n本地模型在本机运行；在线引擎使用个人账号和密钥。\r\n在线引擎的原文会发送至对应服务，费用和配额由服务商管理。"});
            content.Controls.Add(new Button {Name="TutorialPlaceholder", Text=ProductInfo.TutorialPlaceholder,
                Enabled=false, AutoSize=false, Size=new Size(240,40), Margin=new Padding(0,0,0,12)});
            content.Controls.Add(new Label {AutoSize=true, Margin=Padding.Empty,
                Text="教程准备中，完成后将在这里提供入口。"});
        }
    }
}
