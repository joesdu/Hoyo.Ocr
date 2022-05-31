using Hoyo.OcrServer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hoyo.Ocr.Controllers;
[ApiController, Route("[controller]")]
public class IDCardOcrController : ControllerBase
{
    private readonly IHoyoIDCardOcr hoyoOcr;
    public IDCardOcrController(IHoyoIDCardOcr ihoyoOcr) => hoyoOcr = ihoyoOcr;

    [HttpPost("Portrait")]
    public PortraitInfo? Portrait([FromForm] IDCardImg cardimg)
    {
        var stream = cardimg.File?.OpenReadStream()!;
        var bytes = StreamToBytes(stream);
        return hoyoOcr.DetectPortraitInfo(bytes);
    }

    [HttpPost("Emblem")]
    public EmblemInfo? Emblem([FromForm] IDCardImg cardimg)
    {
        var stream = cardimg.File?.OpenReadStream()!;
        var bytes = StreamToBytes(stream);
        return hoyoOcr.DetectEmblemInfo(bytes);
    }

    private static byte[] StreamToBytes(Stream stream)
    {
        var bytes = new byte[stream.Length];
        _ = stream.Read(bytes, 0, bytes.Length);
        // 设置当前流的位置为流的开始 
        _ = stream.Seek(0, SeekOrigin.Begin);
        return bytes;
    }
}

public class IDCardImg
{
    /// <summary>
    /// 上传文件(单或多文件)
    /// </summary>
    [Required]
    public IFormFile? File { get; set; } = null;
}
