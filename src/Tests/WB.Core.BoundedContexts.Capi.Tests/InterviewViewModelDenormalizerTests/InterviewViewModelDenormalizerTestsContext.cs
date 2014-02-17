﻿using System;
using Main.Core.Documents;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ncqrs.Eventing;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Capi.EventHandler;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.BoundedContexts.Supervisor.Implementation.Factories;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Capi.Tests.InterviewViewModelDenormalizerTests
{
    internal class InterviewViewModelDenormalizerTestsContext {
        protected static QuestionnaireDocument CreateQuestionnaireDocument(Guid questionnaireId)
        {
            ServiceLocator.SetLocatorProvider(() => new Mock<IServiceLocator> { DefaultValue = DefaultValue.Mock }.Object);
            var innerDocument = new QuestionnaireDocument
            {
                Title = string.Format("Questionnaire {0}", questionnaireId),
                PublicKey = questionnaireId
            };
            return innerDocument;
        }

        protected static InterviewViewModelDenormalizer CreateInterviewViewModelDenormalizer(
            Mock<IReadSideRepositoryWriter<InterviewViewModel>> storageStub,
            Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>> versionedStorageStub)
        {
            var denormalizer = new InterviewViewModelDenormalizer(storageStub.Object,
                versionedStorageStub.Object,
                Mock.Of<IVersionedReadSideRepositoryWriter<QuestionnaireRosterStructure>>(), new QuestionnaireRosterStructureFactory());

            return denormalizer;
        }

        protected static Mock<IReadSideRepositoryWriter<InterviewViewModel>> CreateInterviewViewModelDenormalizerStorageStub(
            InterviewViewModel document)
        {
            var storageStub = new Mock<IReadSideRepositoryWriter<InterviewViewModel>>();

            storageStub.Setup(d => d.GetById(document.PublicKey.FormatGuid())).Returns(document);

            return storageStub;
        }

        protected static Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>> CreateQuestionnaireDocumentVersionedStorageStub(
            QuestionnaireDocument document)
        {
            var questionnaireStorageMock = new Mock<IVersionedReadSideRepositoryWriter<QuestionnaireDocumentVersioned>>();
            questionnaireStorageMock.Setup(x => x.GetById(Moq.It.IsAny<string>(), Moq.It.IsAny<long>()))
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
                evnt,
                new Version(1, 0))
                );
            return e;
        }
    }
}