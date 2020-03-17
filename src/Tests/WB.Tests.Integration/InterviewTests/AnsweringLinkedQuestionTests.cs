using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;


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

                var interview = SetupInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument, new List<object>());
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

                var interview = SetupInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument, new List<object>());
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

                var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);
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

        [Test]
        public void when_answering_multi_question_linked_to_list_then_question_dependent_on_linked_to_list_with_filter_should_show_only_answered_options()
        {
            var userId = Id.g1;

            var questionnaireId = Id.g2;
            var linkedToListId = Id.g3;
            var linkedToListFilteredId = Id.g4;
            var listQuestionId = Id.g5;

            int[] GetOptionsFromLinkedToListFilteredQuestion(StatefulInterview interview) =>
                interview.GetMultiOptionLinkedToListQuestion(Create.Identity(linkedToListFilteredId)).Options;

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(listQuestionId, variable: "list"),
                    Create.Entity.MultipleOptionsQuestion(linkedToListId, variable: "linkedtolist",
                        linkedToQuestionId: listQuestionId),
                    Create.Entity.MultipleOptionsQuestion(linkedToListFilteredId,
                        linkedToQuestionId: listQuestionId, variable: "linkedtolistfiltered", optionsFilterExpression: "linkedtolist.Contains(@optioncode)")
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                var whenListIsEmpty = GetOptionsFromLinkedToListFilteredQuestion(interview);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                var whenListHas4Answers = GetOptionsFromLinkedToListFilteredQuestion(interview);

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {1, 3});

                var whenLinkedToListHas2Options = GetOptionsFromLinkedToListFilteredQuestion(interview);

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {3});

                var whenLinkedToListRemoved1Option = GetOptionsFromLinkedToListFilteredQuestion(interview);

                return new
                {
                    whenListIsEmpty, 
                    whenListHas4Answers, 
                    whenLinkedToListHas2Options,
                    whenLinkedToListRemoved1Option
                };
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.whenListIsEmpty, Is.Empty);
            Assert.That(results.whenListHas4Answers, Is.Empty);
            Assert.That(results.whenLinkedToListHas2Options, Is.EqualTo(new[] {1, 3}));
            Assert.That(results.whenLinkedToListRemoved1Option, Is.EqualTo(new[] {3}));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_answering_single_question_linked_to_list_then_question_dependent_on_linked_to_list_with_filter_should_show_only_answered_options()
        {
            var userId = Id.g1;

            var questionnaireId = Id.g2;
            var linkedToListId = Id.g3;
            var linkedToListFilteredId = Id.g4;
            var listQuestionId = Id.g5;

            int[] GetOptionsFromLinkedToListFilteredQuestion(StatefulInterview interview) =>
                interview.GetSingleOptionLinkedToListQuestion(Create.Identity(linkedToListFilteredId)).Options;

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(listQuestionId, variable: "list"),
                    Create.Entity.MultipleOptionsQuestion(linkedToListId, variable: "linkedtolist",
                        linkedToQuestionId: listQuestionId),
                    Create.Entity.SingleOptionQuestion(linkedToListFilteredId,
                        linkedToQuestionId: listQuestionId, variable: "linkedtolistfiltered", optionsFilterExpression: "linkedtolist.Contains(@optioncode)")
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                var whenListIsEmpty = GetOptionsFromLinkedToListFilteredQuestion(interview);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                var whenListHas4Answers = GetOptionsFromLinkedToListFilteredQuestion(interview);

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {1, 3});

                var whenLinkedToListHas2Options = GetOptionsFromLinkedToListFilteredQuestion(interview);

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {3});

                var whenLinkedToListRemoved1Option = GetOptionsFromLinkedToListFilteredQuestion(interview);

                return new
                {
                    whenListIsEmpty, 
                    whenListHas4Answers, 
                    whenLinkedToListHas2Options,
                    whenLinkedToListRemoved1Option
                };
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.whenListIsEmpty, Is.Empty);
            Assert.That(results.whenListHas4Answers, Is.Empty);
            Assert.That(results.whenLinkedToListHas2Options, Is.EqualTo(new[] {1, 3}));
            Assert.That(results.whenLinkedToListRemoved1Option, Is.EqualTo(new[] {3}));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_removing_options_from_multi_linked_to_list_question_then_dependent_answered_options_from_filtered_linked_to_list_should_be_removed()
        {
            var userId = Id.g1;

            var questionnaireId = Id.g2;
            var linkedToListId = Id.g3;
            var linkedToListFilteredId = Id.g4;
            var listQuestionId = Id.g5;

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(listQuestionId, variable: "list"),
                    Create.Entity.MultipleOptionsQuestion(linkedToListId, variable: "linkedtolist",
                        linkedToQuestionId: listQuestionId),
                    Create.Entity.MultipleOptionsQuestion(linkedToListFilteredId,
                        linkedToQuestionId: listQuestionId, variable: "linkedtolistfiltered", optionsFilterExpression: "linkedtolist.Contains(@optioncode)")
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {1, 2, 3});

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListFilteredId, RosterVector.Empty,
                    DateTime.Now, new[] {1, 2, 3});

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                return new
                {
                    answerOnLinkedToListFilteredQuestion = interview
                        .GetMultiOptionLinkedToListQuestion(Create.Identity(linkedToListFilteredId)).GetAnswer()
                        ?.CheckedValues
                };
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.answerOnLinkedToListFilteredQuestion, Is.EqualTo(new[] {1, 3}));

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_removing_options_from_single_linked_to_list_question_then_dependent_answered_options_from_filtered_linked_to_list_should_be_removed()
        {
            var userId = Id.g1;

            var questionnaireId = Id.g2;
            var linkedToListId = Id.g3;
            var linkedToListFilteredId = Id.g4;
            var listQuestionId = Id.g5;

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(listQuestionId, variable: "list"),
                    Create.Entity.MultipleOptionsQuestion(linkedToListId, variable: "linkedtolist",
                        linkedToQuestionId: listQuestionId),
                    Create.Entity.SingleOptionQuestion(linkedToListFilteredId,
                        linkedToQuestionId: listQuestionId, variable: "linkedtolistfiltered", optionsFilterExpression: "linkedtolist.Contains(@optioncode)")
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {1, 2, 3});

                interview.AnswerSingleOptionQuestion(userId, linkedToListFilteredId, RosterVector.Empty,
                    DateTime.Now, 2);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                return new
                {
                    answerOnLinkedToListFilteredQuestion = interview
                        .GetSingleOptionLinkedToListQuestion(Create.Identity(linkedToListFilteredId)).GetAnswer()
                        ?.SelectedValue
                };
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.answerOnLinkedToListFilteredQuestion, Is.Null);

            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [Test]
        public void when_removing_options_from_single_linked_to_list_question_then_dependent_answered_options_from_filtered_linked_to_list_should_not_be_removed()
        {
            var userId = Id.g1;

            var questionnaireId = Id.g2;
            var linkedToListId = Id.g3;
            var linkedToListFilteredId = Id.g4;
            var listQuestionId = Id.g5;

            var appDomainContext = AppDomainContext.Create();

            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(listQuestionId, variable: "list"),
                    Create.Entity.MultipleOptionsQuestion(linkedToListId, variable: "linkedtolist",
                        linkedToQuestionId: listQuestionId),
                    Create.Entity.SingleOptionQuestion(linkedToListFilteredId,
                        linkedToQuestionId: listQuestionId, variable: "linkedtolistfiltered", optionsFilterExpression: "linkedtolist.Contains(@optioncode)")
                );

                var interview = SetupStatefullInterviewWithExpressionStorage(appDomainContext.AssemblyLoadContext, questionnaireDocument);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(2, "2"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                interview.AnswerMultipleOptionsQuestion(userId, linkedToListId, RosterVector.Empty,
                    DateTime.Now, new[] {1, 2, 3});

                interview.AnswerSingleOptionQuestion(userId, linkedToListFilteredId, RosterVector.Empty,
                    DateTime.Now, 3);

                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.UtcNow, new[]
                {
                    new Tuple<decimal, string>(1, "1"), 
                    new Tuple<decimal, string>(3, "3"), 
                    new Tuple<decimal, string>(4, "4"), 
                });

                return new
                {
                    answerOnLinkedToListFilteredQuestion = interview
                        .GetSingleOptionLinkedToListQuestion(Create.Identity(linkedToListFilteredId)).GetAnswer()
                        ?.SelectedValue
                };
            });

            Assert.That(results, Is.Not.Null);
            Assert.That(results.answerOnLinkedToListFilteredQuestion, Is.EqualTo(3));

            appDomainContext.Dispose();
            appDomainContext = null;
        }
    }
}
