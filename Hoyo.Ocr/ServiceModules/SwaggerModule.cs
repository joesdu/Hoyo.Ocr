using Hoyo.AutoDependencyInjectionModule.Extensions;
using Hoyo.AutoDependencyInjectionModule.Modules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Hoyo.Ocr;

public class SwaggerModule : AppModule
{
    /**
     * https://github.com/domaindrivendev/Swashbuckle.AspNetCore
     */
    private string Title { get; set; } = string.Empty;
    private string Version { get; set; } = string.Empty;

    public override void ConfigureServices(ConfigureServicesContext context)
    {
        var config = context.Services.GetConfiguration();
        Title = config["Swagger:Title"] ?? "Miracle.SwaggerModule";
        Version = config["Swagger:Version"] ?? "v1.0";
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        _ = context.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(Version, new OpenApiInfo
            {
                Title = Title,
                Version = Version
            });
            //var files = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "*.xml"));
            //foreach (var fiel in files)
            //{
            //    c.IncludeXmlComments(fiel, true);
            //}
            // 一定要返回true！
            c.DocInclusionPredicate((docName, description) => true);
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });
            // 这里使用预定义的过滤器,避免给所有接口均加锁.
            c.OperationFilter<SwaggerOperationFilter>();
        });
    }

    public override void ApplicationInitialization(ApplicationContext context)
    {
        var app = context.GetApplicationBuilder();
        _ = app.UseSwagger().UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{Version}/swagger.json", $"{Title} {Version}"));
    }
}

public class SwaggerOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var authAttributes = context.MethodInfo.DeclaringType?.GetCustomAttributes(true)
            .Union(context.MethodInfo.GetCustomAttributes(true))
            .OfType<AuthorizeAttribute>();

        if (!authAttributes!.Any()) return;
        var securityRequirement = new OpenApiSecurityRequirement()
        {
            {
                // Put here you own security scheme, this one is an example
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    },
                    Scheme = "oauth2",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                },
                new List<string>()
            }
        };
        operation.Security = new List<OpenApiSecurityRequirement> { securityRequirement };
        operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
    }
}
//startup.ConfigureServices中,会导致所有接口加锁图标
//options.AddSecurityRequirement(new OpenApiSecurityRequirement {
//    {new OpenApiSecurityScheme{ Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme,Id = "Bearer" }}, new string[] { } }
//});