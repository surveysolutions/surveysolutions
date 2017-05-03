using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable.Implementation.Services;

namespace WB.Tests.Unit.GenericSubdomains.Utils.HttpStatisticianTests
{
    public class HttpStatisticianTests
    {
        [TestCase("en-US")]
        [TestCase("ru-RU")]
        [TestCase("de-GE")]
        public async Task should_collect_uploaded_data(string cultureInfo)
        {
            // arrange
            var statistician = new HttpStatistician();

            using (var httpTest = new HttpTest())
            {
                httpTest.ResponseQueue.Enqueue(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("response content with the length long enough and equal to 61"),
                    Headers = { {"Server-Timing", "db=1, action=0.125 , hardwork=2"} } // estimated headers size ~ 43
                });

                await "http://example.com"
                    .WithBasicAuth("User", "Password")
                    .ConfigureClient(s =>
                    {
                        s.AfterCall = httpCall =>
                        {
                            // setting call duration to 1.125 seconds
                            var timepoint = DateTime.UtcNow;
                            httpCall.StartedUtc = timepoint.AddSeconds(-1.125);
                            httpCall.EndedUtc = timepoint;

                            // act
                            var culture = Thread.CurrentThread.CurrentCulture;
                            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureInfo);
                            statistician.CollectHttpCallStatistics(httpCall);
                            Thread.CurrentThread.CurrentCulture = culture;
                        };
                    })
                    .PostStringAsync("Just a sample text to add some content to fake request with length 70");
            }

            // assert
            var stats = statistician.GetStats();

            Assert.That(stats.DownloadedBytes, Is.EqualTo(104), "Should take into account estimated headers size.");
            Assert.That(stats.UploadedBytes, Is.EqualTo(108), "Should take into account estimated headers size.");

            Assert.That(stats.Speed, Is.EqualTo(104 + 108), "Duration should be 1 second, so speed is equal to bytes");

            Assert.That(stats.Duration, Is.EqualTo(TimeSpan.FromSeconds(1)), "Should take into account server side action processing time");
        }
    }
}
