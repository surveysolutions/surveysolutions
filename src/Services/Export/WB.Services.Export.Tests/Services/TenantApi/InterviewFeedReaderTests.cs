using System;
using AutoBogus;
using Newtonsoft.Json;
using NUnit.Framework;
using WB.Services.Export.Events;
using WB.Services.Export.Events.Interview;
using WB.Services.Infrastructure.EventSourcing;
using WB.Services.Infrastructure.EventSourcing.Json;
using WB.ServicesIntegration.Export;

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
                      ""$type"": ""InterviewOnClientCreated"",
                      ""GlobalSequence"": 1,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 1,
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T09:53:12.4357076-05:00"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    },
                    {
                      ""$type"": ""InterviewCreated"",
                      ""GlobalSequence"": 2,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 2,
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T16:31:09.363733+02:00"",
                        ""assignmentId"": 7,
                        ""creationTime"": ""2018-12-28T14:31:09.363733Z"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    },
                    {
                      ""$type"": ""RosterInstancesAdded"",
                      ""GlobalSequence"": 2,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 2,
                      ""Payload"": {
                        ""instances"": [
                          {
                            ""groupId"": ""591c5473-2d33-e4ab-4011-8e272b79ff80"",
                            ""sortIndex"": 0,
                            ""rosterInstanceId"": 1.0,
                            ""outerRosterVector"": [1.0, 2.0]
                          }
                        ]
                      }
                    }
                  ],
                  ""NextSequence"": 3
                }
                ";

            var feed = JsonConvert.DeserializeObject<EventsFeed>(feedJson);

            Assert.That(feed.Total, Is.EqualTo(13164971));

            Assert.That(feed.Events.Count, Is.EqualTo(3));

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

            if (feed.Events[2].Payload is RosterInstancesAdded added)
            {
                Assert.That(added.Instances[0].OuterRosterVector, Is.EquivalentTo(new int[] { 1, 2 }));
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
                      ""$type"": ""InterviewOnClientCreated"",
                      ""GlobalSequence"": 1,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 1,                      
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T09:53:12.4357076-05:00"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      }
                    },
                    {
                      ""$type"": ""InterviewCreated2"",
                      ""Payload"": {
                        ""userId"": ""bc606b47-d1d7-4fff-b032-41ef0c9c7635"",
                        ""originDate"": ""2018-12-28T16:31:09.363733+02:00"",
                        ""assignmentId"": 7,
                        ""creationTime"": ""2018-12-28T14:31:09.363733Z"",
                        ""questionnaireId"": ""12aabc0b-963d-4afc-b67f-1f8b838a094e"",
                        ""questionnaireVersion"": 1,
                        ""usesExpressionStorage"": true
                      },
                      ""GlobalSequence"": 2,
                      ""EventSourceId"": ""98b5d3f4-7c89-4f40-820b-158b3977c3f2"",
                      ""Sequence"": 2                      
                    }
                  ],
                   ""NextSequence"": 3
                }";

            var feed = JsonConvert.DeserializeObject<EventsFeed>(feedJson);

            Assert.That(feed.Events[1].Payload, Is.Null);
        }

        [Test]
        public void can_deserialize_event()
        {
            var ev ="{\r\n  \"userId\": \"bc606b47-d1d7-4fff-b032-41ef0c9c7635\",\r\n" +
                "  \"originDate\": \"2018-12-28T16:53:12.4357076+02:00\",\r\n " +
                " \"questionnaireId\": \"12aabc0b-963d-4afc-b67f-1f8b838a094e\",\r\n  " +
                "\"questionnaireVersion\": 1,\r\n  \"usesExpressionStorage\": true\r\n}";

            var obj = JsonConvert.DeserializeObject<InterviewOnClientCreated>(ev);

            Assert.That(obj.UserId, Is.EqualTo(Guid.Parse("bc606b47-d1d7-4fff-b032-41ef0c9c7635")));
            Assert.That(obj.QuestionnaireId, Is.EqualTo(Guid.Parse("12aabc0b-963d-4afc-b67f-1f8b838a094e")));
        }

        [Test]
        public void can_deserialize_staticTextDeclaredInvalid()
        {
            var json = @"{
              ""$type"": ""StaticTextsDeclaredInvalid"",
              ""Payload"": {
                ""originDate"": ""2018-12-06T13:51:50.3777853+02:00"",
                ""failedValidationConditions"": [
                  {
                    ""key"": {
                      ""id"": ""ed668113-2534-36a1-318c-6ba1ab8233ea"",
                      ""rosterVector"": []
                    },
                    ""value"": [{}]
                  }
                ]
              }
            }";

            var envelop = JsonConvert.DeserializeObject<Event>(json);
            var @event =  envelop.Payload as StaticTextsDeclaredInvalid;
            
            Assert.NotNull(@event?.FailedValidationConditions);
            Assert.That(@event?.FailedValidationConditions[0].Key, Is.EqualTo(Create.Identity("ed668113-2534-36a1-318c-6ba1ab8233ea")));
        }

        [Test]
        public void can_deserialize_VariablesChanged()
        {
            var json = @"{  ""changedVariables"": [
                    {
                      ""identity"": {
                        ""id"": ""cb950143-b85d-4c23-b9ff-7620fc756464"",
                        ""rosterVector"": {
                          ""$type"": ""WB.Core.SharedKernels.DataCollection.RosterVector, WB.Core.SharedKernels.DataCollection.Portable"",
                          ""$values"": [1.0,2.0]
                        }
                      },
                      ""newValue"": ""Rajasthan""
                    }
                  ] }";

            var ev = JsonConvert.DeserializeObject<VariablesChanged>(json, new RosterVectorJsonConverter(),
                new IdentityJsonConverter());

            Assert.That(ev.ChangedVariables[0].Identity, Is.EqualTo(
                Create.Identity("cb950143-b85d-4c23-b9ff-7620fc756464", 1, 2)));

            Assert.That(ev.ChangedVariables[0].NewValue, Is.EqualTo("Rajasthan"));
        }

        [Test]
        public void can_deserialize_AnswersDeclaredInvalid()
        {
            var json = @"{""questions"": [  {
                  ""id"": ""5e156e5b-5ebd-4684-5bf6-7d11514f9461"",
                  ""rosterVector"": {
                    ""$type"": ""WB.Core.SharedKernels.DataCollection.RosterVector, WB.Core.SharedKernels.DataCollection.Portable"",
                    ""$values"": [1]
                  }
                }],
              ""failedConditionsStorage"": [
                {
                  ""key"": {
                    ""id"": ""661de5b9-05c4-177b-57ed-34831bfad1a1"",
                    ""rosterVector"": {
                      ""$type"": ""WB.Core.SharedKernels.DataCollection.RosterVector, WB.Core.SharedKernels.DataCollection.Portable"",
                      ""$values"": [-23.00, 4,5,6,7]
                    }
                  },
                  ""value"": [{}]
                }
              ]
            }";

            var ev = JsonConvert.DeserializeObject<AnswersDeclaredInvalid>(json,
                new RosterVectorJsonConverter(), new IdentityJsonConverter());

            var question = Create.Identity("5e156e5b-5ebd-4684-5bf6-7d11514f9461", 1);
            Assert.That(ev.Questions[0], Is.EqualTo(question));

            var faildCondition = Create.Identity("661de5b9-05c4-177b-57ed-34831bfad1a1", -23, 4, 5, 6, 7);
            Assert.That(ev.FailedValidationConditions[faildCondition][0].FailedConditionIndex, Is.EqualTo(0));
        }
    }
}
