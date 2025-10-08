using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf;
using WB.UI.Designer.Areas.Pdf.Services;

namespace WB.UI.Designer.Areas.Pdf.Utils;

public class PdfQuery : IPdfQuery
{
    private static readonly TimeSpan JobTimeout = TimeSpan.FromMinutes(10);
    private readonly IOptions<PdfSettings> options;
    private readonly ILogger<PdfQuery> logger;
    private readonly int maxPerUser;
    private readonly ConcurrentQueue<PdfJob> queue = new();
    private readonly ConcurrentDictionary<string, PdfJob> jobs = new();
    private readonly ConcurrentDictionary<Guid, int> perUserCount = new();
    private readonly SemaphoreSlim signal = new(0);

    public PdfQuery(IOptions<PdfSettings> options,
        ILogger<PdfQuery> logger)
    {
        this.options = options;
        this.logger = logger;
        
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

        var job = new PdfJob(key, userId, runGeneration);

        if (!jobs.TryAdd(key, job))
            return jobs[key].Progress;

        int current = perUserCount.GetOrAdd(userId, 0);
        if (current >= maxPerUser)
            throw new PdfLimitReachedException(maxPerUser);

        queue.Enqueue(job);
        perUserCount.AddOrUpdate(userId, 1, (_, old) => old + 1);
        signal.Release();

        return job.Progress;
    }

    public void Remove(string key)
    {
        if (jobs.TryRemove(key, out var job))
        {
            if (!job.StartedTime.HasValue)
            {
                DecreaseUserJobCount(job.UserId);
            }
        }
    }

    public PdfGenerationProgress? GetOrNull(string key)
    {
        return jobs.TryGetValue(key, out var job) ? job.Progress : null;
    }

    public string GetQueryInfoJson()
    {
        var settingsInfo = new
        {
            MaxPerUser = options.Value.MaxPerUser,
            WorkerCount = options.Value.WorkerCount
        };

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
            Status = job.Value.StartedTime.HasValue ? "Started" : job.Value.Progress.IsFinished ? "Finished" : (job.Value.Progress.IsFailed ? "Failed" : "In Progress"),
            UserId = job.Value.UserId,
            StartedTime = job.Value.StartedTime?.ToString(),
            ElapsedTime = job.Value.Progress.IsFinished ? job.Value.Progress.TimeSinceFinished.ToString() : null
        }).ToArray();

        var result = new
        {
            Settings = settingsInfo,
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
                job.StartedTime = DateTime.Now;

                try
                {
                    await job.Work(job.Progress);
                }
                catch(Exception ex)
                {
                    job.Progress.Fail();
                    this.logger.LogError(ex, $"Failed job {job.Key}");
                    await ex.LogNoContextAsync();
                }
                finally
                {
                    DecreaseUserJobCount(job.UserId);
                }
            }
        }
    }
    
    private void DecreaseUserJobCount(Guid userId)
    {
        while (true)
        {
            if (!perUserCount.TryGetValue(userId, out var oldCount))
                break;
            
            var newCount = Math.Max(0, oldCount - 1);
            if (newCount == 0)
            {
                if (perUserCount.TryRemove(userId, out _))
                    break;
            }
            else
            {
                if (perUserCount.TryUpdate(userId, newCount, oldCount))
                    break;
            }
        }
    }
}


