using SabaSimulator.API.Helper;

namespace SabaSimulator.API.CronJob;

public class ClearMemoryDbJob(ILogger<ClearMemoryDbJob> logger, IMemoryDb memoryDb) : IHostedService, IDisposable
{
	private Timer? _timer = null;

	public Task StartAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Run Clear MemoryDb Job");

		var now = DateTime.Now;
		var nextRun = now.Date.Add(new TimeSpan(1, 0, 0));

		// If the specified time has passed for today, schedule for the same time tomorrow
		if (nextRun <= now)
		{
			nextRun = nextRun.AddDays(1);
		}

		var initialDelay = nextRun - now;
		_timer = new Timer(DoWork, null, initialDelay, TimeSpan.FromDays(1));

		return Task.CompletedTask;
	}

	private void DoWork(object? state)
	{
		logger.LogInformation("Clearing Saba Sport Tickets");

		memoryDb.ClearTicketsAsync();
	}

	public Task StopAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("ClearMemoryDbJob is stopping.");
		_timer?.Change(Timeout.Infinite, 0);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_timer?.Dispose();
		GC.SuppressFinalize(this);
	}
}