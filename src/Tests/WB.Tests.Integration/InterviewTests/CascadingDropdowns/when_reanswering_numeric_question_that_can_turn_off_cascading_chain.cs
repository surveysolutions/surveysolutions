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
    internal class when_reanswering_numeric_question_that_can_turn_off_cascading_chain : InterviewTestsContext
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
                var parentSingleOptionQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var childCascadedComboboxId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var grandChildCascadedComboboxId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                Guid userId = Guid.NewGuid();
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

                interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[] { }, DateTime.Now, 60);
                interview.AnswerSingleOptionQuestion(userId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(userId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(userId, grandChildCascadedComboboxId, new decimal[] { }, DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, numericQuestionId, new decimal[] { }, DateTime.Now, 6);

                    return new InvokeResults
                    {
                        WasAnyAnswerEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasParentEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasParentDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasGrandChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                    };
                }
            });

        It should_not_enable_any_question = () =>
            results.WasAnyAnswerEnabled.ShouldBeFalse();

        It should_disable_parent_question = () =>
            results.WasParentDisabled.ShouldBeTrue();

        It should_not_enable_parent_question = () =>
            results.WasParentEnabled.ShouldBeFalse();

        It should_disable_child_question = () =>
            results.WasChildDisabled.ShouldBeTrue();

        It should_not_enable_child_question = () =>
            results.WasChildEnabled.ShouldBeFalse();

        It should_not_enable_garnd_child_question = () =>
            results.WasGrandChildEnabled.ShouldBeFalse();

        It should_disable_grandchild_question = () =>
            results.WasGrandChildDisabled.ShouldBeTrue();


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
            public bool WasParentEnabled { get; set; }
            public bool WasParentDisabled { get; set; }
        }
    }
}