using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_answering_question_in_roster_that_makes_filtering_options_for_multi_question_invalid_and_answer_using_in_other_enablement_condition : InterviewTestsContext
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
                    Create.Entity.Option("1", "Option 1"),
                    Create.Entity.Option("2", "Option 2"),
                    Create.Entity.Option("3", "Option 3"),
                    Create.Entity.Option("12", "Option 12")
                };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, new IComposite[]
                {
                    Create.Entity.SingleQuestion(q1Id, "q1", options: options),
                    Create.Entity.Roster(rosterId, rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ IntegrationCreate.FixedTitle(1, "Hello")}, children: new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < (int)q1"),
                        Create.Entity.SingleQuestion(q3Id, "q3", options: options, enablementCondition: "q2.Contains(2)"),
                        Create.Entity.MultyOptionsQuestion(q4Id, variable: "q4", options: options, optionsFilter: "@optioncode % (int)q3 == 0"),
                        Create.Entity.SingleQuestion(q5Id, "q5", options: options, enablementCondition: "q4.Contains(12)")
                    })
                });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerSingleOptionQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 12);
                interview.AnswerMultipleOptionsQuestion(userId, q2Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, new[] { 1, 2, 3 });
                interview.AnswerSingleOptionQuestion(userId, q3Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, 2);
                interview.AnswerMultipleOptionsQuestion(userId, q4Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, new[] { 12 });
                interview.AnswerSingleOptionQuestion(userId, q5Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, 3);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 1);

                    result.QuestionsQ5Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q5Id));

                    result.QuestionqQ4HasEmptyAnswer = eventContext.GetSingleEvent<AnswersRemoved>().Questions.Any(x => x.Id == q4Id);
                    result.QuestionqQ2HasEmptyAnswer = eventContext.GetSingleEvent<AnswersRemoved>().Questions.Any(x => x.Id == q2Id);
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_disable_q5 () =>
            results.QuestionsQ5Disabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_have_empty_answer_q2 () =>
            results.QuestionqQ2HasEmptyAnswer.Should().BeTrue();

        [NUnit.Framework.Test] public void should_have_empty_answer_q4 () =>
            results.QuestionqQ4HasEmptyAnswer.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid q4Id = Guid.Parse("44444444444444444444444444444444");
        private static readonly Guid q5Id = Guid.Parse("55555555555555555555555555555555");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ5Disabled { get; set; }
            public bool QuestionqQ4HasEmptyAnswer { get; set; }
            public bool QuestionqQ2HasEmptyAnswer { get; set; }
        }
    }
}
