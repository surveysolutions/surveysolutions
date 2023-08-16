using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

public class OverviewNumericQuestionViewModel : OverviewQuestionViewModel
{
    public AttachmentViewModel Attachment { get; }

    public OverviewNumericQuestionViewModel(InterviewTreeQuestion treeQuestion, 
        IStatefulInterview interview, 
        IUserInteractionService userInteractionService,
        IQuestionnaire questionnaire,
        IInterviewViewModelFactory interviewViewModelFactory) 
        : base(treeQuestion, interview, userInteractionService)
    {
        this.Attachment = interviewViewModelFactory.GetNew<AttachmentViewModel>();

        if (!treeQuestion.IsAnswered()) return;
        
        if (treeQuestion.IsInteger)
        {
            var integerQuestion = treeQuestion.GetAsInterviewTreeIntegerQuestion();
            var answerValue = integerQuestion.GetAnswer()?.Value;
            if (!answerValue.HasValue) return;
                
            var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, answerValue.Value, null);
            if(!string.IsNullOrEmpty(attachmentName))
                Attachment.InitAsStatic(treeQuestion.Tree.InterviewId, attachmentName);
        }
        else if (treeQuestion.IsDouble)
        {
            var doubleQuestion = treeQuestion.GetAsInterviewTreeDoubleQuestion();
            var answerValue = doubleQuestion.GetAnswer()?.Value;

            if (!answerValue.HasValue) return;
                
            var intPart = Math.Truncate(answerValue.Value);
            if (intPart != answerValue.Value)
                return;

            // Double to int conversion can overflow.
            try
            {
                var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, Convert.ToInt32(answerValue.Value), null);
                if(!string.IsNullOrEmpty(attachmentName))
                    Attachment.InitAsStatic(treeQuestion.Tree.InterviewId, attachmentName);
            }
            catch (OverflowException)
            {
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        
        Attachment?.ViewDestroy();
        Attachment?.Dispose();
    }
}
