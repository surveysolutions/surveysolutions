using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    [Subject(typeof (Interview))]
    internal class when_answering_categorical_question_with_cascading_options : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
                var childCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var grandChildCascadedComboboxId = Guid.Parse("33333333333333333333333333333333");

                var questionnaireId = Guid.NewGuid();

                var nonAnsweredCombo = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var comboShouldNotBeRemoved = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(text: "parent option 1", value: "1"),
                        Create.Option(text: "parent option 2", value: "2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        isMandatory: true,
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
                    Create.Event.SingleOptionQuestionAnswered(questionId: parentSingleOptionQuestionId, answer: 1, propagationVector: new decimal[] { }),
                    Create.Event.QuestionsEnabled(Create.Identity(childCascadedComboboxId)),
                    Create.Event.AnswersDeclaredInvalid(Create.Identity(childCascadedComboboxId)),
                    Create.Event.SingleOptionQuestionAnswered(questionId: childCascadedComboboxId, answer: 1, propagationVector: new decimal[] { }),
                    Create.Event.QuestionsEnabled(Create.Identity(grandChildCascadedComboboxId)),
                    Create.Event.AnswersDeclaredValid(Create.Identity(childCascadedComboboxId)),
                    Create.Event.SingleOptionQuestionAnswered(questionId: grandChildCascadedComboboxId, answer: 1, propagationVector: new decimal[] { }),
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(Guid.NewGuid(), parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                    {
                        WasChildCascadingEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildCascadingInvalid = eventContext.AnyEvent<AnswersDeclaredInvalid>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildAnswerDiasbled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasGrandChildAnswerEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasParentAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == parentSingleOptionQuestionId)),
                        WasComboAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == comboShouldNotBeRemoved)),
                        WasChildAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildAnswerRemoved = eventContext.AnyEvent<AnswersRemoved>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId))
                    };
                }
            });


        It should_not_enable_child_question_because_it_was_already_enabled = () =>
           results.WasChildCascadingEnabled.ShouldBeFalse();

        It should_invalidate_child_question_because_it_is_mandatory = () =>
           results.WasChildCascadingInvalid.ShouldBeTrue();

        It should_disable_grandchild_question = () =>
           results.WasGrandChildAnswerDiasbled.ShouldBeTrue();

        It should_not_enable_grandchild_question = () =>
            results.WasGrandChildAnswerEnabled.ShouldBeFalse();

        It should_not_remove_answer_from_self = () =>
           results.WasParentAnswerRemoved.ShouldBeFalse();

        It should_not_remove_answer_from_not_related_question = () =>
           results.WasComboAnswerRemoved.ShouldBeFalse();

        It should_remove_child_answer = () =>
            results.WasChildAnswerRemoved.ShouldBeTrue();

        It should_remove_dependent_answers_on_second_level_of_cascades_if_it_is_answered = () =>
            results.WasGrandChildAnswerRemoved.ShouldBeTrue();

       
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
            public bool WasChildCascadingEnabled { get; set; }
            public bool WasChildCascadingInvalid { get; set; }
            public bool WasGrandChildAnswerDiasbled { get; set; }
            public bool WasGrandChildAnswerEnabled { get; set; }
            public bool WasParentAnswerRemoved { get; set; }
            public bool WasComboAnswerRemoved { get; set; }
            public bool WasChildAnswerRemoved { get; set; }
            public bool WasGrandChildAnswerRemoved { get; set; }
        }
    }
}
