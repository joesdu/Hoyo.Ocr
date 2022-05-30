namespace Hoyo.OcrServer;
public interface IHoyoOcr
{
    object? DetectText(string path, EOcrType type);
}
