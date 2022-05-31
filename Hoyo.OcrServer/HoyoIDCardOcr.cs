using Hoyo.Enums;
using Hoyo.Extensions;
using Hoyo.Tools;
using PaddleOCRSharp;
using System.Drawing;
using System.Text;

namespace Hoyo.OcrServer;

public class HoyoIDCardOcr : IHoyoIDCardOcr
{
    private static readonly OCRParameter oCRParameter = new();
    private static PaddleOCREngine? engine;
    private static PaddleOCREngine? serverEngine;
    public HoyoIDCardOcr()
    {
        var modelPathroot = $@"{Environment.CurrentDirectory}\inferenceserver";
        //服务器中英文模型
        OCRModelConfig config = new()
        {
            det_infer = $@"{modelPathroot}\ch_ppocr_server_v2.0_det_infer",
            cls_infer = $@"{modelPathroot}\ch_ppocr_mobile_v2.0_cls_infer",
            rec_infer = $@"{modelPathroot}\ch_ppocr_server_v2.0_rec_infer",
            keys = $@"{modelPathroot}\ppocr_keys.txt"
        };
        //建议程序全局初始化一次即可，不必每次识别都初始化，容易报错。
        engine = new(null, oCRParameter);
        serverEngine = new(config, oCRParameter);
    }
    public PortraitInfo? DetectPortraitInfo(byte[] imagebyte)
    {
#pragma warning disable CA1416 // 验证平台兼容性
        Bitmap bitmap = new(stream: new MemoryStream(imagebyte));
#pragma warning restore CA1416 // 验证平台兼容性
        var ocrResult = engine!.DetectStructure(bitmap);
        var ocrServer = serverEngine!.DetectStructure(bitmap);
        return ocrResult is null || ocrServer is null ? null : GetPortraitInfo(ocrResult, ocrServer);
    }

    public EmblemInfo? DetectEmblemInfo(byte[] imagebyte)
    {
#pragma warning disable CA1416 // 验证平台兼容性
        Bitmap bitmap = new(stream: new MemoryStream(imagebyte));
#pragma warning restore CA1416 // 验证平台兼容性
        var ocrService = serverEngine!.DetectStructure(bitmap);
        return ocrService is null ? null : GetEmblemInfo(ocrService);
    }

    /// <summary>
    /// 获取身份证正面信息
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static PortraitInfo GetPortraitInfo(OCRStructureResult results, OCRStructureResult serviceresults)
    {
        var cells = results.Cells.FindAll(c => !string.IsNullOrWhiteSpace(c.Text));
        var servicecells = serviceresults.Cells.FindAll(c => !string.IsNullOrWhiteSpace(c.Text));
        var idno = Idno(cells);
        return new()
        {
            Name = Name(servicecells),
            Gender = idno.CalculateGender(),
            Nation = Nation(servicecells),
            Birthday = idno.CalculateBirthday(),
            Address = Address(servicecells, idno),
            IdNumber = idno
        };
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static EmblemInfo GetEmblemInfo(OCRStructureResult results)
    {
        var cells = results.Cells.FindAll(c => !string.IsNullOrWhiteSpace(c.Text));
        return new()
        {
            Agency = Agency(cells),
            ValidDateBegin = StartTime(cells),
            ValidDateEnd = EndTime(cells)
        };
    }

    #region 国徽面数据提取
    /// <summary>
    /// 获取有效期开始时间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Agency(List<StructureCells> cells)
    {
        var begin = false;
        var sb = new StringBuilder();
        foreach (var item in cells)
        {
            if (item.Text.Contains("签发机关"))
            {
                begin = true;
                continue;
            }
            if (!string.IsNullOrWhiteSpace(item.Text) && item.Text.Contains("有效期")) break;
            if (begin & !string.IsNullOrWhiteSpace(item.Text)) _ = sb.Append(item.Text);
        }
        return sb.ToString();
    }
    /// <summary>
    /// 获取有效期开始时间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string StartTime(List<StructureCells> cells)
    {
        var begin = false;
        var start = "";
        foreach (var item in cells)
        {
            if (item.Text.Contains("有效期")) begin = true;
            var temp = item.Text.Replace(".", "");
            var s = temp.IndexOf("-") - 8;
            if (s < 0) continue;
            if (begin & !string.IsNullOrWhiteSpace(item.Text))
            {
                start = temp.Substring(s, 8);
                if (start.IsNumber()) start = start.ToDateTime(true)?.ToString("yyyy-MM-dd")!;
                break;
            }
        }
        return start;
    }
    /// <summary>
    /// 获取有效期结束时间
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string EndTime(List<StructureCells> cells)
    {
        var begin = false;
        var end = "";
        foreach (var item in cells)
        {
            if (item.Text.Contains("有效期")) begin = true;
            var temp = item.Text.Replace(".", "");
            var s = temp.IndexOf("-");
            if (s < 0) continue;
            if (begin & !string.IsNullOrWhiteSpace(item.Text))
            {
                end = temp[(s + 1)..];
                if (end.StartsWith("长期")) end = "长期";
                else if (end.IsNumber())
                {
                    end = end[..8];
                    return end.ToDateTime(true)?.ToString("yyyy-MM-dd")!;
                }
                break;
            }
        }
        return end;
    }
    #endregion

    #region 人像面数据提取
    /// <summary>
    /// 获取地址
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Address(List<StructureCells> cells, string idno)
    {
        var begin = false;
        var sb = new StringBuilder();
        foreach (var item in cells)
        {
            if (item.Text.Contains("住址")) begin = true;
            if (!string.IsNullOrWhiteSpace(item.Text) && (item.Text.Contains("公民身份号码") || idno.Contains(item.Text))) break;
            if (begin && !string.IsNullOrWhiteSpace(item.Text)) _ = sb.Append(item.Text);
        }
        var address = sb.ToString()[2..].ReverseByPointer();
        var index = address.IndexOf("号");
        address = address[index..].ReverseByPointer();
        return address;
    }

    /// <summary>
    /// 获取名族
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static ENation Nation(List<StructureCells> cells)
    {
        var begin = false;
        var result = ENation.其他;
        foreach (var item in cells)
        {
            if (item.Text.Contains("民族")) begin = true;
            if (begin & !string.IsNullOrWhiteSpace(item.Text))
            {
                var s = item.Text.IndexOf("民族") + 2;
                var nation = item.Text[s..];
                result = nation is "穿青人" or "其他" or "外国血统中国籍人士"
                    ? (ENation)Enum.Parse(typeof(ENation), nation)
                    : (ENation)Enum.Parse(typeof(ENation), nation + "族");
                break;
            }
        }
        return result;
    }

    /// <summary>
    /// 获取姓名
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Name(List<StructureCells> cells)
    {
        var begin = false;
        foreach (var item in cells)
        {
            if (item.Text.Contains("姓名")) begin = true;
            if (begin & !string.IsNullOrWhiteSpace(item.Text)) return item.Text[2..];
        }
        return "";
    }

    /// <summary>
    /// 获取身份证号码
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    private static string Idno(List<StructureCells> cells)
    {
        var idno = "";
        var begin = false;
        foreach (var item in cells)
        {
            if (item.Text.Contains("公民身份号码"))
            {
                begin = true;
                continue;
            }
            if (begin & !string.IsNullOrWhiteSpace(item.Text)) idno = item.Text;
        }
        if (!idno.IsNumber()) throw new("不合法的身份证号码");
        _ = idno.CheckIDCard();
        return idno;
    }
    #endregion
}