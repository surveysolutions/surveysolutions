using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.Messenger;
using WB.Core.BoundedContexts.Interviewer.Properties;
using WB.Core.BoundedContexts.Interviewer.Services.Infrastructure;
using WB.Core.BoundedContexts.Interviewer.Views.Dashboard.Messages;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Preloading;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard.DashboardItems
{
    public class AssignmentDashboardItemViewModel : IDashboardItem
    {
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IIdentifyingAnswerConverter identifyingAnswerConverter;
        private readonly ICommandService commandService;
        private readonly IInterviewerPrincipal principal;
        private readonly IViewModelNavigationService viewModelNavigationService;
        private readonly IMvxMessenger messenger;
        private readonly IPlainStorage<InterviewView> interviewViewRepository;
        private readonly IExternalAppLauncher externalAppLauncher;

        public AssignmentDashboardItemViewModel(
            ICommandService commandService,
            IInterviewerPrincipal principal,
            IViewModelNavigationService viewModelNavigationService,
            IMvxMessenger messenger,
            IPlainStorage<InterviewView> interviewViewRepository,
            IExternalAppLauncher externalAppLauncher,
            IQuestionnaireStorage questionnaireRepository,
            IIdentifyingAnswerConverter identifyingAnswerConverter)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.identifyingAnswerConverter = identifyingAnswerConverter;
            this.commandService = commandService;
            this.principal = principal;
            this.viewModelNavigationService = viewModelNavigationService;
            this.messenger = messenger;
            this.interviewViewRepository = interviewViewRepository;
            this.externalAppLauncher = externalAppLauncher;
        }

        private QuestionnaireIdentity questionnaireIdentity;

        public void Init(AssignmentDocument assignment)
        {
            this.assignment = assignment;
            this.questionnaireIdentity = QuestionnaireIdentity.Parse(assignment.QuestionnaireId);

            this.PrefilledQuestions = GetPrefilledQuestions(assignment.IdentifyingData);
            this.GpsLocation = this.GetAssignmentLocation(assignment);
        }

        private AssignmentDocument assignment;

        public string QuestionnaireName => string.Format(InterviewerUIResources.DashboardItem_Title, this.assignment.Title, this.questionnaireIdentity.Version);
        public string Comment
        {
            get
            {
                var interviewsByAssignmentCount = this.interviewViewRepository.Count(interview => interview.Assignment == this.assignment.Id);

                if (this.assignment.Capacity.HasValue)
                {
                    var interviewsLeftByAssignmentCount = this.assignment.Capacity.Value - this.assignment.Quantity - interviewsByAssignmentCount;
                    return InterviewerUIResources.DashboardItem_AssignmentLeftComment.FormatString(interviewsLeftByAssignmentCount);
                }
                else
                {
                    return InterviewerUIResources.DashboardItem_AssignmentCreatedComment.FormatString(interviewsByAssignmentCount);
                }
            }
        }

        public List<PrefilledQuestion> PrefilledQuestions { get; private set; }
        public InterviewGpsCoordinatesView GpsLocation { get; private set; }
        public bool HasGpsLocation => this.GpsLocation != null;

        public bool AllowToCreateNewInterview
        {
            get
            {
                if (this.assignment.Capacity.HasValue)
                {
                    var interviewsByAssignmentCount = this.interviewViewRepository.Count(interview => interview.Assignment == this.assignment.Id);
                    var interviewsLeftByAssignmentCount = this.assignment.Capacity.Value - this.assignment.Quantity - interviewsByAssignmentCount;
                    return interviewsLeftByAssignmentCount > 0;
                }
                return true;
            }
        }

        public IMvxAsyncCommand CreateNewInterviewCommand => new MvxAsyncCommand(CreateNewInterviewAsync); 

        private async Task CreateNewInterviewAsync()
        {
            RaiseStartingLongOperation();
            var interviewId = Guid.NewGuid();
            var interviewerIdentity = this.principal.CurrentUserIdentity;

            var questionnaire = this.questionnaireRepository.GetQuestionnaire(QuestionnaireIdentity.Parse(this.assignment.QuestionnaireId), null);
            var prefilledQuestions = questionnaire.GetPrefilledQuestions();
            var answersToIdentifyingQuestions = GetAnswersToIdentifyingQuestions(this.assignment.IdentifyingData);
            bool hasEmptyAnswers = prefilledQuestions.Count > answersToIdentifyingQuestions.Count;

            ICommand createInterviewCommand;
            
            if (hasEmptyAnswers)
                createInterviewCommand = new CreateInterviewOnClientCommand(interviewId,
                    interviewerIdentity.UserId,
                    this.questionnaireIdentity,
                    DateTime.UtcNow,
                    interviewerIdentity.SupervisorId,
                    null,
                    int.Parse(this.assignment.Id),
                    answersToIdentifyingQuestions
                );
            else
                createInterviewCommand = new CreateInterviewCommand(interviewId,
                    interviewerIdentity.UserId,
                    this.questionnaireIdentity.QuestionnaireId,
                    answersToIdentifyingQuestions,
                    DateTime.UtcNow,
                    interviewerIdentity.SupervisorId,
                    this.questionnaireIdentity.Version,
                    null
                );

            await this.commandService.ExecuteAsync(createInterviewCommand);
            this.viewModelNavigationService.NavigateToPrefilledQuestions(interviewId.FormatGuid());
        }

        private Dictionary<Guid, AbstractAnswer> GetAnswersToIdentifyingQuestions(List<AssignmentDocument.IdentifyingAnswer> identifyingAnswers)
        {
            var questionnaireDocument = this.questionnaireRepository.GetQuestionnaireDocument(QuestionnaireIdentity.Parse(this.assignment.QuestionnaireId));
            var elements = identifyingAnswers.ToDictionary(ia => ia.QuestionId, ia => ConvertToAbstractAnswer(ia, questionnaireDocument));
            return elements;
        }

        private AbstractAnswer ConvertToAbstractAnswer(AssignmentDocument.IdentifyingAnswer identifyingAnswer, QuestionnaireDocument questionnaireDocument)
        {
            var question = questionnaireDocument.Find<IQuestion>(identifyingAnswer.QuestionId);
            return this.identifyingAnswerConverter.GetAbstractAnswer(question.QuestionType, identifyingAnswer.Answer);
        }

        private void RaiseStartingLongOperation()
        {
            messenger.Publish(new StartingLongOperationMessage(this));
        }

        private List<PrefilledQuestion> GetPrefilledQuestions(List<AssignmentDocument.IdentifyingAnswer> identifyingAnswers)
        {
            return identifyingAnswers.Select(fi => new PrefilledQuestion
                {
                    Answer = fi.Answer,
                    Question = fi.Question
                }).Take(3).ToList();
        }

        private InterviewGpsCoordinatesView GetAssignmentLocation(AssignmentDocument assignment)
        {
            if (assignment.LocationQuestionId.HasValue && assignment.LocationLatitude.HasValue && assignment.LocationLongitude.HasValue)
            {
                return new InterviewGpsCoordinatesView
                {
                    Latitude = assignment.LocationLatitude ?? 0,
                    Longitude = assignment.LocationLongitude ?? 0
                };
            }

            return null;
        }

        public IMvxCommand NavigateToGpsLocationCommand
        {
            get { return new MvxCommand(this.NavigateToGpsLocation, () => this.HasGpsLocation); }
        }

        private void NavigateToGpsLocation()
        {
            this.externalAppLauncher.LaunchMapsWithTargetLocation(this.GpsLocation.Latitude, this.GpsLocation.Longitude);
        }
    }
}