using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_that_change_options_list_of_linked_that_is_one_level_upper_than_source_question_that_is_in_nested_rosterss : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var options = new List<Answer>
                {
                    Abc.Create.Entity.Option("1"),
                    Abc.Create.Entity.Option("2"),
                    Abc.Create.Entity.Option("3"),
                    Abc.Create.Entity.Option("4")
                };

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.Roster(rosterId, variable:"r", fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1),  IntegrationCreate.FixedTitle(2)}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                    {
                        Abc.Create.Entity.Roster(roster1Id, variable:"r1", fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1),  IntegrationCreate.FixedTitle(2)}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                        {
                            Abc.Create.Entity.MultyOptionsQuestion(q1Id, variable: "q1", options: options),
                            Abc.Create.Entity.SingleQuestion(q3Id, variable: "q3", linkedToQuestionId: q2Id, linkedFilter: "age > 19"),
                            Abc.Create.Entity.Roster(roster2Id, variable:"r2", rosterSizeQuestionId: q1Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                            {
                                Abc.Create.Entity.NumericIntegerQuestion(q2Id, variable: "age")
                            })
                        })
                    })
                });

                var interview = SetupStatefullInterview(questionnaireDocument);

                interview.AnswerMultipleOptionsQuestion(userId, q1Id, Abc.Create.Entity.RosterVector(new[] {1, 1}), DateTime.Now, new[] { 1, 2, 3 });
                interview.AnswerMultipleOptionsQuestion(userId, q1Id, Abc.Create.Entity.RosterVector(new[] {1, 2}), DateTime.Now, new[] { 1, 3, 4 });
                interview.AnswerMultipleOptionsQuestion(userId, q1Id, Abc.Create.Entity.RosterVector(new[] {2, 1}), DateTime.Now, new[] { 1, 3 });
                interview.AnswerMultipleOptionsQuestion(userId, q1Id, Abc.Create.Entity.RosterVector(new[] {2, 2}), DateTime.Now, new[] { 2, 4 });

                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 1, 1}), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 1, 2}), DateTime.Now, 22);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 1, 3}), DateTime.Now, 18);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 2, 1}), DateTime.Now, 24);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 2, 3}), DateTime.Now, 19);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {2, 1, 1}), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {2, 1, 3}), DateTime.Now, 22);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {2, 2, 2}), DateTime.Now, 21);
                interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {2, 2, 4}), DateTime.Now, 25);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q2Id, Abc.Create.Entity.RosterVector(new[] {1, 2, 4}), DateTime.Now, 20);

                    result.OptionsCountForQuestion3InRoster1_1 = GetChangedOptions(eventContext, q3Id, Abc.Create.Entity.RosterVector(new[] {1, 1}))?.Length ?? 0;
                    result.OptionsCountForQuestion3InRoster1_2 = GetChangedOptions(eventContext, q3Id, Abc.Create.Entity.RosterVector(new[] {1, 2}))?.Length ?? 0;
                    result.OptionsCountForQuestion3InRoster2_1 = GetChangedOptions(eventContext, q3Id, Abc.Create.Entity.RosterVector(new[] {2, 1}))?.Length ?? 0;
                    result.OptionsCountForQuestion3InRoster2_2 = GetChangedOptions(eventContext, q3Id, Abc.Create.Entity.RosterVector(new[] {2, 2}))?.Length ?? 0;
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_not_return_options_for_linked_question_in_1_1_roster () =>
            results.OptionsCountForQuestion3InRoster1_1.Should().Be(0);

        [NUnit.Framework.Test] public void should_return_2_options_for_linked_question_in_1_2_roster () =>
            results.OptionsCountForQuestion3InRoster1_2.Should().Be(2);

        [NUnit.Framework.Test] public void should_not_return_options_for_linked_question_in_2_1_roster () =>
            results.OptionsCountForQuestion3InRoster2_1.Should().Be(0);

        [NUnit.Framework.Test] public void should_not_return_options_for_linked_question_in_2_2_roster () =>
            results.OptionsCountForQuestion3InRoster2_2.Should().Be(0);

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid roster1Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid roster2Id = Guid.Parse("66666666666666666666666666666666");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForQuestion3InRoster1_1;
            public int OptionsCountForQuestion3InRoster1_2;
            public int OptionsCountForQuestion3InRoster2_1;
            public int OptionsCountForQuestion3InRoster2_2;
        }
    }
}
