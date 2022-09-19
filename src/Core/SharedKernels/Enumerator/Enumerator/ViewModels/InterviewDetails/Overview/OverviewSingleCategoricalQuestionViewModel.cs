using MvvmCross.Commands;
using System;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

public class OverviewSingleCategoricalQuestionViewModel : OverviewQuestionViewModel
{
    public AttachmentViewModel Attachment { get; }

    public OverviewSingleCategoricalQuestionViewModel(InterviewTreeQuestion treeQuestion, 
        IStatefulInterview interview, 
        IUserInteractionService userInteractionService,
        IQuestionnaire questionnaire,
        IInterviewViewModelFactory interviewViewModelFactory) 
        : base(treeQuestion, interview, userInteractionService)
    {
        this.Attachment = interviewViewModelFactory.GetNew<AttachmentViewModel>();

        if (treeQuestion.IsAnswered())
        {
            if (treeQuestion.IsSingleFixedOption)
            {
                var singleOptionQuestion = treeQuestion.GetAsInterviewTreeSingleOptionQuestion();
                var selectedValue = singleOptionQuestion.GetAnswer().SelectedValue;
                var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, selectedValue, null);
                Attachment.InitAsStatic(treeQuestion.Tree.InterviewId, attachmentName);
            }

            if (treeQuestion.IsCascading)
            {
                var cascadingQuestion = treeQuestion.GetAsInterviewTreeCascadingQuestion();
                var selectedValue = cascadingQuestion.GetAnswer().SelectedValue;
                var parentAnswer = cascadingQuestion.GetCascadingParentQuestion().GetAnswer();
                var parentValue = parentAnswer.SelectedValue;
                var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, selectedValue, parentValue);
                Attachment.InitAsStatic(treeQuestion.Tree.InterviewId, attachmentName);
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
