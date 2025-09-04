using CryptoMonitoring.NotificationService;
using CryptoMonitoring.NotificationService.Data;
using CryptoMonitoring.NotificationService.Hubs;
using CryptoMonitoring.NotificationService.Infrastructure;
using CryptoMonitoring.NotificationService.Services;
using HealthChecks.MongoDb;
using HealthChecks.NpgSql;
using HealthChecks.RabbitMQ;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using RazorLight;
using Serilog;
using System;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Host.UseSerilog((ctx, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration);
});

builder.Services.Configure<EmailOptions>(config.GetSection("Email"));
builder.Services.Configure<RabbitMqOptions>(config.GetSection("RabbitMq"));
builder.Services.Configure<TelegramOptions>(config.GetSection("Telegram"));

builder.Services.AddDbContext<NotificationsDbContext>(opt =>
    opt.UseNpgsql(config.GetConnectionString("Postgres")));

builder.Services.AddSingleton<IMongoClient>(_ =>
    new MongoClient(config.GetConnectionString("Mongo")));

builder.Services.AddSingleton<IRazorLightEngine>(new RazorLightEngineBuilder()
    .UseEmbeddedResourcesProject(typeof(NotificationService))
    .UseMemoryCachingProvider()
    .Build());

builder.Services.AddHttpClient<WebhookSender>();
builder.Services.AddTransient<IChannelSender, EmailSender>();
builder.Services.AddTransient<IChannelSender, WebhookSender>();
builder.Services.AddHttpClient<TelegramSender>();
builder.Services.AddSingleton<IChannelSender, TelegramSender>();
builder.Services.AddSingleton<IChannelSender, SignalRPushSender>();

builder.Services.AddTransient<IRabbitMqConnectionFactory, RabbitMqConnectionFactory>();
builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();

builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHostedService<NotificationWorker>();

builder.Services.Configure<MongoSettings>(
    builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddScoped<IAlertRuleRepository, MongoAlertRuleRepository>();

builder.Services.AddHostedService<AlertEvaluatorWorker>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, QueryStringUserIdProvider>();

builder.Services.AddHealthChecks()
    .AddNpgSql(config.GetConnectionString("Postgres")!)
    .AddMongoDb(sp => sp.GetRequiredService<IMongoClient>())
    .AddRabbitMQ(new Uri(config.GetConnectionString("RabbitMq")!));

var app = builder.Build();

app.MapHub<NotificationsHub>("/notifications");

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
