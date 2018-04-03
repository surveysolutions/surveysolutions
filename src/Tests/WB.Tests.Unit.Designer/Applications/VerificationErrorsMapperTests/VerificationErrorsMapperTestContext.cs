using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code;

namespace WB.Tests.Unit.Designer.Applications.VerificationErrorsMapperTests
{
    internal class VerificationErrorsMapperTestContext
    {
        public static VerificationErrorsMapper CreateVerificationErrorsMapper()
        {
            return new VerificationErrorsMapper();
        }

        internal static QuestionnaireVerificationMessage[] CreateQuestionnaireVerificationErrors(Guid questionId, Guid groupId, Guid rosterId)
        {
            return new QuestionnaireVerificationMessage[4]
            {
                Create.VerificationError("aaa", "aaaa", new QuestionnaireEntityReference[1] { new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Question, questionId) }),
                Create.VerificationError("bbb", "bbbb", new QuestionnaireEntityReference[1] { new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Group, groupId) }),
                Create.VerificationError("ccc", "cccc", new QuestionnaireEntityReference[3]
                {
                    new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Question, questionId),
                    new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Group, groupId),
                    new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Roster, rosterId)
                }),
                Create.VerificationError("aaa", "aaaa", new QuestionnaireEntityReference[1] { new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Group, groupId) }),
            };
        }

        internal static QuestionnaireVerificationMessage[] CreateStaticTextVerificationError(Guid staticTextId)
        {
            return new QuestionnaireVerificationMessage[1]
            {
                Create.VerificationError("aaa","aaaa", new QuestionnaireEntityReference[1]{ new QuestionnaireEntityReference( QuestionnaireVerificationReferenceType.StaticText, staticTextId)})
            };
        }

        internal static QuestionnaireDocument CreateQuestionnaireDocumentWith2TextQuestions(Guid questionId1, Guid questionId2, Guid groupId)
        {
            return new QuestionnaireDocument
            {
                Children = new List<IComposite>
                {
                    new Group()
                    {
                        PublicKey = groupId,
                        Children = new List<IComposite>
                        {
                            new TextQuestion() { PublicKey = questionId1 },
                            new TextQuestion() { PublicKey = questionId2 },
                        }.ToReadOnlyCollection()
                    }
                }.ToReadOnlyCollection()
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
                            Create.StaticText(staticTextId, null)
                        }.ToReadOnlyCollection()
                    }
                }.ToReadOnlyCollection()
            };
        }

    }
}
