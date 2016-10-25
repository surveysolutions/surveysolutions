using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_numeric_question_that_can_turn_on_cascading : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var numericQuestionId = Guid.Parse("11111111111111111111111111111111");
                var parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
                var childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
                var grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");

                var questionnaireId = Guid.NewGuid();

                var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(numericQuestionId, variable: "numeric"),
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", enablementCondition: "numeric > 10",
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "parent option 1"),
                            Create.Option(value: "2", text: "parent option 2")
                        }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "child 1 for parent option 1", parentValue: "1"),
                            Create.Option(value: "2", text: "child 1 for parent option 2", parentValue: "2"),
                        }),
                    Create.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                        options: new List<Answer>
                        {
                            Create.Option(value: "1", text: "grand child 1 for parent option 1", parentValue: "1"),
                            Create.Option(value: "2", text: "grand child 1 for parent option 2", parentValue: "2"),
                        })
                    );

                var interview = SetupInterview(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numericQuestionId, new decimal[] { }, DateTime.Now, 20);

                    return new InvokeResults
                    {
                        WasAnyAnswerEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasGrandChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                    };
                }
            });

        It should_not_enable_any_question = () =>
            results.WasAnyAnswerEnabled.ShouldBeTrue();

        It should_not_disable_child_question_because_it_was_disabled = () =>
            results.WasChildDisabled.ShouldBeFalse();

        It should_not_enable_child_question = () =>
            results.WasChildEnabled.ShouldBeFalse();

        It should_not_enable_garnd_child_questio = () =>
            results.WasGrandChildEnabled.ShouldBeFalse();

        It should_not_disable_grandchild_question_because_it_was_disabled = () =>
            results.WasGrandChildDisabled.ShouldBeFalse();


        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnyAnswerEnabled { get; set; }
            public bool WasChildDisabled { get; set; }
            public bool WasChildEnabled { get; set; }
            public bool WasGrandChildDisabled { get; set; }
            public bool WasGrandChildEnabled { get; set; }
        }
    }
}