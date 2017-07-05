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
    internal class when_answering_question_that_makes_filtering_options_for_multi_question_invalid_and_answer_using_in_other_enablement_condition : InterviewTestsContext
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
                    Create.Entity.Option("11", "Option 11"),
                    Create.Entity.Option("12", "Option 12")
                };

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, new IComposite[]
                {
                    Create.Entity.SingleQuestion(q1Id, "q1", options: options),
                    Create.Entity.MultyOptionsQuestion(q2Id, variable: "q2", options: options, optionsFilter: "@optioncode < (int)q1"),
                    Create.Entity.SingleQuestion(q3Id, "q3", options: options, enablementCondition: "q2.Contains(2)")
                });

                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerSingleOptionQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 12);
                interview.AnswerMultipleOptionsQuestion(userId, q2Id, RosterVector.Empty, DateTime.Now, new []{ 1, 2, 11 });
                interview.AnswerSingleOptionQuestion(userId, q3Id, RosterVector.Empty, DateTime.Now, 1);

                var result = new InvokeResults();

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(userId, q1Id, RosterVector.Empty, DateTime.Now, 1);

                    result.QuestionsQ3Disabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == q3Id));
                }

                return result;
            });

        It should_disable_q3 = () =>
            results.QuestionsQ3Disabled.ShouldBeTrue();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static readonly Guid questionnaireId = Guid.Parse("99999999999999999999999999999999");
        private static readonly Guid q1Id = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid q2Id = Guid.Parse("22222222222222222222222222222222");
        private static readonly Guid q3Id = Guid.Parse("33333333333333333333333333333333");
        private static readonly Guid userId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

        [Serializable]
        internal class InvokeResults
        {
            public bool QuestionsQ3Disabled { get; set; }
        }
    }
}