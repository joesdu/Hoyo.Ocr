using Hoyo.AutoDependencyInjectionModule.Modules;
using Hoyo.Ocr;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
//添加SeriLog配置
_ = builder.Host.UseSerilog((webHost, logconfig) =>
{
    var configuration = webHost.Configuration.GetSection("Serilog");
    var minilevel = string.IsNullOrWhiteSpace(configuration.Value) ? LogEventLevel.Information.ToString() : configuration["MinimumLevel:Default"]!;
    //日志事件级别
    var logEventLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), minilevel);
    _ = logconfig.ReadFrom.Configuration(webHost.Configuration.GetSection("Serilog")).Enrich.FromLogContext().WriteTo.Console(logEventLevel);
    _ = logconfig.WriteTo.Map(le => MapData(le), (key, log) => log.Async(o => o.File(Path.Combine("logs", @$"{key.time:yyyyMMdd}{Path.DirectorySeparatorChar}{key.level.ToString().ToLower()}.log"), logEventLevel)));
    static (DateTime time, LogEventLevel level) MapData(LogEvent @event) => (@event.Timestamp.LocalDateTime, @event.Level);
}).ConfigureLogging((hostcontext, builder) => builder.ClearProviders().SetMinimumLevel(LogLevel.Information).AddConfiguration(hostcontext.Configuration.GetSection("Logging")).AddConsole().AddDebug());

// 自动注入服务模块
builder.Services.AddApplication<AppWebModule>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) _ = app.UseDeveloperExceptionPage();

// 添加自动化注入的一些中间件.
app.InitializeApplication();

app.MapControllers();

app.Run();
