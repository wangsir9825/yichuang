using System;
using System.Drawing;
using System.Windows.Forms;
namespace YiChuang
{
    public class UserControlDeepseek : UserControl
    {
        private readonly TextBox txtKey=new TextBox {Name="DeepSeekApiKey",UseSystemPasswordChar=true,Dock=DockStyle.Fill};
        private readonly TextBox txtModel=new TextBox {Name="DeepSeekModel",Dock=DockStyle.Fill};
        public UserControlDeepseek()
        {
            Size=new Size(540,220);var table=new TableLayoutPanel {Dock=DockStyle.Top,AutoSize=true,Padding=new Padding(8),ColumnCount=2};
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));Controls.Add(table);
            table.Controls.Add(new Label {Text="API Key",AutoSize=true,Margin=new Padding(3,8,16,8)},0,0);table.Controls.Add(txtKey,1,0);
            table.Controls.Add(new Label {Text="模型名称",AutoSize=true,Margin=new Padding(3,8,16,8)},0,1);table.Controls.Add(txtModel,1,1);
            txtKey.Text=ClassMain.DeepSeekAI_key;txtModel.Text=ClassMain.DeepSeekAI_model;
            txtKey.TextChanged+=(s,e)=>ClassMain.DeepSeekAI_key=txtKey.Text.Trim();txtModel.TextChanged+=(s,e)=>ClassMain.DeepSeekAI_model=txtModel.Text.Trim();
            var note=new Label {AutoSize=true,Margin=new Padding(3,20,3,8),Text="填写个人 API Key 和模型名称后，点击下方“保存并返回”。"};table.Controls.Add(note,0,2);table.SetColumnSpan(note,2);
        }
    }
}
