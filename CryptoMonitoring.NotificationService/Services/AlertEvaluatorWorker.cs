using CryptoMonitoring.NotificationService.Data;
using CryptoMonitoring.NotificationService.Models;
using Microsoft.Extensions.Hosting;

namespace CryptoMonitoring.NotificationService.Services
{
    public class AlertEvaluatorWorker : BackgroundService
    {
        private readonly IAlertRuleRepository _rules;
        private readonly INotificationService _notifier;
        private readonly ILogger<AlertEvaluatorWorker> _logger;
        private readonly TimeSpan _pollInterval;

        public AlertEvaluatorWorker(
            IAlertRuleRepository rules,
            INotificationService notifier,
            ILogger<AlertEvaluatorWorker> logger,
            IConfiguration config)
        {
            _rules = rules;
            _notifier = notifier;
            _logger = logger;

            var seconds = config.GetValue<int?>("Alerts:PollIntervalSeconds") ?? 30;
            _pollInterval = TimeSpan.FromSeconds(Math.Max(5, seconds));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AlertEvaluatorWorker started (interval: {Interval}s)", _pollInterval.TotalSeconds);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await EvaluateAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error while evaluating alerts");
                }

                try
                {
                    await Task.Delay(_pollInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            _logger.LogInformation("AlertEvaluatorWorker stopped");
        }

        private async Task EvaluateAsync(CancellationToken ct)
        {
            var rules = await _rules.GetAllEnabledAsync(ct);
            if (rules.Count == 0) return;

            foreach (var rule in rules)
            {
                try
                {
                    var value = await FetchMetricAsync(rule.Metric, ct);
                    if (!ShouldTrigger(rule, value)) continue;

                    if (!IsCooldownPassed(rule))
                    {
                        _logger.LogDebug("Cooldown not passed for rule {RuleId}", rule.Id);
                        continue;
                    }

                    var alert = new AlertTriggeredEvent
                    {
                        RuleId = rule.Id,
                        Metric = rule.Metric,
                        Operator = rule.Operator,
                        Threshold = rule.Threshold,
                        CurrentValue = value,
                        Severity = rule.Severity,
                        Description = rule.Description,
                        Channels = rule.Channels,
                        Targets = rule.Targets,
                        OccurredAtUtc = DateTime.UtcNow
                    };

                    await _notifier.AlertAsync(alert, ct);

                    await _rules.UpdateLastTriggeredAsync(rule.Id, DateTime.UtcNow, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while processing rule {RuleId}", rule.Id);
                }
            }
        }

        private static bool ShouldTrigger(AlertRule rule, double value)
        {
            return rule.Operator switch
            {
                ThresholdOperator.GreaterOrEqual => value >= rule.Threshold,
                ThresholdOperator.LessOrEqual => value <= rule.Threshold,
                _ => false
            };
        }

        private static bool IsCooldownPassed(AlertRule rule)
        {
            if (rule.LastTriggeredAt == null) return true;
            var since = DateTime.UtcNow - rule.LastTriggeredAt.Value;
            return since.TotalSeconds >= Math.Max(0, rule.CooldownSeconds);
        }

        //переделать
        private static Task<double> FetchMetricAsync(string metricKey, CancellationToken ct)
        {
            var seed = metricKey.GetHashCode();
            var rnd = new Random(unchecked(Environment.TickCount ^ seed));
            return Task.FromResult(rnd.NextDouble() * 100.0); 
        }
    }
}
