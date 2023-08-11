using EasilyNET.Core.Misc;
using Hoyo.OcrServer;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Hoyo.Ocr.Controllers;

/// <inheritdoc />
[ApiController, Route("[controller]")]
public class IDCardOcrController(IHoyoIDCardOcr i_ocr) : ControllerBase
{
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
        return i_ocr.DetectPortraitInfo(base64);
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
        return i_ocr.DetectEmblemInfo(base64);
    }
}

/// <summary>
/// Img
/// </summary>
public class IDCardImg
{
    /// <summary>
    /// 上传图片
    /// </summary>
    [Required]
    public IFormFile? File { get; set; }
}