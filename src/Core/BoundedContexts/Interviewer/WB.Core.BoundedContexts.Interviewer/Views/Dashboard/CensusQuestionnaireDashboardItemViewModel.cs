using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Interviewer.ChangeLog;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CensusQuestionnaireDashboardItemViewModel : IDashboardItem
    {
        private readonly ICommandService commandService;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IChangeLogManipulator changeLogManipulator;
        private readonly IPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;

        public CensusQuestionnaireDashboardItemViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            ICommandService commandService,
            IChangeLogManipulator changeLogManipulator,
            IPrincipal principal,
            IViewModelNavigationService viewModelNavigationService)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.commandService = commandService;
            this.changeLogManipulator = changeLogManipulator;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
        }

        private Guid questionnaireId;
        private int questionnaireVersion;

        public void Init(string id)
        {
            var questionnaire = this.questionnaireRepository.GetById(id);
            questionnaireId = questionnaire.Id;
            questionnaireVersion = 1;
            QuestionariName = questionnaire.Title;
            Comment = "Census mode, Interviews created: 0 / 1 / 100";
        }

        public string QuestionariName { get; set; }
        public string Comment { get; set; }



        public IMvxCommand CreateNewInterviewCommand
        {
            get { return new MvxCommand(this.CreateNewInterview); }
        }

        private async void CreateNewInterview()
        {
            var interviewId = Guid.NewGuid();
            var interviewerIdentity = (InterviewerIdentity)principal.CurrentUserIdentity;

            var createInterviewOnClientCommand = new CreateInterviewOnClientCommand(interviewId, interviewerIdentity.UserId, questionnaireId, questionnaireVersion, DateTime.UtcNow, interviewerIdentity.SupervisorId);
            await commandService.ExecuteAsync(createInterviewOnClientCommand);
            changeLogManipulator.CreatePublicRecord(interviewId);
            this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
        }
    }
}