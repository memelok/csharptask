using Serilog;
using RabbitMQ.Client;
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

builder.Services.AddControllers();

builder.Services.AddHttpClient<CoinGeckoClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Api:CoinGecko:BaseUrl"]);
});
builder.Services.AddSingleton<ICryptoApiClient, CoinGeckoClient>();


builder.Configuration.AddEnvironmentVariables();

builder.Services.AddSingleton<IConnection>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>().GetSection("RabbitMq");
    var host = cfg["Host"] ?? "rabbitmq";
    var user = cfg["User"] ?? "guest";

    Log.Information("→ [RabbitMQ] Connecting to broker at {Host} as {User}", host, user);

    var factory = new ConnectionFactory
    {
        HostName = host,
        UserName = user,
        Password = cfg["Password"] ?? "guest",
        DispatchConsumersAsync = true
    };

    var connection = factory.CreateConnection();

    Log.Information("✔ [RabbitMQ] Connected to {Host}", host);

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

    logger.LogInformation("→ Declaring exchange {Exchange} (type=direct, durable=true)", exchange);
    channel.ExchangeDeclare(
        exchange: exchange,
        type: ExchangeType.Direct,
        durable: true,
        autoDelete: false,
        arguments: null);

    logger.LogInformation("→ Declaring queue {Queue} (durable=true, exclusive=false)", queue);
    channel.QueueDeclare(
        queue: queue,
        durable: true,
        exclusive: false,
        autoDelete: false,
        arguments: null);

    logger.LogInformation(
        "→ Binding queue {Queue} to exchange {Exchange} with routingKey {Key}",
        queue, exchange, routingKey);
    channel.QueueBind(
        queue: queue,
        exchange: exchange,
        routingKey: routingKey,
        arguments: null);

    logger.LogInformation("✔ Queue setup complete");

    return channel;
});

builder.Services.AddHostedService<MarketDataGenerator>();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
