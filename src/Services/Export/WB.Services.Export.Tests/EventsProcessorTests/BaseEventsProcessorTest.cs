using System.Threading.Tasks;
using Moq;
using WB.Services.Export.Events;
using WB.ServicesIntegration.Export;
using IHeadquartersApi = WB.ServicesIntegration.Export.IHeadquartersApi;

namespace WB.Services.Export.Tests.EventsProcessorTests
{
    public class BaseEventsProcessorTest
    {
        protected void SetupEvents(Mock<IHeadquartersApi> api, params (int sequence, EventsFeed feed)[] events)
        {
            foreach (var feedInfo in events)
            {
                api.Setup(s => s.GetInterviewEvents(feedInfo.sequence, It.IsAny<int>()))
                    .Returns(Task.FromResult(feedInfo.feed));
            }
        }
    }
}
