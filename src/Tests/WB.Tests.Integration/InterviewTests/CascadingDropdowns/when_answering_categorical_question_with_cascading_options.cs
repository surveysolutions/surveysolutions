using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
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

                Setup.SetupMockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
                var childCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var grandChildCascadedComboboxId = Guid.Parse("33333333333333333333333333333333");

                var questionnaireId = Guid.NewGuid();
                var actorId = Guid.NewGuid();

                var nonAnsweredCombo = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var comboShouldNotBeRemoved = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                        new SingleQuestion
                        {
                            PublicKey = parentSingleOptionQuestionId,
                            QuestionType = QuestionType.SingleOption,
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "one", AnswerValue = "1", PublicKey = Guid.NewGuid() },
                                new Answer { AnswerText = "two", AnswerValue = "2", PublicKey = Guid.NewGuid() }
                            }
                        },
                        new SingleQuestion
                        {
                            PublicKey = childCascadedComboboxId,
                            QuestionType = QuestionType.SingleOption,
                            StataExportCaption = "q1",
                            Mandatory = true,
                            CascadeFromQuestionId = parentSingleOptionQuestionId,
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                                new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                            }
                        },
                        new SingleQuestion
                        {
                            PublicKey = grandChildCascadedComboboxId,
                            QuestionType = QuestionType.SingleOption,
                            CascadeFromQuestionId = childCascadedComboboxId,
                            StataExportCaption = "q2",
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "grand child", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" }
                            }
                        },
                        new SingleQuestion
                        {
                            PublicKey = nonAnsweredCombo,
                            QuestionType = QuestionType.SingleOption,
                            StataExportCaption = "q3",
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "other cascade", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                            }
                        },
                        new SingleQuestion
                        {
                            PublicKey = comboShouldNotBeRemoved,
                            QuestionType = QuestionType.SingleOption,
                            StataExportCaption = "q4",
                            CascadeFromQuestionId = nonAnsweredCombo,
                            Answers = new List<Answer>
                            {
                                new Answer { AnswerText = "blue", AnswerValue = "1", PublicKey = Guid.NewGuid() }
                            }
                        });

                var interview = SetupInterview(questionnaire);

                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(actorId, grandChildCascadedComboboxId, new decimal[] { }, DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(Guid.NewGuid(), parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                    {
                       WasChildCascadingEnabled= eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
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


        It should_not_enable_child_question_because_it_was_already_anabled = () =>
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
