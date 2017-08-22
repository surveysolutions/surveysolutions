using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories;
using WB.Core.BoundedContexts.Headquarters.Views.Reposts.InputModels;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [Subject(typeof(SpeedReportFactory))]
    internal class SpeedReportFactoryTestContext
    {
        protected static SpeedReportFactory CreateSpeedReportFactory(
          IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses = null, IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans> interviewStatusTimeSpans=null)
        {
            return new SpeedReportFactory(
                interviewStatuses ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatuses>>(),
                interviewStatusTimeSpans??Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans>>());
        }

        protected static SpeedByInterviewersReportInputModel CreateSpeedByInterviewersReportInputModel(
            string period = "d", Guid? supervisorId = null,
            DateTime? from = null, int columnCount = 2)
        {
            return new SpeedByInterviewersReportInputModel()
            {
                Period = period,
                ColumnCount = columnCount,
                From = from ?? new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new []{InterviewExportedAction.Completed }
            };
        }

        protected static SpeedBetweenStatusesByInterviewersReportInputModel CreateSpeedBetweenStatusesByInterviewersReportInputModel(string period = "d", Guid? supervisorId = null)
        {
            return new SpeedBetweenStatusesByInterviewersReportInputModel()
            {
                Period = period,
                ColumnCount = 2,
                From = new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                BeginInterviewStatuses =  new[] { InterviewExportedAction.InterviewerAssigned },
                EndInterviewStatuses = new[] { InterviewExportedAction.ApprovedByHeadquarter }
            };
        }

        protected static SpeedBetweenStatusesBySupervisorsReportInputModel CreateSpeedBetweenStatusesBySupervisorsReportInputModel(string period = "d")
        {
            return new SpeedBetweenStatusesBySupervisorsReportInputModel()
            {
                Period = period,
                ColumnCount = 2,
                From = new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                BeginInterviewStatuses = new[] { InterviewExportedAction.InterviewerAssigned },
                EndInterviewStatuses = new[] { InterviewExportedAction.ApprovedByHeadquarter }
            };
        }

        protected static SpeedBySupervisorsReportInputModel CreateSpeedBySupervisorsReportInputModel(
         string period = "d")
        {
            return new SpeedBySupervisorsReportInputModel()
            {
                Period = period,
                ColumnCount = 2,
                From = new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new[] { InterviewExportedAction.Completed }
            };
        }
    }
}
