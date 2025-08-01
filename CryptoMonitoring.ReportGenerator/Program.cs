using CryptoMonitoring.ReportGenerator.Data;
using CryptoMonitoring.ReportGenerator.Services;
using CryptoMonitoring.ReportGenerator.Builders;
using Microsoft.EntityFrameworkCore;
using RazorLight;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
//builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ReportDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IRazorLightEngine>(new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(Program))
    .UseMemoryCachingProvider()
    .Build());

builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IReportBuilder, DailyReportBuilder>();
builder.Services.AddScoped<IReportBuilder, TechnicalReportBuilder>();

var app = builder.Build();



app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
