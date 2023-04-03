using EasilyNET.Core.Misc;
using Hoyo.OcrServer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hoyo.Ocr.Controllers;

[ApiController, Route("[controller]")]
public class IDCardOcrController : ControllerBase
{
    private readonly IHoyoIDCardOcr _ocr;

    public IDCardOcrController(IHoyoIDCardOcr i_ocr)
    {
        _ocr = i_ocr;
    }

    /// <summary>
    /// 获取人像面信息
    /// </summary>
    /// <param name="img">上传图片</param>
    /// <returns></returns>
    [HttpPost("Portrait")]
    public async Task<PortraitInfo?> Portrait([FromForm] IDCardImg img)
    {
        var stream = img.File?.OpenReadStream()!;
        var bytes = await stream.ToArrayAsync();
        var base64 = Convert.ToBase64String(bytes);
        return _ocr.DetectPortraitInfo(base64);
    }

    /// <summary>
    /// 获取国徽面信息
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    [HttpPost("Emblem")]
    public async Task<EmblemInfo?> Emblem([FromForm] IDCardImg img)
    {
        var stream = img.File?.OpenReadStream()!;
        var bytes = await stream.ToArrayAsync();
        var base64 = Convert.ToBase64String(bytes);
        return _ocr.DetectEmblemInfo(base64);
    }
}

public class IDCardImg
{
    /// <summary>
    /// 上传图片
    /// </summary>
    [Required]
    public IFormFile? File { get; set; }
}