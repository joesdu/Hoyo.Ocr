using Hoyo.OcrServer;
using Microsoft.AspNetCore.Mvc;

namespace Hoyo.Ocr.Controllers;
[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;

    private readonly IHoyoOcr hoyoOcr;
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IHoyoOcr ihoyoOcr)
    {
        _logger = logger;
        hoyoOcr = ihoyoOcr;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public object? Get()
    {
        var path = "G:\\GitHub\\OCRTest\\OCRTest\\bin\\Debug\\net6.0\\test1.jpg";
        return hoyoOcr.DetectText(path, "back");
    }
}
