using System;
using System.Collections.Generic;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_cascading_single_question_with_answer_with_parent_value_2_but_parent_question_answer_is_1 :
        InterviewTestsContext
    {
        [OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();

            BecauseOf();
        }

        private void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");

                Setup.MockedServiceLocator();

                var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                         Create.Entity.Option("1", "parent option 1"),
                         Create.Entity.Option("2", "parent option 2")
                    }),
                    Create.Entity.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                        {
                             Create.Entity.Option("11", "child 1 for parent option 1", "1"),
                             Create.Entity.Option("12", "child 2 for parent option 1", "1"),
                             Create.Entity.Option("21", "child 1 for parent option 2", "2"),
                             Create.Entity.Option("22", "child 2 for parent option 2", "2"),
                             Create.Entity.Option("23", "child 3 for parent option 2", "2")
                        })
                    );

                var interview = SetupInterviewWithExpressionStorage(questionnaire, new List<object>
                {
                    Create.Event.SingleOptionQuestionAnswered(
                        parentSingleOptionQuestionId, new decimal[] { }, 1, null, null
                    ),
                    Create.Event.QuestionsEnabled(DateTimeOffset.Now, Create.Identity(childCascadedComboboxId))
                });

                using (var eventContext = new EventContext())
                {
                    var exception = Assert.Throws<AnswerNotAcceptedException>(() =>
                        interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 22)
                        );

                    return new InvokeResults
                    {
                        ExceptionType = exception.GetType(),
                        ErrorMessage = exception.Message.ToLower()
                    };
                }
            });

        [NUnit.Framework.Test] public void should_throw_AnswerNotAcceptedException () =>
            results.ExceptionType.Should().Be(typeof(AnswerNotAcceptedException));

        [NUnit.Framework.Test]
        public void should_throw_exception_with_message() =>
            results.ErrorMessage.Should().Be("selected value do not correspond to the parent answer selected value");

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
            public Type ExceptionType { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}
