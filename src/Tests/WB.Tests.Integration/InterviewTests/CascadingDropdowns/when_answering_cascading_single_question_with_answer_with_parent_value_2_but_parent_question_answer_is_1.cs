using System;
using System.Collections.Generic;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_cascading_single_question_with_answer_with_parent_value_2_but_parent_question_answer_is_1 :
        InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        private Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");
                var questionnaireId = Guid.Parse("22222222222222222222222222222222");
                var actorId = Guid.Parse("33333333333333333333333333333333");

                Setup.MockedServiceLocator();

                var questionnaire = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.SingleQuestion(parentSingleOptionQuestionId, "q1", options: new List<Answer>
                    {
                        Create.Option(value: "1", text: "parent option 1"),
                        Create.Option(value: "2", text: "parent option 2")
                    }),
                    Create.SingleQuestion(childCascadedComboboxId, "q2", cascadeFromQuestionId: parentSingleOptionQuestionId,
                        options: new List<Answer>
                        {
                            Create.Option(value: "1.1", text: "child 1 for parent option 1", parentValue: "1"),
                            Create.Option(value: "1.2", text: "child 2 for parent option 1", parentValue: "1"),
                            Create.Option(value: "2.1", text: "child 1 for parent option 2", parentValue: "2"),
                            Create.Option(value: "2.2", text: "child 2 for parent option 2", parentValue: "2"),
                            Create.Option(value: "2.3", text: "child 3 for parent option 2", parentValue: "2"),
                        })
                    );

                var interview = SetupInterview(questionnaire, new List<object>
                {
                    Create.Event.SingleOptionQuestionAnswered(questionId: parentSingleOptionQuestionId, answer: 1,
                        propagationVector: new decimal[] { })
                });

                using (var eventContext = new EventContext())
                {
                    var exception = Catch.Exception(() =>
                        interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 2.2m)
                        );

                    return new InvokeResults
                    {
                        ExceptionType = exception.GetType(),
                        ErrorMessage = exception.Message.ToLower()
                    };
                }
            });

        It should_throw_InterviewException = () =>
            results.ExceptionType.ShouldEqual(typeof(InterviewException));

        It should_throw_exception_with_message_containting__answer____parent_value____incorrect__ = () =>
            new[] { "answer", "parent value", "do not correspond" }.ShouldEachConformTo(
                keyword => results.ErrorMessage.Contains(keyword));

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
            public Type ExceptionType { get; set; }
            public string ErrorMessage { get; set; }
        }
    }
}