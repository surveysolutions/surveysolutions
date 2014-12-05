using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_categorical_question_with_cascading_options_with_mandatory_child_under_condition_expression :
        InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
                var childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
                var grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");

                var questionnaireId = Guid.NewGuid();

                var nonAnsweredCombo = Guid.NewGuid();
                var comboShouldNotBeRemoved = Guid.NewGuid();

                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        isMandatory: true,
                        enablementCondition: "q1==2",
                        options: new List<Answer>
                        {
                            Create.Option(text: "child 1 for parent option 1", value: "1", parentValue: "1"),
                            Create.Option(text: "child 1 for parent option 2", value: "2", parentValue: "2"),
                        }),
                    Create.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                        options: new List<Answer>
                        {
                            Create.Option(text: "grand child 1 for parent option 1", value: "1", parentValue: "1"),
                            Create.Option(text: "grand child 1 for parent option 2", value: "2", parentValue: "2"),
                        }),
                    Create.SingleQuestion(nonAnsweredCombo, "q4", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.SingleQuestion(comboShouldNotBeRemoved, "q5", cascadeFromQuestionId: nonAnsweredCombo,
                        options: new List<Answer>
                        {
                            Create.Option(text: "child 1 for parent option 1", value: "1", parentValue: "1"),
                            Create.Option(text: "child 1 for parent option 2", value: "2", parentValue: "2"),
                        })
                    );

                var interview = SetupInterview(questionnaire, new List<object>
                {
                    Create.Event.SingleOptionQuestionAnswered(questionId: parentSingleOptionQuestionId, answer: 1, propagationVector: new decimal[] { })
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(Guid.NewGuid(), parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                    {
                        WasAnyAnswerEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasChildDeclaredValid = eventContext.AnyEvent<AnswersDeclaredValid>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildDeclaredInvalid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasParentAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasComboAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == comboShouldNotBeRemoved)),
                        WasGrandChildAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                    };
                }
            });

        It should_not_enable_any_question = () =>
           results.WasAnyAnswerEnabled.ShouldBeTrue();

        It should_not_declare_child_as_valid = () =>
            results.WasChildDeclaredValid.ShouldBeFalse();

        It should_not_declare_child_as_invalid = () =>
            results.WasChildDeclaredInvalid.ShouldBeTrue();

        It should_enable_child_question_because_of_condition_is_true = () =>
           results.WasChildEnabled.ShouldBeTrue();

        It should_not_disable_grandchild_question_because_it_already_disabled = () =>
           results.WasGrandChildDisabled.ShouldBeFalse();

        It should_not_remove_answer_from_self = () =>
            results.WasParentAnswerRemoved.ShouldBeFalse();

        It should_not_remove_answer_from_not_related_question = () =>
            results.WasComboAnswerRemoved.ShouldBeFalse();

        It should_not_remove_dependent_answers_on_second_level_because_it_was_not_answered = () =>
            results.WasGrandChildAnswerRemoved.ShouldBeFalse();

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        private static InvokeResults results;
        private static AppDomainContext appDomainContext;

        [Serializable]
        internal class InvokeResults
        {
            public bool WasAnyAnswerEnabled { get; set; }
            public bool WasChildDeclaredValid { get; set; }
            public bool WasChildEnabled { get; set; }
            public bool WasGrandChildDisabled { get; set; }
            public bool WasParentAnswerRemoved { get; set; }
            public bool WasComboAnswerRemoved { get; set; }
            public bool WasGrandChildAnswerRemoved { get; set; }
            public bool WasChildDeclaredInvalid { get; set; }
        }
    }
}