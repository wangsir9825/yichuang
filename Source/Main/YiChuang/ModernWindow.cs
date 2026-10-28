using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace YiChuang {
 internal sealed class ReadingCard:Panel {
  internal ReadingCard(){DoubleBuffered=true;BackColor=UiTheme.Line;Padding=new Padding(1);}
  protected override void OnResize(EventArgs e){base.OnResize(e);if(Width<8||Height<8)return;using(var p=new GraphicsPath()){int r=18;p.AddArc(0,0,r,r,180,90);p.AddArc(Width-r-1,0,r,r,270,90);p.AddArc(Width-r-1,Height-r-1,r,r,0,90);p.AddArc(0,Height-r-1,r,r,90,90);p.CloseFigure();var old=Region;Region=new Region(p);if(old!=null)old.Dispose();}}
 }
 public partial class Form2 {
  private bool modernReady,applyingLayout;
  private double modernScale=1;
  private ToolStrip captureStrip,sourceStrip,targetStrip,titleStrip;
  private ToolStripButton layoutButton;
  private ToolStripDropDownButton captureModeButton;
  private ToolStripDropDownButton directionButton;
  private ToolStripLabel sourceLabel,targetLabel;
  private StatusStrip modernStatusStrip;
  private ToolStripStatusLabel modernStatus;
  private TableLayoutPanel mainScaffold;
  private Panel brandBar;
  private PictureBox brandLogo;
  private Label brandName,brandVersion;
  private Process keyboardCopy;
  
  private readonly Dictionary<string,Image> modernImages=new Dictionary<string,Image>();
  [DllImport("user32.dll")]private static extern bool ReleaseCapture();
  private int Px(int value){return (int)Math.Round(value*modernScale);}
  private Image UiIcon(string name){Image value;if(!modernImages.TryGetValue(name,out value)){value=ModernIcons.Draw(name.Replace("-white",""),name.EndsWith("-white")?Color.White:UiTheme.Accent,32);modernImages.Add(name,value);}return value;}
  private ToolStrip NewStrip(string name){
   var strip=name=="WindowActions"?(ToolStrip)new WindowActionStrip():new ToolStrip();
   strip.Name=name;strip.Dock=DockStyle.Fill;strip.AutoSize=false;strip.GripStyle=ToolStripGripStyle.Hidden;
   strip.BackColor=UiTheme.Background;strip.ForeColor=UiTheme.Ink;strip.Font=Font;strip.Renderer=new QuietToolStripRenderer();
   strip.ShowItemToolTips=true;strip.CanOverflow=false;strip.Margin=Padding.Empty;return strip;
  }
  private void DragTitle(object sender,MouseEventArgs e){if(e.Button==MouseButtons.Left){ReleaseCapture();SendMessage(Handle,0xA1,(IntPtr)2,IntPtr.Zero);}}
  private ToolStripButton HeaderButton(string name,string icon,string hint,Action action){var b=new ToolStripButton(hint,UiIcon(icon)){Name=name,ToolTipText=hint,AccessibleName=hint,DisplayStyle=ToolStripItemDisplayStyle.Image,Alignment=ToolStripItemAlignment.Right};b.Click+=(s,e)=>action();return b;}
  private void InitializeModernUi(){
   using(var g=Graphics.FromHwnd(IntPtr.Zero))modernScale=Math.Max(1,g.DpiX/96.0);
   SuspendLayout();AutoScaleMode=AutoScaleMode.None;FormBorderStyle=FormBorderStyle.None;Font=new Font("Microsoft YaHei UI",10F);Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);BackColor=UiTheme.Background;Padding=new Padding(Px(8));
   MaximizedBounds=Screen.FromControl(this).WorkingArea;
   mainScaffold=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=1,RowCount=5,BackColor=UiTheme.Background,Margin=Padding.Empty,Padding=Padding.Empty};
   mainScaffold.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));
   mainScaffold.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(56)));mainScaffold.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(64)));mainScaffold.RowStyles.Add(new RowStyle(SizeType.Percent,100));mainScaffold.RowStyles.Add(new RowStyle(SizeType.Absolute,Px(64)));mainScaffold.RowStyles.Add(new RowStyle(SizeType.Absolute,0));
   brandBar=new Panel{Dock=DockStyle.Fill,Margin=Padding.Empty,BackColor=Color.White};brandBar.MouseDown+=DragTitle;brandBar.SizeChanged+=(s,e)=>{if(modernReady)RefreshModernToolbar();};
   brandLogo=new PictureBox{Name="BrandLogo",AccessibleName="译窗图标",Image=Icon.ToBitmap(),SizeMode=PictureBoxSizeMode.Zoom,Size=new Size(Px(30),Px(30)),Location=new Point(Px(20),Px(13))};brandBar.Controls.Add(brandLogo);brandLogo.MouseDown+=DragTitle;
   brandName=new Label{Text=ProductInfo.Name,AutoSize=true,Font=new Font(Font.FontFamily,13F,FontStyle.Bold),Location=new Point(Px(61),Px(15))};brandBar.Controls.Add(brandName);brandName.MouseDown+=DragTitle;
   brandVersion=new Label{Text=ProductInfo.Version,AutoSize=true,ForeColor=UiTheme.Muted,Font=new Font(Font.FontFamily,8F),Location=new Point(Px(106),Px(20))};brandBar.Controls.Add(brandVersion);brandVersion.MouseDown+=DragTitle;
   titleStrip=NewStrip("WindowActions");titleStrip.Dock=DockStyle.Right;titleStrip.LayoutStyle=ToolStripLayoutStyle.HorizontalStackWithOverflow;titleStrip.BackColor=Color.White;
   layoutButton=new ToolStripButton{ Name="ToggleLayout"};layoutButton.Click+=(s,e)=>{CaptureModernGeometry();UiPreferences.Layout=UiPreferences.Layout=="compact"?"wide":"compact";ApplyModernLayout(true);jiluset();};
   var min=HeaderButton("MinimizeWindow","minimize","最小化（"+ClassMain.快捷键隐藏显示+"）",()=>WindowState=FormWindowState.Minimized);min.Alignment=ToolStripItemAlignment.Left;
   var max=HeaderButton("MaximizeWindow","maximize","最大化 / 还原",()=>{WindowState=WindowState==FormWindowState.Maximized?FormWindowState.Normal:FormWindowState.Maximized;});max.Alignment=ToolStripItemAlignment.Left;
   var close=HeaderButton("CloseWindow","close","关闭译窗",()=>Close());close.Alignment=ToolStripItemAlignment.Left;
   titleStrip.Items.AddRange(new ToolStripItem[]{layoutButton,tbtntop,toolStripButton1,min,max,close});brandBar.Controls.Add(titleStrip);
   toolStripButton3.Visible=false;
   toolStrip1.Items.Clear();toolStrip1.Visible=false;mainScaffold.RowStyles[1].Height=0;
   toolStripButton4.Alignment=toolStripButton2.Alignment=ToolStripItemAlignment.Right;
   captureStrip=NewStrip("CaptureToolbar");captureModeButton=new ToolStripDropDownButton{Name="CaptureMode",AccessibleName="取词方式"};
   foreach(var mode in new[]{1,2,3,0}){int chosen=mode;var item=new ToolStripMenuItem{Tag=mode};item.Click+=(s,e)=>SetCaptureMode(chosen==0?_翻译模式:chosen,chosen!=0);captureModeButton.DropDownItems.Add(item);}
   foreach(var dropdown in new[]{toolStripDropDownButton1,captureModeButton}){dropdown.DropDownOpened+=(s,e)=>dropdown.Invalidate();dropdown.DropDownClosed+=(s,e)=>dropdown.Invalidate();}
   captureStrip.Items.AddRange(new ToolStripItem[]{captureModeButton,toolStripButton4,toolStripButton2,localRetryButton,localCancelButton});localRetryButton.Alignment=localCancelButton.Alignment=ToolStripItemAlignment.Right;toolStripButton4.Alignment=toolStripButton2.Alignment=ToolStripItemAlignment.Left;
   modernStatusStrip=new StatusStrip{Name="TranslationStatus",Dock=DockStyle.Fill,Visible=false,SizingGrip=false,Margin=Padding.Empty,BackColor=UiTheme.Background,Font=Font};modernStatus=new ToolStripStatusLabel{ForeColor=Color.Firebrick,Spring=true,TextAlign=ContentAlignment.MiddleLeft};modernStatusStrip.Items.Add(modernStatus);
   sourceStrip=NewStrip("SourceActions");targetStrip=NewStrip("TargetActions");sourceStrip.Dock=targetStrip.Dock=DockStyle.Top;sourceStrip.BackColor=Color.White;targetStrip.BackColor=Color.FromArgb(252,253,253);
   sourceLabel=new ToolStripLabel("原文",UiIcon("source")){ToolTipText="输入或粘贴原文",AccessibleName="原文"};targetLabel=new ToolStripLabel("译文",UiIcon("target")){ToolTipText="译文",AccessibleName="译文"};sourceStrip.Items.Add(sourceLabel);targetStrip.Items.Add(targetLabel);
   sourceLabel.Visible=targetLabel.Visible=false;
   directionButton=new ToolStripDropDownButton{Name="TranslationDirection",AccessibleName="翻译方向"};
   foreach(string mode in new[]{"auto","to-en","to-zh"}){string chosen=mode;var choice=new ToolStripMenuItem(TranslationDirection.Label(mode)){Tag=mode};choice.Click+=(s,e)=>SetTranslationDirection(chosen);directionButton.DropDownItems.Add(choice);}
   directionButton.DropDownOpened+=(s,e)=>directionButton.Invalidate();directionButton.DropDownClosed+=(s,e)=>directionButton.Invalidate();
   sourceStrip.Items.Add(directionButton);targetStrip.Items.Add(toolStripDropDownButton1);
   sourceStrip.SizeChanged+=(s,e)=>{if(modernReady)RefreshModernToolbar();};targetStrip.SizeChanged+=(s,e)=>{if(modernReady)RefreshModernToolbar();};
   sourceStrip.Items.Add(HeaderButton("ClearSource","clear","清空原文",()=>rtb原文.Clear()));targetStrip.Items.Add(HeaderButton("CopyTranslation","copy","复制译文",()=>{if(rtb译文.TextLength>0)Clipboard.SetText(rtb译文.Text);}));
   var sourcePanel=new Panel{Dock=DockStyle.Fill,Padding=new Padding(Px(22),Px(6),Px(22),Px(20)),BackColor=Color.White};
   var targetPanel=new Panel{Dock=DockStyle.Fill,Padding=sourcePanel.Padding,BackColor=targetStrip.BackColor};
   rtb原文.BackColor=sourcePanel.BackColor;rtb译文.BackColor=targetPanel.BackColor;rtb原文.ForeColor=rtb译文.ForeColor=UiTheme.Ink;rtb原文.BorderStyle=rtb译文.BorderStyle=BorderStyle.None;rtb原文.ScrollBars=rtb译文.ScrollBars=RichTextBoxScrollBars.Vertical;rtb原文.WordWrap=rtb译文.WordWrap=true;
   rtb原文.AccessibleName="原文";rtb译文.AccessibleName="译文";sourcePanel.Controls.Add(rtb原文);sourcePanel.Controls.Add(sourceStrip);targetPanel.Controls.Add(rtb译文);targetPanel.Controls.Add(targetStrip);
   splitContainer1.Panel1.Controls.Clear();splitContainer1.Panel2.Controls.Clear();splitContainer1.Panel1.Controls.Add(sourcePanel);splitContainer1.Panel2.Controls.Add(targetPanel);dSkinPanel1.Dispose();dSkinPanel2.Dispose();splitContainer1.BorderStyle=BorderStyle.None;splitContainer1.BackColor=UiTheme.Line;splitContainer1.SplitterWidth=Px(1);splitContainer1.Panel1MinSize=80;splitContainer1.Panel2MinSize=80;
   var card=new ReadingCard{Dock=DockStyle.Fill,Margin=new Padding(Px(20),0,Px(20),0)};card.Controls.Add(splitContainer1);splitContainer1.Dock=DockStyle.Fill;
   palMain.Controls.Clear();palMain.Padding=Padding.Empty;palMain.Controls.Add(mainScaffold);mainScaffold.Controls.Add(brandBar,0,0);mainScaffold.Controls.Add(toolStrip1,0,1);mainScaffold.Controls.Add(card,0,2);mainScaffold.Controls.Add(captureStrip,0,3);mainScaffold.Controls.Add(modernStatusStrip,0,4);
   foreach(ToolStripItem item in toolStrip1.Items)item.Click+=(s,e)=>RefreshModernToolbar();foreach(ToolStripItem item in titleStrip.Items)item.Click+=(s,e)=>RefreshModernToolbar();foreach(ToolStripItem item in toolStripDropDownButton1.DropDownItems)item.Click+=(s,e)=>RefreshModernToolbar();
   localCancelButton.EnabledChanged+=(s,e)=>{if(modernReady)RefreshModernStatus();};splitContainer1.SplitterMoved+=(s,e)=>CaptureModernGeometry();ResizeEnd+=(s,e)=>CaptureModernGeometry();SizeChanged+=(s,e)=>{if(modernReady)RefreshModernToolbar();};
   FormClosed+=(s,e)=>{ClassSetHotKey.xiezaimoren();};modernReady=true;RefreshModernToolbar();ResumeLayout(true);
  }
  private void StyleAction(ToolStripItem item,string icon,string text,string hint,bool compact){
   item.Image=UiIcon(icon);item.Text=text;item.ToolTipText=hint;item.AccessibleName=hint;item.Font=Font;item.ForeColor=UiTheme.Ink;item.TextAlign=ContentAlignment.MiddleLeft;item.DisplayStyle=compact?ToolStripItemDisplayStyle.Image:ToolStripItemDisplayStyle.ImageAndText;item.AutoSize=false;item.Margin=new Padding(Px(2),0,Px(2),0);item.Padding=new Padding(Px(7),0,Px(7),0);item.Size=new Size(compact?Px(34):Math.Max(Px(64),TextRenderer.MeasureText(text,Font).Width+Px(34)),Px(36));
   var dropdown=item as ToolStripDropDownButton;if(dropdown!=null){dropdown.ShowDropDownArrow=true;dropdown.Width+=Px(14);dropdown.ImageAlign=ContentAlignment.MiddleLeft;}
  }
  private void RefreshModernToolbar(){
   // CloseWindow disposes controls/images before later Click subscribers run.
   if(!modernReady||IsDisposed||Disposing||modernImagesDisposed)return;bool compact=UiPreferences.Layout=="compact";
   foreach(var strip in new[]{toolStrip1,captureStrip,titleStrip,sourceStrip,targetStrip})strip.ImageScalingSize=new Size(Px(17),Px(17));
   toolStrip1.Padding=new Padding(Px(20),Px(12),Px(18),Px(12));captureStrip.Padding=new Padding(Px(20),Px(13),Px(20),Px(13));titleStrip.Padding=new Padding(0,Px(10),Px(8),Px(10));sourceStrip.Height=targetStrip.Height=Px(44);
   StyleAction(toolStripDropDownButton1,"engine",EngineLabels.MenuText(ClassMain.isjiekounum),"译文引擎："+EngineLabels.MenuText(ClassMain.isjiekounum),false);
   StyleAction(directionButton,"source",TranslationDirection.Label(TranslationDirection.Mode),"原文翻译方向："+TranslationDirection.Label(TranslationDirection.Mode),false);
   foreach(ToolStripMenuItem item in directionButton.DropDownItems)item.Checked=(string)item.Tag==TranslationDirection.Mode;
   toolStripDropDownButton1.Width=Math.Max(Px(70),targetStrip.ClientSize.Width-Px(48));directionButton.Width=Math.Max(Px(70),sourceStrip.ClientSize.Width-Px(48));
   toolStripDropDownButton1.ImageAlign=ContentAlignment.MiddleLeft;
   StyleAction(toolStripButton4,"ocr","截图取字","截图识别文字并翻译",compact);StyleAction(toolStripButton2,"append","追加","追加下次捕获的文字",compact);
   StyleAction(layoutButton,"layout",compact?"舒展双栏":"轻巧侧窗","切换为"+(compact?"舒展双栏":"轻巧侧窗"),compact);StyleAction(tbtntop,"pin","置顶","窗口置顶",compact);StyleAction(toolStripButton1,"settings","设置","打开设置",compact);
   if(compact)foreach(ToolStripItem item in new ToolStripItem[]{layoutButton,tbtntop,toolStripButton1}){item.Width=Px(30);item.Margin=new Padding(Px(1),0,Px(1),0);item.Padding=Padding.Empty;}
   foreach(ToolStripItem item in titleStrip.Items)if(item.Name=="MinimizeWindow"||item.Name=="MaximizeWindow"||item.Name=="CloseWindow"){item.AutoSize=false;item.Size=new Size(Px(compact?26:30),Px(34));item.Padding=Padding.Empty;}
   int titleWidth=Px(16);foreach(ToolStripItem item in titleStrip.Items)titleWidth+=item.Width+item.Margin.Horizontal;titleStrip.Width=titleWidth;
   // Keep explicit references: docking/visibility can change child control order.
   brandLogo.Visible=brandName.Visible=brandVersion.Visible=true;
   brandLogo.Location=new Point(Px(compact?12:20),Px(13));
   brandName.Location=new Point(Px(compact?49:61),Px(compact?5:15));
   brandVersion.Location=new Point(compact?brandName.Left:brandName.Right+Px(5),Px(compact?32:20));
   var names=new[]{"鼠标划选","选中后按 "+UiPreferences.TranslateShortcut,"鼠标组合","暂停取词"};int selected=ClassMain._is自动翻译总开关?Math.Max(0,Math.Min(2,_翻译模式-1)):3;
   int i=0;foreach(ToolStripMenuItem item in captureModeButton.DropDownItems){item.Text=names[i];item.Checked=i==selected;i++;}
   StyleAction(captureModeButton,selected==0?"select":selected==1?"keyboard":selected==2?"mouse":"power","取词 · "+names[selected],"取词方式："+names[selected],compact);captureModeButton.ForeColor=selected==3?Color.FromArgb(169,99,35):UiTheme.Ink;
   StyleAction(localRetryButton,"retry-white","重新翻译","使用当前引擎重新翻译",compact);localRetryButton.Visible=true;localRetryButton.BackColor=UiTheme.Accent;localRetryButton.ForeColor=Color.White;
   StyleAction(localCancelButton,"cancel","取消翻译","取消当前翻译",compact);localCancelButton.Visible=localCancelButton.Enabled;
   tbtntop.Checked=Convert.ToString(tbtntop.Tag)=="1";toolStripButton2.Checked=ClassMain._is追加文本;
   sourceLabel.DisplayStyle=targetLabel.DisplayStyle=compact?ToolStripItemDisplayStyle.Image:ToolStripItemDisplayStyle.ImageAndText;sourceLabel.Font=targetLabel.Font=Font;
   RefreshModernStatus();
  }
  private void RefreshModernStatus(){if(!modernReady)return;localCancelButton.Visible=localCancelButton.Enabled;string error=ClassSetHotKey.LastError;modernStatus.Text=string.IsNullOrWhiteSpace(error)?timingText:error+(timingText.Length>0?" · "+timingText:"");modernStatus.ForeColor=string.IsNullOrWhiteSpace(error)?UiTheme.Muted:Color.Firebrick;modernStatus.ToolTipText="从发起翻译到结果显示的耗时，包含排队、网络等待及本地模型加载。";modernStatusStrip.ShowItemToolTips=true;modernStatusStrip.Visible=!string.IsNullOrWhiteSpace(modernStatus.Text);mainScaffold.RowStyles[4].Height=modernStatusStrip.Visible?Px(28):0;}
  private void SetCaptureMode(int mode,bool enabled){
   _翻译模式=mode<1||mode>3?1:mode;ClassMain._is自动翻译总开关=enabled;tbtntrans.Tag=enabled?"1":"0";ClassMain.Master_switch=enabled?"1":"0";
   timer1.Stop();clearHook();guanbi1();guanbi2();guanbi3();CancelExplicitCapture();shangyicifuzhi="";old_ClipboardData="";
   if(enabled){if(_翻译模式==1)qidong1();else if(_翻译模式==2)qidong2();else qidong3();}
   RefreshModernToolbar();if(!_isInitializing)jiluset();
  }
  private bool IsTranslatorWindow(IntPtr window){if(window==IntPtr.Zero)return true;int processId;GetWindowThreadProcessId(window.ToInt32(),out processId);return processId==Process.GetCurrentProcess().Id;}
  private void CaptureModernGeometry(){if(!modernReady||applyingLayout||_isInitializing||WindowState!=FormWindowState.Normal)return;int total=splitContainer1.Orientation==Orientation.Vertical?splitContainer1.Width:splitContainer1.Height;double ratio=Math.Max(.2,Math.Min(.8,(double)splitContainer1.SplitterDistance/Math.Max(1,total)));if(UiPreferences.Layout=="compact"){UiPreferences.CompactWidth=(int)(Width/modernScale);UiPreferences.CompactHeight=(int)(Height/modernScale);UiPreferences.CompactSplit=ratio;}else{UiPreferences.WideWidth=(int)(Width/modernScale);UiPreferences.WideHeight=(int)(Height/modernScale);UiPreferences.WideSplit=ratio;}}
  private void ApplyModernLayout(bool resize){if(!modernReady)return;applyingLayout=true;SuspendLayout();try{bool compact=UiPreferences.Layout=="compact";var work=Screen.FromControl(this).WorkingArea;if(resize&&WindowState==FormWindowState.Maximized)WindowState=FormWindowState.Normal;MinimumSize=new Size(Math.Min(work.Width,Px(compact?330:760)),Math.Min(work.Height,Px(420)));if(resize)Size=new Size(Math.Min(work.Width,Px(compact?UiPreferences.CompactWidth:UiPreferences.WideWidth)),Math.Min(work.Height,Px(compact?UiPreferences.CompactHeight:UiPreferences.WideHeight)));splitContainer1.Orientation=compact?Orientation.Horizontal:Orientation.Vertical;ResumeLayout(true);PerformLayout();splitContainer1.PerformLayout();int total=compact?splitContainer1.Height:splitContainer1.Width;if(total>164)splitContainer1.SplitterDistance=Math.Max(80,Math.Min(total-80-splitContainer1.SplitterWidth,(int)(total*(compact?UiPreferences.CompactSplit:UiPreferences.WideSplit))));Location=new Point(Math.Max(work.Left,Math.Min(Left,work.Right-Width)),Math.Max(work.Top,Math.Min(Top,work.Bottom-Height)));RefreshModernToolbar();}finally{applyingLayout=false;ResumeLayout(true);}}
  private bool modernImagesDisposed;
  private void DisposeModernImages(){if(modernImagesDisposed)return;modernImagesDisposed=true;foreach(var image in modernImages.Values)image.Dispose();brandLogo?.Image?.Dispose();}
  private void CaptureSelectedTranslation(){ CaptureExplicitSelection(2,GetForegroundWindow()); }
 }
}
