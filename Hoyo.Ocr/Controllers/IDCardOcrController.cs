using EasilyNET.Core.Misc;
using Hoyo.OcrServer.Abstraction;
using Hoyo.OcrServer.Models;
using Microsoft.AspNetCore.Mvc;

namespace Hoyo.Ocr.Controllers;

/// <inheritdoc />
[ApiController, Route("api/[controller]/[action]")]
public class IDCardOcrController(IHoyoIDCardOcr ocr) : ControllerBase
{
    /// <summary>
    /// 获取人像面信息
    /// </summary>
    /// <param name="img">上传图片</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<PortraitInfo?> Portrait(IFormFile img)
    {
        await using var stream = img.OpenReadStream();
        var bytes = await stream.ToArrayAsync();
        return ocr.PortraitInfo(bytes);
    }

    /// <summary>
    /// 获取国徽面信息
    /// </summary>
    /// <param name="img"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<EmblemInfo?> Emblem(IFormFile img)
    {
        await using var stream = img.OpenReadStream();
        var bytes = await stream.ToArrayAsync();
        return ocr.EmblemInfo(bytes);
    }
}