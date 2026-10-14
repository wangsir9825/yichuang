using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
namespace Xtranslator {
 internal static class UiPreferences {
  internal static string Layout="wide", TranslateShortcut="F5";
  internal static int WideWidth=1000,WideHeight=660,CompactWidth=460,CompactHeight=720;
  internal static double WideSplit=.5,CompactSplit=.46;
  static int Number(JObject v,string k,int fallback,int min,int max){int n;return int.TryParse((string)v[k],out n)?Math.Max(min,Math.Min(max,n)):fallback;}
  static double Ratio(JObject v,string k,double fallback){double n;return double.TryParse((string)v[k],NumberStyles.Float,CultureInfo.InvariantCulture,out n)&&!double.IsNaN(n)?Math.Max(.2,Math.Min(.8,n)):fallback;}
  internal static void Read(JObject v){
   TranslationDirection.Mode=TranslationDirection.Normalize((string)v["TranslationDirection"]);
   Layout=(string)v["UiLayout"]=="compact"?"compact":"wide";
   WideWidth=Number(v,"WideWidth",1000,760,3200);WideHeight=Number(v,"WideHeight",660,420,2000);
   CompactWidth=Number(v,"CompactWidth",460,330,1600);CompactHeight=Number(v,"CompactHeight",720,420,2000);
   WideSplit=Ratio(v,"WideSplit",.5);CompactSplit=Ratio(v,"CompactSplit",.46);
   TranslateShortcut=ClassSetHotKey.Normalize((string)v["TranslateShortcut"],"F5");
  }
  internal static string ReadShowShortcut(JObject v){
   string saved=(string)v["ShortcutkeyHiddenDisplay"];
   if(v["ShortcutSchema"]==null&&string.Equals(saved,"F3",StringComparison.OrdinalIgnoreCase))saved="F6";
   return ClassSetHotKey.Normalize(saved,"F6");
  }
  internal static void Write(Dictionary<string,string> v){
   v["TranslationDirection"]=TranslationDirection.Normalize(TranslationDirection.Mode);
   v["ShortcutSchema"]="2";v["TranslateShortcut"]=TranslateShortcut;v["UiLayout"]=Layout;
   v["WideWidth"]=WideWidth.ToString();v["WideHeight"]=WideHeight.ToString();v["CompactWidth"]=CompactWidth.ToString();v["CompactHeight"]=CompactHeight.ToString();
   v["WideSplit"]=WideSplit.ToString(CultureInfo.InvariantCulture);v["CompactSplit"]=CompactSplit.ToString(CultureInfo.InvariantCulture);
  }
 }
}
