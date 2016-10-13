using System;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireDenormalizerTests
{
    internal class QuestionChangeEventTestContext
    {
        internal static Mock<IReadSideRepositoryWriter<QuestionnaireDocument>> CreateQuestionnaireDenormalizerStorageStub(QuestionnaireDocument document)
        {
            var storageStub = new Mock<IReadSideRepositoryWriter<QuestionnaireDocument>>();

            storageStub.Setup(d => d.GetById(document.PublicKey.FormatGuid())).Returns(document);

            return storageStub;
        }

        internal static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId)
        {
            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId
            };
            return innerDocument;
        }
    }
}