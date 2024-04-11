using Microsoft.Extensions.Logging;

namespace Moonstone.Framework;

public abstract class BackgroundWorker<TWorker> : IDisposable
{
    public State State { get; private set; } = State.Initializing;
    
    private Task? _backgroundTask;
    private readonly ILogger<TWorker> _logger;

    protected BackgroundWorker(CancellationToken ct, ILogger<TWorker> logger)
    {
        _logger = logger;
    }

    public void StartBackgroundWorker(CancellationToken ct)
    {
        _backgroundTask = Task.Run(async () =>
        {
            _logger.LogInformation("Initializing");

            try
            {
                await Initialize(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured during initialization");
                throw;
            }
            
            _logger.LogInformation("Initialized");

            while (!ct.IsCancellationRequested)
            {
                try
                {
                    State = State.Running;
                    await ProcessWork(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occured during processing of work");
                }
                finally
                {
                    State = State.Idle;
                }

                await Task.Delay(200, ct);
            }

            var cts = new CancellationTokenSource();
            cts.CancelAfter(2000);
            
            await Stop(cts.Token);
        }, ct);
    }
    
    protected abstract Task Initialize(CancellationToken ct);
    protected abstract Task ProcessWork(CancellationToken ct);

    protected virtual Task Stop(CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _backgroundTask.Dispose();
    }
}

public enum State
{
    Initializing,
    Running,
    Idle,
}