using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Modules;
using Hoyo.OcrServer;
using Hoyo.OcrServer.Abstraction;

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