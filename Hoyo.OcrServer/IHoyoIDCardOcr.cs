namespace Hoyo.OcrServer;

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
    PortraitInfo? DetectPortraitInfo(byte[] img);

    /// <summary>
    /// 识别国徽面信息
    /// </summary>
    /// <param name="img">图片数据</param>
    /// <returns></returns>
    EmblemInfo? DetectEmblemInfo(byte[] img);
}