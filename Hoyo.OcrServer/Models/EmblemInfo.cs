namespace Hoyo.OcrServer.Models;

/// <summary>
/// 国徽面信息
/// </summary>
public class EmblemInfo
{
    /// <summary>
    /// 签发机关
    /// </summary>
    public string Agency { get; set; } = string.Empty;

    /// <summary>
    /// 有效期开始时间
    /// </summary>
    public string ValidDateBegin { get; set; } = string.Empty;

    /// <summary>
    /// 有效期结束时间
    /// </summary>
    public string ValidDateEnd { get; set; } = string.Empty;
}