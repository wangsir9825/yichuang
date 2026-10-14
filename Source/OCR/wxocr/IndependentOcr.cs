using System;
using System.IO;
using Tesseract;

namespace wxocr;

// Bundled offline fallback; uses no WeChat installation, accounts or network.
public static class IndependentOcr
{
    public static string Recognize(string image)
    {
        string data=Path.Combine(Path.GetDirectoryName(typeof(IndependentOcr).Assembly.Location),"tessdata");
        if(!File.Exists(Path.Combine(data,"chi_sim.traineddata"))||!File.Exists(Path.Combine(data,"eng.traineddata")))
            throw new InvalidOperationException("内置OCR语言数据缺失，请重新安装完整的译窗安装包。");
        using(var engine=new TesseractEngine(data,"chi_sim+eng",EngineMode.LstmOnly))
        using(var pix=Pix.LoadFromFile(image))
        using(var page=engine.Process(pix,PageSegMode.Auto))return System.Text.RegularExpressions.Regex.Replace((page.GetText()??"").Trim(),@"(\r?\n[ \t]*){3,}","\n\n");
    }
}
