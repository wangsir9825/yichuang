using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;

namespace Xtranslator
{
    public class UserControl常用设置 : UserControl
    {
        public delegate void SetTranslationData();
        public delegate void SetTranslationSize();
        public delegate void SetYuanData();
        public delegate void SetYuanSize();
        public event SetTranslationData Event_SetTranslationData;
        public event SetTranslationSize Event_SetTranslationSizeType;
        public event SetYuanData Event_SetYuanData;
        public event SetYuanSize Event_SetYuanSizeType;
        private readonly ComboBox cbb译文区字体类型=new ComboBox();
        private readonly ComboBox cbb译文区字体大小=new ComboBox();
        private readonly ComboBox cbb原文区字体类型=new ComboBox();
        private readonly ComboBox cbb原文区字体大小=new ComboBox();
        public UserControl常用设置()
        {
            Size=new Size(550,430);AutoSize=true;AutoSizeMode=AutoSizeMode.GrowAndShrink;
            var layout=new FlowLayoutPanel {AutoSize=true,AutoSizeMode=AutoSizeMode.GrowAndShrink,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=Padding.Empty};
            layout.Controls.Add(new Label{Text="让阅读更舒服",Font=new Font("Microsoft YaHei UI",15F,FontStyle.Bold),AutoSize=true,Margin=new Padding(0,0,0,8)});
            layout.Controls.Add(new Label{Text="原文和译文可以使用不同的字体与字号。",AutoSize=true,ForeColor=UiTheme.Muted,Margin=new Padding(0,0,0,24)});
            layout.Controls.Add(FontGroup("原文",cbb原文区字体类型,cbb原文区字体大小,ClassMain.originalFontType,ClassMain.originalFontSize));
            layout.Controls.Add(FontGroup("译文",cbb译文区字体类型,cbb译文区字体大小,ClassMain.translationFontType,ClassMain.translationFontSize));
            var sample=new FlowLayoutPanel{Name="FontSample",FlowDirection=FlowDirection.TopDown,WrapContents=false,AutoSize=true,Padding=new Padding(16),BackColor=UiTheme.Background,Margin=new Padding(0,16,0,0)};
            var original=new Label{AutoSize=true,MaximumSize=new Size(480,0),Text="Good tools make things simple.",Margin=new Padding(0,0,0,12)};
            var translated=new Label{AutoSize=true,MaximumSize=new Size(480,0),Text="好的工具，让复杂的事变简单。"};sample.Controls.Add(original);sample.Controls.Add(translated);layout.Controls.Add(sample);Controls.Add(layout);
            Action preview=()=>{original.Font=new Font(ClassMain.originalFontType,ClassMain.originalFontSize);translated.Font=new Font(ClassMain.translationFontType,ClassMain.translationFontSize);};preview();
            cbb译文区字体类型.SelectedIndexChanged+=(s,e)=>{ClassMain.translationFontType=cbb译文区字体类型.Text;Event_SetTranslationSizeType?.Invoke();Event_SetTranslationData?.Invoke();preview();};
            cbb原文区字体类型.SelectedIndexChanged+=(s,e)=>{ClassMain.originalFontType=cbb原文区字体类型.Text;Event_SetYuanSizeType?.Invoke();Event_SetYuanData?.Invoke();preview();};
            cbb译文区字体大小.SelectedValueChanged+=(s,e)=>{float n;if(TrySize(cbb译文区字体大小,out n)){ClassMain.translationFontSize=n;Event_SetTranslationSizeType?.Invoke();Event_SetTranslationData?.Invoke();preview();}};
            cbb原文区字体大小.SelectedValueChanged+=(s,e)=>{float n;if(TrySize(cbb原文区字体大小,out n)){ClassMain.originalFontSize=n;Event_SetYuanSizeType?.Invoke();Event_SetYuanData?.Invoke();preview();}};
        }
        private static bool TrySize(ComboBox control,out float size) {return float.TryParse(Convert.ToString(control.SelectedValue),NumberStyles.Float,CultureInfo.InvariantCulture,out size)&&size>0;}
        private static Control FontGroup(string title,ComboBox family,ComboBox size,string currentFamily,float currentSize)
        {
            var group=new Panel {Size=new Size(540,90),Margin=new Padding(0,0,0,10)};
            var table=new TableLayoutPanel {Dock=DockStyle.Fill,Padding=Padding.Empty,ColumnCount=2,RowCount=2};
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,78));table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,22));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute,32));table.RowStyles.Add(new RowStyle(SizeType.Percent,100));
            table.Controls.Add(new Label {Text=title+"字体",AutoSize=true,Margin=new Padding(0,0,16,8)},0,0);
            table.Controls.Add(new Label {Text="字号",AutoSize=true,Margin=new Padding(0,0,0,8)},1,0);
            family.DropDownStyle=size.DropDownStyle=ComboBoxStyle.DropDownList;family.Dock=size.Dock=DockStyle.Fill;
            foreach(var f in FontFamily.Families) family.Items.Add(f.Name);
            if(!family.Items.Contains(currentFamily)) family.Items.Add(currentFamily);family.SelectedItem=currentFamily;
            var values=new DataTable();values.Columns.Add("name");values.Columns.Add("ID");
            bool found=false;foreach(int value in new[]{9,10,11,12,14,16,18,20,22,24,28,32}){string text=value.ToString();values.Rows.Add(text,text);found|=value==currentSize;}
            string selected=currentSize.ToString(CultureInfo.InvariantCulture);if(!found) values.Rows.Add(selected,selected);
            size.BindingContext=new BindingContext();
            size.DisplayMember="name";size.ValueMember="ID";size.DataSource=values;size.SelectedValue=selected;
            family.Margin=new Padding(0,0,16,0);size.Margin=Padding.Empty;table.Controls.Add(family,0,1);table.Controls.Add(size,1,1);group.Controls.Add(table);return group;
        }
    }
}
