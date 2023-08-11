using EasilyNET.Core.Enums;

namespace Hoyo.OcrServer.Models;

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