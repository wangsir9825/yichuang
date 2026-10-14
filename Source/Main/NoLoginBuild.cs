using System;
using YiChuang;

internal static class NoLoginBuild
{
    internal static bool IsAllowedEngine(int number) {
        return EngineRegistry.IsVisible(number);
    }
    internal static bool IsUserApiEngine(int number) {
        return IsAllowedEngine(number) && number != 4 && number != 5 && !LocalModelClient.IsLocalEngine(number);
    }
    internal static void NormalizeSettings() {
        if (!IsAllowedEngine(ClassMain.isjiekounum)) ClassMain.isjiekounum = 0;
        if (ClassMain.DeeplInterface != "2" && ClassMain.DeeplInterface != "3") ClassMain.DeeplInterface = "2";
        ClassMain.is单词悬浮框 = false;
        ClassMain.formLocation = "1";
    }
}
