namespace Hoyo.OcrServer;
public interface IHoyoOcr
{
    string DetectText(string path, string type);
}
