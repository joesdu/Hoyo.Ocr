using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Modules;
using Hoyo.OcrServer;

namespace Hoyo.Ocr;

/// <inheritdoc />
public class OcrServerModule : AppModule
{
    /// <inheritdoc />
    public override void ConfigureServices(ConfigureServicesContext context)
    {
        context.Services.AddSingleton<IHoyoIDCardOcr, HoyoIDCardOcr>();
    }
}