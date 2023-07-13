using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewNodeDetailsViewModel : BaseViewModel<OverviewNodeDetailsViewModelArgs>
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;
        private string interviewId;
        private Identity identity;

        public OverviewNodeDetailsViewModel(IStatefulInterviewRepository interviewRepository,
            IDynamicTextViewModelFactory dynamicTextViewModelFactory,
            CommentsViewModel comments,
            IPrincipal principal,
            IViewModelNavigationService navigationState): base(principal, navigationState)
        {
            Comments = comments;
            this.interviewRepository = interviewRepository;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public override void Prepare(OverviewNodeDetailsViewModelArgs parameter)
        {
            this.interviewId = parameter.InterviewId;
            this.identity = parameter.TargetEntity;
        }

        public override async Task Initialize()
        {
            await base.Initialize().ConfigureAwait(false);

            var interview = this.interviewRepository.Get(interviewId);

            this.Errors = interview.GetFailedValidationMessages(identity, UIResources.Error).Select(ConvertToSafeHtml).ToList();
            this.Warnings = interview.GetFailedWarningMessages(identity, UIResources.Error).Select(ConvertToSafeHtml).ToList();
            this.Comments.Init(this.interviewId, identity);
            this.Comments.HasComments = this.Comments.Comments.Count > 0;
        }

        private string ConvertToSafeHtml(string x)
        {
            var errorText = dynamicTextViewModelFactory.CreateErrorMessage();
            errorText.InitAsStatic(x);
            return errorText.HtmlText;
        }

        public CommentsViewModel Comments { get; set; }

        public bool HasErrors => Errors.Count > 0;

        public bool HasWarnings => Warnings.Count > 0;

        public List<string> Errors { get; set; }

        public List<string> Warnings { get; set; }
        
        public IMvxAsyncCommand CancelCommand => new MvxAsyncCommand(this.Cancel);
        private async Task Cancel() => await ViewModelNavigationService.Close(this);
        
        public override void Dispose()
        {
            Comments.Dispose();
            base.Dispose();
        }
    }

    public class OverviewNodeDetailsViewModelArgs
    {
        public Identity TargetEntity { get; set; }

        public string InterviewId { get; set; }
    }
}
