namespace Hoyo.OcrServer;
public interface IHoyoIDCardOcr
{
    PortraitInfo? DetectPortraitInfo(byte[] imagebyte);
    EmblemInfo? DetectEmblemInfo(byte[] imagebyte);
}
