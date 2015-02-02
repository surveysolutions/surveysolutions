using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.UI.QuestionnaireTester.Properties;
using WB.UI.QuestionnaireTester.Services;
using WB.UI.Shared.Android.Controls.ScreenItems;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class QuestionnairePrefilledQuestionsViewModel : BaseViewModel
    {
        private readonly IRestService restService;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly ICommandService commandService;
        private readonly IReadSideStorage<QuestionnaireDocument> questionnairesStorage;
        public QuestionnaireListItem Questionnaire { get; private set; }
        public Action<Guid> OnInterviewCreated { get; set; }
        public Action<Guid> OnInterviewDetailsOpened { get; set; }

        private bool isAnswering = false;
        public bool IsAnswering
        {
            get { return isAnswering; }
            set
            {
                isAnswering = value;
                RaisePropertyChanged(() => IsAnswering);
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

        private ImportQuestionnaireDownloadingProgress progressIndicator = new ImportQuestionnaireDownloadingProgress();
        public ImportQuestionnaireDownloadingProgress ProgressIndicator
        {
            get { return progressIndicator; }
            set
            {
                progressIndicator = value;
                RaisePropertyChanged(() => ProgressIndicator);
            }
        }

        public QuestionnairePrefilledQuestionsViewModel(ILogger logger, IRestService restService,
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, ICommandService commandService,
            IPrincipal principal, IAnswerProgressIndicator answerProgressIndicator, IUserInteraction uiDialogs,
            IReadSideStorage<QuestionnaireDocument> questionnairesStorage)
            : base(logger, principal: principal, uiDialogs: uiDialogs)
        {
            answerProgressIndicator.Setup(() => this.IsAnswering = true, () => this.IsAnswering = false);

            this.restService = restService;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.commandService = commandService;
            this.questionnairesStorage = questionnairesStorage;
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get
            {
                return loadQuestionnaireCommand ?? (loadQuestionnaireCommand =
                    new MvxCommand(async ()=>await this.LoadQuestionnaireAndCreateInterivew(), () => !this.IsInProgress));
            }
        }

        private IMvxCommand openInterviewCommand;
        public IMvxCommand OpenInterviewCommand
        {
            get
            {
                return openInterviewCommand ?? (openInterviewCommand =
                    new MvxCommand(this.OpenInterview));
            }
        }

        private IMvxCommand goBackCommand;
        public IMvxCommand GoBackCommand
        {
            get
            {
                return goBackCommand ?? (goBackCommand = new MvxCommand(this.GoBack));
            }
        }

        public async void Init(QuestionnaireListItem questionnaire)
        {
            this.Questionnaire = questionnaire;

            await this.LoadQuestionnaireAndCreateInterivew();
        }

        private void OpenInterview()
        {
            if (this.interviewId.HasValue && this.OnInterviewDetailsOpened != null)
                this.OnInterviewDetailsOpened(this.interviewId.Value);

        }

        private Guid? interviewId;

        public Task LoadQuestionnaireAndCreateInterivew()
        {
            return Task.Run(async () =>
            {
                this.IsInProgress = true;

                try
                {
                    var questionnaireDocumentFromStorage = this.questionnairesStorage.GetById(this.Questionnaire.Id.FormatGuid());

                    if (questionnaireDocumentFromStorage == null ||
                        questionnaireDocumentFromStorage.LastEntryDate != (await this.GetQuestionnaireLastEntryDate()).LastEntryDate)
                    {
                        var questionnaireCommunicationPackage = await this.GetQuestionnaireFromServer();

                        this.questionnaireAssemblyFileAccessor.StoreAssembly(
                            questionnaireCommunicationPackage.Questionnaire.PublicKey, 0,
                            questionnaireCommunicationPackage.QuestionnaireAssembly);

                        this.questionnairesStorage.Store(questionnaireCommunicationPackage.Questionnaire, questionnaireCommunicationPackage.Questionnaire.PublicKey.FormatGuid());
                        questionnaireDocumentFromStorage = questionnaireCommunicationPackage.Questionnaire;
                    }

                    this.ExecuteImportFromDesignerForTesterCommand(questionnaireDocumentFromStorage);

                    Guid interviewUserId = Guid.NewGuid();
                    Guid interviewId = Guid.NewGuid();

                    this.ExecuteCreateInterviewCommand(interviewId, interviewUserId, questionnaireDocumentFromStorage.PublicKey);

                    this.interviewId = interviewId;
                    if (this.OnInterviewCreated != null)
                    {
                        this.InvokeOnMainThread(() => this.OnInterviewCreated(interviewId));
                    }
                }
                catch (RestException ex)
                {
                    switch (ex.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            this.SignOut();
                            break;
                        case HttpStatusCode.Forbidden:
                            this.UIDialogs.Alert(UIResources.ImportQuestionnaireForbidden);
                            break;
                        case HttpStatusCode.UpgradeRequired:
                            this.UIDialogs.Alert(UIResources.ImportQuestionnaireUpgradeRequired);
                            break;
                        case HttpStatusCode.PreconditionFailed:
                            this.UIDialogs.Alert(string.Format(UIResources.ImportQuestionnairePreconditionFailed,
                                this.Questionnaire.Title));
                            break;
                        case HttpStatusCode.NotFound:
                            this.UIDialogs.Alert(string.Format(UIResources.ImportQuestionnaireNotFound,
                                this.Questionnaire.Title));
                            break;
                        case HttpStatusCode.ServiceUnavailable:
                            this.UIDialogs.Alert(ex.Message.Contains("maintenance")
                                ? UIResources.Maintenance
                                : UIResources.ServiceUnavailable);
                            break;
                        case HttpStatusCode.RequestTimeout:
                            this.UIDialogs.Alert(UIResources.RequestTimeout);
                            break;
                        default:
                            throw;
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.Error("Exception when downloading questionnaire/creating interview", ex);
                }
                finally
                {
                    this.IsInProgress = false;
                }
            });
        }

        private void ExecuteCreateInterviewCommand(Guid interviewId, Guid interviewUserId, Guid questionnaireId)
        {
            this.commandService.Execute(new CreateInterviewForTestingCommand(interviewId, interviewUserId, questionnaireId, new Dictionary<Guid, object>(), DateTime.UtcNow));
        }

        private void ExecuteImportFromDesignerForTesterCommand(QuestionnaireDocument template)
        {
            this.commandService.Execute(new ImportFromDesignerForTester(template));
        }

        private async Task<QuestionnaireLastEntryDateResponse> GetQuestionnaireLastEntryDate()
        {
            return await this.restService.PostAsync<QuestionnaireLastEntryDateResponse>(
                url: "GetQuestionnaireLastModifiedDate",
                credentials:
                    new RestCredentials()
                    {
                        Login = this.Principal.CurrentIdentity.Name,
                        Password = this.Principal.CurrentIdentity.Password
                    },
                request: new QuestionnaireLastEntryDateRequest()
                {
                    QuestionnaireId = Questionnaire.Id
                });
        }

        public async Task<QuestionnaireCommunicationPackage> GetQuestionnaireFromServer()
        {
            var supportedVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion();

            return await this.restService.PostWithProgressAsync<QuestionnaireCommunicationPackage>(
                url: "questionnaire",
                credentials:
                    new RestCredentials()
                    {
                        Login = this.Principal.CurrentIdentity.Name,
                        Password = this.Principal.CurrentIdentity.Password
                    },
                request: new DownloadQuestionnaireRequest()
                {
                    QuestionnaireId = Questionnaire.Id,
                    SupportedVersion = new QuestionnaireVersion()
                    {
                        Major = supportedVersion.Major,
                        Minor = supportedVersion.Minor,
                        Patch = supportedVersion.Patch
                    }
                }, progress: this.ProgressIndicator, token: new CancellationToken());
        }
    }

    public class ImportQuestionnaireDownloadingProgress : MvxViewModel, IProgress<decimal>
    {
        private decimal progress = 0;
        public decimal Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                RaisePropertyChanged(() => Progress);
            }
        }

        public void Report(decimal value)
        {
            this.Progress = value;
        }
    }
}