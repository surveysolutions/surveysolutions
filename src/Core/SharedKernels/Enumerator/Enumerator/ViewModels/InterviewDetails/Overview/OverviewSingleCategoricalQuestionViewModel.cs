using MvvmCross.Commands;
using System;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview;

public class OverviewSingleCategoricalQuestionViewModel : OverviewQuestionViewModel
{
    private readonly IViewModelNavigationService navigationService;
    private readonly Guid interviewId;
    private readonly Guid? attachmentId;

    public OverviewSingleCategoricalQuestionViewModel(InterviewTreeQuestion treeQuestion, 
        IStatefulInterview interview, 
        IUserInteractionService userInteractionService,
        IViewModelNavigationService navigationService,
        IAttachmentContentStorage attachmentContentStorage,
        IQuestionnaire questionnaire) 
        : base(treeQuestion, interview, userInteractionService)
    {
        this.navigationService = navigationService;
        interviewId = Guid.Parse(treeQuestion.Tree.InterviewId);
        if (treeQuestion.IsAnswered())
        {
            if (treeQuestion.IsSingleFixedOption)
            {
                var singleOptionQuestion = treeQuestion.GetAsInterviewTreeSingleOptionQuestion();
                var selectedValue = singleOptionQuestion.GetAnswer().SelectedValue;
                var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, selectedValue, null);
                this.attachmentId = questionnaire.GetAttachmentIdByName(attachmentName);
            }

            if (treeQuestion.IsCascading)
            {
                var cascadingQuestion = treeQuestion.GetAsInterviewTreeCascadingQuestion();
                var selectedValue = cascadingQuestion.GetAnswer().SelectedValue;
                var parentAnswer = cascadingQuestion.GetCascadingParentQuestion().GetAnswer();
                var parentValue = parentAnswer.SelectedValue;
                var attachmentName = interview.GetAttachmentForEntityOption(treeQuestion.Identity, selectedValue, parentValue);
                this.attachmentId = questionnaire.GetAttachmentIdByName(attachmentName);
            }

            if (attachmentId.HasValue)
            {
                var attachment = questionnaire.GetAttachmentById(this.attachmentId.Value);
                this.Image = attachmentContentStorage.GetContent(attachment.ContentId);
            }
        }
    }

    public byte[] Image { get; set; }

    public IMvxAsyncCommand ShowPhotoView => new MvxAsyncCommand(async () =>
    {
        if (this.Image?.Length > 0)
        {
            await this.navigationService.NavigateToAsync<PhotoViewViewModel, PhotoViewViewModelArgs>(
                new PhotoViewViewModelArgs
                {
                    InterviewId = this.interviewId,
                    AttachmentId = this.attachmentId,
                });
        }
    });
}
