using System;
using System.Globalization;
using System.Threading;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;

        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        private Guid interviewId;

        private Guid questionnaireId;

        private QuestionnaireListItem selectedQuestionnaire;

        private bool canCreateInterview = true;
        public bool CanCreateInterview
        {
            get { return canCreateInterview; }
            set
            {
                canCreateInterview = value;
                RaisePropertyChanged(() => CanCreateInterview);
            }
        }

        private bool isInProgress = false;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set
            {
                isInProgress = value;
                RaisePropertyChanged(() => IsInProgress);
            }
        }

        private bool hasErrors = false;
        public bool HasErrors
        {
            get { return hasErrors; }
            set
            {
                hasErrors = value;
                RaisePropertyChanged(() => HasErrors);
            }
        }

        private bool isServerUnavailable = false;
        public bool IsServerUnavailable
        {
            get { return isServerUnavailable; }
            set
            {
                isServerUnavailable = value;
                RaisePropertyChanged(() => IsServerUnavailable);
            }
        }

        private string errorMessage = string.Empty;
        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged(() => ErrorMessage);
            }
        }

        private string progressIndicator;
        public string ProgressIndicator
        {
            get { return progressIndicator; }
            set
            {
                progressIndicator = value;
                RaisePropertyChanged(() => ProgressIndicator);
            }
        }

        public PrefilledQuestionsViewModel(
            ILogger logger, 
            ICommandService commandService,
            IPrincipal principal)
            : base(logger)
        {
            this.commandService = commandService;
            this.principal = principal;
        }

        private IMvxCommand openInterviewCommand;
        public IMvxCommand OpenInterviewCommand
        {
            get
            {
                return openInterviewCommand ?? (openInterviewCommand = new MvxCommand(this.OpenInterview, () => this.CanCreateInterview));
            }
        }

        public void Init(Guid interviewId, Guid questionnaireId)
        {
            this.questionnaireId = questionnaireId;
            this.interviewId = interviewId;
        }

        private void OpenInterview()
        {
            this.ShowViewModel<InterviewGroupViewModel>(new { id = this.interviewId });
        }

        public override void NavigateToPreviousViewModel()
        {
            this.tokenSource.Cancel();
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}