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
    internal class when_answering_numeric_question_that_can_turn_on_cascading : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                SetUp.MockedServiceLocator();
                var numericQuestionId = Guid.Parse("11111111111111111111111111111111");
                var parentSingleOptionQuestionId = Guid.Parse("9E96D4AB-DF91-4FC9-9585-23FA270B25D7");
                var childCascadedComboboxId = Guid.Parse("C6CC807A-3E81-406C-A110-1044AE3FD89B");
                var grandChildCascadedComboboxId = Guid.Parse("4C603B8A-3237-4915-96FA-8D1568C679E2");

                var questionnaireId = Guid.NewGuid();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.NumericIntegerQuestion(numericQuestionId, "numeric"),
                    Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", "numeric > 10",
                        options: new List<Answer>
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
                        })
                    );

                var interview = SetupInterviewWithExpressionStorage(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numericQuestionId, new decimal[] { }, DateTime.Now, 20);

                    return new InvokeResults
                    {
                        WasAnyAnswerEnabled = eventContext.AnyEvent<QuestionsEnabled>(),
                        WasChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId)),
                        WasGrandChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId)),
                        WasGrandChildDisabled = eventContext.AnyEvent<QuestionsDisabled>(x => x.Questions.Any(q => q.Id == grandChildCascadedComboboxId))
                    };
                }
            });

        [NUnit.Framework.Test] public void should_not_enable_any_question () =>
            results.WasAnyAnswerEnabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_disable_child_question_because_it_was_disabled () =>
            results.WasChildDisabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_enable_child_question () =>
            results.WasChildEnabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_enable_garnd_child_questio () =>
            results.WasGrandChildEnabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_disable_grandchild_question_because_it_was_disabled () =>
            results.WasGrandChildDisabled.Should().BeFalse();


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
            public bool WasAnyAnswerEnabled { get; set; }
            public bool WasChildDisabled { get; set; }
            public bool WasChildEnabled { get; set; }
            public bool WasGrandChildDisabled { get; set; }
            public bool WasGrandChildEnabled { get; set; }
        }
    }
}
