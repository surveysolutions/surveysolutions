using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.DataExport.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Implementation.Factories;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Tests.Unit.SharedKernels.SurveyManagement;

namespace WB.Tests.Unit.BoundedContexts.Headquarters.InterviewsExportDenormalizerTests
{
    [TestFixture]
    internal class InterviewsExportDenormalizerNunitTests
    {
        [Test]
        public void
            When_InterviewStatusChanged_On_Completed_event_received_after_Reject_and_the_list_of_levels_has_changed_Then_all_previous_levels_must_be_replaced
            ()
        {
            Guid interviewId=Guid.NewGuid();
            var interviewDataExportView = Create.Entity.InterviewDataExportView(interviewId,
                levels:
                    new[]
                    {
                        Create.Entity.InterviewDataExportLevelView(interviewId,
                            records:
                                new []
                                {
                                    Create.Entity.InterviewDataExportRecord(interviewId),
                                    Create.Entity.InterviewDataExportRecord(interviewId),
                                    Create.Entity.InterviewDataExportRecord(interviewId)
                                })
                    });
            var exportViewFactoryMock=new Mock<IExportViewFactory>();

            exportViewFactoryMock.Setup(
                x => x.CreateInterviewDataExportView(Moq.It.IsAny<QuestionnaireExportStructure>(),
                        Moq.It.IsAny<InterviewData>())).Returns(interviewDataExportView);
            var exportRecords = new TestInMemoryWriter<InterviewDataExportRecord>();

            var exportStructure = new QuestionnaireExportStructure();
            var questionnaireExportStructureStorageMock = new Mock<IQuestionnaireExportStructureStorage>();
            questionnaireExportStructureStorageMock.Setup(
                    x => x.GetQuestionnaireExportStructure(Moq.It.IsAny<QuestionnaireIdentity>()))
                .Returns(exportStructure);

            var interviewsExportDenormalizer = CreateInterviewsExportDenormalizer(interviewId, exportViewFactoryMock.Object, 
                exportRecords, questionnaireExportStructureStorageMock.Object);

            interviewsExportDenormalizer.Handle(Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.Completed,
                interviewId: interviewId));

            var countInterviewRecords = exportRecords.Query(_ => _.Count(i => i.InterviewId == interviewId));

            Assert.That(countInterviewRecords, Is.EqualTo(3));

            var newInterviewDataExportView = Create.Entity.InterviewDataExportView(interviewId,
            levels:
                new[]
                {
                        Create.Entity.InterviewDataExportLevelView(interviewId,
                            records:
                                new []
                                {
                                    Create.Entity.InterviewDataExportRecord(interviewId),
                                    Create.Entity.InterviewDataExportRecord(interviewId)
                                })
                });
            exportViewFactoryMock.Setup(
                x =>
                    x.CreateInterviewDataExportView(Moq.It.IsAny<QuestionnaireExportStructure>(),
                        Moq.It.IsAny<InterviewData>())).Returns(newInterviewDataExportView);

            interviewsExportDenormalizer.Handle(Create.PublishedEvent.InterviewStatusChanged(InterviewStatus.Completed,
               interviewId: interviewId));

            countInterviewRecords = exportRecords.Query(_ => _.Count(i => i.InterviewId == interviewId));

            Assert.That(countInterviewRecords, Is.EqualTo(2));
        }

        [Test]
        public void
            When_InterviewDeleted_event_received_Then_all_interview_levels_must_be_deleted
            ()
        {
            Guid interviewId = Guid.NewGuid();

            var exportRecords = new TestInMemoryWriter<InterviewDataExportRecord>();

            exportRecords.Store(Create.Entity.InterviewDataExportRecord(interviewId),$"{interviewId}1");
            exportRecords.Store(Create.Entity.InterviewDataExportRecord(interviewId), $"{interviewId}2");
            exportRecords.Store(Create.Entity.InterviewDataExportRecord(interviewId), $"{interviewId}3");

            var interviewsExportDenormalizer = CreateInterviewsExportDenormalizer(interviewId, Mock.Of<IExportViewFactory>(), exportRecords);

            interviewsExportDenormalizer.Handle(Create.PublishedEvent.InterviewDeleted(interviewId: interviewId));

            var countInterviewRecords = exportRecords.Query(_ => _.Count(i => i.InterviewId == interviewId));

            Assert.That(countInterviewRecords, Is.EqualTo(0));
        }

        [Test]
        public void
            When_InterviewHardDeleted_event_received_Then_all_interview_levels_must_be_deleted
            ()
        {
            Guid interviewId = Guid.NewGuid();

            var exportRecords = new TestInMemoryWriter<InterviewDataExportRecord>();

            exportRecords.Store(Create.Entity.InterviewDataExportRecord(interviewId), $"{interviewId}1");
            exportRecords.Store(Create.Entity.InterviewDataExportRecord(interviewId), $"{interviewId}2");
            exportRecords.Store(Create.Entity.InterviewDataExportRecord(interviewId), $"{interviewId}3");

            var interviewsExportDenormalizer = CreateInterviewsExportDenormalizer(interviewId, Mock.Of<IExportViewFactory>(), exportRecords);

            interviewsExportDenormalizer.Handle(Create.PublishedEvent.InterviewHardDeleted(interviewId: interviewId));

            var countInterviewRecords = exportRecords.Query(_ => _.Count(i => i.InterviewId == interviewId));

            Assert.That(countInterviewRecords, Is.EqualTo(0));
        }

        private InterviewsExportDenormalizer CreateInterviewsExportDenormalizer(
            Guid interviewId, IExportViewFactory exportViewFactory, IReadSideRepositoryWriter<InterviewDataExportRecord> exportRecords,
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage = null)
        {
            var interviewData = Create.Entity.InterviewData();
            var interviewReferenceStorage = new TestInMemoryWriter<InterviewReferences>();
            interviewReferenceStorage.Store(new InterviewReferences(interviewId, Guid.NewGuid(), 1),
                interviewId);
            var interviewDataStorage = new TestInMemoryWriter<InterviewData>();
            interviewDataStorage.Store(interviewData, interviewId);

            return new InterviewsExportDenormalizer(interviewDataStorage, interviewReferenceStorage,
                exportViewFactory, exportRecords,
                questionnaireExportStructureStorage ?? Mock.Of<IQuestionnaireExportStructureStorage>());
        }
    }
}