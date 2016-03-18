using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views
{
    public interface IInterviewDataAndQuestionnaireMerger
    {
        InterviewDetailsView Merge(InterviewData interview, IQuestionnaireDocument questionnaire, UserLight responsible, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions, IEnumerable<AttachmentInfoView> attachmentInfos);
    }
}