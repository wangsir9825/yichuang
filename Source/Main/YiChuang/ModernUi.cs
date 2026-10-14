using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YiChuang
{
    internal static class UiTheme
    {
        internal static readonly Color Background=Color.FromArgb(245,247,248), Ink=Color.FromArgb(36,52,59), Muted=Color.FromArgb(104,121,128), Line=Color.FromArgb(225,234,237), Accent=Color.FromArgb(24,120,109), Tint=Color.FromArgb(232,243,239);
        internal static readonly Color Selected=Color.FromArgb(164,210,198), Hover=Color.FromArgb(216,235,228), Frame=Color.FromArgb(128,159,152);
        internal static void StyleSettings(Control control) {
            control.ForeColor=Ink;
            if(control is Button){var b=(Button)control;b.FlatStyle=FlatStyle.Flat;b.FlatAppearance.BorderColor=Line;b.BackColor=Color.White;b.Padding=new Padding(7,3,7,3);}
            else if(control is TextBoxBase || control is ComboBox || control is UserControl || control is GroupBox || control is TableLayoutPanel || control is FlowLayoutPanel)control.BackColor=Color.White;
            foreach(Control child in control.Controls)StyleSettings(child);
            if(control.Name=="FontSample")control.BackColor=Background;
        }
    }
    internal sealed class QuietToolStripRenderer:ToolStripProfessionalRenderer
    {
        internal QuietToolStripRenderer(){RoundedEdges=false;}
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e){e.Graphics.Clear(e.ToolStrip.BackColor);}
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e){}
        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e){var b=e.Item as ToolStripButton;Fill(e,b!=null&&b.Checked);}
        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e){Fill(e,false);}
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e){e.TextFormat|=TextFormatFlags.EndEllipsis; base.OnRenderItemText(e);}
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e){
            var button=e.Item as ToolStripDropDownButton;
            if(button==null){base.OnRenderArrow(e);return;}
            float scale=Math.Max(1,e.Item.Height/36f),x=e.Item.Width-11*scale,y=e.Item.Height/2f;
            bool open=button.HasDropDownItems&&button.DropDown.Visible;
            float halfWidth=5*scale,halfHeight=2.5f*scale,direction=open?-1:1;
            var state=e.Graphics.Save();
            try{
                e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;
                using(var pen=new Pen(e.Item.Enabled?(open?UiTheme.Accent:UiTheme.Ink):SystemColors.GrayText,1.6f*scale)){
                    pen.StartCap=pen.EndCap=LineCap.Round;pen.LineJoin=LineJoin.Round;
                    e.Graphics.DrawLines(pen,new[]{new PointF(x-halfWidth,y-direction*halfHeight),new PointF(x,y+direction*halfHeight),new PointF(x+halfWidth,y-direction*halfHeight)});
                }
            }finally{e.Graphics.Restore(state);}
        }
        private static void Fill(ToolStripItemRenderEventArgs e,bool active){
            bool primary=e.Item.Name=="RetryLocalTranslation";
            if(primary||e.Item.Selected||e.Item.Pressed||active){
                using(var b=new SolidBrush(primary?UiTheme.Accent:(active||e.Item.Pressed?UiTheme.Selected:UiTheme.Hover)))e.Graphics.FillRectangle(b,new Rectangle(Point.Empty,e.Item.Size));
                if(active)using(var pen=new Pen(UiTheme.Frame))e.Graphics.DrawRectangle(pen,0,0,e.Item.Width-1,e.Item.Height-1);
            }
        }
    }
    internal static class ModernIcons
    {
        internal static Bitmap Draw(string name,Color color,int size=24) {
            var bitmap=new Bitmap(size,size);
            using(var g=Graphics.FromImage(bitmap))using(var pen=new Pen(color,1.7f)){
                g.SmoothingMode=SmoothingMode.AntiAlias;g.ScaleTransform(size/24f,size/24f);pen.StartCap=pen.EndCap=LineCap.Round;pen.LineJoin=LineJoin.Round;
                Action<float,float,float,float> line=(a,b,c,d)=>g.DrawLine(pen,a,b,c,d);
                switch(name){
                    case "layout":g.DrawRectangle(pen,3,4,18,16);line(12,4,12,20);line(3,8,21,8);break;
                    case "minimize":line(5,12,19,12);break;
                    case "maximize":g.DrawRectangle(pen,5,5,14,14);break;
                    case "close":line(6,6,18,18);line(6,18,18,6);break;
                    case "engine":g.DrawRectangle(pen,6,6,12,12);g.DrawRectangle(pen,9,9,6,6);for(int i=8;i<=16;i+=4){line(i,3,i,6);line(i,18,i,21);line(3,i,6,i);line(18,i,21,i);}break;
                    case "ocr":line(8,3,3,3);line(3,3,3,8);line(16,3,21,3);line(21,3,21,8);line(3,16,3,21);line(3,21,8,21);line(21,16,21,21);line(16,21,21,21);line(7,9,17,9);line(7,13,17,13);line(7,17,13,17);break;
                    case "append":line(4,6,14,6);line(4,11,12,11);line(4,16,10,16);line(17,12,17,20);line(13,16,21,16);break;
                    case "pin":g.DrawPolygon(pen,new[]{new Point(8,3),new Point(16,3),new Point(15,10),new Point(19,14),new Point(5,14),new Point(9,10)});line(12,14,12,21);break;
                    case "settings":line(3,6,21,6);line(3,12,21,12);line(3,18,21,18);g.FillEllipse(Brushes.White,6,3,6,6);g.DrawEllipse(pen,6,3,6,6);g.FillEllipse(Brushes.White,14,9,6,6);g.DrawEllipse(pen,14,9,6,6);g.FillEllipse(Brushes.White,5,15,6,6);g.DrawEllipse(pen,5,15,6,6);break;
                    case "exit":line(10,4,4,4);line(4,4,4,20);line(4,20,10,20);line(9,12,21,12);line(17,8,21,12);line(17,16,21,12);break;
                    case "power":g.DrawArc(pen,4,4,16,16,-55,290);line(12,2,12,11);break;
                    case "select":g.DrawPolygon(pen,new[]{new Point(5,3),new Point(6,19),new Point(10,15),new Point(14,21),new Point(17,19),new Point(13,13),new Point(20,12)});break;
                    case "keyboard":g.DrawRectangle(pen,2,5,20,14);for(int y=9;y<15;y+=4)for(int x=6;x<20;x+=4)line(x,y,x+.4f,y);line(8,16,16,16);break;
                    case "mouse":g.DrawEllipse(pen,6,2,12,20);line(12,2,12,10);line(6,10,18,10);break;
                    case "cancel":g.DrawRectangle(pen,6,6,12,12);break;
                    case "retry":g.DrawArc(pen,4,4,16,16,25,305);line(19,3,20,9);line(14,8,20,9);break;
                    case "copy":g.DrawRectangle(pen,8,7,12,14);line(5,17,3,17);line(3,17,3,3);line(3,3,16,3);line(16,3,16,5);break;
                    case "clear":g.DrawPolygon(pen,new[]{new Point(4,15),new Point(13,4),new Point(21,11),new Point(13,21),new Point(9,21)});line(8,11,17,18);break;
                    case "source":line(4,5,20,5);line(12,5,12,20);line(8,20,16,20);break;
                    case "target":line(4,6,20,6);line(4,11,20,11);line(4,16,15,16);line(4,21,12,21);break;
                    default:g.DrawEllipse(pen,4,4,16,16);line(12,8,12,13);line(12,17,12.2f,17);break;
                }
            }return bitmap;
        }
    }
}
