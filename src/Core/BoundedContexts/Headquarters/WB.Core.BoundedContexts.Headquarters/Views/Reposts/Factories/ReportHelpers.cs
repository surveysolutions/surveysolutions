using System;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Views.Reposts.Factories
{
    public class ReportHelpers
    {
        public static DateTime? GetFirstInterviewCreatedDate(QuestionnaireIdentity questionnaire, IQueryableReadSideRepositoryReader<InterviewStatuses> interviewstatusStorage)
        {
            DateTime? minDate;
            if (questionnaire != null && questionnaire.QuestionnaireId != Guid.Empty)
            {
                minDate = interviewstatusStorage.Query(_ => _
                    .Where(x => x.QuestionnaireId == questionnaire.QuestionnaireId &&
                                x.QuestionnaireVersion == questionnaire.Version)
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Min(x => (DateTime?)x.Timestamp));
            }
            else
            {
                minDate = interviewstatusStorage.Query(_ => _
                    .SelectMany(x => x.InterviewCommentedStatuses)
                    .Min(x => (DateTime?)x.Timestamp));
            }
            return minDate;
        }

    }
}