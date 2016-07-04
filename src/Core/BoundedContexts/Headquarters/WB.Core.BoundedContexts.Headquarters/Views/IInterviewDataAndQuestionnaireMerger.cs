using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views
{
    public interface IInterviewDataAndQuestionnaireMerger
    {
        InterviewDetailsView Merge(InterviewData interview, QuestionnaireDocument questionnaire, UserLight responsible, InterviewLinkedQuestionOptions interviewLinkedQuestionOptions, IEnumerable<AttachmentInfoView> attachmentInfos);
    }
}