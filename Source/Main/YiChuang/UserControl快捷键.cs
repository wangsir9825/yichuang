using System;
using System.Drawing;
using System.Windows.Forms;
namespace YiChuang {
 public class UserControl快捷键:UserControl {
  readonly TextBox shortcut=new TextBox{Name="ShowHideShortcut",ReadOnly=true,Width=260};
  readonly TextBox translate=new TextBox{Name="TranslateShortcut",ReadOnly=true,Width=260};
  readonly Label error=new Label{Name="ShortcutError",AutoSize=true,ForeColor=Color.Firebrick,MaximumSize=new Size(500,0)};
  public UserControl快捷键(){
   Size=new Size(560,360);BackColor=Color.White;
   var flow=new FlowLayoutPanel{Dock=DockStyle.Fill,FlowDirection=FlowDirection.TopDown,WrapContents=false,Padding=new Padding(16)};
   flow.Controls.Add(new Label{AutoSize=true,Text="选中文字后翻译",Margin=new Padding(0,0,0,7)});translate.Text=UiPreferences.TranslateShortcut;flow.Controls.Add(translate);
   flow.Controls.Add(new Label{AutoSize=true,Text="最小化 / 恢复窗口",Margin=new Padding(0,20,0,7)});shortcut.Text=ClassMain.快捷键隐藏显示;flow.Controls.Add(shortcut);
   var reset=new Button{Text="恢复默认 F5 / F6",AutoSize=true,Margin=new Padding(0,18,0,10)};
   reset.Click+=(s,e)=>{translate.Text="F5";shortcut.Text="F6";error.Text="";};flow.Controls.Add(reset);
   flow.Controls.Add(new Label{AutoSize=true,MaximumSize=new Size(510,0),Text="点击输入框，再按想使用的组合键。Backspace 恢复该项默认值。\r\n翻译快捷键在“选中后按 F5”模式下生效；最小化 / 恢复始终可用。\r\n保存时检查重复与其他程序占用。",Margin=new Padding(0,0,0,12)});
   flow.Controls.Add(error);Controls.Add(flow);Bind(translate,"F5");Bind(shortcut,"F6");
  }
  void Bind(TextBox input,string fallback){input.KeyDown+=(s,e)=>{
   if(e.KeyCode==Keys.Tab)return;e.SuppressKeyPress=true;
   if(e.KeyCode==Keys.ControlKey||e.KeyCode==Keys.ShiftKey||e.KeyCode==Keys.Menu)return;
   string text=e.KeyCode==Keys.Back?fallback:(e.Control?"Ctrl + ":"")+(e.Alt?"Alt + ":"")+(e.Shift?"Shift + ":"")+e.KeyCode;
   try{ClassSetHotKey.ParseShortcut(text,"+");input.Text=text;error.Text="";}catch(Exception ex){error.Text=ex.Message;}
  };}
  internal bool Apply(){string message;bool valid=ClassSetHotKey.TryApply(shortcut.Text,translate.Text,out message);error.Text=message;return valid;}
 }
}
