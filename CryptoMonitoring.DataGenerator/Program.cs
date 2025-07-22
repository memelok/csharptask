using Serilog;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CryptoMonitoring.DataGenerator.Services;
using CryptoMonitoring.DataGenerator;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();

builder.Services.AddHttpClient<CoinGeckoClient>(client =>
{
    var baseUrl = builder.Configuration["Api:CoinGecko:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.EndsWith('/'))
        baseUrl += "/";
    client.BaseAddress = new Uri(baseUrl ?? "https://api.coingecko.com/api/v3/");
});

builder.Services.AddSingleton<ICryptoApiClient>(sp =>
    sp.GetRequiredService<CoinGeckoClient>());


builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMq");
    var host = cfg["Host"] ?? "rabbitmq";
    var user = cfg["User"] ?? "guest";
    var pass = cfg["Password"] ?? "guest";

    Log.Information("→ [RabbitMQ] Connecting to {Host} as {User}", host, user);

    var factory = new ConnectionFactory
    {
        HostName = host,
        UserName = user,
        Password = pass,
        DispatchConsumersAsync = true,
        RequestedConnectionTimeout = TimeSpan.FromSeconds(5)
    };

    IConnection? connection = null;

    for (int attempt = 1; attempt <= 10; attempt++)
    {
        try
        {
            connection = factory.CreateConnection();
            Log.Information("✔ RabbitMQ connected on attempt #{Attempt}", attempt);
            break;
        }
        catch (BrokerUnreachableException ex)
        {
            Log.Warning("⚠ RabbitMQ not available (Attempt #{Attempt}): {Message}", attempt, ex.Message);
            Thread.Sleep(2000);
        }
    }

    if (connection == null || !connection.IsOpen)
    {
        Log.Error("❌ Could not connect to RabbitMQ.");
        throw new InvalidOperationException("RabbitMQ connection failed");
    }

    while (!connection.IsOpen)
    {
        Log.Debug("⏳ Waiting for RabbitMQ connection...");
        Thread.Sleep(1000);
    }

    Log.Information("✔ RabbitMQ connection is active");
    return connection;
});

builder.Services.AddSingleton<IModel>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMq");
    var exchange = cfg["Exchange"] ?? "crypto-data";
    var queue = cfg["Queue"] ?? "market.prices";
    var routingKey = queue;

    var channel = sp.GetRequiredService<IConnection>().CreateModel();
    var logger = sp.GetRequiredService<ILogger<Program>>();

    logger.LogInformation("→ Declaring exchange {Exchange}", exchange);
    channel.ExchangeDeclare(exchange, ExchangeType.Direct, durable: true);

    logger.LogInformation("→ Declaring queue {Queue}", queue);
    channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);

    channel.QueueBind(queue, exchange, routingKey);
    logger.LogInformation("✔ Queue {Queue} bound to exchange {Exchange}", queue, exchange);

    return channel;
});

builder.Services.AddHostedService<MarketDataGenerator>();

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.Run();
