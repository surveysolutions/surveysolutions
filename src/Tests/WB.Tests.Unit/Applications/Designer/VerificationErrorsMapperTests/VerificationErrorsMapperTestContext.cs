using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Code;

namespace WB.Tests.Unit.Applications.Designer.VerificationErrorsMapperTests
{
    internal class VerificationErrorsMapperTestContext
    {
        public static VerificationErrorsMapper CreateVerificationErrorsMapper()
        {
            return new VerificationErrorsMapper();
        }

        internal static QuestionnaireVerificationError[] CreateQuestionnaireVerificationErrors(Guid questionId, Guid groupId)
        {
            return new QuestionnaireVerificationError[2]
            {
                new QuestionnaireVerificationError("aaa","aaaa", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.Question, questionId)}), 
                new QuestionnaireVerificationError("bbb","bbbb", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.Group, groupId)})
            };
        }

        internal static QuestionnaireVerificationError[] CreateStaticTextVerificationError(Guid staticTextId)
        {
            return new QuestionnaireVerificationError[1]
            {
                new QuestionnaireVerificationError("aaa","aaaa", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.StaticText, staticTextId)})
            };
        }

        internal static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionId, Guid groupId, string groupTitle, string questionTitle)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group(groupTitle)
                    {
                        PublicKey = groupId,
                        Children = new List<IComposite>
                        {
                            new TextQuestion(questionTitle)
                            {
                                PublicKey = questionId
                            }
                        }
                    }
                }
            };
        }

        internal static QuestionnaireDocument CreateQuestionnaireDocumentWithStaticText(Guid staticTextId, Guid chapterId)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group()
                    {
                        PublicKey = chapterId,
                        Children = new List<IComposite>
                        {
                            new StaticText(staticTextId, null)
                        }
                    }
                }
            };
        }

    }
}
