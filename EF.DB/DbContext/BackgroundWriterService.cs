using Commons.Utilities;

using EF.Data.DBContexts;

using System.Collections.Concurrent;

namespace EF.Data.Services;

public class BackgroundWriterService : IDisposable
{
    private abstract class WriteJobBase
    {
        public int RetryCount { get; set; } = 0;
        public abstract Task ExecuteAsync(DbContext context);
    }

    private class WriteJob : WriteJobBase
    {
        public Func<DbContext, Task> Action { get; }
        public TaskCompletionSource<bool> Tcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public WriteJob(Func<DbContext, Task> action)
        {
            Action = action;
        }

        public override async Task ExecuteAsync(DbContext context)
        {
            await Action(context);
            Tcs.SetResult(true);
        }
    }

    private class WriteJob<T> : WriteJobBase
    {
        public Func<DbContext, Task<T>> Action { get; }
        public TaskCompletionSource<T> Tcs { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public WriteJob(Func<DbContext, Task<T>> action)
        {
            Action = action;
        }

        public override async Task ExecuteAsync(DbContext context)
        {
            var result = await Action(context);
            Tcs.SetResult(result);
        }
    }

    private readonly BlockingCollection<WriteJobBase> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Func<DbContext> _contextFactory;
    private readonly Task _worker;

    public BackgroundWriterService(Func<DbContext> contextFactory)
    {
        _contextFactory = contextFactory;
        _worker = Task.Run(ConsumeQueueAsync);
    }

    public Task EnqueueWriteAsync(Func<DbContext, Task> action)
    {
        var job = new WriteJob(action);
        _queue.Add(job);
        return job.Tcs.Task;
    }

    public Task<T> EnqueueWriteAsync<T>(Func<DbContext, Task<T>> action)
    {
        var job = new WriteJob<T>(action);
        _queue.Add(job);
        return job.Tcs.Task;
    }

    private async Task ConsumeQueueAsync()
    {
        foreach (var job in _queue.GetConsumingEnumerable(_cts.Token))
        {
            bool success = false;
            while (!success && job.RetryCount < 3)
            {
                using var context = _contextFactory();
                try
                {
                    await job.ExecuteAsync(context);
                    success = true;
                }
                catch (Exception ex)
                {
                    job.RetryCount++;
                    Logger.WriteLogAndTrace(LogTypes.Debug, $"[WriteJob Error - Retry {job.RetryCount}] {ex.Message}");

                    if (job.RetryCount >= 3)
                    {
                        switch (job)
                        {
                            case WriteJob wj:
                                wj.Tcs.TrySetException(ex);
                                break;
                            case WriteJob<object> wjo:
                                wjo.Tcs.TrySetException(ex);
                                break;
                        }
                    }

                    await Task.Delay(300);
                }
            }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _queue.CompleteAdding();
        _worker.Wait();
    }
}
