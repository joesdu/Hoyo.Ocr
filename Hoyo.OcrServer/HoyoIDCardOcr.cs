using EasilyNET.Core.Enums;
using EasilyNET.Core.IdCard;
using EasilyNET.Core.Misc;
using PaddleOCRSharp;
using System.Text;

namespace Hoyo.OcrServer;

public class HoyoIDCardOcr : IHoyoIDCardOcr
{
    //OCR参数
    private static readonly OCRParameter oCRParameter = new()
    {
        numThread = 8,            //预测并发线程数
        Enable_mkldnn = true,     //web部署该值建议设置为0,否则出错，内存如果使用很大，建议该值也设置为0.
        cls = true,               //是否执行文字方向分类；默认false
        use_angle_cls = true,     //是否开启方向检测，用于检测识别180旋转
        det_db_score_mode = true, //是否使用多段线，即文字区域是用多段线还是用矩形，
        UnClipRatio = 1.6f,
        MaxSideLen = 2000
    };

    //private static readonly string root = $@"{Environment.CurrentDirectory}\inference";

    //private static readonly PaddleOCREngine engine = new(new()
    //{
    //    det_infer = $@"{root}\ch_PP-OCRv3_det_infer",
    //    cls_infer = $@"{root}\ch_ppocr_mobile_v2.0_cls_infer",
    //    rec_infer = $@"{root}\ch_PP-OCRv3_rec_infer",
    //    keys = $@"{root}\ppocr_keys.txt"
    //}, oCRParameter);

    private static readonly PaddleOCREngine engine = new(null, oCRParameter);

    /// <summary>
    /// 获取身份证正面信息
    /// </summary>
    /// <param name="base64">图片二进制数据</param>
    /// <returns></returns>
    public PortraitInfo DetectPortraitInfo(string base64)
    {
        var ocrResult = engine.DetectTextBase64(base64);
        var cells = ocrResult.TextBlocks.FindAll(c => !string.IsNullOrWhiteSpace(c.Text) && c.Score >= 0.85f);
        return GetPortraitInfo(cells);
    }

    public EmblemInfo DetectEmblemInfo(string base64)
    {
        var ocrResult = engine.DetectTextBase64(base64);
        var cells = ocrResult.TextBlocks.FindAll(c => !string.IsNullOrWhiteSpace(c.Text) && c.Score >= 0.85f);
        return GetEmblemInfo(cells);
    }

    /// <summary>
    /// 获取人像面信息
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static PortraitInfo GetPortraitInfo(List<TextBlock> cells)
    {
        var sb = new StringBuilder();
        foreach (var cell in cells)
        {
            sb.Append(cell.Text);
        }
        Console.WriteLine(sb.ToString());
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
    private static EmblemInfo GetEmblemInfo(List<TextBlock> cells) =>
        new()
        {
            Agency = Agency(cells),
            ValidDateBegin = StartTime(cells),
            ValidDateEnd = EndTime(cells)
        };

    #region 国徽面数据提取

    /// <summary>
    /// 获取签发机关
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static string Agency(List<TextBlock> cells)
    {
        var begin = false;
        var sb = new StringBuilder();
        foreach (var item in cells)
        {
            if (item.Text.Contains("签发机关") && item.Text.Length > "签发机关".Length)
            {
                sb.Append(item.Text["签发机关".Length..]);
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
    /// <param name="cells"></param>
    /// <returns></returns>
    private static string StartTime(List<TextBlock> cells)
    {
        var begin = false;
        var start = "";
        foreach (var item in cells)
        {
            if (item.Text.Contains("有效期")) begin = true;
            var temp = item.Text.Replace(".", "");
            var s = temp.IndexOf("-", StringComparison.Ordinal) - 8;
            if (s < 0) continue;
            if (!(begin & !string.IsNullOrWhiteSpace(item.Text))) continue;
            start = temp.Substring(s, 8);
            if (start.IsNumber()) start = start.ToDateTime(true)?.ToString("yyyy-MM-dd")!;
            break;
        }
        return start;
    }

    /// <summary>
    /// 获取有效期结束时间
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static string EndTime(List<TextBlock> cells)
    {
        var begin = false;
        var end = "";
        foreach (var item in cells)
        {
            if (item.Text.Contains("有效期")) begin = true;
            var temp = item.Text.Replace(".", "");
            var s = temp.IndexOf("-", StringComparison.Ordinal);
            if (s < 0) continue;
            if (!(begin & !string.IsNullOrWhiteSpace(item.Text))) continue;
            end = temp[(s + 1)..];
            if (end.StartsWith("长期"))
            {
                end = "长期";
            }
            else if (end.IsNumber())
            {
                end = end[..8];
                return end.ToDateTime(true)?.ToString("yyyy-MM-dd")!;
            }
            break;
        }
        return end;
    }

    #endregion

    #region 人像面数据提取

    /// <summary>
    /// 获取地址
    /// </summary>
    /// <param name="cells"></param>
    /// <param name="idno"></param>
    /// <returns></returns>
    private static string Address(List<TextBlock> cells, string idno)
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
    private static ENation Nation(List<TextBlock> cells)
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
                         : (ENation)Enum.Parse(typeof(ENation), nation + "族");
            break;
        }
        return result;
    }

    /// <summary>
    /// 获取姓名
    /// </summary>
    /// <param name="cells"></param>
    /// <returns></returns>
    private static string Name(List<TextBlock> cells)
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
    private static string Idno(List<TextBlock> cells)
    {
        var idno = "";
        foreach (var item in cells)
        {
            // 当这行字符串包含身份证号,并且长度超过这几个字,那么表示数据在同一行
            if (item.Text.Contains("公民身份号码") && item.Text.Length > "公民身份号码".Length)
            {
                idno = item.Text["公民身份号码".Length..];
            }
            if (item.Text.Length is 15 or 18)
            {
                idno = item.Text;
            }
        }
        _ = idno.CheckIDCard();
        return idno;
    }

    #endregion
}