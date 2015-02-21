using System;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Factories;

namespace WB.Tests.Unit.BoundedContexts.Capi.InterviewViewModelDenormalizerTests
{
    internal class InterviewViewModelDenormalizerTestsContext
    {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId)
        {
            AssemblyContext.SetupServiceLocator();

            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId
            };
            return innerDocument;
        }

        protected static InterviewViewModelDenormalizer CreateInterviewViewModelDenormalizer(
            Mock<IReadSideRepositoryWriter<InterviewViewModel>> storageStub,
            Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>> versionedStorageStub)
        {
            var denormalizer = new InterviewViewModelDenormalizer(storageStub.Object,
                versionedStorageStub.Object,
                Mock.Of<IReadSideKeyValueStorage<QuestionnaireRosterStructure>>(), new QuestionnaireRosterStructureFactory());

            return denormalizer;
        }

        protected static Mock<IReadSideRepositoryWriter<InterviewViewModel>> CreateInterviewViewModelDenormalizerStorageStub(
            InterviewViewModel document)
        {
            var storageStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();

            storageStub.Setup(d => d.GetById(document.PublicKey.FormatGuid())).Returns(document);

            return storageStub;
        }

        protected static Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>> CreateQuestionnaireDocumentVersionedStorageStub(
            QuestionnaireDocument document)
        {
            var questionnaireStorageMock = new Mock<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>();
            questionnaireStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>()))
                .Returns(new QuestionnaireDocumentVersioned()
                {
                    Questionnaire = document
                });
            return questionnaireStorageMock;
        }

        protected static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                questionnaireId,
                1,
                1,
                DateTime.Now,
                evnt)
                );
            return e;
        }
    }
}