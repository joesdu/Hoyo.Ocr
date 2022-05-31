using Hoyo.AutoDependencyInjectionModule.Modules;
using Hoyo.OcrServer;

namespace Hoyo.Ocr;

public class OcrServerModule : AppModule
{
    public override void ConfigureServices(ConfigureServicesContext context)
    {
        context.Services.AddSingleton<IHoyoIDCardOcr, HoyoIDCardOcr>();
    }
}
