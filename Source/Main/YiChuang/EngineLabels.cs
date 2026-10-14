namespace YiChuang
{
    internal static class EngineLabels
    {
        // Display numbers are separate from persisted engine IDs, so existing selections still work.
        public const string LocalSettingsTitle = "内置本地模型";

        public static string Number(int engine)
        {
            int index = System.Array.IndexOf(EngineRegistry.VisibleIds, engine);
            return index < 0 ? "" : (index + 1).ToString();
        }

        public static string BuiltinName(int engine)
        {
            switch (engine)
            {
                case 4: return "Transmart 网页翻译";
                case 5: return "搜狗 网页翻译";
                case 6: return "DeepL 官方API翻译";
                case 14: return "DeepSeek AI翻译";
                case 16: return "腾讯 Hy-MT2-7B AI翻译（本地）";
                case 17: return "Google TranslateGemma-12B AI翻译（本地）";
                case 18: return "Google Gemini AI翻译";
                case 19: return "Qwen-MT Plus AI翻译";
                default: return "选择引擎";
            }
        }

        public static string Name(int engine) { var e=EngineRegistry.Get(engine); return e==null ? BuiltinName(engine) : e.Name + ((e.Provider=="ollama"||e.Provider=="llamacpp") && !e.Name.Contains("本地") ? "（本地）" : ""); }
        public static string MenuText(int engine) { string n=Number(engine); return (n.Length==0 ? "" : n+"：") + Name(engine); }
    }
}
