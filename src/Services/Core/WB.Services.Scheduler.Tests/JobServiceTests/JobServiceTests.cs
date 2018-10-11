using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using WB.Services.Infrastructure.Tenant;
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
        public async Task should_not_return_free_job_if_there_is_already_running_job_for_tenant()
        {
            var tenant1 = new TenantInfo("http://localhost/1", "apiKey1", "test1");
            var tenant2 = new TenantInfo("http://localhost/2", "apiKey2", "test2");

            await CreateNewJobs(
                Create.Entity.Job(tag: "job1", tenant: tenant1.ToString()).Start(),
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

            Assert.Null(await service.GetFreeJobAsync());
        }
    }
}
