using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.PlainStorage;
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
            UseTransactionToSaveSummaryAndSpeedReport(interviewSummary);

            var interviewSummaryStorage = SetupAndCreateInterviewSummaryRepository();

            var denormalizer = CreateDenormalizer(interviewSummaryStorage);

            denormalizer.Handle(new[] { Create.PublishedEvent.InterviewDeleted(interviewId: interviewId) });

            Assert.That(interviewSummaryStorage.GetById(interviewId.FormatGuid()), Is.Null);
        }

        protected static InterviewSummaryCompositeDenormalizer CreateDenormalizer(
            IReadSideRepositoryWriter<InterviewSummary> interviewStatuses)
        {
            var defaultUserView = Create.Entity.UserView(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<UserViewInputModel>()) == defaultUserView);
            var questionnaireDocument = Create.Entity.QuestionnaireDocument();
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), It.IsAny<long>()) == questionnaireDocument);

            var questionnaireItems = Mock.Of<IPlainStorageAccessor<QuestionnaireCompositeItem>>();
            Mock.Get(questionnaireItems)
                .Setup(reader => reader.Query(It.IsAny<Func<IQueryable<QuestionnaireCompositeItem>, List<QuestionnaireCompositeItem>>>()))
                .Returns(new List<QuestionnaireCompositeItem>());

            return new InterviewSummaryCompositeDenormalizer(
                new EventBusSettings(),
                interviewStatuses,
                new InterviewSummaryDenormalizer(userViewFactory, questionnaireStorage, Create.Storage.NewMemoryCache()),
                new StatusChangeHistoryDenormalizerFunctional(userViewFactory),
                new InterviewStatusTimeSpanDenormalizer(),
                Mock.Of<IInterviewStatisticsReportDenormalizer>(), 
                new InterviewGeoLocationAnswersDenormalizer(questionnaireStorage),
                new InterviewExportedCommentariesDenormalizer(userViewFactory, questionnaireStorage),
                new InterviewDynamicReportAnswersDenormalizer(questionnaireStorage, questionnaireItems));
        }

        protected void UseTransactionToSaveSummaryAndSpeedReport(InterviewSummary interviewSummary)
        {
            SetupSessionFactory();

            var interviewSummaryStorage = CreateInterviewSummaryRepository();
            interviewSummary = AppendSpeedReportInfo(interviewSummary);
            interviewSummaryStorage.Store(interviewSummary, interviewSummary.InterviewId.FormatGuid());

            UnitOfWork.Session.Flush();
        }
    }
}
