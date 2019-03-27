using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reports.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.Tests.Abc.Storage;

namespace WB.Tests.Integration.ReportTests.SpeedReportTests
{
    [TestFixture]
    internal class when_interview_is_deleted : SpeedReportContext
    {
        [Test]
        public void when_interview_is_deleted_event_should_store_without_summary_exists_check()
        {
            var interviewId = Guid.NewGuid();

            var interviewSummary = Create.Entity.InterviewSummary(interviewId);
            var speedReportInterviewItem = Create.Entity.SpeedReportInterviewItem(interviewSummary);
            UseTransactionToSaveSummaryAndSpeedReport(interviewSummary, speedReportInterviewItem);

            var interviewSummaryStorage = SetupAndCreateInterviewSummaryRepository();
            var speedReportItemsStorage = CreateSpeedReportInterviewItemsRepository();

            var denormalizer = CreateDenormalizer(interviewSummaryStorage, speedReportItemsStorage);

            denormalizer.Handle(new[] { Create.PublishedEvent.InterviewDeleted(interviewId: interviewId) }, interviewId);

            Assert.That(interviewSummaryStorage.GetById(interviewId.FormatGuid()), Is.Null);
            Assert.That(speedReportItemsStorage.GetById(interviewId.FormatGuid()), Is.Null);
        }

        protected static InterviewSummaryCompositeDenormalizer CreateDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> interviewStatuses,
            IReadSideRepositoryWriter<SpeedReportInterviewItem> speedReportItems)
        {
            var defaultUserView = Create.Entity.UserView(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<UserViewInputModel>()) == defaultUserView);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument();
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), It.IsAny<long>()) == questionnaireDocument);
            return new InterviewSummaryCompositeDenormalizer(
                interviewStatuses,
                new InterviewSummaryDenormalizer(userViewFactory, questionnaireStorage),
                new StatusChangeHistoryDenormalizerFunctional(userViewFactory),
                new InterviewStatusTimeSpanDenormalizer(),
                new SpeedReportDenormalizerFunctional(speedReportItems),
                new InterviewGeoLocationAnswersDenormalizer(null, questionnaireStorage));
        }

        protected void UseTransactionToSaveSummaryAndSpeedReport(InterviewSummary interviewSummary, 
            SpeedReportInterviewItem speedReportInterviewItem)
        {
            SetupSessionFactory();

            var interviewSummaryStorage = CreateInterviewSummaryRepository();
            var speedReportItemsStorage = CreateSpeedReportInterviewItemsRepository();
            speedReportItemsStorage.Store(Create.Entity.SpeedReportInterviewItem(interviewSummary), interviewSummary.InterviewId.FormatGuid());
            interviewSummaryStorage.Store(interviewSummary, interviewSummary.InterviewId.FormatGuid());

            UnitOfWork.AcceptChanges();
        }
    }
}
