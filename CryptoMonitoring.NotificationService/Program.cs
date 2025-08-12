using CryptoMonitoring.NotificationService;
using CryptoMonitoring.NotificationService.Data;
using CryptoMonitoring.NotificationService.Infrastructure;
using CryptoMonitoring.NotificationService.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RazorLight;
using CryptoMonitoring.NotificationService.Hubs;
using Microsoft.AspNetCore.SignalR;

using Serilog;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.Configure<EmailOptions>(config.GetSection("Email"));
builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMq"));

builder.Services.AddDbContext<NotificationsDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IMongoClient>(
    _ => new MongoClient(config.GetConnectionString("Mongo")));

builder.Services.AddSingleton<IRazorLightEngine>(new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(NotificationService))
    .UseMemoryCachingProvider()
    .Build());

builder.Services.AddHttpClient<WebhookSender>();
builder.Services.AddTransient<IChannelSender, EmailSender>();
builder.Services.AddTransient<IChannelSender, WebhookSender>();

builder.Services.AddTransient<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationWorker>();

builder.Services.AddSignalR();

var app = builder.Build();
app.MapHub<NotificationsHub>("/notifications");
app.Run();
