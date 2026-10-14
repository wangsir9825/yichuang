using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Xtranslator {
 public class Frmsetting:Form {
  public delegate void SetTranslationData();public delegate void SetTranslationSize();public delegate void SetYuanData();public delegate void SetYuanSize();
  public event SetTranslationData Event_SetTranslationData;public event SetTranslationSize Event_SetTranslationSizeType;public event SetYuanData Event_SetYuanData;public event SetYuanSize Event_SetYuanSizeType;
  private readonly FlowLayoutPanel navigation=new FlowLayoutPanel();
  private readonly Panel content=new Panel(),footer=new Panel(),right=new Panel(),engineRow=new Panel();
  private readonly TableLayoutPanel layout=new TableLayoutPanel();
  private readonly Dictionary<string,Func<Control>> factories=new Dictionary<string,Func<Control>>();
  private readonly Dictionary<string,Control> pages=new Dictionary<string,Control>();
  private readonly ComboBox enginePicker=new ComboBox();
  private readonly Button save=new Button();
  private readonly double scale;
  private bool showing,editingEngine;
  private bool ownedResourcesDisposed,hotkeysSuspended;
  private readonly Dictionary<int,string> builtinPages=new Dictionary<int,string>();
  private sealed class EnginePageChoice {public int Id;public string Key;public override string ToString(){return Id==0?Key:EngineLabels.MenuText(Id);}}
  public string[] SettingsPageTitles {get{return factories.Keys.ToArray();}}
  private int Px(int n){return (int)Math.Round(n*scale);}
  public Frmsetting(){
   using(var g=Graphics.FromHwnd(IntPtr.Zero))scale=Math.Max(1,g.DpiX/96.0);
   Text=ProductInfo.Name+"设置";Font=new Font("Microsoft YaHei UI",10F);BackColor=Color.White;ForeColor=UiTheme.Ink;AutoScaleMode=AutoScaleMode.None;StartPosition=FormStartPosition.CenterParent;ShowInTaskbar=false;Icon=Icon.ExtractAssociatedIcon(Application.ExecutablePath);
   var work=Screen.PrimaryScreen.WorkingArea;ClientSize=new Size(Math.Min(Px(930),work.Width-Px(32)),Math.Min(Px(558),work.Height-Px(70)));MinimumSize=new Size(Px(720),Px(408));
   navigation.Name="SettingsNavigation";navigation.Dock=DockStyle.Fill;navigation.FlowDirection=FlowDirection.TopDown;navigation.WrapContents=false;navigation.Padding=new Padding(Px(12),Px(18),Px(12),0);navigation.Margin=Padding.Empty;navigation.BackColor=UiTheme.Background;
   content.Name="SettingsContent";content.Dock=DockStyle.Fill;content.AutoScroll=true;content.Padding=new Padding(Px(24));content.BackColor=Color.White;
   engineRow.Dock=DockStyle.Top;engineRow.Height=Px(62);engineRow.Padding=new Padding(Px(24),Px(16),Px(24),Px(8));engineRow.BackColor=Color.White;engineRow.Visible=false;
   enginePicker.Name="SettingsEnginePicker";enginePicker.DropDownStyle=ComboBoxStyle.DropDownList;enginePicker.Dock=DockStyle.Fill;enginePicker.Font=Font;engineRow.Controls.Add(enginePicker);
   right.Dock=DockStyle.Fill;right.Margin=Padding.Empty;right.Controls.Add(content);right.Controls.Add(engineRow);
   layout.Dock=DockStyle.Fill;layout.ColumnCount=2;layout.RowCount=1;layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute,Px(174)));layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,100));layout.RowStyles.Add(new RowStyle(SizeType.Percent,100));layout.Controls.Add(navigation,0,0);layout.Controls.Add(right,1,0);
   footer.Dock=DockStyle.Bottom;footer.Height=Px(72);footer.Padding=new Padding(Px(24),Px(16),Px(24),Px(16));footer.BackColor=Color.White;
   save.Name="SaveSettings";save.Text="保存并关闭";save.Size=new Size(Px(148),Px(40));save.Dock=DockStyle.Right;save.FlatStyle=FlatStyle.Flat;save.FlatAppearance.BorderSize=0;save.BackColor=UiTheme.Accent;save.ForeColor=Color.White;save.Padding=Padding.Empty;save.Font=Font;save.Click+=(s,e)=>{if(editingEngine){if(TrySaveSettings())ShowPage("引擎管理");}else Close();};footer.Controls.Add(save);
   Controls.Add(layout);Controls.Add(footer);
   Add("常用设置",()=>{var c=new UserControl常用设置();c.Event_SetTranslationData+=()=>Event_SetTranslationData?.Invoke();c.Event_SetTranslationSizeType+=()=>Event_SetTranslationSizeType?.Invoke();c.Event_SetYuanData+=()=>Event_SetYuanData?.Invoke();c.Event_SetYuanSizeType+=()=>Event_SetYuanSizeType?.Invoke();return c;});
   foreach(int id in new[]{6,14,18,19})builtinPages[id]=EngineLabels.MenuText(id);builtinPages[16]=builtinPages[17]=EngineLabels.LocalSettingsTitle;
   Add("引擎管理",()=>new EngineManagerControl(id=>ShowPage(builtinPages[id])));
   Add("快捷键",()=>new UserControl快捷键());Add(EngineLabels.MenuText(6),()=>new UserControlDeepl());Add(EngineLabels.MenuText(14),()=>new UserControlDeepseek());Add(EngineLabels.LocalSettingsTitle,()=>new UserControlLocalModels());Add(EngineLabels.MenuText(18),()=>new UserControlCloudTranslation(true));Add(EngineLabels.MenuText(19),()=>new UserControlCloudTranslation(false));Add("配置备份设置",()=>new UserControl配置备份设置());Add(ProductInfo.AboutPage,()=>new UserControlAbout());
   Nav("文字显示","source","常用设置");Nav("翻译引擎","engine","引擎管理");Nav("快捷键","keyboard","快捷键");Nav("配置备份","copy","配置备份设置");Nav("关于译窗","info",ProductInfo.AboutPage);
   enginePicker.SelectedIndexChanged+=(s,e)=>{if(!showing&&enginePicker.SelectedItem is EnginePageChoice)ShowPage(((EnginePageChoice)enginePicker.SelectedItem).Key);};
   Load+=(s,e)=>ShowPage("常用设置");Shown+=(s,e)=>{ClassSetHotKey.Suspend();hotkeysSuspended=true;if(content.Controls.Count>0)FitPage(content.Controls[0]);};
   FormClosing+=(s,e)=>{if(!TrySaveSettings())e.Cancel=true;};
   FormClosed+=(s,e)=>ResumeHotkeys();
  }
  private bool TrySaveSettings(){
   try{
    if(EngineRegistry.Get(18)!=null&&pages.ContainsKey(builtinPages[18]))CloudTranslationClient.ValidateGeminiModel(ClassMain.GeminiModel);
    if(EngineRegistry.Get(19)!=null&&pages.ContainsKey(builtinPages[19]))CloudTranslationClient.ValidateQwenEndpoint(ClassMain.QwenEndpoint);
    if(pages.ContainsKey(EngineLabels.LocalSettingsTitle))LocalModelSettings.Save();
    if(pages.ContainsKey("快捷键")&&!((UserControl快捷键)pages["快捷键"]).Apply()){ShowPage("快捷键");return false;}
    NoLoginBuild.NormalizeSettings();ClassMain.jiluset();if(ClassMain.SettingsSaveError!=null)throw new InvalidOperationException(ClassMain.SettingsSaveError);
    return true;
   }catch(Exception ex){MessageBox.Show(this,ex.Message,"设置未保存",MessageBoxButtons.OK,MessageBoxIcon.Information);return false;}
  }
  private void ResumeHotkeys(){if(hotkeysSuspended){hotkeysSuspended=false;ClassSetHotKey.Resume();}}
  protected override void Dispose(bool disposing) {
   if(disposing&&!ownedResourcesDisposed) {
    ownedResourcesDisposed=true;ResumeHotkeys();
    foreach(Control p in pages.Values)if(p.Parent==null)p.Dispose();
    foreach(Button b in navigation.Controls){b.Image?.Dispose();b.Image=null;}
    var ownedFont=Font;var ownedIcon=Icon;
    base.Dispose(true);ownedFont?.Dispose();ownedIcon?.Dispose();return;
   }
   base.Dispose(disposing);
  }
  private static bool IsEngine(string key){return key!="常用设置"&&key!="快捷键"&&key!="配置备份设置"&&key!=ProductInfo.AboutPage;}
  private void Add(string name,Func<Control> factory){factories.Add(name,factory);}
  private void Nav(string title,string icon,string key){var b=new Button{Text="  "+title,Tag=key,Size=new Size(Px(148),Px(43)),Margin=new Padding(0,0,0,Px(5)),FlatStyle=FlatStyle.Flat,TextAlign=ContentAlignment.MiddleLeft,ImageAlign=ContentAlignment.MiddleLeft,TextImageRelation=TextImageRelation.ImageBeforeText,Image=ModernIcons.Draw(icon,UiTheme.Accent,Px(17)),Padding=new Padding(Px(10),0,0,0),Font=Font};b.FlatAppearance.BorderSize=0;b.Click+=(s,e)=>ShowPage(key);navigation.Controls.Add(b);}
  public void ShowPage(string key){
   if(!factories.ContainsKey(key))return;showing=true;editingEngine=IsEngine(key)&&key!="引擎管理";save.Text=editingEngine?"保存并返回":"保存并关闭";
   try{engineRow.Visible=IsEngine(key)&&key!="引擎管理";if(IsEngine(key)){enginePicker.Items.Clear();enginePicker.Items.Add(new EnginePageChoice{Key="引擎管理"});foreach(var pair in builtinPages.Where(p=>EngineRegistry.Get(p.Key)!=null).GroupBy(p=>p.Value).Select(g=>g.First()))enginePicker.Items.Add(new EnginePageChoice{Id=pair.Key==16||pair.Key==17?0:pair.Key,Key=pair.Value});foreach(EnginePageChoice choice in enginePicker.Items)if(choice.Key==key){enginePicker.SelectedItem=choice;break;}}foreach(Button b in navigation.Controls){bool active=IsEngine(key)?IsEngine((string)b.Tag):(string)b.Tag==key;b.BackColor=active?UiTheme.Selected:UiTheme.Background;b.FlatAppearance.MouseOverBackColor=active?UiTheme.Selected:UiTheme.Hover;b.FlatAppearance.MouseDownBackColor=UiTheme.Selected;b.ForeColor=active?UiTheme.Accent:UiTheme.Ink;}
    Control page;if(!pages.TryGetValue(key,out page)){page=factories[key]();var user=page as UserControl;if(user!=null)user.AutoScaleMode=AutoScaleMode.None;UiTheme.StyleSettings(page);page.Font=Font;page.Scale(new SizeF((float)scale,(float)scale));pages.Add(key,page);}
    var manager=page as EngineManagerControl;if(manager!=null)manager.RefreshList(manager.SelectedEngineId);
    content.SuspendLayout();content.Controls.Clear();content.AutoScrollMinSize=Size.Empty;content.AutoScrollPosition=Point.Empty;page.Location=new Point(content.Padding.Left,content.Padding.Top);content.Controls.Add(page);page.Visible=true;content.ResumeLayout(true);if(IsHandleCreated)FitPage(page);
   }finally{showing=false;}
  }
  private void FitPage(Control page){
   page.PerformLayout();Size preferred=page.GetPreferredSize(Size.Empty);int wantedWidth=Px(174)+Math.Max(page.Width,preferred.Width)+content.Padding.Horizontal+Px(24);int wantedHeight=Math.Max(page.Height,preferred.Height)+content.Padding.Vertical+footer.Height+(engineRow.Visible?engineRow.Height:0);var work=Screen.FromControl(this).WorkingArea;
   ClientSize=new Size(Math.Min(Math.Max(ClientSize.Width,wantedWidth),work.Width-(Width-ClientSize.Width)-Px(24)),Math.Min(Math.Max(ClientSize.Height,wantedHeight),work.Height-(Height-ClientSize.Height)-Px(24)));layout.PerformLayout();content.AutoScrollPosition=Point.Empty;page.Location=new Point(content.Padding.Left,content.Padding.Top);Location=new Point(Math.Max(work.Left,Math.Min(Left,work.Right-Width)),Math.Max(work.Top,Math.Min(Top,work.Bottom-Height)));
  }
 }
}
