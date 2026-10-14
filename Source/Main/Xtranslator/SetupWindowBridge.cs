namespace Xtranslator
{
    public partial class Form2
    {
        internal void RefreshAfterSetup(){RebuildEngineMenu();RefreshModernToolbar();}
        internal void ShowRuntimeNotice(string message){if(rtb原文.TextLength==0&&LocalModelClient.IsLocalEngine(ClassMain.isjiekounum))ShowTranslationStatus(message);}
    }
}
