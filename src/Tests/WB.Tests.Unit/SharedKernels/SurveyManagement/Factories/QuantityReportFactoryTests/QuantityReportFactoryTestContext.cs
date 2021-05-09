using System;
using Moq;
using WB.Core.BoundedContexts.Headquarters.DataExport.Views;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.ServicesIntegration.Export;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityReportFactoryTests
{
    [NUnit.Framework.TestOf(typeof(QuantityReportFactory))]
    internal class QuantityReportFactoryTestContext
    {
        protected static QuantityReportFactory CreateQuantityReportFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewStatuses = null)
        {
            return new QuantityReportFactory(
                interviewStatuses ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>());
        }

        protected static QuantityByInterviewersReportInputModel CreateQuantityByInterviewersReportInputModel(
            string period = "d", Guid? supervisorId = null,
            DateTime? from = null, int columnCount=2)
        {
            return new QuantityByInterviewersReportInputModel()
            {
                Period = period,
                ColumnCount = columnCount,
                From = from ?? new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new[] {InterviewExportedAction.ApprovedBySupervisor}
            };
        }

        protected static QuantityBySupervisorsReportInputModel CreateQuantityBySupervisorsReportInputModel(
            string period = "d",
            int columnCount = 2,
            DateTime? from = null)
        {
            return new QuantityBySupervisorsReportInputModel()
            {
                Period = period,
                ColumnCount = columnCount,
                From = from ?? new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new[] { InterviewExportedAction.ApprovedBySupervisor }
            };
        }
    }
}
