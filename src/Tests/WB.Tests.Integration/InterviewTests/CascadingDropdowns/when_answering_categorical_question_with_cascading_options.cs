using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_categorical_question_with_cascading_options : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var parentSingleOptionQuestionId = Guid.Parse("11111111111111111111111111111111");
                var childCascadedComboboxId = Guid.Parse("22222222222222222222222222222222");
                var grandChildCascadedComboboxId = Guid.Parse("33333333333333333333333333333333");

                var questionnaireId = Guid.NewGuid();

                var nonAnsweredCombo = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var comboShouldNotBeRemoved = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Entity.Option("1", "parent option 1"),
                        Create.Entity.Option("2", "parent option 2")
                    }),
                    Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                        {
                            Create.Entity.Option("1", "child 1 for parent option 1", "1"),
                            Create.Entity.Option("2", "child 1 for parent option 2", "2")
                        }),
                    Create.Entity.SingleQuestion(grandChildCascadedComboboxId, "q3", cascadeFromQuestionId: childCascadedComboboxId,
                        options: new List<Answer>
                        {
                            Create.Entity.Option("1", "grand child 1 for parent option 1", "1"),
                            Create.Entity.Option("2", "grand child 1 for parent option 2", "2")
                        }),
                    Create.Entity.SingleQuestion(nonAnsweredCombo, "q4", options: new List<Answer>
                    {
                        Create.Entity.Option("1", "parent option 1"),
                        Create.Entity.Option("2", "parent option 2")
                    }),
                    Create.Entity.SingleQuestion(comboShouldNotBeRemoved, "q5", cascadeFromQuestionId: nonAnsweredCombo,
                        options: new List<Answer>
                        {
                            Create.Entity.Option("1", "child 1 for parent option 1", "1"),
                            Create.Entity.Option("2", "child 1 for parent option 2", "2")
                        })
                    );

                var interview = SetupInterviewWithExpressionStorage(questionnaire, new List<object>
                {
                    Create.Event.SingleOptionQuestionAnswered(
                        parentSingleOptionQuestionId, new decimal[] { }, 1, null, null
                    ),
                    Create.Event.QuestionsEnabled(Create.Identity(childCascadedComboboxId)),
                    Create.Event.AnswersDeclaredInvalid(new[] {Create.Identity(childCascadedComboboxId)}),
                    Create.Event.SingleOptionQuestionAnswered(
                        childCascadedComboboxId, new decimal[] { }, 1, null, null
                    ),
                    Create.Event.QuestionsEnabled(Create.Identity(grandChildCascadedComboboxId)),
                    Create.Event.AnswersDeclaredValid(Create.Identity(childCascadedComboboxId)),
                    Create.Event.SingleOptionQuestionAnswered(
                        grandChildCascadedComboboxId, new decimal[] { }, 1, null, null
                    )
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


        [NUnit.Framework.Test] public void should_not_enable_child_question_because_it_was_already_enabled () =>
           results.WasChildCascadingEnabled.Should().BeFalse();
        
        [NUnit.Framework.Test] public void should_disable_grandchild_question () =>
           results.WasGrandChildAnswerDiasbled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_enable_grandchild_question () =>
            results.WasGrandChildAnswerEnabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_remove_answer_from_self () =>
           results.WasParentAnswerRemoved.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_remove_answer_from_not_related_question () =>
           results.WasComboAnswerRemoved.Should().BeFalse();

        [NUnit.Framework.Test] public void should_remove_child_answer () =>
            results.WasChildAnswerRemoved.Should().BeTrue();

        [NUnit.Framework.Test] public void should_remove_dependent_answers_on_second_level_of_cascades_if_it_is_answered () =>
            results.WasGrandChildAnswerRemoved.Should().BeTrue();

       
        [OneTimeTearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

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
