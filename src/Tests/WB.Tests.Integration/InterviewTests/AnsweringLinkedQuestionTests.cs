using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;


namespace WB.Tests.Integration.InterviewTests
{
    [TestFixture]
    public class AnsweringLinkedQuestionTests : InterviewTestsContext
    {
        [Test]
        public void when_answering_single_option_question_linked_on_roster()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var singleLinkedQuestionId      = Guid.Parse("11111111111111111111111111111111");
            var textListQuestionId      = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId      = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextListQuestion(textListQuestionId, variable: "l"),
                    Abc.Create.Entity.Roster(rosterId, variable: "r", rosterSizeQuestionId: textListQuestionId),
                    Abc.Create.Entity.SingleOptionQuestion(singleLinkedQuestionId, variable: "sl", linkedToRosterId: rosterId)
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());
                interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow, answers: new Tuple<decimal, string>[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionLinkedQuestion(userId, singleLinkedQuestionId, RosterVector.Empty, DateTime.Now, new decimal[] { 3 });

                    return new
                    {
                        SingleOptionQuestionAnswered = GetFirstEventByType<SingleOptionLinkedQuestionAnswered>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.SingleOptionQuestionAnswered, Is.Not.Null);
            Assert.That(results.SingleOptionQuestionAnswered.SelectedRosterVector, Is.EqualTo(new decimal[] { 3 }));
            Assert.That(results.SingleOptionQuestionAnswered.QuestionId, Is.EqualTo(singleLinkedQuestionId));


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_answering_single_option_question_linked_on_roster_placed_in_other_roster()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var singleLinkedQuestionId      = Guid.Parse("11111111111111111111111111111111");
            var multiOptionQuestionId      = Guid.Parse("22222222222222222222222222222222");
            var textListQuestionId      = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var rosterId      = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var roster2Id      = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextListQuestion(textListQuestionId, variable: "tl"),
                    Abc.Create.Entity.Roster(rosterId, variable: "rtl", rosterSizeQuestionId: textListQuestionId),
                    Abc.Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, variable: "m", options: new Answer[]
                    {
                        new Answer() { AnswerCode = 1, AnswerText = "1"}
                    }),
                    Abc.Create.Entity.Roster(roster2Id, variable: "rm", rosterSizeQuestionId: multiOptionQuestionId, children: new []
                    {
                        Abc.Create.Entity.SingleOptionQuestion(singleLinkedQuestionId, variable: "sl", linkedToRosterId: rosterId)
                    })
                );

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument, new List<object>());
                interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow, answers: new Tuple<decimal, string>[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });
                interview.AnswerMultipleOptionsQuestion(userId, multiOptionQuestionId, RosterVector.Empty, DateTime.UtcNow, new int[] {1});

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionLinkedQuestion(userId, singleLinkedQuestionId, Abc.Create.RosterVector(1), DateTime.Now, new decimal[] { 3 });

                    return new
                    {
                        SingleOptionQuestionAnswered = GetFirstEventByType<SingleOptionLinkedQuestionAnswered>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.SingleOptionQuestionAnswered, Is.Not.Null);
            Assert.That(results.SingleOptionQuestionAnswered.SelectedRosterVector, Is.EqualTo(new decimal[] { 3 }));
            Assert.That(results.SingleOptionQuestionAnswered.QuestionId, Is.EqualTo(singleLinkedQuestionId));


            appDomainContext.Dispose();
            appDomainContext = null;
        }


        [Test]
        public void when_answering_question_linked_on_roster_with_filter_options()
        {
            var userId = Guid.Parse("11111111111111111111111111111111");

            var questionnaireId = Guid.Parse("77778888000000000000000000000000");
            var intForEnablementQuestionId      = Guid.Parse("11111111111111111111111111111111");
            var multiOptionQuestionId      = Guid.Parse("22222222222222222222222222222222");
            var textListQuestionId      = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var intQuestionId      = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterId      = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            var groupId      = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.TextListQuestion(textListQuestionId, variable: "tl"),
                    Abc.Create.Entity.NumericIntegerQuestion(intForEnablementQuestionId, "ie"),
                    Abc.Create.Entity.Roster(rosterId, variable: "rtl", rosterSizeQuestionId: textListQuestionId, children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(intQuestionId, variable: "ii")
                    }),
                    Abc.Create.Entity.Group(groupId, children: new IComposite[]
                    {
                        Abc.Create.Entity.MultyOptionsQuestion(multiOptionQuestionId, variable: "m", linkedToRosterId: rosterId,
                            linkedFilter: "ii > 10", enablementCondition: "ie == 1")
                    })
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(questionnaireDocument);
                interview.AnswerTextListQuestion(userId, textListQuestionId, RosterVector.Empty, DateTime.UtcNow, answers: new Tuple<decimal, string>[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, intQuestionId, Abc.Create.RosterVector(1), DateTime.Now, 11);

                    return new
                    {
                        LinkedQuestionOptionsCount = interview.GetLinkedMultiOptionQuestion(Abc.Create.Identity(multiOptionQuestionId)).Options.Count,
                        LinkedOptionsChanged = GetFirstEventByType<LinkedOptionsChanged>(eventContext.Events)
                    };
                }
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.LinkedQuestionOptionsCount, Is.EqualTo(1));
            Assert.That(results.LinkedOptionsChanged, Is.Not.Null);
            Assert.That(results.LinkedOptionsChanged.ChangedLinkedQuestions.Single().Options.Length, Is.EqualTo(1));
            Assert.That(results.LinkedOptionsChanged.ChangedLinkedQuestions.Single().QuestionId.Id, Is.EqualTo(multiOptionQuestionId));

            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
