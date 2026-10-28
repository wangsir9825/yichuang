using System;
using System.Diagnostics;
using System.Globalization;

namespace YiChuang
{
    public partial class Form2
    {
        private Stopwatch translationClock;
        private int timedRevision;
        private string timingText="";

        private void BeginTranslationTiming(int revision)
        {
            timedRevision=revision;
            translationClock=Stopwatch.StartNew();
            timingText="正在翻译…";
            RefreshModernStatus();
        }
        private void ResetTranslationTiming()
        {
            translationClock=null;timingText="";RefreshModernStatus();
        }
        private void FinishTranslationTiming(int revision,string outcome)
        {
            if(IsDisposed||Disposing||!IsHandleCreated)return;
            if(InvokeRequired){try{BeginInvoke(new Action(()=>FinishTranslationTiming(revision,outcome)));}catch(InvalidOperationException){}return;}
            // A canceled/replaced request can never change the current request's result or clock.
            if(revision!=localRevision||revision!=timedRevision||translationClock==null||!translationClock.IsRunning)return;
            translationClock.Stop();
            timingText=(outcome=="成功"?"翻译耗时":outcome+" · 耗时")+"："+translationClock.Elapsed.TotalSeconds.ToString("F2",CultureInfo.CurrentCulture)+" 秒";
            RefreshModernStatus();
        }
    }
}
