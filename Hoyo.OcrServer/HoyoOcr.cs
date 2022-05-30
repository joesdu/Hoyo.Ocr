using Hoyo.Enums;
using Hoyo.Extensions;
using Hoyo.Tools;
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
    public object? DetectText(string path, EOcrType type = EOcrType.人像面)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new("身份证路径不能为空");
        var imagebyte = File.ReadAllBytes(path);
#pragma warning disable CA1416 // 验证平台兼容性
        Bitmap bitmap = new(stream: new MemoryStream(imagebyte));
#pragma warning restore CA1416 // 验证平台兼容性
        OCRResult ocrResult = engine!.DetectText(bitmap);
        return ocrResult is not null
            ? type switch
            {
                EOcrType.人像面 => GetFrontInfo(ocrResult.Text),
                EOcrType.国徽面 => GetBackInfo(ocrResult.Text),
                _ => null,
            }
            : null;
    }

    /// <summary>
    /// 获取身份证正面信息
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static FrontInfo GetFrontInfo(string str)
    {
        var idno = Idno(str);
        return new()
        {
            Name = Name(str),
            Gender = idno.CalculateGender(),
            Nation = Nation(str),
            Birthday = idno.CalculateBirthday(),
            Address = Address(str),
            IdNumber = idno
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static BackInfo GetBackInfo(string str)
    {
        //签发机关有效期限居民身份证中华人民共和国2014.1224-20241224重庆市公安局沙坪坝分局
        BackInfo obj = new();
        Console.WriteLine(str);
        return obj;
    }

    /// <summary>
    /// 获取地址
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Address(string str)
    {
        var s = str.IndexOf("住址") + 2;
        var l = str.IndexOf("公民身份号码") - s;
        return str.Substring(s, l);
    }

    /// <summary>
    /// 获取名族
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static ENation Nation(string str)
    {
        var s = str.IndexOf("民族") + 2;
        var l = str.IndexOf("出生") - s;
        var nation = str.Substring(s, l);
        return nation is "穿青人" or "其他" or "外国血统中国籍人士"
            ? (ENation)Enum.Parse(typeof(ENation), nation)
            : (ENation)Enum.Parse(typeof(ENation), nation + "族");
    }

    /// <summary>
    /// 获取姓名
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Name(string str)
    {
        var s = str.IndexOf("姓名") + 2;
        var l = str.IndexOf("性别") - s;
        return str.Substring(s, l);
    }

    /// <summary>
    /// 获取身份证号码
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Idno(string str)
    {
        string idno;
        var reverse = str.ReverseByPointer();
        var reverseidno = reverse.Substring(0, 18);
        if (reverseidno.IsNumber())
        {
            idno = reverseidno.ReverseByPointer();
        }
        else
        {
            reverseidno = str[15..];
            idno = reverseidno.IsNumber() ? reverseidno.ReverseByPointer() : throw new("不合法的身份证号码");
        }
        _ = idno.CheckIDCard();
        str.ReverseByPointer();
        return idno;
    }
}