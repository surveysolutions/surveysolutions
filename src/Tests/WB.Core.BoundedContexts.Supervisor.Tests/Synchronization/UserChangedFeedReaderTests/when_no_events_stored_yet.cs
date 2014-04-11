using System;
using System.Collections.Generic;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Users;
using It = Machine.Specifications.It;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests
{
    public class when_no_events_stored_yet
    {
        Establish context = () =>
        {
            var handler = new Mock<HttpMessageHandler>();
            var settings = new HeadquartersSettings(new Uri("http://localhost"), new Uri("http://localhost/feed"));

            handler.SetupResponseFromResource("http://localhost/feed",
                "WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.Simplefeed.xml");
            handler.SetupResponseFromResource("http://localhost/Headquarters/api/feeds/users/v1/archive/2",
                "WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.SimpleArchiveFeed.xml");

            feedReader = Create.UserChangedFeedReader(messageHandler: handler.Object,
                settings: settings);
        };

        Because of = () => feedEntries = feedReader.ReadAfterAsync(null).Result;

        private It should_read_all_events_from_feed = () => feedEntries.Count.ShouldEqual(4);

        private static UserChangedFeedReader feedReader;
        private static List<UserChangedFeedEntry> feedEntries;
    }
}