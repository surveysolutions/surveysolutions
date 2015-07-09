using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.SpeedReportFactoryTests
{
    [Subject(typeof(SpeedReportFactory))]
    internal class SpeedReportFactoryTestContext
    {
        protected static SpeedReportFactory CreateSpeedReportFactory(
          IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses = null)
        {
            return new SpeedReportFactory(
                interviewStatuses ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatuses>>(),
                Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatusTimeSpans>>());
        }

        protected static SpeedByInterviewersReportInputModel CreateSpeedByInterviewersReportInputModel(
            string period = "d", Guid? supervisorId = null)
        {
            return new SpeedByInterviewersReportInputModel()
            {
                Period = period,
                ColumnCount = 2,
                From = new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                SupervisorId = supervisorId ?? Guid.NewGuid(),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1,
                InterviewStatuses = new []{InterviewExportedAction.Completed }
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
