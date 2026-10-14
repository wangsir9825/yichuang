using System;
using System.Drawing;
using System.Windows.Forms;

namespace Xtranslator
{
    public class UserControlDeepl : UserControl
    {
        public UserControlDeepl()
        {
            AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
            MinimumSize = new Size(550,0);
            Padding = new Padding(8);
            var grid = new TableLayoutPanel { Name = "DeepLSettingsLayout", Dock = DockStyle.Top,
                AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, ColumnCount = 2,
                RowCount = 4, Margin = Padding.Empty, Padding = new Padding(4) };
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            for(int row=0;row<4;row++) grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            var heading = new Label {Text=EngineLabels.Name(6), AutoSize=true, Margin=new Padding(0,0,0,16)};
            grid.Controls.Add(heading,0,0); grid.SetColumnSpan(heading,2);
            var account = new ComboBox {Name="DeepLAccountType", DropDownStyle=ComboBoxStyle.DropDownList,
                Dock=DockStyle.Fill, Width=330, MinimumSize=new Size(260,0)};
            account.Items.AddRange(new object[]{"API Free / Developer", "API Pro / Growth"});
            account.SelectedIndex = ClassMain.DeeplInterface == "3" ? 1 : 0;
            account.SelectedIndexChanged += (s,e) => ClassMain.DeeplInterface = account.SelectedIndex == 1 ? "3" : "2";
            AddField(grid,"账号类型","AccountTypeLabel",account,1);
            var key = new TextBox {Name="ApiKey", Dock=DockStyle.Fill, Width=330,
                MinimumSize=new Size(260,0), UseSystemPasswordChar=true, Text=ClassMain.Deeplkey_official};
            key.TextChanged += (s,e) => ClassMain.Deeplkey_official = key.Text.Trim();
            AddField(grid,"API Key","ApiKeyLabel",key,2);
            var note = new Label {AutoSize=true, MaximumSize=new Size(510,0), Margin=new Padding(0,18,0,0),
                Text="在 DeepL 网页点击 Copy key，粘贴完整密钥。\r\n末尾为 :fx 的密钥选择 API Free / Developer。\r\n翻译方向统一在主界面原文区上方选择。\r\n点击下方“保存并返回”保存参数。"};
            grid.Controls.Add(note,0,3); grid.SetColumnSpan(note,2);
            Controls.Add(grid);
        }
        private static void AddField(TableLayoutPanel grid,string text,string name,Control field,int row)
        {
            var label = new Label {Name=name, Text=text, AutoSize=true, Anchor=AnchorStyles.Left,
                Margin=new Padding(0,6,18,8)};
            field.Margin = new Padding(0,4,0,10);
            field.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            grid.Controls.Add(label,0,row); grid.Controls.Add(field,1,row);
        }
    }
}
