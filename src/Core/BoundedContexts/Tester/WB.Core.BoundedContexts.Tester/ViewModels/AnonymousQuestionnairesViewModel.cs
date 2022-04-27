using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using MvvmCross.Commands;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.BoundedContexts.Tester.Views;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.HttpServices.HttpClient;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;
using WB.Core.SharedKernels.Enumerator.ViewModels;

namespace WB.Core.BoundedContexts.Tester.ViewModels;

public class AnonymousQuestionnairesViewModel : BaseViewModel
{
    private CancellationTokenSource tokenSource;

    private readonly IUserInteractionService userInteractionService;
    private IReadOnlyCollection<AnonymousQuestionnaireListItem> localQuestionnaires = new List<AnonymousQuestionnaireListItem>();
    private readonly IPlainStorage<AnonymousQuestionnaireListItem> questionnaireListStorage;
    private readonly ILogger logger;
    private readonly IQRBarcodeScanService qrBarcodeScanService;

    private QuestionnaireDownloadViewModel QuestionnaireDownloader { get; }

    public AnonymousQuestionnairesViewModel(
        IPrincipal principal,
        IViewModelNavigationService viewModelNavigationService,
        IUserInteractionService userInteractionService,
        IPlainStorage<AnonymousQuestionnaireListItem> questionnaireListStorage, 
        ILogger logger,
        QuestionnaireDownloadViewModel questionnaireDownloader,
        IQRBarcodeScanService qrBarcodeScanService)
        : base(principal, viewModelNavigationService)
    {
        this.userInteractionService = userInteractionService;
        this.questionnaireListStorage = questionnaireListStorage;
        this.logger = logger;
        this.qrBarcodeScanService = qrBarcodeScanService;
        this.QuestionnaireDownloader = questionnaireDownloader;
    }

    public override async Task Initialize()
    {
        await base.Initialize().ConfigureAwait(false);

        this.localQuestionnaires = this.questionnaireListStorage.LoadAll();
        
        if (localQuestionnaires.Any())
            this.SearchByLocalQuestionnaires();

        this.ShowEmptyQuestionnaireListText = true;
        this.IsSearchVisible = false;
    }
   
    private void SearchByLocalQuestionnaires(string searchTerm = null)
    {
        var filteredBySearchTerm = this.localQuestionnaires.Where(x => this.HasSearchTerm(searchTerm, x)).ToArray();

        this.Questionnaires = filteredBySearchTerm
            .Select(x => ToViewModel(x, searchTerm))
            .OrderByDescending(questionnaire => questionnaire.LastEntryDate)
            .ToList();

        this.IsListEmpty = !this.Questionnaires.Any();
    }

    private QuestionnaireListItemViewModel ToViewModel(AnonymousQuestionnaireListItem item, string searchTerm) =>
        new QuestionnaireListItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            LastEntryDate = item.LastEntryDate,
            SearchTerm = searchTerm,
        };

    private bool HasSearchTerm(string searchTerm, AnonymousQuestionnaireListItem x)
    {
        if (string.IsNullOrEmpty(searchTerm)) return true;

        var titleHasSearchTerm = CultureInfo.CurrentCulture.CompareInfo.IndexOf(x.Title, searchTerm, CompareOptions.IgnoreCase) >= 0;

        return titleHasSearchTerm;
    }

    private IList<QuestionnaireListItemViewModel> questionnaires = new List<QuestionnaireListItemViewModel>();
    public IList<QuestionnaireListItemViewModel> Questionnaires
    {
        get => this.questionnaires;
        set => this.SetProperty(ref this.questionnaires, value);
    }

    private bool showEmptyQuestionnaireListText;
    public bool ShowEmptyQuestionnaireListText
    {
        get => this.showEmptyQuestionnaireListText;
        set => this.SetProperty(ref this.showEmptyQuestionnaireListText, value);
    }

    private bool isInProgress;
    public bool IsInProgress
    {
        get => isInProgress;
        set => this.SetProperty(ref this.isInProgress, value);
    }

    private string progressIndicator;
    public string ProgressIndicator
    {
        get => progressIndicator;
        set => this.SetProperty(ref this.progressIndicator, value);
    }

    private bool isSearchVisible;
    public bool IsSearchVisible
    {
        get => isSearchVisible;
        set => this.SetProperty(ref this.isSearchVisible, value);
    }

    private bool isListEmpty;
    public bool IsListEmpty
    {
        get => this.isListEmpty;
        set => this.SetProperty(ref this.isListEmpty, value);
    }

    private string searchText;
    public string SearchText
    {
        get => this.searchText;
        set => this.SetProperty(ref this.searchText, value);
    }

    public IMvxCommand ClearSearchCommand => new MvxCommand(this.ClearSearch);

    public IMvxCommand ShowSearchCommand => new MvxCommand(this.ShowSearch);

    public IMvxCommand SearchCommand => new MvxCommand<string>(this.SearchByLocalQuestionnaires);

    public IMvxCommand SignOutCommand => new MvxAsyncCommand(this.SignOut);

    public IMvxAsyncCommand<QuestionnaireListItemViewModel> LoadQuestionnaireCommand =>
        new MvxAsyncCommand<QuestionnaireListItemViewModel>(this.LoadQuestionnaireAsync, _ => !this.IsInProgress);

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

    private Task SignOut()
    {
        this.CancelLoadServerQuestionnaires();
        
        return this.ViewModelNavigationService.SignOutAndNavigateToLoginAsync();
    }


    private async Task LoadQuestionnaireAsync(QuestionnaireListItemViewModel questionnaireListItem)
    {
        if (this.IsInProgress) return;

        this.tokenSource = new CancellationTokenSource();
        this.IsInProgress = true;

        var progress = new Progress<string>();

        try
        {
            await this.QuestionnaireDownloader
                .LoadQuestionnaireAsync(questionnaireListItem.Id, questionnaireListItem.Title, progress,
                    this.tokenSource.Token);
        }
        finally
        {
            this.IsInProgress = false;
        }
    }

    public void CancelLoadServerQuestionnaires()
    {
        if (tokenSource != null && !tokenSource.IsCancellationRequested)
        {
            this.tokenSource.Cancel();
        }
    }


    private IMvxAsyncCommand scanQrCodeCommand;
    
    public IMvxAsyncCommand ScanQrCodeCommand
    {
        get { return this.scanQrCodeCommand ??= new MvxAsyncCommand(this.ScanQrCodeAsync, () => !IsInProgress); }
    }

    private async Task ScanQrCodeAsync()
    {
        this.IsInProgress = true;

        try
        {
            var scanCode = await this.qrBarcodeScanService.ScanAsync();

            if (scanCode?.Code != null)
            {
                if (Uri.TryCreate(scanCode.Code, UriKind.Absolute, out var scannedUrl) && scannedUrl != null)
                {
                    var idFromUrl = scannedUrl.ToString().Trim('/').Split('/').Last();
                    if (Guid.TryParse(idFromUrl, out var qId))
                    {
                        var questionnaireListItem = new QuestionnaireListItemViewModel()
                        {
                            Id = qId.FormatGuid(),
                            Title = idFromUrl
                        };
                        await LoadQuestionnaireAsync(questionnaireListItem);
                    }
                }
            }
        }
        catch (Exception e)
        {
            this.logger.Error("Qrbarcode reader error: ", e);
            userInteractionService.ShowToast(EnumeratorUIResources.FinishInstallation_QrBarcodeReaderErrorMessage);
        }
        finally
        {
            this.IsInProgress = false;
        }
    }
}