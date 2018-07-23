using MvvmCross;
using MvvmCross.Commands;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewStaticTextViewModel : OverviewStaticText
    {
        private readonly IUserInteractionService userInteractionService;

        public OverviewStaticTextViewModel(InterviewTreeStaticText treeNode,
            AttachmentViewModel attachmentViewModel,
            IStatefulInterview interview,
            IUserInteractionService userInteractionService) : base(treeNode, interview)
        {
            this.userInteractionService = userInteractionService;
            this.Attachment = attachmentViewModel;
            this.Attachment.Init(treeNode.Tree.InterviewId, treeNode.Identity);
        }

        public AttachmentViewModel Attachment { get; set; }

        
        public IMvxCommand ShowErrors => new MvxCommand(() =>
        {
            foreach (var error in this.ErrorMessages)
            {
                userInteractionService.ShowToast(error);
            }
        }, () => ErrorMessages.Count > 0);
    }
}
