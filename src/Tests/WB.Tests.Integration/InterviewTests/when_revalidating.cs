using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.Tests.Integration.InterviewTests
{
    internal class when_revalidating : InterviewTestsContext
    {
        private Establish context = () =>
        {
            userId = Guid.Parse("AAAA0000AAAA00000000AAAA0000AAAA");
            answerTime = new DateTime(2013, 10, 02);

            numericQuestionId = Guid.Parse("11111111111111111111111111111111");
            var propagatableGroupId = Guid.Parse("22222222222222222222222222222222");
            var referencedQuestionId = Guid.Parse("22220000000000000000000000000000");
            linkedQuestionId = Guid.Parse("33333333333333333333333333333333");

            var questionnaireDocument = new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        Children = new List<IComposite>
                        {
                            new NumericQuestion("QuestionX")
                            {
                                PublicKey = Guid.NewGuid(),
                                StataExportCaption = "QuestionX",
                                QuestionType = QuestionType.Numeric,
                                IsInteger = true,
                            },
                            new NumericQuestion("Question1")
                            {
                                PublicKey = Guid.Parse("21111111111111111111111111111111"),
                                ConditionExpression = "[QuestionX] > 0 ",
                                QuestionType = QuestionType.Numeric,
                                IsInteger = true,
                                StataExportCaption = "Question1",
                            },
                            new NumericQuestion("Question2")
                            {
                                PublicKey = Guid.Parse("22222222222222222222222222222222"),
                                ConditionExpression = "[QuestionX] > 0 or [Question1] = 1",
                                QuestionType = QuestionType.Numeric,
                                IsInteger = true,
                                StataExportCaption = "Question2",
                            }
                        }
                    }
                }
            };

            interview = CreateInterviewFromQuestionnaireDocumentRegisteringAllNeededDependencies(questionnaireDocument);

            
            eventContext = new EventContext();
        };

        private Because of = () =>
            interview.ReevaluateSynchronizedInterview();

        private It should_ = () =>
            eventContext.Events.Count().ShouldBeGreaterThan(0);
        
        private Cleanup stuff = () =>
        {
            eventContext.Dispose();
            eventContext = null;
        };

        private static EventContext eventContext;
        private static Interview interview;
        private static Guid userId;
        private static Guid numericQuestionId;
        private static DateTime answerTime;
        private static Guid linkedQuestionId;
    }
}