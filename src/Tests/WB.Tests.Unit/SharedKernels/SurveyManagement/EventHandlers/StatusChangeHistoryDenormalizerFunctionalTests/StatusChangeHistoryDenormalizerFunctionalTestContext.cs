﻿using System;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.EventHandler;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.EventHandlers.StatusChangeHistoryDenormalizerFunctionalTests
{
    [TestOf(typeof(InterviewSummaryCompositeDenormalizer))]
    internal class StatusChangeHistoryDenormalizerFunctionalTestContext
    {
        public static InterviewSummaryCompositeDenormalizer CreateDenormalizer(IReadSideRepositoryWriter<InterviewSummary> interviewStatuses = null)
        {
            var defaultUserView = Create.Entity.UserView(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<UserViewInputModel>()) == defaultUserView);
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument());
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(_ => _.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), It.IsAny<string>()) == questionnaire);
            return new InterviewSummaryCompositeDenormalizer(
                interviewStatuses ?? Mock.Of<IReadSideRepositoryWriter<InterviewSummary>>(),
                new InterviewSummaryDenormalizer(userViewFactory, questionnaireStorage), 
                new StatusChangeHistoryDenormalizerFunctional(userViewFactory),
                new InterviewStatusTimeSpanDenormalizer(), 
                Mock.Of<IInterviewStatisticsReportDenormalizer>(),
                new InterviewGeoLocationAnswersDenormalizer(null, questionnaireStorage),
                new InterviewExportedCommentariesDenormalizer(userViewFactory, questionnaireStorage));
        }

        public static StatusChangeHistoryDenormalizerFunctional CreateStatusChangeHistoryDenormalizerFunctional()
        {
            var defaultUserView = Create.Entity.UserViewLite(supervisorId: Guid.NewGuid());
            var userViewFactory = Mock.Of<IUserViewFactory>(_ => _.GetUser(Moq.It.IsAny<Guid>()) == defaultUserView);
            return new StatusChangeHistoryDenormalizerFunctional(userViewFactory);
        }
    }
}
