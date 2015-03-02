using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Raven.Client;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate
{
    internal class InterviewsToDeleteFactory : IInterviewsToDeleteFactory
    {
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public InterviewsToDeleteFactory(IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.indexAccessor = indexAccessor;
        }

        public List<InterviewSummary> Load(Guid questionnaireId, long questionnaireVersion)
        {
            return
              indexAccessor.Query<SeachIndexContent>(typeof(InterviewsSearchIndex).Name)
                  .Where(interview => !interview.IsDeleted &&
                                      interview.QuestionnaireId == questionnaireId &&
                                      interview.QuestionnaireVersion == questionnaireVersion)
                  .ProjectFromIndexFieldsInto<InterviewSummary>().ToList();
        }
    }
}
