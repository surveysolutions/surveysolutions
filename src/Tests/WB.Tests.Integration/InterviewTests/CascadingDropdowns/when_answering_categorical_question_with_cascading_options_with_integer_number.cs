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
                    }
                    );

                var interview = SetupInterview(questionnaire);

                interview.AnswerSingleOptionQuestion(actorId, parentSingleOptionQuestionId, new decimal[] { }, DateTime.Now, 1);

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