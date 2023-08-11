using EasilyNET.Core.Enums;

namespace Hoyo.OcrServer;

/// <summary>
/// 人像面信息
/// </summary>
public class PortraitInfo
{
    /// <summary>
    /// 姓名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 性别
    /// </summary>
    public EGender Gender { get; set; } = EGender.男;

    /// <summary>
    /// 民族
    /// </summary>
    public ENation Nation { get; set; } = ENation.汉族;

    /// <summary>
    /// 生日
    /// </summary>
    public DateOnly Birthday { get; set; }

    /// <summary>
    /// 联系地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 身份证号码
    /// </summary>
    public string IdNumber { get; set; } = string.Empty;
}

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