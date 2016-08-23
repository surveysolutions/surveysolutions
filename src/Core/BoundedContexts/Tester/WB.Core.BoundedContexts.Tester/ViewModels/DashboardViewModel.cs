using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Core.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Services;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;
using QuestionnaireListItem = WB.Core.BoundedContexts.Tester.Views.QuestionnaireListItem;

namespace WB.Core.BoundedContexts.Tester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly ICommandService commandService;

        private CancellationTokenSource tokenSource;
        private readonly IDesignerApiService designerApiService;

        private readonly IQuestionnaireImportService questionnaireImportService;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IUserInteractionService userInteractionService;
        private IReadOnlyCollection<QuestionnaireListItem> localQuestionnaires = new List<QuestionnaireListItem>();
        private readonly IAsyncPlainStorage<QuestionnaireListItem> questionnaireListStorage;
        private readonly IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage;
        private readonly ILogger logger;
        private readonly IAttachmentContentStorage attachmentContentStorage;
        private readonly IAsyncRunner asyncRunner;
        private readonly IAsyncPlainStorage<TranslationInstance> translationsStorage;

        private readonly IFriendlyErrorMessageService friendlyErrorMessageService;

        public DashboardViewModel(
            IPrincipal principal,
            IDesignerApiService designerApiService, 
            ICommandService commandService, 
            IQuestionnaireImportService questionnaireImportService,
            IViewModelNavigationService viewModelNavigationService,
            IFriendlyErrorMessageService friendlyErrorMessageService,
            IUserInteractionService userInteractionService,
            IAsyncPlainStorage<QuestionnaireListItem> questionnaireListStorage, 
            IAsyncPlainStorage<DashboardLastUpdate> dashboardLastUpdateStorage,
            ILogger logger,
            IAttachmentContentStorage attachmentContentStorage,
            IAsyncRunner asyncRunner,
            IAsyncPlainStorage<TranslationInstance> translationsStorage)
            : base(principal, viewModelNavigationService)
        {
            this.designerApiService = designerApiService;
            this.commandService = commandService;
            this.questionnaireImportService = questionnaireImportService;
            this.viewModelNavigationService = viewModelNavigationService;
            this.userInteractionService = userInteractionService;
            this.questionnaireListStorage = questionnaireListStorage;
            this.dashboardLastUpdateStorage = dashboardLastUpdateStorage;
            this.logger = logger;
            this.attachmentContentStorage = attachmentContentStorage;
            this.asyncRunner = asyncRunner;
            this.translationsStorage = translationsStorage;
            this.friendlyErrorMessageService = friendlyErrorMessageService;
        }

        public override void Load()
        {
            this.localQuestionnaires = this.questionnaireListStorage.LoadAll();
            
            if (!localQuestionnaires.Any())
            {
                this.asyncRunner.RunAsync(this.LoadServerQuestionnairesAsync);
            }
            else
            {
                this.SearchByLocalQuestionnaires();
            }

            this.ShowEmptyQuestionnaireListText = true;
            this.IsSearchVisible = false;

            var lastUpdate = this.dashboardLastUpdateStorage.GetById(this.principal.CurrentUserIdentity.Name);

            this.HumanizeLastUpdateDate(lastUpdate?.LastUpdateDate);
        }
       
        private void SearchByLocalQuestionnaires(string searchTerm = null)
        {
            var trimmedSearchText = (searchTerm ?? "").Trim();

            Func<QuestionnaireListItem, bool> emptyFilter = x => true;
            
            Func<QuestionnaireListItem, bool> titleSearchFilter = x => CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Title, trimmedSearchText, CompareOptions.IgnoreCase) >= 0 ||
                    (x.OwnerName != null && x.OwnerName.Contains(trimmedSearchText));
            Func<QuestionnaireListItem, bool> searchFilter = string.IsNullOrEmpty(trimmedSearchText)
                ? emptyFilter
                : titleSearchFilter;

            var myQuestionnaires = this.localQuestionnaires
                .Where(questionnaire =>
                    searchFilter(questionnaire)
                    &&
                    (
                        string.Equals(questionnaire.OwnerName, this.principal.CurrentUserIdentity.Name, StringComparison.OrdinalIgnoreCase)
                        ||
                        questionnaire.IsShared
                    ))
                .ToList();

            var publicQuestionnaires = this.localQuestionnaires
                .Where(questionnaire => searchFilter(questionnaire) && questionnaire.IsPublic)
                .ToList();

            var selectedQuestionnaires = this.IsPublicShowed ? publicQuestionnaires : myQuestionnaires;

            this.MyQuestionnairesCount = myQuestionnaires.Count;
            this.PublicQuestionnairesCount = publicQuestionnaires.Count;

            this.IsListEmpty = !selectedQuestionnaires.Any();

            this.Questionnaires = selectedQuestionnaires
                .Select(questionnaire => ToDashboardQuestionnaire(questionnaire, searchTerm))
                .OrderByDescending(questionnaire => questionnaire.LastEntryDate)
                .ToList();
        }

        private string humanizedLastUpdateDate;
        public string HumanizedLastUpdateDate
        {
            get { return this.humanizedLastUpdateDate; }
            set { this.humanizedLastUpdateDate = value; RaisePropertyChanged(); }
        }

        private IList<QuestionnaireListItem> questionnaires = new List<QuestionnaireListItem>();
        public IList<QuestionnaireListItem> Questionnaires
        {
            get { return this.questionnaires; }
            set { this.questionnaires = value; RaisePropertyChanged(); }
        }

        private bool showEmptyQuestionnaireListText;
        public bool ShowEmptyQuestionnaireListText
        {
            get { return this.showEmptyQuestionnaireListText; }
            set { this.showEmptyQuestionnaireListText = value; RaisePropertyChanged(); }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(); }
        }

        private string progressIndicator;
        public string ProgressIndicator
        {
            get { return progressIndicator; }
            set { progressIndicator = value; RaisePropertyChanged(); }
        }

        private int myQuestionnairesCount;
        public int MyQuestionnairesCount
        {
            get { return myQuestionnairesCount; }
            set { myQuestionnairesCount = value; RaisePropertyChanged(); }
        }

        private int publicQuestionnairesCount;
        public int PublicQuestionnairesCount
        {
            get { return publicQuestionnairesCount; }
            set { publicQuestionnairesCount = value; RaisePropertyChanged(); }
        }

        private bool isPublicShowed;
        public bool IsPublicShowed
        {
            get { return isPublicShowed; }
            set { isPublicShowed = value; RaisePropertyChanged(); }
        }

        private bool isSearchVisible;
        public bool IsSearchVisible
        {
            get { return isSearchVisible; }
            set { isSearchVisible = value; RaisePropertyChanged(); }
        }

        public bool IsListEmpty
        {
            get { return this.isListEmpty; }
            set { this.isListEmpty = value; RaisePropertyChanged(); }
        }
        
        public IMvxCommand ClearSearchCommand => new MvxCommand(this.ClearSearch);

        public IMvxCommand ShowSearchCommand => new MvxCommand(this.ShowSearch);

        public IMvxCommand SearchCommand => new MvxCommand<string>(this.SearchByLocalQuestionnaires);

        public IMvxCommand SignOutCommand => new MvxCommand(this.SignOut);

        private IMvxCommand loadQuestionnaireCommand;

        public IMvxCommand LoadQuestionnaireCommand => this.loadQuestionnaireCommand ??
                                                       (this.loadQuestionnaireCommand = new MvxCommand<QuestionnaireListItem>(
                                                               async (questionnaire) => await this.LoadQuestionnaireAsync(questionnaire), (item) => !this.IsInProgress));

        private IMvxAsyncCommand refreshQuestionnairesCommand;

        public IMvxAsyncCommand RefreshQuestionnairesCommand => this.refreshQuestionnairesCommand ??
                                                           (this.refreshQuestionnairesCommand =
                                                               new MvxAsyncCommand(this.LoadServerQuestionnairesAsync, () => !this.IsInProgress));
        
        public IMvxCommand ShowMyQuestionnairesCommand => new MvxCommand(this.ShowMyQuestionnaires);
        public IMvxCommand ShowPublicQuestionnairesCommand => new MvxCommand(this.ShowPublicQuestionnaires);

        private string searchText;
        public string SearchText
        {
            get { return this.searchText; }
            set { this.searchText = value; RaisePropertyChanged(); }
        }

        private bool isListEmpty;

        private void ShowSearch()
        {
            if (IsInProgress)
                return;
            IsSearchVisible = true;
        }

        private void ClearSearch()
        {
            this.SearchText = null;
            IsSearchVisible = false;
        }

        private void SignOut()
        {
            this.CancelLoadServerQuestionnaires();
            
            this.viewModelNavigationService.SignOutAndNavigateToLogin();
        }

        private void ShowPublicQuestionnaires()
        {
            this.IsPublicShowed = true;

            this.SearchByLocalQuestionnaires(this.SearchText);
        }

        private void ShowMyQuestionnaires()
        {
            this.IsPublicShowed = false;

            this.SearchByLocalQuestionnaires(this.SearchText);
        }

        private async Task LoadQuestionnaireAsync(QuestionnaireListItem selectedQuestionnaire)
        {
            if (this.IsInProgress) return;
            this.tokenSource = new CancellationTokenSource();
            this.IsInProgress = true;

            this.ProgressIndicator = TesterUIResources.ImportQuestionnaire_CheckConnectionToServer;

            try
            {
                var questionnairePackage = await this.DownloadQuestionnaire(selectedQuestionnaire);

                if (questionnairePackage != null)
                {
                    this.ProgressIndicator = TesterUIResources.ImportQuestionnaire_StoreQuestionnaire;

                    await this.DownloadQuestionnaireAttachments(questionnairePackage);

                    var fakeQuestionnaireIdentity = GenerateFakeQuestionnaireIdentity();

                    var translations = await this.designerApiService.GetTranslationsAsync(questionnaireId: selectedQuestionnaire.Id, token: this.tokenSource.Token);

                    await this.StoreQuestionnaireWithNewIdentity(fakeQuestionnaireIdentity, questionnairePackage, translations);

                    var interviewId = await this.CreateInterview(fakeQuestionnaireIdentity);

                    this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
                }
            }
            catch (RestException ex)
            {
                if (ex.Type == RestExceptionType.RequestCanceledByUser)
                    return;

                string errorMessage;
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Forbidden:
                        errorMessage = string.Format(TesterUIResources.ImportQuestionnaire_Error_Forbidden, selectedQuestionnaire.Title);
                        break;
                    case HttpStatusCode.PreconditionFailed:
                        errorMessage = String.Format(TesterUIResources.ImportQuestionnaire_Error_PreconditionFailed, selectedQuestionnaire.Title);
                        break;
                    case HttpStatusCode.NotFound:
                        errorMessage = String.Format(TesterUIResources.ImportQuestionnaire_Error_NotFound, selectedQuestionnaire.Title);
                        break;
                    default:
                        errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);
                        break;
                }

                if (!string.IsNullOrEmpty(errorMessage))
                    await this.userInteractionService.AlertAsync(errorMessage);
                else throw;
            }
            catch (Exception ex)
            {
                this.logger.Error("Import questionnaire exception. ", ex);
            }
            finally
            {
                this.IsInProgress = false;   
            }
        }

        private QuestionnaireIdentity GenerateFakeQuestionnaireIdentity()
        {
            var questionnaireIdentity = new QuestionnaireIdentity(Guid.Parse("11111111-1111-1111-1111-111111111111"), 1);
            return questionnaireIdentity;
        }

        private async Task<Guid> CreateInterview(QuestionnaireIdentity questionnaireIdentity)
        {
            this.ProgressIndicator = TesterUIResources.ImportQuestionnaire_CreateInterview;

            var interviewId = Guid.NewGuid();

            await this.commandService.ExecuteAsync(new CreateInterviewOnClientCommand(
                interviewId: interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionnaireIdentity: questionnaireIdentity,
                answersTime: DateTime.UtcNow,
                supervisorId: Guid.NewGuid()));
            return interviewId;
        }

        private async Task StoreQuestionnaireWithNewIdentity(QuestionnaireIdentity questionnaireIdentity, Questionnaire questionnairePackage, TranslationDto[] translations)
        {
            this.ProgressIndicator = TesterUIResources.ImportQuestionnaire_StoreQuestionnaire;

            var questionnaireDocument = questionnairePackage.Document;
            questionnaireDocument.PublicKey = questionnaireIdentity.QuestionnaireId;
            questionnaireDocument.Id = questionnaireIdentity.QuestionnaireId.FormatGuid();

            var supportingAssembly = questionnairePackage.Assembly;
            
            await this.questionnaireImportService.ImportQuestionnaireAsync(questionnaireIdentity, questionnaireDocument, supportingAssembly, translations);
        }

        private async Task<Questionnaire> DownloadQuestionnaire(QuestionnaireListItem selectedQuestionnaire)
        {
            return await this.designerApiService.GetQuestionnaireAsync(
                selectedQuestionnaire: selectedQuestionnaire,
                onDownloadProgressChanged: (downloadProgress) =>
                {
                    this.ProgressIndicator = string.Format(TesterUIResources.ImportQuestionnaire_DownloadProgress, downloadProgress);
                },
                token: this.tokenSource.Token);
        }

        private async Task DownloadQuestionnaireAttachments(Questionnaire questionnaire)
        {
            if (questionnaire == null)
                return;

            var attachments = questionnaire.Document.Attachments;

            foreach (var attachment in attachments)
            {
                var attachmentContentId = attachment.ContentId;

                var isExistsContent = this.attachmentContentStorage.Exists(attachmentContentId);
                if (!isExistsContent)
                {
                    var attachmentContent = await this.designerApiService.GetAttachmentContentAsync(attachmentContentId,
                                        onDownloadProgressChanged: (downloadProgress) =>
                                        {
                                            this.ProgressIndicator = string.Format(TesterUIResources.ImportQuestionnaireAttachments_DownloadProgress, downloadProgress);
                                        },
                                        token: this.tokenSource.Token);

                    await this.attachmentContentStorage.StoreAsync(attachmentContent);
                }
            }
        }
       
        private async Task LoadServerQuestionnairesAsync()
        {
            this.IsInProgress = true;
            this.tokenSource = new CancellationTokenSource();
            try
            {
                this.ClearSearch();

                await this.questionnaireListStorage.RemoveAsync(this.localQuestionnaires);

                this.localQuestionnaires = await this.designerApiService.GetQuestionnairesAsync(token: tokenSource.Token);
                
                await this.questionnaireListStorage.StoreAsync(this.localQuestionnaires);

                var lastUpdateDate = DateTime.UtcNow;
                this.HumanizeLastUpdateDate(lastUpdateDate);

                await this.dashboardLastUpdateStorage.StoreAsync(new DashboardLastUpdate
                {
                    Id = this.principal.CurrentUserIdentity.Name,
                    LastUpdateDate = lastUpdateDate
                });

                this.SearchByLocalQuestionnaires();
            }
            catch (RestException ex)
            {
                if (ex.Type == RestExceptionType.RequestCanceledByUser)
                    return;

                var errorMessage = this.friendlyErrorMessageService.GetFriendlyErrorMessageByRestException(ex);

                if (!string.IsNullOrEmpty(errorMessage))
                    await this.userInteractionService.AlertAsync(errorMessage);
                else throw;
            }
            catch (Exception ex)
            {
                this.logger.Error("Load questionnaire list exception. ", ex);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private void HumanizeLastUpdateDate(DateTime? lastUpdate)
        {
            this.HumanizedLastUpdateDate = lastUpdate.HasValue
                ? string.Format(TesterUIResources.Dashboard_LastUpdated, lastUpdate.Value.Humanize(dateToCompareAgainst: DateTime.UtcNow, culture: CultureInfo.InvariantCulture))
                : TesterUIResources.Dashboard_HaveNotBeenUpdated;
        }

        private QuestionnaireListItem ToDashboardQuestionnaire(QuestionnaireListItem x, string searchTerm)
        {
            return new QuestionnaireListItem
                   {
                       Id = x.Id,
                       IsPublic = this.IsPublicShowed,
                       LastEntryDate = x.LastEntryDate,
                       OwnerName = x.OwnerName,
                       Title = ToDashboardTitle(x.Title, searchTerm)
            };
        }

        private static string ToDashboardTitle(string questionnaireTitle, string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm)) return questionnaireTitle;

            var index = CultureInfo.CurrentCulture.CompareInfo.IndexOf(questionnaireTitle, searchTerm, CompareOptions.IgnoreCase);
                //questionnaireTitle.IndexOf(searchTerm, StringComparison.CurrentCultureIgnoreCase);

            string title;
            if (index >= 0)
            {
                var substringToHightlight =  questionnaireTitle.Substring(index, searchTerm.Length);
                title = Regex.Replace(questionnaireTitle, searchTerm, "<b>" + substringToHightlight + "</b>", RegexOptions.IgnoreCase);
            }
            else
            {
                title = questionnaireTitle;
            }
            return title;
        }

        public void CancelLoadServerQuestionnaires()
        {
            if (tokenSource != null && !tokenSource.IsCancellationRequested)
            {
                this.tokenSource.Cancel();
            }
        }
    }
}