namespace Hoyo.OcrServer;

/// <summary>
/// 身份证OCR
/// </summary>
public interface IHoyoIDCardOcr
{
    /// <summary>
    /// 识别人像面信息
    /// </summary>
    /// <param name="base64">图片base64数据</param>
    /// <returns></returns>
    PortraitInfo? DetectPortraitInfo(string base64);

    /// <summary>
    /// 识别国徽面信息
    /// </summary>
    /// <param name="base64">图片base64数据</param>
    /// <returns></returns>
    EmblemInfo? DetectEmblemInfo(string base64);
}