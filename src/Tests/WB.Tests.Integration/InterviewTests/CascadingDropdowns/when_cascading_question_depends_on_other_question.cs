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
    internal class when_cascading_question_depends_on_other_question : InterviewTestsContext
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
                var actorId = Guid.Parse("366404A9-374E-4DCC-9B56-1F15103DE880");
                var questionnaireId = Guid.Parse("3B7145CD-A235-44D0-917C-7B34A1017AEC");
                var numericQuestionId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var grandChildCascading = Guid.Parse("107E3563-8005-41D3-8073-06DCDA4C528D");

                var conditionExpression = "numeric==2";

                var numericQuestionVariable = "numeric";

                var questionnaire = Create.QuestionnaireDocument(questionnaireId,
                    new NumericQuestion
                    {
                        PublicKey = numericQuestionId,
                        QuestionType = QuestionType.Numeric,
                        StataExportCaption = numericQuestionVariable,
                        IsInteger = true
                    },
                    new SingleQuestion
                    {
                        PublicKey = parentSingleOptionQuestionId,
                        QuestionType = QuestionType.SingleOption,
                        StataExportCaption = "parent",
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
                        CascadeFromQuestionId = parentSingleOptionQuestionId,
                        StataExportCaption = "child",
                        Mandatory = true,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = grandChildCascading,
                        QuestionType = QuestionType.SingleOption,
                        CascadeFromQuestionId = childCascadedComboboxId,
                        StataExportCaption = "grandChild",
                        ConditionExpression = conditionExpression,
                        Answers = new List<Answer>
                        {
                            new Answer { AnswerText = "child 1", AnswerValue = "1", PublicKey = Guid.NewGuid(), ParentValue = "1" },
                            new Answer { AnswerText = "child 2", AnswerValue = "2", PublicKey = Guid.NewGuid(), ParentValue = "2" }
                        }
                    });

                var interview = SetupInterview(questionnaire);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(actorId, numericQuestionId, new decimal[] { }, DateTime.Now, 2);

                    return new InvokeResults
                    {
                        WasGrandChildEnabled = eventContext.AnyEvent<QuestionsEnabled>(e => e.Questions.Any(q => q.Id == grandChildCascading))
                    };
                }

            });

        It should_not_enable_grand_child_question = () => 
            results.WasGrandChildEnabled.ShouldBeTrue();

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
            public bool WasGrandChildEnabled { get; set; }
        }
    }
}
