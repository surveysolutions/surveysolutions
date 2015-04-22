using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class InterviewsToDeleteFactory : IInterviewsToDeleteFactory
    {
        private readonly IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries;

        public InterviewsToDeleteFactory(IQueryableReadSideRepositoryReader<InterviewSummary> interviewSummaries)
        {
            this.interviewSummaries = interviewSummaries;
        }

        public List<InterviewSummary> Load(Guid questionnaireId, long questionnaireVersion)
        {
            var result = this.interviewSummaries.Query(_ => _.Where(interview => !interview.IsDeleted &&
                interview.QuestionnaireId == questionnaireId &&
                interview.QuestionnaireVersion == questionnaireVersion).ToList());

            return result;
        }
    }
}
