using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using It = Machine.Specifications.It;

namespace WB.Tests.Integration.InterviewTests.CascadingDropdowns
{
    internal class when_answering_categorical_question_with_cascading_options_with_integer_number : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {

                Setup.MockedServiceLocator();
                var questionnaireId = Guid.Parse("88888888888888888888888888888888");
                var actorId = Guid.Parse("99999999999999999999999999999999");

                var parentSingleOptionQuestionId = Guid.Parse("00000000000000000000000000000000");
                var childCascadedComboboxId = Guid.Parse("11111111111111111111111111111111");

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
                        })
                   );

                var interview = SetupInterview(questionnaire, new List<object>
                {
                    Create.Event.SingleOptionQuestionAnswered(questionId: parentSingleOptionQuestionId, answer: 1, propagationVector: new decimal[] { }),
                    Create.Event.QuestionsEnabled(Create.Identity(childCascadedComboboxId))
                });

                using (var eventContext = new EventContext())
                {
                    interview.AnswerSingleOptionQuestion(actorId, childCascadedComboboxId, new decimal[] { }, DateTime.Now, 1);

                    return new InvokeResults
                    {
                        WasChildAnswerValid = eventContext.AnyEvent<AnswersDeclaredValid>(x => x.Questions.Any(q => q.Id == childCascadedComboboxId))
                    };
                }
            });

        It should_declare_child_question_as_valid = () =>
            results.WasChildAnswerValid.ShouldBeTrue();


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
            public bool WasChildAnswerValid { get; set; }
        }
    }
}