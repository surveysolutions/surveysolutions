using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Integration.InterviewTests.LinkedQuestions
{
    internal class when_answering_question_that_change_options_list_of_linked_that_is_one_level_deeper_than_source_question : InterviewTestsContext
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
                    Abc.Create.Entity.Option("12")
                };

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, children: new IComposite[]
                {
                    Abc.Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options),
                    Abc.Create.Entity.Roster(rosterId, variable:"r", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]
                    {
                        Abc.Create.Entity.NumericIntegerQuestion(q3Id, variable: "age"),
                        Abc.Create.Entity.Roster(roster1Id, variable:"r1", fixedRosterTitles: new [] { IntegrationCreate.FixedTitle(1),  IntegrationCreate.FixedTitle(2)}, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, children: new IComposite[]
                        {
                            Abc.Create.Entity.SingleQuestion(q4Id, variable: "q4", linkedToQuestionId: q3Id, linkedFilter: "age > current.age")
                        }),
                    })
                });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new[] { 1, 2, 3 });
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Abc.Create.Entity.RosterVector(new[] {2}), DateTime.Now, 15);
                interview.AnswerNumericIntegerQuestion(userId, q3Id, Abc.Create.Entity.RosterVector(new[] {3}), DateTime.Now, 35);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q3Id, Abc.Create.Entity.RosterVector(new[] {1}), DateTime.Now, 20);

                    result.OptionsCountForQuestion4InRoster1_1 = GetChangedOptions(eventContext, q4Id, Abc.Create.Entity.RosterVector(new[] {1, 1}))?.Length ?? 0;
                    result.OptionsCountForQuestion4InRoster2_1 = GetChangedOptions(eventContext, q4Id, Abc.Create.Entity.RosterVector(new[] {2, 1}))?.Length ?? 0;
                    result.OptionsCountForQuestion4InRoster3_1 = GetChangedOptions(eventContext, q4Id, Abc.Create.Entity.RosterVector(new[] {3, 1}))?.Length ?? 0;
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_return_1_option_for_linked_question_in_1_1_roster () =>
            results.OptionsCountForQuestion4InRoster1_1.Should().Be(1);

        [NUnit.Framework.Test] public void should_return_2_options_for_linked_question_in_2_1_roster () =>
            results.OptionsCountForQuestion4InRoster2_1.Should().Be(2);

        [NUnit.Framework.Test] public void should_not_return_options_for_linked_question_in_3_1_roster () =>
            results.OptionsCountForQuestion4InRoster3_1.Should().Be(0);

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
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public int OptionsCountForQuestion4InRoster1_1 { get; set; }
            public int OptionsCountForQuestion4InRoster2_1 { get; set; }
            public int OptionsCountForQuestion4InRoster3_1 { get; set; }
        }
    }
}
