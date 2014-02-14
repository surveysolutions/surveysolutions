using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Moq;
using WB.Core.SharedKernels.ExpressionProcessor.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Tests.QuestionnaireVerifierTests
{
    [Subject(typeof(QuestionnaireVerifier))]
    internal class QuestionnaireVerifierTestsContext
    {
        protected static QuestionnaireVerifier CreateQuestionnaireVerifier(IExpressionProcessor expressionProcessor = null)
        {
            return new QuestionnaireVerifier(expressionProcessor ?? new Mock<IExpressionProcessor>().Object);
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireChildren)
        {
            return new QuestionnaireDocument
            {
                Children = questionnaireChildren.ToList(),
            };
        }

        protected static QuestionnaireDocument CreateQuestionnaireWithRosterAndGroupAfterWithConditionReferencingQuestionInRoster(Guid underDeeperRosterLevelQuestionId, Guid questionWithCustomValidation)
        {
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterQuestionId = Guid.Parse("13333333333333333333333333333333");
            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = rosterGroupId, 
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = rosterGroupId,
                        IsRoster = true,
                        RosterSizeQuestionId = rosterQuestionId,
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                PublicKey = underDeeperRosterLevelQuestionId
                            }
                        }
                    },
                    new Group
                    {
                        PublicKey = questionWithCustomValidation,
                        ConditionExpression = "some random expression"
                    }
                });

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireWithRosterAndQuestionAfterWithConditionReferencingQuestionInRoster(Guid underDeeperRosterLevelQuestionId, Guid questionWithCustomValidation)
        {
            var rosterGroupId = Guid.Parse("13333333333333333333333333333333");
            var rosterQuestionId = Guid.Parse("13333333333333333333333333333333");
            var questionnaire = CreateQuestionnaireDocument(new IComposite[]
                {
                    new NumericQuestion
                    {
                        PublicKey = rosterGroupId, 
                        IsInteger = true, 
                        MaxValue = 5
                    },
                    new Group
                    {
                        PublicKey = rosterGroupId,
                        IsRoster = true,
                        RosterSizeQuestionId = rosterQuestionId,
                        Children = new List<IComposite>
                        {
                            new NumericQuestion
                            {
                                PublicKey = underDeeperRosterLevelQuestionId
                            }
                        }
                    },
                    new SingleQuestion
                    {
                        PublicKey = questionWithCustomValidation,
                        ConditionExpression = "some random expression"
                    }
                });

            return questionnaire;
        }

        protected static QuestionnaireDocument CreateQuestionnaireDocumentWithOneChapter(params IComposite[] chapterChildren)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group("Chapter")
                    {
                        PublicKey = Guid.Parse("FFF000AAA111EE2DD2EE111AAA000FFF"),
                        Children = chapterChildren.ToList(),
                        IsRoster = false
                    }
                }
            };
        }
    }
}