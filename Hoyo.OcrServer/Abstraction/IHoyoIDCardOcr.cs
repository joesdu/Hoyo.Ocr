using Hoyo.OcrServer.Models;

namespace Hoyo.OcrServer.Abstraction;

/// <summary>
/// 身份证OCR
/// </summary>
public interface IHoyoIDCardOcr
{
    /// <summary>
    /// 识别人像面信息
    /// </summary>
    /// <param name="img">图片数据</param>
    /// <returns></returns>
    PortraitInfo? PortraitInfo(byte[] img);

    /// <summary>
    /// 识别国徽面信息
    /// </summary>
    /// <param name="img">图片数据</param>
    /// <returns></returns>
    EmblemInfo? EmblemInfo(byte[] img);
}