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
    [Subject(typeof(UserChangedFeedReader))]
    public class when_recent_feed_has_no_last_stored_event
    {
        Establish context = () =>
        {
            var handler = new Mock<HttpMessageHandler>();
            var settings = Create.HeadquartersSettings(usersChangedFeedUri: new Uri("http://localhost/feed"));

            handler.SetupResponseFromResource("http://localhost/feed", 
                "WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.Simplefeed.xml");
            handler.SetupResponseFromResource("http://localhost/Headquarters/api/feeds/users/v1/archive/2", 
                "WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.UserChangedFeedReaderTests.SimpleArchiveFeed.xml");

            lastStoredEntry = JsonConvert.DeserializeObject<LocalUserChangedFeedEntry>(@"{
      ""ChangedUserId"": ""40ad9dff03d24137997db418d0f3ae1b"",
      ""Timestamp"": ""2014-04-09T15:33:38.9251308Z"",
      ""SupervisorId"": ""40ad9dff03d24137997db418d0f3ae1b"",
      ""EntryId"": ""preLastArchiveEntry""
      }");

            feedReader = Create.UserChangedFeedReader(messageHandler: () => handler.Object,
                settings: settings);
        };

        Because of = () => feedEntries = feedReader.ReadAfterAsync(lastStoredEntry).Result;

        It should_append_events_from_archive_feed_and_from_recent_feed = () => feedEntries.Count.ShouldEqual(3);

        It should_append_events_from_acrchive_feed_that_are_after_requested_event = () => feedEntries.First().EntryId.ShouldEqual("lastArchiveEntry");

        It should_end_with_last_event_in_recent = () => feedEntries.Last().EntryId.ShouldEqual("lastEntry");

        private static UserChangedFeedReader feedReader;
        private static List<LocalUserChangedFeedEntry> feedEntries;
        private static LocalUserChangedFeedEntry lastStoredEntry;
    }
}