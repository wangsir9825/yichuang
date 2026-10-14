using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace YiChuang
{
    public sealed class SentenceSpan
    {
        public int Start { get; private set; }
        public int Length { get; private set; }
        public int End { get { return Start + Length; } }
        public SentenceSpan(int start, int length) { Start = start; Length = length; }
    }

    public sealed class SentencePair
    {
        public SentenceSpan Source { get; private set; }
        public SentenceSpan Target { get; private set; }
        public SentencePair(SentenceSpan source, SentenceSpan target) { Source = source; Target = target; }
    }

    // Services return plain text, so alignment is local and monotonic. Grouping supports
    // split/merged sentences; it cannot infer arbitrary semantic reordering.
    public static class SentenceAlignment
    {
        private static readonly Regex Abbreviation = new Regex(@"(?:\b(?:Mr|Mrs|Ms|Dr|Prof|Sr|Jr|St|vs|etc|Fig|Eq|No|Vol)|\b(?:[A-Za-z]\.)+[A-Za-z])\.$", RegexOptions.IgnoreCase);
        public static List<SentenceSpan> Split(string text)
        {
            var spans = new List<SentenceSpan>();
            text = text ?? "";
            int start = 0;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool boundary = c == '。' || c == '！' || c == '？' || c == '!' || c == '?';
                if (c == '.')
                {
                    bool decimalPoint = i > 0 && i + 1 < text.Length && char.IsDigit(text[i-1]) && char.IsDigit(text[i+1]);
                    bool insideWord = i + 1 < text.Length && char.IsLetterOrDigit(text[i+1]);
                    string prefix = text.Substring(Math.Max(start, i-18), Math.Min(i-start+1,19));
                    bool initial = i > start && char.IsLetter(text[i-1]) && (i-1 == start || char.IsWhiteSpace(text[i-2]));
                    boundary = !decimalPoint && !insideWord && !initial && !Abbreviation.IsMatch(prefix);
                }
                // An empty paragraph is a boundary; single OCR line breaks are not.
                if (c == '\n') {
                    int k=i+1; while(k<text.Length && (text[k]=='\r' || text[k]==' ' || text[k]=='\t')) k++;
                    if(k<text.Length && text[k]=='\n') boundary=true;
                }
                if (!boundary) continue;
                int end = i + 1;
                while (end < text.Length && "。！？.!?\"'”’）)]】」』".IndexOf(text[end]) >= 0) end++;
                AddSpan(spans, text, start, end); start = end; i = end-1;
            }
            AddSpan(spans, text, start, text.Length);
            return spans;
        }
        private static void AddSpan(List<SentenceSpan> spans, string text, int start, int end)
        {
            while (start < end && char.IsWhiteSpace(text[start])) start++;
            while (end > start && char.IsWhiteSpace(text[end-1])) end--;
            if (end > start) spans.Add(new SentenceSpan(start,end-start));
        }
        private static SentenceSpan Join(List<SentenceSpan> spans, int start, int count)
        { return new SentenceSpan(spans[start].Start, spans[start+count-1].End-spans[start].Start); }
        private static double[] Weights(string text, List<SentenceSpan> spans)
        {
            var result = new double[spans.Count+1];
            for (int i=0;i<spans.Count;i++)
            {
                double weight=0;
                for(int k=spans[i].Start;k<spans[i].End;k++)
                { char c=text[k]; if(char.IsLetterOrDigit(c)) weight += c >= 0x2e80 ? 1 : 0.22; }
                result[i+1]=result[i]+Math.Max(1,weight);
            }
            double total=result[spans.Count];
            if(total>0) for(int i=1;i<result.Length;i++) result[i]/=total;
            return result;
        }
        public static List<SentencePair> Align(string source, string target)
        {
            var a=Split(source); var b=Split(target); var pairs=new List<SentencePair>();
            int n=a.Count,m=b.Count;
            if(n==0 || m==0) return pairs;
            if(n==m) { for(int i=0;i<n;i++) pairs.Add(new SentencePair(a[i],b[i])); return pairs; }
            if(n==1 || m==1) { pairs.Add(new SentencePair(Join(a,0,n),Join(b,0,m))); return pairs; }
            // Bound work on book-length input; use proportional monotonic groups there.
            if(n>300 || m>300 || n>m*3 || m>n*3)
            {
                int groups=Math.Min(n,m);
                for(int i=0;i<groups;i++) {
                    int x=i*n/groups,y=i*m/groups;
                    pairs.Add(new SentencePair(Join(a,x,(i+1)*n/groups-x),Join(b,y,(i+1)*m/groups-y)));
                }
                return pairs;
            }
            var aw=Weights(source,a); var bw=Weights(target,b);
            var cost=new double[n+1,m+1]; var prevA=new int[n+1,m+1]; var prevB=new int[n+1,m+1];
            for(int i=0;i<=n;i++) for(int j=0;j<=m;j++) cost[i,j]=double.PositiveInfinity;
            cost[0,0]=0;
            for(int i=0;i<n;i++) for(int j=0;j<m;j++)
            {
                if(double.IsInfinity(cost[i,j])) continue;
                for(int x=1;x<=3 && i+x<=n;x++) for(int y=1;y<=3 && j+y<=m;y++)
                {
                    if(x>1 && y>1) continue;
                    double length=Math.Abs(Math.Log((aw[i+x]-aw[i])/(bw[j+y]-bw[j])));
                    double value=cost[i,j]+length*length+0.45*(x+y-2)+2*Math.Abs(aw[i+x]-bw[j+y]);
                    if(value<cost[i+x,j+y]) {cost[i+x,j+y]=value;prevA[i+x,j+y]=x;prevB[i+x,j+y]=y;}
                }
            }
            for(int i=n,j=m;i>0 && j>0;) {
                int x=prevA[i,j],y=prevB[i,j];
                if(x==0 || y==0) { pairs.Clear();pairs.Add(new SentencePair(Join(a,0,n),Join(b,0,m)));return pairs; }
                i-=x;j-=y;pairs.Add(new SentencePair(Join(a,i,x),Join(b,j,y)));
            }
            pairs.Reverse(); return pairs;
        }
    }

    // Paint-only highlight: never changes RTF, selection, focus, undo, or TextChanged.
    public class HoverRichTextBox : RichTextBoxEx
    {
        public int HoverStart { get; private set; } = -1;
        public int HoverLength { get; private set; }
        private object hoverOwner;
        private NativeTextGeometry geometry;
        protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);geometry=new NativeTextGeometry(Handle);}
        protected override void OnHandleDestroyed(EventArgs e){if(geometry!=null){geometry.Dispose();geometry=null;}base.OnHandleDestroyed(e);}
        public void SetHoverRange(int start,int length)
        { SetOwnedHoverRange(null,start,length); }
        public void SetOwnedHoverRange(object owner,int start,int length)
        {
            if(start<0 || length<=0 || start>=TextLength) {start=-1;length=0;}
            else length=Math.Min(length,TextLength-start);
            hoverOwner=start<0 ? null : owner;
            if(HoverStart==start && HoverLength==length) return;
            HoverStart=start;HoverLength=length;Invalidate();
        }
        public void ClearHover() { SetHoverRange(-1,0); }
        public void ClearOwnedHover(object owner) {if(ReferenceEquals(hoverOwner,owner)) ClearHover();}
        private Rectangle RowBounds(int start,int end)
        {
            while(end>start && (Text[end-1]=='\n' || Text[end-1]=='\r')) end--;
            if(end<=start) return Rectangle.Empty;
            Rectangle native;
            if(geometry!=null&&geometry.Bounds(start,end,out native))return native;
            Point left=GetPositionFromCharIndex(start),last=GetPositionFromCharIndex(end-1),next=GetPositionFromCharIndex(end);
            int width=next.Y==last.Y && next.X>last.X ? next.X-last.X :
                TextRenderer.MeasureText(Text.Substring(end-1,1),Font,Size.Empty,TextFormatFlags.NoPadding|TextFormatFlags.NoPrefix).Width;
            using(var g=CreateGraphics())return new Rectangle(left.X,left.Y,Math.Max(1,last.X+width-left.X),(int)Math.Ceiling(Font.GetHeight(g)*ZoomFactor));
        }
        public int HitText(Point point)
        {
            if(TextLength==0 || !ClientRectangle.Contains(point)) return -1;
            int index=GetCharIndexFromPosition(point),line=GetLineFromCharIndex(index);
            int start=GetFirstCharIndexFromLine(line),end=GetFirstCharIndexFromLine(line+1);
            if(start<0) return -1;
            if(end<0) end=TextLength;
            return RowBounds(start,end).Contains(point) ? index : -1;
        }
        protected override void OnTextChanged(EventArgs e) { ClearHover();base.OnTextChanged(e); }
        protected override void OnVScroll(EventArgs e) { base.OnVScroll(e);Invalidate(); }
        protected override void OnHScroll(EventArgs e) { base.OnHScroll(e);Invalidate(); }
        protected override void WndProc(ref Message m)
        {
            // Repaint the complete native text before blending to avoid tint accumulation
            // when RichEdit invalidates a small caret/selection region.
            if(m.Msg==0x0f && HoverStart>=0) Invalidate();
            base.WndProc(ref m);
            if(HoverStart<0 || IsDisposed) return;
            if(m.Msg==0x0f) {using(var g=Graphics.FromHwnd(Handle)) PaintHover(g);}
            else if(m.Msg==0x318 && m.WParam!=IntPtr.Zero) {using(var g=Graphics.FromHdc(m.WParam)) PaintHover(g);}
        }
        private void PaintHover(Graphics g)
        {
            int last=HoverStart+HoverLength;
            int firstVisible=GetLineFromCharIndex(GetCharIndexFromPosition(new Point(1,1)));
            int line=Math.Max(firstVisible,GetLineFromCharIndex(HoverStart));
            {
                for(;line<LineCount;line++)
                {
                    int first=GetFirstCharIndexFromLine(line);
                    if(first<0 || first>=last) break;
                    Point position=GetPositionFromCharIndex(first);
                    if(position.Y>=ClientSize.Height) break;
                    int next=GetFirstCharIndexFromLine(line+1);if(next<0) next=TextLength;
                    Rectangle bounds=RowBounds(Math.Max(first,HoverStart),Math.Min(next,last));
                    bounds.Intersect(ClientRectangle);
                    if(!bounds.IsEmpty) TintBackground(g,bounds);
                }
            }
        }
        [DllImport("gdi32.dll")] private static extern IntPtr CreateSolidBrush(int color);
        [DllImport("gdi32.dll")] private static extern IntPtr SelectObject(IntPtr dc,IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool DeleteObject(IntPtr obj);
        [DllImport("gdi32.dll")] private static extern bool PatBlt(IntPtr dc,int x,int y,int width,int height,uint operation);
        private static void TintBackground(Graphics graphics,Rectangle bounds) {
            // PATAND colors the white paper while preserving dark glyphs and antialiasing.
            IntPtr brush=CreateSolidBrush(ColorTranslator.ToWin32(Color.FromArgb(255,241,163))),dc=graphics.GetHdc();
            IntPtr old=SelectObject(dc,brush);
            try{PatBlt(dc,bounds.X,bounds.Y,bounds.Width,bounds.Height,0x00A000C9);}
            finally{SelectObject(dc,old);graphics.ReleaseHdc(dc);DeleteObject(brush);}
        }
        private int LineCount { get { return TextLength==0 ? 0 : GetLineFromCharIndex(TextLength-1)+1; } }
    }

    public sealed class SentenceHoverController : IDisposable
    {
        private readonly HoverRichTextBox source,target;
        private List<SentencePair> pairs=new List<SentencePair>();
        private List<SentenceSpan> sourceSentences=new List<SentenceSpan>();
        private bool ready;
        private SentencePair active;
        private SentenceSpan activeSource;
        public bool IsSourceHover { get; private set; }
        public event EventHandler HoverActivated;
        public SentenceHoverController(HoverRichTextBox source,HoverRichTextBox target)
        {
            this.source=source;this.target=target;
            source.TextChanged+=Changed;target.TextChanged+=Changed;
            target.MouseMove+=Moved;target.MouseLeave+=TargetLeft;target.MouseDown+=Left;
            target.VScroll+=Scrolled;target.HScroll+=Scrolled;
            source.MouseMove+=SourceMoved;source.MouseLeave+=SourceLeft;source.MouseDown+=Left;
            source.VScroll+=Scrolled;source.HScroll+=Scrolled;
        }
        public void SetResultReady(bool value)
        {
            Reset();ready=value;
            if(ready) {pairs=SentenceAlignment.Align(source.Text,target.Text);sourceSentences=SentenceAlignment.Split(source.Text);}
        }
        private void Changed(object sender,EventArgs e) {Reset();}
        private void Left(object sender,EventArgs e) {ClearHighlight();}
        private void SourceLeft(object sender,EventArgs e) {if(IsSourceHover) ClearHighlight();}
        private void TargetLeft(object sender,EventArgs e) {if(!IsSourceHover) ClearHighlight();}
        private void Scrolled(object sender,EventArgs e) {ClearHighlight();}
        private void Moved(object sender,MouseEventArgs e) {HoverAt(e.Location,e.Button!=MouseButtons.None);}
        private void SourceMoved(object sender,MouseEventArgs e) {HoverSourceAt(e.Location,e.Button!=MouseButtons.None);}
        public void HoverAt(Point point,bool selecting=false)
        {
            int index=ready && !selecting ? target.HitText(point) : -1;
            SentencePair found=null;
            foreach(var pair in pairs) if(index>=pair.Target.Start && index<pair.Target.End) {found=pair;break;}
            if(found==null) {ClearHighlight();return;}
            Activate(found,found.Source,false);
        }
        public void HoverSourceAt(Point point,bool selecting=false)
        {
            int index=ready && !selecting ? source.HitText(point) : -1;
            SentencePair found=null;SentenceSpan sentence=null;
            foreach(var span in sourceSentences) if(index>=span.Start && index<span.End) {sentence=span;break;}
            if(sentence!=null) foreach(var pair in pairs) if(index>=pair.Source.Start && index<pair.Source.End) {found=pair;break;}
            if(found==null) {ClearHighlight();return;}
            // Keep the hovered original sentence identical across comparison models,
            // even when their translations merge different adjacent sentences.
            Activate(found,sentence,true);
        }
        private void Activate(SentencePair found,SentenceSpan sourceSpan,bool fromSource)
        {
            if(active==found && activeSource.Start==sourceSpan.Start && IsSourceHover==fromSource) return;
            IsSourceHover=fromSource;
            if(HoverActivated!=null) HoverActivated(this,EventArgs.Empty);
            active=found;activeSource=sourceSpan;
            source.SetOwnedHoverRange(this,sourceSpan.Start,sourceSpan.Length);target.SetHoverRange(found.Target.Start,found.Target.Length);
        }
        public void ClearHighlight() {source.ClearOwnedHover(this);active=null;activeSource=null;IsSourceHover=false;target.ClearHover();}
        public void Reset() {ClearHighlight();ready=false;pairs.Clear();sourceSentences.Clear();}
        public void Dispose()
        {
            Reset();source.TextChanged-=Changed;target.TextChanged-=Changed;
            target.MouseMove-=Moved;target.MouseLeave-=TargetLeft;target.MouseDown-=Left;
            target.VScroll-=Scrolled;target.HScroll-=Scrolled;
            source.MouseMove-=SourceMoved;source.MouseLeave-=SourceLeft;source.MouseDown-=Left;
            source.VScroll-=Scrolled;source.HScroll-=Scrolled;
        }
    }
}
