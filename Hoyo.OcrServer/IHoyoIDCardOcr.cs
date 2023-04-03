namespace Hoyo.OcrServer;

public interface IHoyoIDCardOcr
{
    /// <summary>
    /// </summary>
    /// <param name="base64">图片base64数据</param>
    /// <returns></returns>
    PortraitInfo? DetectPortraitInfo(string base64);

    /// <summary>
    /// </summary>
    /// <param name="base64">图片base64数据</param>
    /// <returns></returns>
    EmblemInfo? DetectEmblemInfo(string base64);
}