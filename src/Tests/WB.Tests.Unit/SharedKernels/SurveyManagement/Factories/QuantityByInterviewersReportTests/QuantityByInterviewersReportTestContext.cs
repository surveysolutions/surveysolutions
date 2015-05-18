using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.Factories.QuantityByInterviewersReportTests
{
    [Subject(typeof(QuantityReportFactory))]
    internal class QuantityByInterviewersReportTestContext
    {
        protected static QuantityReportFactory CreateQuantityByInterviewersReport(
            IQueryableReadSideRepositoryReader<InterviewStatuses> interviewStatuses = null,
            IQueryableReadSideRepositoryReader<UserDocument> userDocuments = null)
        {
            return new QuantityReportFactory(
                interviewStatuses ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewStatuses>>(),
                userDocuments ?? Mock.Of<IQueryableReadSideRepositoryReader<UserDocument>>());
        }

        protected static QuantityByInterviewersReportInputModel CreateQuantityByInterviewersReportInputModel(
            string period = "d", Guid? supervisorId = null)
        {
            return new QuantityByInterviewersReportInputModel()
            {
                Period = period,
                ColumnCount = 2,
                From = new DateTime(1984, 4, 18),
                Page = 0,
                PageSize = 20,
                SupervisorId = supervisorId?? Guid.NewGuid(),
                QuestionnaireId = Guid.NewGuid(),
                QuestionnaireVersion = 1
            };
        }
    }
}
