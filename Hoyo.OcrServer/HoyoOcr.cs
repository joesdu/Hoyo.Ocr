// See https://aka.ms/new-console-template for more information
using PaddleOCRSharp;
using System.Drawing;

namespace Hoyo.OcrServer;

public class HoyoOcr : IHoyoOcr
{
    private static readonly OCRModelConfig? config = null;
    private static readonly OCRParameter oCRParameter = new();
    private static PaddleOCREngine? engine;
    public HoyoOcr()
    {
        //建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。
        engine = new(config, oCRParameter);
    }
    public string DetectText(string path, string type = "front")
    {
        if (string.IsNullOrWhiteSpace(path) || string.IsNullOrWhiteSpace(type)) throw new("身份证路径或正反面不能为空");
        var imagebyte = File.ReadAllBytes(path);
#pragma warning disable CA1416 // 验证平台兼容性
        Bitmap bitmap = new(stream: new MemoryStream(imagebyte));
#pragma warning restore CA1416 // 验证平台兼容性
        OCRResult ocrResult = new();
        string resut = "";
        ocrResult = engine!.DetectText(bitmap);
        if (ocrResult != null)
        {
            resut = GetInfo(ocrResult.Text, type);
        }
        return resut;
    }

    private static string GetInfo(string str, string type)
    {
        if (type == "front")
        {
            Console.WriteLine(str);
        }
        else
        {
            Console.WriteLine(str);
        }
        return str;
    }
}