using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests
{
    [Subject(typeof(UserChangedFeedReader))]
    public class when_recent_feed_contains_one_new_event
    {
        private Establish context = () =>
        {
            var handler = new Mock<HttpMessageHandler>();

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .Returns(Task<HttpResponseMessage>.Factory.StartNew(() => new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(ResourceHelper.ReadResourceFile("WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.Simplefeed.xml"))
                }));

            lastStoredEntry = JsonConvert.DeserializeObject<LocalUserChangedFeedEntry>(@"{
      ""ChangedUserId"": ""8a25e5113b214d26a48ad6b50379afce"",
      ""Timestamp"": ""2014-04-09T16:25:51.6176054Z"",
      ""SupervisorId"": ""8a09d88f04b04eff9f471e25903540b1"",
      ""EntryId"": ""preLastEntry""
      }");

            feedReader = Create.UserChangedFeedReader(messageHandler: () => handler.Object);
        };

        private Because of = () => feedEntries = feedReader.ReadAfterAsync(lastStoredEntry).Result;

        It should_read_one_from_the_feed = () => feedEntries.Count.ShouldEqual(1);

        It should_read_last_new_event_from_feed = () => feedEntries.Last().EntryId.ShouldEqual("lastEntry");

        private static UserChangedFeedReader feedReader;
        private static List<LocalUserChangedFeedEntry> feedEntries;
        private static LocalUserChangedFeedEntry lastStoredEntry;
    }
}