using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Infrastructure.Native.Storage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement
{
    [TestOf(typeof(SurveysAndStatusesReport))]
    internal class SurveysAndStatusesReportTests
    {
        private static SurveysAndStatusesReport CreateSurveysAndStatusesReport(INativeReadSideStorage<InterviewSummary> summariesRepository = null)
        {
            return new SurveysAndStatusesReport(summariesRepository ?? Stub.ReadSideRepository<InterviewSummary>());
        }

        [Test]
        public void when_GetReport_and_interviews_by_1_questionnaire_only_then_should_not_throw_NRE()
        {
            //arrange
            var userId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var teamLeadName = "userName";

            Guid questionnaireId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            List<InterviewSummary> interviews = new List<InterviewSummary>()
            {
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, teamLeadId: userId, teamLeadName: teamLeadName),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, teamLeadId: userId, teamLeadName: teamLeadName),
                Create.Entity.InterviewSummary(questionnaireId: questionnaireId, status: InterviewStatus.Completed, teamLeadId: Guid.NewGuid()),
            };

            var interviewsReader = Stub.ReadSideRepository<InterviewSummary>();
            interviews.ForEach(summary => interviewsReader.Store(summary, Guid.NewGuid().FormatGuid()));

            var reportFactory = CreateSurveysAndStatusesReport(interviewsReader);

            //act && assert
            Assert.DoesNotThrow(() =>
            {
                reportFactory.GetReport(new SurveysAndStatusesReportInputModel
                {
                    TeamLeadName = teamLeadName,
                    Page = 1,
                    PageSize = 20,
                    Orders = new[]
                        {new OrderRequestItem {Field = "QuestionnaireTitle", Direction = OrderDirection.Asc},}
                });
            });
        }
    }
}