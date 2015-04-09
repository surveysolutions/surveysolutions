using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IPrincipal principal;
        private readonly IRestService restService;
        private readonly IUserInteraction uiDialogs;
        private readonly IQueryablePlainStorageAccessor<QuestionnaireListItem> questionnairesStorageAccessor;
        private readonly CancellationTokenSource tokenSource = new CancellationTokenSource();

        public DashboardViewModel(IPrincipal principal, IRestService restService, ILogger logger, IUserInteraction uiDialogs,
            IQueryablePlainStorageAccessor<QuestionnaireListItem> questionnairesStorageAccessor)
            : base(logger)
        {
            this.principal = principal;
            this.restService = restService;
            this.uiDialogs = uiDialogs;
            this.questionnairesStorageAccessor = questionnairesStorageAccessor;
        }

        public async void Init()
        {
            await this.BindQuestionnairesFromStorage();
        }

        private ObservableCollection<QuestionnaireListItem> myQuestionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> MyQuestionnaires
        {
            get { return myQuestionnaires; }
            set
            {
                myQuestionnaires = value;
                RaisePropertyChanged(() => MyQuestionnaires);
            }
        }

        private ObservableCollection<QuestionnaireListItem> publicQuestionnaires = new ObservableCollection<QuestionnaireListItem>();
        public ObservableCollection<QuestionnaireListItem> PublicQuestionnaires
        {
            get { return publicQuestionnaires; }
            set
            {
                publicQuestionnaires = value;
                RaisePropertyChanged(() => PublicQuestionnaires);
            }
        }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set
            {
                isInProgress = value;
                RaisePropertyChanged(() => IsInProgress);
            }
        }

        private bool isPublicShowed;
        public bool IsPublicShowed
        {
            get { return isPublicShowed; }
            set
            {
                isPublicShowed = value;
                RaisePropertyChanged(() => IsPublicShowed);
            }
        }

        private IMvxCommand signOutCommand;
        public IMvxCommand SignOutCommand
        {
            get { return signOutCommand ?? (signOutCommand = new MvxCommand(this.SignOut)); }
        }

        private IMvxCommand showSettingsCommand;
        public IMvxCommand ShowSettingsCommand
        {
            get { return showSettingsCommand ?? (showSettingsCommand = new MvxCommand(() => this.ShowViewModel<SettingsViewModel>())); }
        }

        private IMvxCommand showAboutCommand;
        public IMvxCommand ShowAboutCommand
        {
            get { return showAboutCommand ?? (showAboutCommand = new MvxCommand(() => this.ShowViewModel<AboutViewModel>())); }
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get { return loadQuestionnaireCommand ?? (loadQuestionnaireCommand = new MvxCommand<QuestionnaireListItem>(this.LoadQuestionnaire)); }
        }

        private IMvxCommand refreshQuestionnairesCommand;
        public IMvxCommand RefreshQuestionnairesCommand
        {
            get { return refreshQuestionnairesCommand ?? (refreshQuestionnairesCommand = new MvxCommand(async () => await this.GetServerQuestionnaires(), () => !this.IsInProgress)); }
        }

        private IMvxCommand searchQuestionnairesCommand;
        public IMvxCommand SearchQuestionnairesCommand
        {
            get { return searchQuestionnairesCommand ?? (searchQuestionnairesCommand = new MvxCommand(() => this.ShowViewModel<SearchQuestionnairesViewModel>())); }
        }

        private IMvxCommand showMyQuestionnairesCommand;
        public IMvxCommand ShowMyQuestionnairesCommand
        {
            get { return showMyQuestionnairesCommand ?? (showMyQuestionnairesCommand = new MvxCommand(this.ShowMyQuestionnaires)); }
        }

        private IMvxCommand showPublicQuestionnairesCommand;
        public IMvxCommand ShowPublicQuestionnairesCommand
        {
            get { return showPublicQuestionnairesCommand ?? (showPublicQuestionnairesCommand = new MvxCommand(this.ShowPublicQuestionnaires)); }
        }

        private void ShowPublicQuestionnaires()
        {
            this.IsPublicShowed = true;
        }

        private void ShowMyQuestionnaires()
        {
            this.IsPublicShowed = false;
        }

        private void LoadQuestionnaire(QuestionnaireListItem questionnaire)
        {
            this.tokenSource.Cancel();

            this.ShowViewModel<PrefilledQuestionsViewModel>(questionnaire.Id);
        }

        private Task BindQuestionnairesFromStorage()
        {
            return Task.Run(async () =>
            {
                var userQuestionnaires = this.questionnairesStorageAccessor.Query(storageModel => storageModel.OwnerName == this.principal.CurrentUserIdentity.Name);
                if (userQuestionnaires.Any())
                {
                    this.MyQuestionnaires = new ObservableCollection<QuestionnaireListItem>(userQuestionnaires.Where(qli => !qli.IsPublic));
                    this.PublicQuestionnaires = new ObservableCollection<QuestionnaireListItem>(userQuestionnaires.Where(qli => qli.IsPublic));    
                }
                else
                {
                    await this.GetServerQuestionnaires();
                }
                
            });
        }

        public Task GetServerQuestionnaires()
        {
            return Task.Run(async () =>
            {
                this.IsInProgress = true;
            
            try
            {
                this.ClearQuestionnaires();

                const int pageSize = 20;
                int pageIndex = 1;

                foreach (var serverQuestionnaires in GetPagedQuestionnaires(pageIndex++, pageSize))
                {
                    var convertedQuestionnaires = serverQuestionnaires.Select(ConvertToLocalQuestionnaireListItem);

                    this.questionnairesStorageAccessor.Store(convertedQuestionnaires.Select(qli => new Tuple<QuestionnaireListItem, string>(qli, qli.Id)));
                    this.InvokeOnMainThread(() => this.AppendToQuestionnaires(convertedQuestionnaires));
                }
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        this.SignOut();
                        break;
                    case HttpStatusCode.ServiceUnavailable:
                        this.uiDialogs.Alert(ex.Message.Contains("maintenance")
                            ? UIResources.Maintenance
                            : UIResources.ServiceUnavailable);
                        break;
                    case HttpStatusCode.RequestTimeout:
                        this.uiDialogs.Alert(UIResources.RequestTimeout);
                        break;
                    case HttpStatusCode.InternalServerError:
                        this.Logger.Error("Internal server error when getting questionnaires.", ex);
                        this.uiDialogs.Alert(UIResources.InternalServerError);
                        break;
                    default:
                        throw;
                }
            }
            finally
            {
                this.IsInProgress = false;
            }
            });
        }

        private IEnumerable<IEnumerable<QuestionnaireListItem>> GetPagedQuestionnaires(int pageIndex, int pageSize)
        {
            yield return this.restService.GetAsync<IEnumerable<QuestionnaireListItem>>(
                url: "questionnaires",
                token: tokenSource.Token,
                credentials:
                    new RestCredentials()
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                queryString: new {pageIndex = pageIndex, pageSize = pageSize}).Result;
        }

        private void SignOut()
        {
            this.principal.SignOut();
            this.ShowViewModel<LoginViewModel>();
        }

        private QuestionnaireListItem ConvertToLocalQuestionnaireListItem(QuestionnaireListItem questionnaireListItem)
        {
            questionnaireListItem.OwnerName = this.principal.CurrentUserIdentity.Name;
            return questionnaireListItem;
        }

        private void ClearQuestionnaires()
        {
            var userQuestionnaires = this.questionnairesStorageAccessor.Query(storageModel => storageModel.OwnerName == this.principal.CurrentUserIdentity.Name);
            this.questionnairesStorageAccessor.Remove(userQuestionnaires);
            this.MyQuestionnaires.Clear();
            this.PublicQuestionnaires.Clear();
        }

        private void AppendToQuestionnaires(IEnumerable<QuestionnaireListItem> questionnaires)
        {
            foreach (var questionnaireListItem in questionnaires)
            {
                if (questionnaireListItem.IsPublic)
                    this.PublicQuestionnaires.Add(questionnaireListItem);
                else
                    this.MyQuestionnaires.Add(questionnaireListItem);
            }
        }

        public override void NavigateToPreviousViewModel()
        {
            
        }
    }
}