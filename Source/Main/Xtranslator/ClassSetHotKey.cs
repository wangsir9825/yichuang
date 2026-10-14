using System;
using System.Windows.Forms;
namespace Xtranslator {
 internal class ClassSetHotKey {
  internal const int ShowId=100,TranslateId=105;
  internal static bool TranslateEnabled,Suspended;
  internal static string LastError="";
  public static object[] ParseShortcut(string text,string separator){
   if(string.IsNullOrWhiteSpace(text))throw new ArgumentException("请按下一个快捷键。");
   var parts=text.Split(new[]{'+'},StringSplitOptions.RemoveEmptyEntries);
   HotKey.KeyModifiers mods=HotKey.KeyModifiers.None;
   for(int i=0;i<parts.Length-1;i++){switch(parts[i].Trim().ToLowerInvariant()){
    case "ctrl":case "control":mods|=HotKey.KeyModifiers.Ctrl;break;
    case "alt":mods|=HotKey.KeyModifiers.Alt;break;case "shift":mods|=HotKey.KeyModifiers.Shift;break;
    default:throw new ArgumentException("支持 Ctrl、Alt、Shift 与按键的组合。");}}
   Keys key=(Keys)new KeysConverter().ConvertFromString(parts[parts.Length-1].Trim());
   if(key==Keys.None||key==Keys.ControlKey||key==Keys.ShiftKey||key==Keys.Menu||key==Keys.LWin||key==Keys.RWin||(int)key>255)throw new ArgumentException("请使用功能键，或 Ctrl / Alt / Shift 与其他按键的组合。");
   if((mods==HotKey.KeyModifiers.None||mods==HotKey.KeyModifiers.Shift)&&(key<Keys.F1||key>Keys.F24))throw new ArgumentException("单独按键请使用 F1–F24；字母与数字请搭配 Ctrl 或 Alt。");
   if((mods==HotKey.KeyModifiers.Ctrl&&(key==Keys.C||key==Keys.V||key==Keys.X||key==Keys.A||key==Keys.Z))||(mods==HotKey.KeyModifiers.Alt&&(key==Keys.F4||key==Keys.Tab))||(mods==(HotKey.KeyModifiers.Ctrl|HotKey.KeyModifiers.Alt)&&key==Keys.Delete))throw new ArgumentException("这个组合用于编辑或系统操作，请选择其他快捷键。");
   return new object[]{mods,key};
  }
  static string Format(object[] p){var m=(HotKey.KeyModifiers)p[0];return ((m&HotKey.KeyModifiers.Ctrl)!=0?"Ctrl + ":"")+((m&HotKey.KeyModifiers.Alt)!=0?"Alt + ":"")+((m&HotKey.KeyModifiers.Shift)!=0?"Shift + ":"")+(Keys)p[1];}
  internal static string Normalize(string text,string fallback){try{return Format(ParseShortcut(text,"+"));}catch{return fallback;}}
  static bool Register(int id,string text){var p=ParseShortcut(text,"+");return HotKey.RegisterHotKey(ClassMain.FromMainHandle,id,(HotKey.KeyModifiers)((int)(HotKey.KeyModifiers)p[0]|0x4000),(Keys)p[1]);}
  internal static void Suspend(){Suspended=true;xiezaimoren();}
  internal static void Resume(){Suspended=false;sethotkey();}
  internal static void SetTranslateEnabled(bool enabled){TranslateEnabled=enabled;if(!Suspended)sethotkey();}
  internal static bool TryApply(string display,string translate,out string error){
   error="";try{display=Format(ParseShortcut(display,"+"));translate=Format(ParseShortcut(translate,"+"));if(display==translate)throw new ArgumentException("翻译与最小化不能使用同一个快捷键。");}catch(Exception ex){error=ex.Message;return false;}
   if(ClassMain.FromMainHandle==IntPtr.Zero){error="主窗口尚未就绪，请稍后设置快捷键。";return false;}
   xiezaimoren();bool show=Register(ShowId,display),capture=show&&Register(TranslateId,translate);
   if(!show||!capture){error=(show?translate:display)+" 已被其他程序占用，请换一个组合。";xiezaimoren();if(!Suspended)sethotkey();return false;}
   ClassMain.快捷键隐藏显示=display;UiPreferences.TranslateShortcut=translate;LastError="";
   if(Suspended)xiezaimoren();else if(!TranslateEnabled)HotKey.UnregisterHotKey(ClassMain.FromMainHandle,TranslateId);
   return true;
  }
  public static void sethotkey(){
   if(Suspended||ClassMain.FromMainHandle==IntPtr.Zero)return;
   xiezaimoren();LastError="";ClassMain.快捷键隐藏显示=Normalize(ClassMain.快捷键隐藏显示,"F6");UiPreferences.TranslateShortcut=Normalize(UiPreferences.TranslateShortcut,"F5");
   if(!Register(ShowId,ClassMain.快捷键隐藏显示))LastError=ClassMain.快捷键隐藏显示+" 已被占用，请在设置→快捷键修改。";
   if(TranslateEnabled&&!Register(TranslateId,UiPreferences.TranslateShortcut))LastError=UiPreferences.TranslateShortcut+" 已被占用，请在设置→快捷键修改。";
  }
  public static void setmorenhoykey(){ClassMain.快捷键隐藏显示="F6";UiPreferences.TranslateShortcut="F5";sethotkey();}
  public static void xiezaimoren(){if(ClassMain.FromMainHandle==IntPtr.Zero)return;HotKey.UnregisterHotKey(ClassMain.FromMainHandle,ShowId);HotKey.UnregisterHotKey(ClassMain.FromMainHandle,TranslateId);}
 }
}
