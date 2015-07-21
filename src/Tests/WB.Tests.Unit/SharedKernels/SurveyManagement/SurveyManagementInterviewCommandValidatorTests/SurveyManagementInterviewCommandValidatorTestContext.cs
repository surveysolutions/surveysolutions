using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.SurveyManagementInterviewCommandValidatorTests
{
    [Subject(typeof(SurveyManagementInterviewCommandValidator))]
    internal class SurveyManagementInterviewCommandValidatorTestContext
    {
        protected static SurveyManagementInterviewCommandValidator CreateSurveyManagementInterviewCommandValidator(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaryStorage=null, int? limit=null)
        {
            return
                new SurveyManagementInterviewCommandValidator(
                    interviewSummaryStorage ?? Mock.Of<IQueryableReadSideRepositoryReader<InterviewSummary>>(),
                    new InterviewPreconditionsServiceSettings(limit));
        }
    }
}
