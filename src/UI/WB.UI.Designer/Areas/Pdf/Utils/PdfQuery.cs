using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.UI.Designer.Areas.Pdf.Services;

namespace WB.UI.Designer.Areas.Pdf.Utils;

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

public class PdfQuery : IPdfQuery
{
    private readonly int maxPerUser;
    private readonly ConcurrentQueue<PdfJob> queue = new();
    private readonly ConcurrentDictionary<string, PdfJob> jobs = new();
    private readonly ConcurrentDictionary<Guid, int> perUserCount = new();
    private readonly SemaphoreSlim signal = new(0);

    public PdfQuery(IOptions<PdfSettings> options)
    {
        this.maxPerUser = options.Value.MaxPerUser;
        var workerCount = options.Value.WorkerCount;

        for (int i = 0; i < workerCount; i++)
            Task.Run(WorkerLoop);
    }

    public PdfGenerationProgress GetOrAdd(
        Guid userId, 
        string key,
        Func<PdfGenerationProgress, Task> runGeneration)
    {
        if (jobs.TryGetValue(key, out var existing))
            return existing.Progress;

        int current = perUserCount.GetOrAdd(userId, 0);
        if (current >= maxPerUser)
        {
            throw new PdfLimitReachedException(maxPerUser);
        }

        var job = new PdfJob(key, userId, runGeneration);

        if (!jobs.TryAdd(key, job))
            return jobs[key].Progress;

        queue.Enqueue(job);
        perUserCount.AddOrUpdate(userId, 1, (_, old) => old + 1);
        signal.Release();

        return job.Progress;
    }

    public void Remove(string key)
    {
        jobs.TryRemove(key, out var job);
    }

    public PdfGenerationProgress? GetOrNull(string key)
    {
        return jobs.TryGetValue(key, out var job) ? job.Progress : null;
    }

    public string GetQueryInfoJson()
    {
        var queueInfo = new
        {
            QueueSize = queue.Count,
            ActiveJobs = jobs.Count
        };

        var userLimits = perUserCount.Select(userCount => new
        {
            UserId = userCount.Key,
            CurrentTasks = userCount.Value,
            MaxAllowed = maxPerUser,
            Remaining = Math.Max(0, maxPerUser - userCount.Value)
        }).ToArray();

        var activeJobs = jobs.Select(job => new
        {
            Key = job.Key,
            Status = job.Value.Progress.IsFinished ? "Finished" : (job.Value.Progress.IsFailed ? "Failed" : "In Progress"),
            UserId = job.Value.UserId,
            ElapsedTime = job.Value.Progress.IsFinished ? job.Value.Progress.TimeSinceFinished.ToString() : null
        }).ToArray();

        var result = new
        {
            QueueStatus = queueInfo,
            UserLimits = userLimits,
            ActiveJobs = activeJobs
        };

        return System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
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
                    // decrease user count limit
                    perUserCount.AddOrUpdate(job.UserId, 0, (_, old) => Math.Max(0, old - 1));
                }
            }
        }
    }
}
