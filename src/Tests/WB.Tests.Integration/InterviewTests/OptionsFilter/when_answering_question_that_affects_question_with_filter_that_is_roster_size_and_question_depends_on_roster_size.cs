using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.OptionsFilter
{
    internal class when_answering_question_that_affects_question_with_filter_that_is_roster_size_and_question_depends_on_roster_size : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var options = new List<Answer>
                {
                    Create.Entity.Option("1", "Option 1"),
                    Create.Entity.Option("2", "Option 2"),
                    Create.Entity.Option("3", "Option 3"),
                    Create.Entity.Option("12", "Option 12")
                };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(q1Id, "q1"),
                    Create.Entity.Roster(rosterId, variable:"r", rosterSizeSourceType: RosterSizeSourceType.FixedTitles, fixedRosterTitles: new []{ IntegrationCreate.FixedTitle(1, "Hello")}, children: new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < q1"),
                        Create.Entity.Roster(roster2Id, variable:"r1", rosterSizeQuestionId: q2Id, rosterSizeSourceType: RosterSizeSourceType.Question, children: new IComposite[]{}),
                        Create.Entity.SingleQuestion(q3Id, "q3", options: options, enablementCondition: "r1.Count() < 2")
                    })
                });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 10);
                interview.AnswerMultipleOptionsQuestion(userId, q2Id, Create.Entity.RosterVector(new[] {1}), DateTime.Now, new[] { 1, 2, 3 });

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 2);

                    result.QuestionsQ3Enabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == q3Id));
                }

                return result;
            });

        It should_enable_q3 = () =>
            results.QuestionsQ3Enabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

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