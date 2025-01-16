using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using WB.Services.Infrastructure.Tenant;
using WB.Services.Scheduler.Model;
using WB.Services.Scheduler.Model.Events;
using WB.Services.Scheduler.Services.Implementation;

namespace WB.Services.Scheduler.Tests.JobServiceTests
{
    [TestFixture]
    public class JobServiceTests : with_scheduler
    {
        [SetUp]
        public void Setup()
        {
            var serviceCollection = NewServiceCollection();

            serviceCollection.AddTransient<JobService>();

            this.serviceProvider = serviceCollection.BuildServiceProvider();
        }

        [Test]
        public async Task should_not_return_any_jobs_if_there_is_no_jobs()
        {
            var jobService = serviceProvider.GetService<JobService>();

            var job = await jobService.GetFreeJobAsync();
            Assert.That(job, Is.Null);
        }

        [Test]
        public async Task should_return_free_job_if_there()
        {
            var jobService = serviceProvider.GetService<JobService>();

            await jobService.AddNewJobAsync(Create.Entity.Job());

            var job = await jobService.GetFreeJobAsync();
            Assert.That(job, Is.Not.Null);
        }

        [Test]
        public async Task only_one_worker_should_acquire_job_for_same_tenant()
        {
            var addJobEvent = new ManualResetEvent(false);
            var canRunEvent = new ManualResetEvent(false);
            var sw = new Stopwatch();
            int waitForThreads = 8;

            var tasks = Enumerable.Range(0, waitForThreads).Select(i => Task.Run(() => Runner(i))).ToArray();

            async Task<JobItem> Runner(int id)
            {
                using var scope = serviceProvider.CreateScope();
                var jobService = scope.ServiceProvider.GetService<JobService>();

                if (Interlocked.Decrement(ref waitForThreads) == 0)
                {
                    canRunEvent.Set();
                }

                addJobEvent.WaitOne();

                int attempt = 5;

                while (attempt-- > 0)
                {
                    var job = await jobService.GetFreeJobAsync();
                   
                    if (job != null)
                    {
                        Console.WriteLine($"[{sw.ElapsedMilliseconds}ms] #{id} - got job {job?.Tenant ?? "null"}");
                        return job;
                    }
                }

                return null;
            }

            canRunEvent.WaitOne();

            // generate several hundreds completed jobs for longer DB work
            
            var count = 200;
            Console.WriteLine($"Generating {count} jobs history");
            while (count-- > 0)
            {
                await CreateNewJobs(Create.Entity.Job(tag: "job_" + count, tenant: "tenant_" + count % 13).Cancel(""));
            }

            Console.WriteLine("Adding 4 jobs for 4 tenants to process");

            await CreateNewJobs(
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job1", tenant: "oneTenant"),
                Create.Entity.Job(tag: "job2", tenant: "twoTenant"),
                Create.Entity.Job(tag: "job3", tenant: "threeeTenant"),
                Create.Entity.Job(tag: "job4", tenant: "fourTenant"));

            Console.WriteLine("Start workers");
            sw.Restart();
            addJobEvent.Set();

            var result = await Task.WhenAll(tasks);

            Assert.That(result.Count(r => r != null), Is.EqualTo(4));
        }

        [Test]
        public async Task should_not_return_free_job_if_there_is_already_running_job_for_tenant()
        {
            var tenant1 = new TenantInfo("http://localhost/1", "apiKey1", "test1");
            var tenant2 = new TenantInfo("http://localhost/2", "apiKey2", "test2");

            await CreateNewJobs(
                Create.Entity.Job(tag: "job1", tenant: tenant1.ToString()).Start("worker1"),
                Create.Entity.Job(tag: "job2", tenant: tenant1.ToString()),
                Create.Entity.Job(tag: "job1", tenant: tenant2.ToString()));

            var service = serviceProvider.GetService<JobService>();

            var freeJob = await service.GetFreeJobAsync();

            Assert.That(freeJob.Tenant, Is.EqualTo(tenant2.ToString()));
            Assert.That(freeJob.Tag, Is.EqualTo("job1"));

            Assert.That(await service.GetFreeJobAsync(), Is.Null, "No free jobs should be available");
        }

        [Test]
        public async Task Should_not_return_scheduled_in_future_jobs()
        {
            var job = Create.Entity.Job(scheduledAt: DateTime.UtcNow.AddMinutes(1));

            await CreateNewJobs(job);

            var service = serviceProvider.GetService<JobService>();

            ClassicAssert.Null(await service.GetFreeJobAsync());
        }

        [Test]
        public void should_increase_error_count_on_communication_errors()
        {
            var job = Create.Entity.Job();

            var failedTimes = job.FailedTimes;
            
            job.Handle(new FailJobEvent(job.Id, new HttpRequestException("error", new SocketException())));

            Assert.That(job.FailedTimes, Is.EqualTo(failedTimes + 1));
        }

        [Test]
        public void should_retry_job_specified_max_retry_times()
        {
            var job = Create.Entity.Job();
            job.MaxRetryAttempts = 3;

            job.Handle(new FailJobEvent(job.Id, new Exception()));
            Assert.That(job.FailedTimes, Is.EqualTo(1));
            Assert.That(job.Status, Is.EqualTo(JobStatus.Created));
            Assert.That(job.ShouldDropTenantSchema, Is.EqualTo(false));

            job.Handle(new FailJobEvent(job.Id, new Exception()));
            Assert.That(job.FailedTimes, Is.EqualTo(2));
            Assert.That(job.Status, Is.EqualTo(JobStatus.Created));
            Assert.That(job.ShouldDropTenantSchema, Is.EqualTo(false));

            job.Handle(new FailJobEvent(job.Id, new Exception()));
            Assert.That(job.FailedTimes, Is.EqualTo(3));
            Assert.That(job.Status, Is.EqualTo(JobStatus.Created));
            Assert.That(job.ShouldDropTenantSchema, Is.EqualTo(false));

            job.Handle(new FailJobEvent(job.Id, new Exception()));
            Assert.That(job.FailedTimes, Is.EqualTo(4));
            Assert.That(job.Status, Is.EqualTo(JobStatus.Fail));
            Assert.That(job.ShouldDropTenantSchema, Is.EqualTo(false));
        }

        [Test]
        public async Task should_return_own_running_jobs()
        {
            var tenantA = new TenantInfo("a", "A", "A");
            var tenantB = new TenantInfo("B", "B", "B");

            var runningTenantA = Create.Entity.Job(tenant: tenantA.Id.Id, tag: "jobA").Start("A");
            var runningTenantB = Create.Entity.Job(tenant: tenantB.Id.Id, tag: "jobB").Start("A");

            await CreateNewJobs(runningTenantA, runningTenantB);

            var service = serviceProvider.GetService<JobService>();

            var jobs = await service.GetRunningOrQueuedJobs(tenantA);

            Assert.That(jobs.Count, Is.EqualTo(1));
            Assert.That(jobs[0].Tag, Is.EqualTo("jobA"));
        }
    }
}
