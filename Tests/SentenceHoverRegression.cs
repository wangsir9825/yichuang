using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

class SentenceHoverRegression
{
    static string Data;
    static int Requests;
    static BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static List<string> Log=new List<string>();
    public static string Folder(Environment.SpecialFolder folder) {return Data;}
    class Guard:IWebRequestCreate {public WebRequest Create(Uri uri) {Requests++;throw new Exception("Unexpected request in hover test");}}
    static object Call(object obj,string method,params object[] values) {return obj.GetType().GetMethod(method).Invoke(obj,values);}
    static object Field(object obj,string name) {return obj.GetType().GetField(name,Private).GetValue(obj);}
    static int Number(object obj,string name) {return (int)obj.GetType().GetProperty(name).GetValue(obj,null);}
    static void Assert(bool ok,string message) {if(!ok) throw new Exception(message);}
    static Point At(RichTextBox view,int index) {Point p=view.GetPositionFromCharIndex(index);return new Point(p.X+2,p.Y+view.Font.Height/2);}
    static void Hover(object controller,RichTextBox view,int index) {Call(controller,"HoverAt",At(view,index),false);Application.DoEvents();}
    static void Move(RichTextBox view,int index) {
        Point p=At(view,index);
        typeof(Control).GetMethod("OnMouseMove",Private).Invoke(view,new object[]{new MouseEventArgs(MouseButtons.None,0,p.X,p.Y,0)});
        Application.DoEvents();
    }
    static void Render(Form form,string path) {using(var b=new Bitmap(form.Width,form.Height)){form.DrawToBitmap(b,new Rectangle(0,0,b.Width,b.Height));b.Save(path);}}
    static void Leave(Control view) {typeof(Control).GetMethod("OnMouseLeave",Private).Invoke(view,new object[]{EventArgs.Empty});Application.DoEvents();}
    [STAThread]
    static int Main(string[] args)
    {
        string output=Path.GetFullPath(args[1]);Directory.CreateDirectory(output);
        using(var watchdog=new System.Threading.Timer(s=>Environment.Exit(124),null,35000,Timeout.Infinite))
        try
        {
            string exe=Path.GetFullPath(args[0]);Directory.SetCurrentDirectory(Path.GetDirectoryName(exe));
            AppDomain.CurrentDomain.AssemblyResolve+=(s,e)=>{string f=Path.Combine(Path.GetDirectoryName(exe),new AssemblyName(e.Name).Name+".dll");return File.Exists(f)?Assembly.LoadFrom(f):null;};
            var app=Assembly.LoadFrom(exe);var alignment=app.GetType("YiChuang.SentenceAlignment");
            var controllerType=app.GetType("YiChuang.SentenceHoverController");var viewType=app.GetType("YiChuang.HoverRichTextBox");
            Func<string,IList> split=s=>(IList)alignment.GetMethod("Split").Invoke(null,new object[]{s});
            string en="Dr. Smith measured 3.14 units, e.g. in Fig. 2. The result is stable! Is it useful?";
            var parts=split(en);Assert(parts.Count==3,"Abbreviation/decimal split incorrect");
            Assert(en.Substring(Number(parts[0],"Start"),Number(parts[0],"Length")).EndsWith("Fig. 2."),"First English sentence range incorrect");
            Assert(split("他说：“这是第一句！”这是第二句。还有第三句？").Count==3,"Chinese punctuation/quotes split incorrect");
            Assert(split("OCR text wraps\r\non one line.\r\n\r\nAnother paragraph").Count==2,"OCR line break incorrectly splits a sentence");
            Assert(split("First paragraph\r\n\r\nSecond paragraph").Count==2,"Empty paragraph boundary missing");
            var groups=(IList)alignment.GetMethod("Align").Invoke(null,new object[]{"The sun is bright. The water is cold and the wind is strong.","阳光明亮。水很冷。风很大。"});
            Assert(groups.Count==2,"Split sentence should form two aligned groups");
            var last=groups[1];var targetSpan=last.GetType().GetProperty("Target").GetValue(last,null);
            Assert(Number(targetSpan,"Length")=="水很冷。风很大。".Length,"Split sentences not grouped together");
            Log.Add("English abbreviations/decimal, Chinese quotes, OCR line wraps/paragraphs and split/merge ranges: passed");

            Application.EnableVisualStyles();
            using(var form=new Form {Text="Sentence hover component verification",ClientSize=new Size(920,350)})
            using(var source=(RichTextBox)Activator.CreateInstance(viewType))
            using(var target=(RichTextBox)Activator.CreateInstance(viewType))
            {
                source.SetBounds(10,10,430,325);target.SetBounds(455,10,450,325);
                source.Font=target.Font=new Font("Microsoft YaHei UI",13);
                source.Text="The first sentence is short. This second sentence is long enough to wrap across several lines in the original pane without changing its sentence correspondence. The final sentence is simple.";
                target.Text="第一句很短。第二句比较长，在译文区域换成多行显示，仍然应该对应同一句原文，而且悬停其中任意一行都会同时高亮。最后一句很简单。";
                target.ReadOnly=true;form.Controls.Add(source);form.Controls.Add(target);form.Show();Application.DoEvents();
                object controller=Activator.CreateInstance(controllerType,new object[]{source,target});Call(controller,"SetResultReady",true);
                source.Select(4,8);target.Select(1,3);
                string sRtf=source.Rtf,tRtf=target.Rtf;int changed=0;source.TextChanged+=(s,e)=>changed++;target.TextChanged+=(s,e)=>changed++;
                int index=target.Text.IndexOf("任意");Hover(controller,target,index);
                Assert(Number(source,"HoverStart")==source.Text.IndexOf("This"),"Hovered wrapped sentence highlights wrong source");
                Assert(Number(target,"HoverStart")==target.Text.IndexOf("第二"),"Hovered wrapped sentence highlights wrong target");
                for(int i=0;i<8;i++){source.Invalidate();target.Invalidate();Application.DoEvents();}
                Render(form,Path.Combine(output,"paired-hover.png"));
                Assert(source.Rtf==sRtf&&target.Rtf==tRtf&&source.SelectionStart==4&&source.SelectionLength==8&&target.SelectionStart==1&&target.SelectionLength==3&&changed==0,"Hover changes RTF, selection, or text events");
                Leave(target);Assert(Number(source,"HoverStart")==-1&&Number(target,"HoverStart")==-1,"MouseLeave did not clear both highlights");
                Render(form,Path.Combine(output,"paired-leave.png"));
                Move(source,source.Text.IndexOf("without"));
                Assert(Number(source,"HoverStart")==source.Text.IndexOf("This")&&Number(target,"HoverStart")==target.Text.IndexOf("第二"),"Source MouseMove did not highlight corresponding wrapped sentence");
                Render(form,Path.Combine(output,"source-hover.png"));
                Assert(source.Rtf==sRtf&&target.Rtf==tRtf&&source.SelectionStart==4&&source.SelectionLength==8&&target.SelectionStart==1&&target.SelectionLength==3&&changed==0,"Source hover changes RTF, selection, or events");
                Call(controller,"HoverSourceAt",At(source,source.Text.IndexOf("without")),true);
                Assert(Number(source,"HoverStart")==-1&&Number(target,"HoverStart")==-1,"Source drag did not suspend both highlights");
                Move(source,source.Text.IndexOf("without"));Leave(source);
                Assert(Number(source,"HoverStart")==-1&&Number(target,"HoverStart")==-1,"Source MouseLeave did not clear both");
                Call(controller,"HoverSourceAt",new Point(source.ClientSize.Width-8,source.ClientSize.Height-8),false);
                Assert(Number(source,"HoverStart")==-1&&Number(target,"HoverStart")==-1,"Blank source area snaps to final sentence");
                Call(controller,"HoverAt",new Point(target.ClientSize.Width-8,target.ClientSize.Height-8),false);
                Assert(Number(target,"HoverStart")==-1,"Empty area snaps to final sentence");
                Hover(controller,target,index);Call(controller,"HoverAt",At(target,index),true);
                Assert(Number(target,"HoverStart")==-1,"Text selection drag did not suspend hover");
                Hover(controller,target,index);target.AppendText("新译文。");
                Hover(controller,target,index);Assert(Number(target,"HoverStart")==-1,"Changed result retains stale mapping");
                Call(controller,"SetResultReady",true);Hover(controller,target,index);source.AppendText(" New source.");
                Assert(Number(source,"HoverStart")==-1&&Number(target,"HoverStart")==-1,"Source update retains stale highlight");
                Call(controller,"Dispose");form.Close();
            }
            Log.Add("Wrapped-row hover + mouse leave/blank area/selection drag, unchanged text/RTF/selection/events and stale-source/result reset: passed");

            Data=Path.Combine(output,"isolated-data-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(Data);
            var proxies=(object[])app.GetType("_63AF42A1").GetField("_71AC5D16").GetValue(null);
            proxies[318]=Delegate.CreateDelegate(proxies[318].GetType(),typeof(SentenceHoverRegression).GetMethod("Folder"));
            app.GetType("YiChuang.Program").GetField("ProgramStarted").SetValue(null,new EventWaitHandle(false,EventResetMode.AutoReset));
            app.GetType("RebuildInfo").GetField("DiagnosticOutput",BindingFlags.Static|BindingFlags.NonPublic).SetValue(null,output);
            WebRequest.RegisterPrefix("http://",new Guard());WebRequest.RegisterPrefix("https://",new Guard());
            using(var main=(Form)Activator.CreateInstance(app.GetType("YiChuang.Form2")))
            {
                main.Opacity=0;main.Show();Application.DoEvents();
                main.GetType().GetField("_isInitializing",Private).SetValue(main,true);
                var source=(RichTextBox)Field(main,"rtb原文");var target=(RichTextBox)Field(main,"rtb译文");var controller=Field(main,"sentenceHover");
                source.Text="Hello there. The sun is bright.";
                main.GetType().GetMethod("YiStr",Private).Invoke(main,new object[]{"你好。阳光明亮。"});Application.DoEvents();
                Hover(controller,target,target.Text.IndexOf("阳光"));Assert(Number(source,"HoverStart")==13,"Main completed translation not aligned");
                Leave(target);Move(source,source.Text.IndexOf("sun"));
                Assert(Number(source,"HoverStart")==13&&Number(target,"HoverStart")==3,"Main source hover not aligned");
                main.GetType().GetMethod("ShowTranslationStatus",Private).Invoke(main,new object[]{"本地模型正在翻译…"});
                Hover(controller,target,0);Assert(Number(target,"HoverStart")==-1,"Loading status is highlighted as a translation");
                main.GetType().GetMethod("YiStr",Private).Invoke(main,new object[]{"翻译失败：请求失败。"});Hover(controller,target,0);
                Assert(Number(target,"HoverStart")==-1,"Error text is highlighted as a translation");
                main.GetType().GetMethod("YiStrChatgpt",Private).Invoke(main,new object[]{"partial sentence"});Hover(controller,target,0);
                Assert(Number(target,"HoverStart")==-1,"Incomplete stream is aligned");main.Close();
            }
            Assert(Requests==0,"Hover triggered a translation/network call");
            Log.Add("Main completed/result-status/error/stream lifecycle and bidirectional hover; zero network calls: passed");
            Log.Add("Bidirectional source MouseMove/MouseLeave/blank/drag; source and target keep exact hovered sentence and clear stale ranges: passed");
            File.WriteAllLines(Path.Combine(output,"sentence-hover-regression.txt"),Log);return 0;
        }
        catch(Exception ex){Log.Add(ex.ToString());File.WriteAllLines(Path.Combine(output,"sentence-hover-regression.txt"),Log);Console.Error.WriteLine(ex);return 1;}
    }
}
