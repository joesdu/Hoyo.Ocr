using EasilyNET.Core.Enums;
using EasilyNET.Core.IdCard;
using EasilyNET.Core.Misc;
using Hoyo.OcrServer.Abstraction;
using Hoyo.OcrServer.Models;
using Microsoft.Extensions.Logging;
using OpenCvSharp;
using Sdcb.PaddleInference;
using Sdcb.PaddleOCR;
using Sdcb.PaddleOCR.Models.Local;
using System.Text;

// ReSharper disable SuggestBaseTypeForParameterInConstructor

namespace Hoyo.OcrServer;

/// <inheritdoc />
public sealed class HoyoIDCardOcr : IHoyoIDCardOcr
{
    private readonly ILogger<HoyoIDCardOcr> logger;
    private readonly PaddleOcrAll poa;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="logger"></param>
    public HoyoIDCardOcr(ILogger<HoyoIDCardOcr> logger)
    {
        this.logger = logger;
        poa = new(LocalFullModels.ChineseV3, PaddleDevice.Mkldnn())
        {
            // 允许识别有角度的文字
            AllowRotateDetection = true,
            // 允许识别旋转角度大于90度的文字
            Enable180Classification = false
        };
    }

    /// <inheritdoc />
    public PortraitInfo PortraitInfo(byte[] img) => GetPortraitInfo(GetDetectResult(img));

    /// <inheritdoc />
    public EmblemInfo EmblemInfo(byte[] img) => GetEmblemInfo(GetDetectResult(img));

    /// <summary>
    /// 解析数据
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    private List<PaddleOcrResultRegion> GetDetectResult(byte[] img)
    {
        using var src = Cv2.ImDecode(img, ImreadModes.AnyDepth);
        var result = poa.Run(src);
        logger.LogInformation("Detected all texts: \n{a}", result.Text);
        foreach (var region in result.Regions)
        {
            logger.LogInformation("Text: {a}, Score: {b}, RectCenter: {c}, RectSize: {d}, Angle: {e}", region.Text, region.Score, region.Rect.Center, region.Rect.Size, region.Rect.Angle);
        }
        return result.Regions.ToList().FindAll(c => !string.IsNullOrWhiteSpace(c.Text) && c.Score >= 0.80f);
    }

    /// <summary>
    /// 获取人像面信息
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static PortraitInfo GetPortraitInfo(List<PaddleOcrResultRegion> cells)
    {
        var idno = Idno(cells);
        idno.CalculateBirthday(out DateOnly birthday);
        return new()
        {
            Name = Name(cells),
            Gender = idno.CalculateGender(),
            Nation = Nation(cells),
            Birthday = birthday,
            Address = Address(cells, idno),
            IdNumber = idno
        };
    }

    /// <summary>
    /// 获取国徽面数据
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static EmblemInfo GetEmblemInfo(IReadOnlyCollection<PaddleOcrResultRegion> cells)
    {
        // 在中国身份证应该一定是公安局发的,所以一定会有一个 局 字
        var agency_cell = cells.FirstOrDefault(c => c.Text.Contains('局'));
        // 身份证有效期使用 - 连接所以使用查找横线的方式来找到所需的区域
        var times = cells.FirstOrDefault(c => c.Text.Contains('-'));
        var validDate = GetValidDate(times);
        return new()
        {
            Agency = GetAgency(agency_cell),
            ValidDateBegin = validDate.Item1,
            ValidDateEnd = validDate.Item2
        };
    }

    /// <summary>
    /// 签发机关信息
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private static string GetAgency(PaddleOcrResultRegion cell)
    {
        var agency = cell.Text.RemoveWhiteSpace();
        if (agency.StartsWith("签发机关"))
        {
            agency = agency["签发机关".Length..];
        }
        if (agency.EndsWith("签发机关"))
        {
            agency = agency.Substring(agency.Length - "签发机关".Length - 1, "签发机关".Length);
        }
        return agency;
    }

    /// <summary>
    /// 处理有效期
    /// </summary>
    /// <param name="cell"></param>
    /// <returns></returns>
    private static (string, string) GetValidDate(PaddleOcrResultRegion cell)
    {
        //    有效期限 2016. 10. 27 - 2021. 10. 27
        var times = cell.Text.RemoveWhiteSpace().Split('-');
        // 一定有开始日期,所以开始日期一定是正常的格式. 如: 2023.01.12
        var start = times[0].Replace('.', '-');
        if (start.StartsWith("有效日期"))
        {
            start = start[.."有效日期".Length];
        }
        start = start[^10..];
        // 结束日期之所以不转化格式.是因为可能会存在 长期
        var end = times[1];
        if (end != "长期") end = end.Replace('.', '-')[^10..];
        return (start, end);
    }

    #region 人像面数据提取

    /// <summary>
    /// 获取地址
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="idno"></param>
    /// <returns></returns>
    private static string Address(List<PaddleOcrResultRegion> cells, string idno)
    {
        var begin = false;
        var sb = new StringBuilder();
        foreach (var item in cells)
        {
            if (item.Text.Contains("住址")) begin = true;
            if (!string.IsNullOrWhiteSpace(item.Text) && (item.Text.Contains("公民身份号码") || idno.Contains(item.Text))) break;
            if (begin && !string.IsNullOrWhiteSpace(item.Text)) _ = sb.Append(item.Text);
        }
        var address = sb.ToString()[2..];
        address.Reverse();
        var index = address.IndexOf("号", StringComparison.Ordinal);
        address = address[index..];
        address.Reverse();
        return address;
    }

    /// <summary>
    /// 获取名族
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static ENation Nation(List<PaddleOcrResultRegion> cells)
    {
        var begin = false;
        var result = ENation.其他;
        foreach (var item in cells)
        {
            if (item.Text.Contains("民族")) begin = true;
            if (!(begin & !string.IsNullOrWhiteSpace(item.Text))) continue;
            var s = item.Text.IndexOf("民族", StringComparison.Ordinal) + 2;
            var nation = item.Text[s..];
            result = nation is "穿青人" or "其他" or "外国血统中国籍人士"
                         ? (ENation)Enum.Parse(typeof(ENation), nation)
                         : (ENation)Enum.Parse(typeof(ENation), $"{nation}族");
            break;
        }
        return result;
    }

    /// <summary>
    /// 获取姓名
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static string Name(List<PaddleOcrResultRegion> cells)
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
    /// <param name="cells"></param>
    /// <returns></returns>
    private static string Idno(List<PaddleOcrResultRegion> cells)
    {
        var idno = "";
        foreach (var item in cells)
        {
            // 当这行字符串包含身份证号,并且长度超过这几个字,那么表示数据在同一行
            if (item.Text.Contains("公民身份号码") && item.Text.Length > "公民身份号码".Length)
            {
                idno = item.Text["公民身份号码".Length..];
                break;
            }
            if (item.Text.Length is not (15 or 18)) continue;
            idno = item.Text;
            break;
        }
        _ = idno.CheckIDCard();
        return idno;
    }

    #endregion
}