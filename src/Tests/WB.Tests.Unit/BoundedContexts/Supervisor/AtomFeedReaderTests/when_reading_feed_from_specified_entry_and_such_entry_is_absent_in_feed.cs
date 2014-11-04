using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.BoundedContexts.Supervisor.AtomFeedReaderTests
{
    public class when_reading_feed_from_specified_entry_and_such_entry_is_absent_in_feed
    {
        Establish context = () =>
        {
            var handler = Mock.Of<HttpMessageHandler>();

            handler.SetupResponseFromResource("http://localhost/feed", "WB.Tests.Unit.BoundedContexts.Supervisor.AtomFeedReaderTests.SimpleStringFeed.xml");
            handler.SetupResponseFromResource("http://localhost/feed/archive", "WB.Tests.Unit.BoundedContexts.Supervisor.AtomFeedReaderTests.SimpleStringArchiveFeed.xml");

            reader = Create.AtomFeedReader(messageHandler: () => handler);
        };

        Because of = () =>
            exception = Catch.Exception(() =>
            {
                var x = reader.ReadAfterAsync<string>(new Uri("http://localhost/feed"), "absentFeedEntry").Result;
            });

        It should_throw_exception_with_message_containing__find____entry____feed__ = () =>
            exception.InnerException.Message.ToLower().ToSeparateWords().ShouldContain("find", "entry", "feed");

        private static Exception exception;
        private static AtomFeedReader reader;
    }
}
