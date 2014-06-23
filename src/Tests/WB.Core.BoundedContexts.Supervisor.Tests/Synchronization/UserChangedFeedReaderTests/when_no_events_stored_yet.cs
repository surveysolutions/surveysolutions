using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Machine.Specifications;
using Moq;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
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
            handler.SetupResponseFromResource("http://localhost/feed",
                "WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.Simplefeed.xml");
            handler.SetupResponseFromResource("http://localhost/Headquarters/api/feeds/users/v1/archive/2",
                "WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.SimpleArchiveFeed.xml");

            var settings = Create.HeadquartersSettings(usersChangedFeedUri: new Uri("http://localhost/feed")); 
            feedReader = Create.UserChangedFeedReader(messageHandler: () => handler.Object,
                settings: settings);
        };

        Because of = () => feedEntries = feedReader.ReadAfterAsync(null).Result;

        It should_read_all_events_from_feed = () => feedEntries.Count.ShouldEqual(4);

        It should_read_details_uri = () => feedEntries.First().UserDetailsUri.ShouldEqual(new Uri("http://localhost/Headquarters/api/resources/users/v1/40ad9dff03d24137997db418d0f3ae1b"));

        static UserChangedFeedReader feedReader;
        static List<LocalUserChangedFeedEntry> feedEntries;
    }
}