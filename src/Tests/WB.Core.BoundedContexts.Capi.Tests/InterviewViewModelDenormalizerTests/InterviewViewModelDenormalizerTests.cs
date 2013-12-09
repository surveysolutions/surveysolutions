using System;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using NUnit.Framework;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.InterviewViewModelDenormalizerTests
{
    internal class InterviewViewModelDenormalizerTests
    {
        private static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId)
        {
            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId
            };
            return innerDocument;
        }

        private static InterviewViewModelDenormalizer CreateInterviewViewModelDenormalizer(
            Mock<IReadSideRepositoryWriter<InterviewViewModel>> storageStub,
            Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>> versionedStorageStub)
        {
            var denormalizer = new InterviewViewModelDenormalizer(storageStub.Object,
                versionedStorageStub.Object, 
                Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>());

            return denormalizer;
        }

        private static Mock<IReadSideRepositoryWriter<InterviewViewModel>> CreateInterviewViewModelDenormalizerStorageStub(
            InterviewViewModel document)
        {
            var storageStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();

            storageStub.Setup(d => d.GetById(document.PublicKey)).Returns(document);

            return storageStub;
        }

        private static Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>> CreateQuestionnaireDocumentVersionedStorageStub(
            QuestionnaireDocument document)
        {
            var questionnaireStorageMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>();
            questionnaireStorageMock.Setup(x => x.GetById(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()))
                .Returns(new QuestionnaireDocumentVersioned()
                {
                    Questionnaire = document
                });
            return questionnaireStorageMock;
        }

        private static IPublishedEvent<T> CreatePublishedEvent<T>(Guid questionnaireId, T evnt)
        {
            IPublishedEvent<T> e = new PublishedEvent<T>(new UncommittedEvent(Guid.NewGuid(),
                                                                              questionnaireId,
                                                                              1,
                                                                              1,
                                                                              DateTime.Now,
                                                                              evnt,
                                                                              new Version(1, 0))
                );
            return e;
        }

        [Test]
        public void HandleInterviewForTestingCreated_When_InterviewForTestingCreated_event_is_come_Then_ViewModel_Is_Stored()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);
            InterviewForTestingCreated evnt = new InterviewForTestingCreated(userId, questionnaireId, 1);

            var interviewViewModelStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();
            
            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

            var denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);
            
            //Act
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

            //Assert
            interviewViewModelStub.Verify(
                x => x.Store(It.Is<InterviewViewModel>(i => i.PublicKey == questionnaireId), questionnaireId));
        }

        [Test]
        public void HandleSynchronizationMetadataApplied_When_SynchronizationMetadataApplied_event_is_come_Then_ViewModel_Is_Removed()
        {
            // Arrange
            var questionnaireId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var userId = Guid.Parse("22222222-2222-2222-2222-222222222222");

            var questionnaireDocument = CreateQuestionnaireDocument(questionnaireId);

            var evnt = new SynchronizationMetadataApplied(userId, questionnaireId, InterviewStatus.RejectedBySupervisor, null);
            
            var interviewViewModelStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();

            var questionnaireDocumentVersionedStub = CreateQuestionnaireDocumentVersionedStorageStub(questionnaireDocument);

            var denormalizer = CreateInterviewViewModelDenormalizer(interviewViewModelStub, questionnaireDocumentVersionedStub);

            //Act
            denormalizer.Handle(CreatePublishedEvent(questionnaireId, evnt));

            //Assert
            interviewViewModelStub.Verify(
                x => x.Remove(It.Is<Guid>(i => i== questionnaireId)));
        }
    }
}
