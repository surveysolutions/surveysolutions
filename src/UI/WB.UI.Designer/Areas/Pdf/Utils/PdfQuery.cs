using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using WB.UI.Designer.Areas.Pdf.Services;

namespace WB.UI.Designer.Areas.Pdf.Utils;

public class PdfQuery
{
    private readonly int workerCount;
    private readonly int maxPerUser;
    private readonly ConcurrentQueue<PdfJob> queue = new();
    private readonly ConcurrentDictionary<string, PdfJob> jobs = new();
    private readonly ConcurrentDictionary<Guid, int> perUserCount = new();
    private readonly SemaphoreSlim signal = new(0);

    public PdfQuery(int workerCount, int maxPerUser)
    {
        this.workerCount = workerCount;
        this.maxPerUser = maxPerUser;

        for (int i = 0; i < workerCount; i++)
            Task.Run(WorkerLoop);
    }

    public PdfGenerationProgress GetOrAdd(
        string key,
        Guid userId,
        Func<string, PdfGenerationProgress> startGeneration)
    {
        if (jobs.TryGetValue(key, out var existing))
            return existing.Progress;

        int current = perUserCount.GetOrAdd(userId, 0);
        if (current >= maxPerUser)
            throw new InvalidOperationException(
                $"You have already posted {maxPerUser} requests to create PDF documents."
            );

        var job = new PdfJob(key, userId, (progress) =>
        {
            try
            {
                var pg = startGeneration(key); 
                if (pg.IsFailed) 
                    progress.Fail();
                else 
                    progress.Finish();
            }
            catch
            {
                progress.Fail();
            }

            return Task.CompletedTask;
        });

        if (!jobs.TryAdd(key, job))
            return jobs[key].Progress;

        queue.Enqueue(job);
        perUserCount.AddOrUpdate(userId, 1, (_, old) => old + 1);
        signal.Release();

        return job.Progress;
    }

    public void Remove(string key)
    {
        if (jobs.TryRemove(key, out var job))
        {
            perUserCount.AddOrUpdate(job.UserId, 0, (_, old) => Math.Max(0, old - 1));
        }
    }

    public PdfGenerationProgress? GetOrNull(string key)
    {
        return jobs.TryGetValue(key, out var job) ? job.Progress : null;
    }

    private async Task WorkerLoop()
    {
        while (true)
        {
            await signal.WaitAsync();
            if (queue.TryDequeue(out var job))
            {
                try
                {
                    await job.Work(job.Progress);
                }
                catch
                {
                    job.Progress.Fail();
                }
                finally
                {
                    Remove(job.Key);
                }
            }
        }
    }
}
