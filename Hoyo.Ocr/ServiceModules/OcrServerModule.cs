using EasilyNET.AutoDependencyInjection.Contexts;
using EasilyNET.AutoDependencyInjection.Modules;
using Hoyo.OcrServer;

namespace Hoyo.Ocr;

public class OcrServerModule : AppModule
{
    public override void ConfigureServices(ConfigureServicesContext context)
    {
        context.Services.AddSingleton<IHoyoIDCardOcr, HoyoIDCardOcr>();
    }
}