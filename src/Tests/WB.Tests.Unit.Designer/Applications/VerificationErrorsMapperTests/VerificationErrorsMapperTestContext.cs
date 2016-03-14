using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.UI.Designer.Code;

namespace WB.Tests.Unit.Designer.Applications.VerificationErrorsMapperTests
{
    internal class VerificationErrorsMapperTestContext
    {
        public static VerificationErrorsMapper CreateVerificationErrorsMapper()
        {
            return new VerificationErrorsMapper();
        }

        internal static QuestionnaireVerificationMessage[] CreateQuestionnaireVerificationErrors(Guid questionId, Guid groupId)
        {
            return new QuestionnaireVerificationMessage[4]
            {
                Create.VerificationError("aaa", "aaaa", new QuestionnaireVerificationReference[1] { new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, questionId) }),
                Create.VerificationError("bbb", "bbbb", new QuestionnaireVerificationReference[1] { new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, groupId) }),
                Create.VerificationError("ccc", "cccc", new QuestionnaireVerificationReference[2]
                {
                    new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, questionId),
                    new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, groupId)
                }),
                Create.VerificationError("aaa", "aaaa", new QuestionnaireVerificationReference[1] { new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Group, groupId) }),
            };
        }

        internal static QuestionnaireVerificationMessage[] CreateStaticTextVerificationError(Guid staticTextId)
        {
            return new QuestionnaireVerificationMessage[1]
            {
                Create.VerificationError("aaa","aaaa", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.StaticText, staticTextId)})
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
