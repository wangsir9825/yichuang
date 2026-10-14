using System;
using System.Threading;
using System.Windows.Forms;
using System.Linq;

namespace YiChuang
{
    public partial class Form2
    {
        private void RebuildEngineMenu()
        {
            foreach(ToolStripItem item in toolStripDropDownButton1.DropDownItems.Cast<ToolStripItem>().ToArray())if(item.Name.StartsWith("CustomEngine")||item.Name=="NoConfiguredEngines")item.Dispose();
            toolStripDropDownButton1.DropDownItems.Clear();
            foreach(int id in EngineRegistry.VisibleIds) {
                ToolStripMenuItem item;
                switch(id) {
                    case 4:item=Blind翻译ToolStripMenuItem;break;case 5:item=搜狗翻译引擎ToolStripMenuItem;break;
                    case 6:item=deepl翻译引擎ToolStripMenuItem;break;case 14:item=eDeepseek官方AI翻译ToolStripMenuItem;break;
                    case 16:item=localHunyuanMenu;break;case 17:item=localGemmaMenu;break;
                    case 18:item=geminiMenu;break;case 19:item=qwenMenu;break;
                    default:int selected=id;item=new ToolStripMenuItem{Name="CustomEngine"+id};item.Click+=(s,e)=>SelectManagedEngine(selected);break;
                }
                item.Tag=id;item.Text=EngineLabels.MenuText(id);item.Checked=ClassMain.isjiekounum==id;item.Image=null;toolStripDropDownButton1.DropDownItems.Add(item);
            }
            if(toolStripDropDownButton1.DropDownItems.Count==0)toolStripDropDownButton1.DropDownItems.Add(new ToolStripMenuItem("请在设置中勾选翻译引擎"){Name="NoConfiguredEngines",Enabled=false});
            RefreshModernToolbar();
        }
        private void SelectManagedEngine(int engine)
        {
            if(!NoLoginBuild.IsAllowedEngine(engine))return;
            bool changed=ClassMain.isjiekounum!=engine;ClassMain.isjiekounum=engine;
            Interlocked.Increment(ref localRevision);localCancelButton.Enabled=false;sentenceHover?.Reset();
            foreach(ToolStripItem item in toolStripDropDownButton1.DropDownItems){var menu=item as ToolStripMenuItem;if(menu!=null)menu.Checked=menu.Tag is int&&(int)menu.Tag==engine;}
            toolStripDropDownButton1.Image=null;toolStripDropDownButton1.Text=EngineLabels.MenuText(engine);toolStripDropDownButton1.ToolTipText=EngineLabels.MenuText(engine);
            localCancelButton.Visible=localRetryButton.Visible=true;
            localCancelButton.ToolTipText="取消当前翻译请求";localRetryButton.ToolTipText="用当前引擎重新翻译";
            RefreshModernToolbar();
            if(!_isInitializing&&changed)QueueSelectedTranslation();
        }
        private void QueueCustomTranslation(string text,int engine)
        {
            int revision=Interlocked.Increment(ref localRevision);var configured=TranslationDirection.Snapshot(EngineRegistry.Get(engine),text);
            localCancelButton.Enabled=configured!=null&&!string.IsNullOrWhiteSpace(text);
            if(configured==null){ShowTranslationStatus("该引擎已被移除，请重新选择。");return;}
            if(string.IsNullOrWhiteSpace(text)){YiStr("");return;}
            ShowTranslationStatus(configured.Name+" 正在翻译…");
            translationScheduler.Schedule(()=>{
                Func<bool> current=()=>revision==Volatile.Read(ref localRevision)&&ClassMain.isjiekounum==engine&&!IsDisposed;
                if(!current())return;
                try{PublishLocalTranslation(CustomTranslationClient.Translate(configured,text,current),revision,engine);}
                catch(OperationCanceledException){}
                catch(Exception ex){PublishLocalTranslation("翻译失败："+ex.Message,revision,engine);}
            });
        }
        private void SetTranslationDirection(string mode)
        {
            mode=TranslationDirection.Normalize(mode);if(TranslationDirection.Mode==mode)return;
            TranslationDirection.Mode=mode;Interlocked.Increment(ref localRevision);sentenceHover?.Reset();
            上一次翻译的文本="";RefreshModernToolbar();jiluset();
            if(!_isInitializing)QueueSelectedTranslation();
        }
    }
}
