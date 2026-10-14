using System;

namespace Xtranslator
{
    // One direction for the whole window. Capture it before dispatching a request.
    public static class TranslationDirection
    {
        public static string Mode = "auto";
        public static string Normalize(string value) { return value == "to-en" || value == "to-zh" ? value : "auto"; }
        public static string Label(string mode) { return mode == "to-en" ? "译为英语" : mode == "to-zh" ? "译为简体中文" : "中英自动互译"; }
        public static string Target(string text, string mode)
        {
            if (mode == "to-en" || mode == "zh-en") return "EN";
            if (mode == "to-zh" || mode == "en-zh") return "ZH";
            return ClassMain.CalculateChineseCharacterPercentage(text) >= 0.35 ? "EN" : "ZH";
        }
        public static EngineDefinition Snapshot(EngineDefinition value, string text)
        {
            if (value == null) return null;
            var result = value.Copy();
            result.Direction = Normalize(Mode);
            result.Automatic = false;
            result.SourceLanguage = "";
            result.TargetLanguage = Target(text, result.Direction);
            return result;
        }
    }
}
