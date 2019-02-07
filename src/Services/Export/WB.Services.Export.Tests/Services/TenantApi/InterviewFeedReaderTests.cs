using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview;
using WB.Services.Infrastructure.EventSourcing;

namespace WB.Services.Export.Tests.Services.TenantApi
{
    public class InterviewFeedReaderTests
    {
        [Test]
        public void should_deserialize_events()
        {
            var feedJson =
                @"{
                  ""total"": 13164971,
                  ""Events"": [
                    {
                      ""GlobalSequence"": 1,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 1,
                      ""EventTypeName"": ""InterviewOnClientCreated"",
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T09:53:12.4357076-05:00"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    },
                    {
                      ""GlobalSequence"": 2,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 2,
                      ""EventTypeName"": ""InterviewCreated"",
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T16:31:09.363733+02:00"",
                        ""assignmentId"": 7,
                        ""creationTime"": ""2018-12-28T14:31:09.363733Z"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    }
                  ],
                  ""NextSequence"": 3
                }";

            var feed = JsonConvert.DeserializeObject<EventsFeed>(feedJson);

            Assert.That(feed.Total, Is.EqualTo(13164971));
            Assert.That(feed.NextSequence, Is.EqualTo(3));

            Assert.That(feed.Events.Count, Is.EqualTo(2));

            Assert.That(feed.Events[0].GlobalSequence, Is.EqualTo(1));
            Assert.That(feed.Events[0].Sequence, Is.EqualTo(1));
            Assert.That(feed.Events[0].EventSourceId, Is.EqualTo(Guid.Parse("98b5d3f4-7c89-4f40-820b-158b3977c3f2")));
            Assert.That(feed.Events[0].EventTypeName, Is.EqualTo("InterviewOnClientCreated"));
            Assert.That(feed.Events[0].Payload.GetType(), Is.EqualTo(typeof(InterviewOnClientCreated)));

            if (feed.Events[0].Payload is InterviewOnClientCreated interviewOnClient)
            {
                Assert.That(interviewOnClient.AssignmentId, Is.Null);
                Assert.That(interviewOnClient.UserId, Is.EqualTo(Guid.Parse("bc606b47-d1d7-4fff-b032-41ef0c9c7635")));
                Assert.That(interviewOnClient.QuestionnaireId, Is.EqualTo(Guid.Parse("12aabc0b-963d-4afc-b67f-1f8b838a094e")));
                Assert.That(interviewOnClient.QuestionnaireVersion, Is.EqualTo(1));
                Assert.That(interviewOnClient.UsesExpressionStorage, Is.EqualTo(true));
            }
            else
            {
                Assert.Fail("Payload has wrong type");
            }
        }

        [Test]
        public void deserializer_should_gracefully_handle_unknown_events()
        {
            var feedJson =
                @"{
                  ""total"": 13164971,
                  ""Events"": [
                    {
                      ""GlobalSequence"": 1,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 1,
                      ""EventTypeName"": ""InterviewOnClientCreated"",
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T09:53:12.4357076-05:00"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    },
                    {
                      ""GlobalSequence"": 2,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 2,
                      ""EventTypeName"": ""InterviewCreated2"",
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T16:31:09.363733+02:00"",
                        ""assignmentId"": 7,
                        ""creationTime"": ""2018-12-28T14:31:09.363733Z"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    }
                  ],
                  ""NextPageUrl"": ""/headquarters/api/export/v1/interview/events?sequence=3&pageSize=2""
                }";

            var feed = JsonConvert.DeserializeObject<EventsFeed>(feedJson);

            Assert.That(feed.Events[1].Payload, Is.Null);
        }

        [Test]
        public void can_deserialize_event()
        {
            var ev =
                "{\r\n  \"userId\": \"bc606b47-d1d7-4fff-b032-41ef0c9c7635\",\r\n  \"originDate\": \"2018-12-28T16:53:12.4357076+02:00\",\r\n " +
                " \"questionnaireId\": \"12aabc0b-963d-4afc-b67f-1f8b838a094e\",\r\n  \"questionnaireVersion\": 1,\r\n  \"usesExpressionStorage\": true\r\n}";

            var obj = JsonConvert.DeserializeObject<InterviewOnClientCreated>(ev);

            Assert.That(obj.UserId, Is.EqualTo(Guid.Parse("bc606b47-d1d7-4fff-b032-41ef0c9c7635")));
            Assert.That(obj.QuestionnaireId, Is.EqualTo(Guid.Parse("12aabc0b-963d-4afc-b67f-1f8b838a094e")));
        }

    }

    public class EventHandlerTests
    {
        [Test]
        public void can_resolve_all_handlers()
        {
            var calledEvents = new HashSet<(IEvent, Guid)>();
            IFunctionalHandler testSubj = new TestHandler(calledEvents);

            var events = new []
            {
                new Event {Payload = new AnswersRemoved(), EventSourceId = Id.g1 },
                new Event {Payload = new AnswersRemoved(), EventSourceId = Id.g2}
            };

            foreach (var ev in events)
            {
                testSubj.Handle(ev);
            }
            
            Assert.That(calledEvents.Contains((events[0].Payload, events[0].EventSourceId)));
            Assert.That(calledEvents.Contains((events[1].Payload, events[1].EventSourceId)));
        }

        class TestHandler : IFunctionalHandler,
            IEventHandler<InterviewCreated>,
            IEventHandler<AnswersRemoved>
        {
            private readonly HashSet<(IEvent,Guid)>  track;

            public TestHandler(HashSet<(IEvent, Guid)> track)
            {
                this.track = track;
            }

            public Task SaveStateAsync(CancellationToken cancellationToken = default)
            {
                return Task.CompletedTask;
            }

            public Task HandleAsync(PublishedEvent<InterviewCreated> @event, CancellationToken cancellationToken = default)
            {
                track.Add((@event.Event, @event.EventSourceId));
                return Task.CompletedTask;
            }

            public Task HandleAsync(PublishedEvent<AnswersRemoved> @event, CancellationToken cancellationToken = default)
            {
                track.Add((@event.Event, @event.EventSourceId));
                return Task.CompletedTask;
            }
        }
    }
}
