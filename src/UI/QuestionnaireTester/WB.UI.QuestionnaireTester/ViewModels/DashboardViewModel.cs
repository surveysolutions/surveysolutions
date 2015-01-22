using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.QuestionnaireTester.Services;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;

namespace WB.UI.QuestionnaireTester.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly IRestService restService;

        private readonly QuestionnaireVersion supportedQuestionnaireVersion;

        public DashboardViewModel(IPrincipal principal, IRestService restService, ILogger logger,
            IUserInteraction uiDialogs) : base(logger, principal: principal, uiDialogs: uiDialogs)
        {
            this.restService = restService;

            var engineVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion();
            this.supportedQuestionnaireVersion = new QuestionnaireVersion()
            {
                Major = engineVersion.Major,
                Minor = engineVersion.Minor,
                Patch = engineVersion.Patch
            };
        }

        public async void Init()
        {
            await this.RefreshQuestionnaires();
        }

        private IList<QuestionnaireListItem> allQuestionnaires;
        private IList<QuestionnaireListItem> questionnaires;
        public IList<QuestionnaireListItem> Questionnaires
        {
            get { return questionnaires; }
            set
            {
                questionnaires = value;
                RaisePropertyChanged(() => Questionnaires);
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

        public string LoginName
        {
            get { return this.Principal.CurrentIdentity.Name; }
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

        private IMvxCommand showHelpCommand;
        public IMvxCommand ShowHelpCommand
        {
            get { return showHelpCommand ?? (showHelpCommand =  new MvxCommand(() => this.ShowViewModel<HelpViewModel>())); }
        }

        private IMvxCommand loadQuestionnaireCommand;
        public IMvxCommand LoadQuestionnaireCommand
        {
            get
            {
                return loadQuestionnaireCommand ?? (loadQuestionnaireCommand =
                    new MvxCommand<QuestionnaireListItem>(
                        LoadQuestionnaire,
                        (questionnaire) => questionnaire.Version <= supportedQuestionnaireVersion));
            }
        }

        private void  LoadQuestionnaire(QuestionnaireListItem questionnaire)
        {
            this.ShowViewModel<QuestionnairePrefilledQuestionsViewModel>(questionnaire);
        }

        private IMvxCommand refreshQuestionnairesCommand;
        public IMvxCommand RefreshQuestionnairesCommand
        {
            get { return refreshQuestionnairesCommand ?? (refreshQuestionnairesCommand = new MvxCommand(async ()=>await this.RefreshQuestionnaires(), () => !this.IsInProgress)); }
        }

        private IMvxCommand findQuestionnairesCommand;
        public IMvxCommand FindQuestionnairesCommand
        {
            get { return findQuestionnairesCommand ?? (findQuestionnairesCommand = new MvxCommand<string>(this.FindQuestionnaires, (qyery) => !this.IsInProgress)); }
        }

        private void FindQuestionnaires(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                if (this.allQuestionnaires == null) return;

                this.Questionnaires = this.allQuestionnaires;
                this.allQuestionnaires = null;
            }
            else
            {
                if (this.allQuestionnaires == null)
                {
                    this.allQuestionnaires = this.Questionnaires;
                }

                if (this.allQuestionnaires != null)
                {
                    this.Questionnaires = this.allQuestionnaires.Where(
                        item =>
                            item.Title.IndexOf(query, StringComparison.OrdinalIgnoreCase) > -1 ||
                            item.OwnerName.IndexOf(query, StringComparison.OrdinalIgnoreCase) > -1).ToList();
                }
            }
        }

        public async Task RefreshQuestionnaires()
        {
            this.IsInProgress = true;
            try
            {
                var questionnaireListCommunicationPackage = await this.restService.GetAsync<QuestionnaireListCommunicationPackage>(
                        url: "questionnairelist",
                        credentials: new RestCredentials() { Login = this.Principal.CurrentIdentity.Name, Password = this.Principal.CurrentIdentity.Password });
                
                    Questionnaires = questionnaireListCommunicationPackage.Items.ToList();
            }
            catch (RestException ex)
            {
                switch (ex.StatusCode)
                {
                    case HttpStatusCode.Unauthorized:
                        this.SignOut();
                        break;
                    default:
                        this.UIDialogs.Alert(ex.Message);
                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.Error("Unhandled exception when request list of questionnaires", ex);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }
    }
}