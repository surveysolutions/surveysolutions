using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewNodeDetailsViewModel : MvxViewModel<OverviewNodeDetailsViewModelArgs> 
    {
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;

        private string interviewId;
        private Identity identity;

        public OverviewNodeDetailsViewModel(IStatefulInterviewRepository interviewRepository, IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.interviewRepository = interviewRepository;
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public override void Prepare(OverviewNodeDetailsViewModelArgs parameter)
        {
            this.interviewId = parameter.InterviewId;
            this.identity = parameter.TargetEntity;
        }

        public override Task Initialize()
        {
            var interview = this.interviewRepository.Get(interviewId);

            this.Errors = interview.GetFailedValidationMessages(identity, UIResources.Error).Select(ConvertToSafeHtml).ToList();
            this.Warnings = interview.GetFailedWarningMessages(identity, UIResources.Error).Select(ConvertToSafeHtml).ToList();
            this.Comments = interview.GetQuestionComments(identity).ToList();

            return Task.CompletedTask;
        }

        private string ConvertToSafeHtml(string x)
        {
            var errorText = dynamicTextViewModelFactory.CreateErrorMessage();
            errorText.InitAsStatic(x);
            return errorText.HtmlText;
        }

        public List<AnswerComment> Comments { get; set; }

        public bool HasErrors => Errors.Count > 0;

        public bool HasWarnings => Warnings.Count > 0;

        public List<string> Errors { get; set; }

        public List<string> Warnings { get; set; }
    }

    public class OverviewNodeDetailsViewModelArgs
    {
        public Identity TargetEntity { get; set; }

        public string InterviewId { get; set; }
    }
}
