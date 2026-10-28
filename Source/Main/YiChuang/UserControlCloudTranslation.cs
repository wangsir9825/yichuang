using System;
using System.Drawing;
using System.Windows.Forms;

namespace YiChuang
{
    public sealed class UserControlCloudTranslation : UserControl
    {
        private readonly bool gemini;
        public UserControlCloudTranslation(bool gemini)
        {
            this.gemini=gemini;Size=new Size(540,455);
            string title=EngineLabels.Name(gemini ? CloudTranslationClient.GeminiEngine : CloudTranslationClient.QwenEngine);
            var table=new TableLayoutPanel {Dock=DockStyle.Top,AutoSize=true,ColumnCount=2,Padding=new Padding(8)};
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
            Controls.Add(table);
            AddRow(table,"AI翻译引擎",new Label {AutoSize=true,Text=title,Margin=new Padding(3,8,3,8)});
            var key=new TextBox {Name=gemini ? "GeminiApiKey" : "QwenApiKey",UseSystemPasswordChar=true,Text=gemini ? ClassMain.GeminiKey : ClassMain.QwenKey,Dock=DockStyle.Fill};
            AddRow(table,"API Key",key);
            key.TextChanged+=(s,e)=>{if(gemini) ClassMain.GeminiKey=key.Text.Trim();else ClassMain.QwenKey=key.Text.Trim();};
            if(gemini)
            {
                var model=new TextBox {Name="GeminiModel",Text=ClassMain.GeminiModel,Dock=DockStyle.Fill};
                AddRow(table,"模型名称",model);model.TextChanged+=(s,e)=>ClassMain.GeminiModel=model.Text.Trim();
                AddRow(table,"接口",new Label {AutoSize=true,MaximumSize=new Size(370,0),Text="Google AI Studio / Gemini Developer API\r\n默认 gemini-3.8-flash，可填账号支持的文本模型。"});
            }
            else
            {
                AddRow(table,"模型",new Label {AutoSize=true,Text="qwen-mt-plus"});
                var endpoint=new TextBox {Name="QwenEndpoint",Text=ClassMain.QwenEndpoint,Dock=DockStyle.Fill,
                    Multiline=true,WordWrap=true,MinimumSize=new Size(0,48),Height=48};
                AddRow(table,"接口地址",endpoint);endpoint.TextChanged+=(s,e)=>ClassMain.QwenEndpoint=endpoint.Text.Trim();
                AddRow(table,"地域/空间",new Label {AutoSize=true,MaximumSize=new Size(370,0),Text="默认北京地域。可填百炼业务空间专属地址，\r\n地址以 /compatible-mode/v1 结尾；密钥须匹配地域。"});
            }
            var note=new Label {AutoSize=true,MaximumSize=new Size(500,0),Text="填写后点击“保存”返回列表，或点击“应用”继续编辑。\r\n在主窗口引擎菜单选择 "+title+"。\r\n单词和段落使用所选引擎；缺少密钥或请求失败会提示，不自动改用其他服务。\r\n此为在线API，所译原文会发送至配置的服务；费用与配额由服务商管理。",Margin=new Padding(3,18,3,3)};
            table.Controls.Add(note,0,table.RowCount++);table.SetColumnSpan(note,2);
        }
        private static void AddRow(TableLayoutPanel table,string label,Control control)
        {
            int row=table.RowCount++;table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.Controls.Add(new Label {Text=label,AutoSize=true,Margin=new Padding(3,8,12,8)},0,row);
            control.Margin=new Padding(3,5,3,8);table.Controls.Add(control,1,row);
        }
    }
}
