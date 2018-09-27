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
    internal class when_answering_numeric_question_after_answering_single_option_question_and_dependent_cascading_question_has_no_options : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");
                var numericId = Guid.Parse("44444444444444444444444444444444");

                SetUp.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId, Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                {
                    Create.Entity.Option("1", "parent option 1"),
                    Create.Entity.Option("2", "parent option 2")
                }), Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId, options: new List<Answer>
                {
                    Create.Entity.Option("11", "child 1 for parent option 1", "1"),
                    Create.Entity.Option("12", "child 2 for parent option 1", "1")
                }), Create.Entity.NumericIntegerQuestion(numericId, "numeric"));

                var interview = SetupInterviewWithExpressionStorage(questionnaire, new object[]{});

                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);
                interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 11);
                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 2);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, numericId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                    {
                        DisabledQuestions =
                            eventContext.AnyEvent<QuestionsDisabled>()
                                ? eventContext.GetSingleEvent<QuestionsDisabled>().Questions.Select(identity => identity.Id).ToArray()
                                : null,
                        EnabledQuestions =
                            eventContext.AnyEvent<QuestionsEnabled>()
                                ? eventContext.GetSingleEvent<QuestionsEnabled>().Questions.Select(identity => identity.Id).ToArray()
                                : null
                    };
                }
            });

        [NUnit.Framework.Test] public void should_not_raise_QuestionsDisabled_event () =>
            results.DisabledQuestions.Should().BeNull();

        [NUnit.Framework.Test] public void should_not_raise_QuestionsEnabled_event () =>
            results.EnabledQuestions.Should().BeNull();

        [OneTimeTearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        private static InvokeResults results;
        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        private static readonly Guid childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");

        [Serializable]
        internal class InvokeResults
        {
            public Guid[] DisabledQuestions { get; set; }
            public Guid[] EnabledQuestions { get; set; }
        }
    }
}
