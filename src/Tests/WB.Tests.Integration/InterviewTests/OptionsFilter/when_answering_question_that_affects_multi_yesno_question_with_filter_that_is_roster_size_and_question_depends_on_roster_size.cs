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
    internal class when_answering_question_that_affects_multi_yesno_question_with_filter_that_is_roster_size_and_question_depends_on_roster_size : InterviewTestsContext
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

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, Create.Entity.NumericIntegerQuestion(q1Id, "q1"), Create.Entity.Roster(rosterId, variable:"r", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ IntegrationCreate.FixedTitle(1, "Hello")}, children: new IComposite[]
                {
                    Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < q1", yesNoView: true),
                    Create.Entity.Roster(roster2Id, variable:"r1", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]{}),
                    Create.Entity.SingleQuestion(q3Id, "q3", options: options, enablementCondition: "r1.Count() < 2")
                }));

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 10);
                interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: q2Id, rosterVector: Create.Entity.RosterVector(new[] {1}),
                    answeredOptions: new[]
                    {
                        Create.Entity.AnsweredYesNoOption(1m, true),
                        Create.Entity.AnsweredYesNoOption(2m, true),
                        Create.Entity.AnsweredYesNoOption(3m, true)
                    }
                ));

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 2);
                    
                    result.QuestionsQ3Enabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == q3Id));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_enable_q3 () =>
            results.QuestionsQ3Enabled.Should().BeTrue();

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid rosterId = Guid.Parse("88888888888888888888888888888888");
        private static readonly Guid roster2Id = Guid.Parse("77777777777777777777777777777777");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ3Enabled { get; set; }
        }
    }
}
