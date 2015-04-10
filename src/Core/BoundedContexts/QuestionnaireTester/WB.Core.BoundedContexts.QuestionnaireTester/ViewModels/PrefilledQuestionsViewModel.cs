using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;

using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class PrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IUserInteraction uiDialogs;
        private readonly IPlainStorageAccessor<QuestionnaireListItem> questionnaireInfoAccessor;

        private readonly IRestServiceSettings restServiceSettings;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();
        private readonly IErrorProcessor errorProcessor;


        private Guid newInterviewId;
        private QuestionnaireListItem selectedQuestionnaire;

        private bool canCreateInterview = false;
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
            IPrincipal principal, 
            IUserInteraction uiDialogs, 
            IPlainStorageAccessor<QuestionnaireListItem> questionnaireInfoAccessor, 
            IRestServiceSettings restServiceSettings, 
            IErrorProcessor errorProcessor)
            : base(logger)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.uiDialogs = uiDialogs;
            this.questionnaireInfoAccessor = questionnaireInfoAccessor;
            this.restServiceSettings = restServiceSettings;
            this.errorProcessor = errorProcessor;
        }

        private IMvxCommand openInterviewCommand;
        public IMvxCommand OpenInterviewCommand
        {
            get
            {
                return openInterviewCommand ?? (openInterviewCommand = new MvxCommand(this.OpenInterview, () => this.CanCreateInterview));
            }
        }

        public void Init(string questionnaireId)
        {
            //this.selectedQuestionnaire = this.questionnaireInfoAccessor.GetById(questionnaireId);
        }

        private void OpenInterview()
        {
            this.ShowViewModel<InterviewGroupViewModel>(this.newInterviewId);
        }

        private Task CreateInterview(QuestionnaireDocument questionnaireDocument)
        {
            return Task.Run(() =>
            {
                this.IsServerUnavailable = false;
                this.HasErrors = false;
                this.IsInProgress = true;

                this.ProgressIndicator = UIResources.ImportQuestionnaire_PrepareQuestionnaire;

                if (tokenSource.IsCancellationRequested) return;
                this.ExecuteImportFromDesignerForTesterCommand(questionnaireDocument);

                this.ProgressIndicator = UIResources.ImportQuestionnaire_CreateInterview;

                Guid interviewUserId = Guid.NewGuid();
                this.newInterviewId = Guid.NewGuid();

                if (tokenSource.IsCancellationRequested) return;
                this.ExecuteCreateInterviewCommand(this.newInterviewId, interviewUserId, questionnaireDocument.PublicKey);

                if (tokenSource.IsCancellationRequested) return;
                this.InvokeOnMainThread(this.OpenInterview);

                this.CanCreateInterview = true;
                this.IsInProgress = false;

            }, this.tokenSource.Token);
        }

        private void ExecuteCreateInterviewCommand(Guid interviewId, Guid interviewUserId, Guid questionnaireId)
        {
            this.commandService.Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId, questionnaireId, new Dictionary<Guid, object>(), DateTime.UtcNow));
        }

        private void ExecuteImportFromDesignerForTesterCommand(QuestionnaireDocument template)
        {
            this.commandService.Execute(new ImportFromDesignerForTester(template));
        }

        public override void NavigateToPreviousViewModel()
        {
            this.tokenSource.Cancel();
            this.ShowViewModel<DashboardViewModel>();
        }
    }
}