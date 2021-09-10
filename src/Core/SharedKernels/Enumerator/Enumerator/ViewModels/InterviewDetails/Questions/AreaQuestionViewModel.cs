using System;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class AreaQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        IViewModelEventHandler<AnswersRemoved>,
        IViewModelEventHandler<AreaQuestionAnswered>,
        ICompositeQuestion,
        IDisposable
    {
        private Area answer;
        private GeometryType? geometryType;

        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IMapInteractionService mapInteractionService;
        private readonly IViewModelEventRegistry eventRegistry;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private string interviewId;
        private readonly QuestionStateViewModel<AreaQuestionAnswered> questionState;
        private readonly IUserInteractionService userInteractionService;

        public AreaQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IMapInteractionService mapInteractionService,
            IViewModelEventRegistry eventRegistry,
            IUserInteractionService userInteractionService,
            IQuestionnaireStorage questionnaireRepository,
            QuestionStateViewModel<AreaQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.mapInteractionService = mapInteractionService;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.userInteractionService = userInteractionService;

            this.questionnaireRepository = questionnaireRepository;
        }

        public Identity Identity { get; private set; }
        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        public AnsweringViewModel Answering { get; }

        private bool isInProgress;
        public bool IsInProgress
        {
            get => this.isInProgress;
            set => this.RaiseAndSetIfChanged(ref this.isInProgress, value);
        }

        private string lengthText;
        public string LengthText
        {
            get => this.lengthText;
            set => this.RaiseAndSetIfChanged(ref this.lengthText, value);
        }

        private string areaText;
        public string AreaText
        {
            get => this.areaText;
            set => this.RaiseAndSetIfChanged(ref this.areaText, value);
        }

        private string pointsText;
        public string PointsText
        {
            get => this.pointsText;
            set => this.RaiseAndSetIfChanged(ref this.pointsText, value);
        }

        private bool hasLength;
        public bool HasLength
        {
            get => this.hasLength;
            set => this.RaiseAndSetIfChanged(ref this.hasLength, value);
        }

        private bool hasArea;
        public bool HasArea
        {
            get => this.hasArea;
            set => this.RaiseAndSetIfChanged(ref this.hasArea, value);
        }

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);
        public IMvxAsyncCommand SaveAnswerCommand => new MvxAsyncCommand(this.SaveAnswerAsync, () => !this.IsInProgress);

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.Identity = entityIdentity;
            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(interviewId);

            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            this.geometryType = questionnaire.GetQuestionGeometryType(entityIdentity.Id);

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);

            this.UpdateSelfFromModel();
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async Task SaveAnswerAsync()
        {
            this.IsInProgress = true;
            try
            {
                var answerArea = await this.mapInteractionService.EditAreaAsync(this.answer, geometryType);

                if (answerArea != null)
                {
                    var command = new AnswerGeographyQuestionCommand(
                        interviewId: Guid.Parse(this.interviewId),
                        userId: this.userId,
                        questionId: this.Identity.Id,
                        rosterVector: this.Identity.RosterVector,
                        geometry: answerArea.Geometry,
                        mapName: answerArea.MapName,
                        area: answerArea.Area,
                        coordinates:answerArea.Coordinates,
                        length: answerArea.Length,
                        distanceToEditor: answerArea.DistanceToEditor,
                        numberOfPoints: answerArea.NumberOfPoints);

                    await this.Answering.SendAnswerQuestionCommandAsync(command);
                    this.QuestionState.Validity.ExecutedWithoutExceptions();
                    this.answer = new Area(answerArea.Geometry, answerArea.MapName, answerArea.NumberOfPoints, answerArea.Area, answerArea.Length,
                        answerArea.DistanceToEditor);
                    this.UpdateLabels();
                }
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
            catch (NotImplementedException)
            {
                userInteractionService.ShowToast(UIResources.Version_Not_Supports);
            }
            catch (NotSupportedException)
            {
                userInteractionService.ShowToast(UIResources.Device_Does_Not_Support);
            }
            catch (MissingPermissionsException)
            {
                await this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.MissingPermissions_Storage);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(
                        Guid.Parse(this.interviewId),
                        this.userId,
                        this.Identity));

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        private void UpdateLabels()
        {
            this.HasArea = this.answer.AreaSize > 0;
            this.HasLength = this.answer.Length > 0;

            this.PointsText = string.Format(UIResources.AreaMap_PointsFormat, this.answer.NumberOfPoints);
            this.AreaText = string.Format(UIResources.AreaMap_AreaFormat, this.answer.AreaSize?.ToString("#.##"));
            this.LengthText = string.Format(
                this.geometryType == GeometryType.Polygon
                    ? UIResources.AreaMap_PerimeterFormat
                    : UIResources.AreaMap_LengthFormat, this.answer.Length?.ToString("#.##"));
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
            this.InstructionViewModel.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.Identity.Equals(question.Id, question.RosterVector))
                {
                    this.answer = null;
                }
            }
        }

        public void Handle(AreaQuestionAnswered @event)
        {
            if (@event.QuestionId == this.Identity.Id &&
                @event.RosterVector.Identical(this.Identity.RosterVector))
            {
                this.UpdateSelfFromModel();
            }
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(interviewId);
            var areaQuestion = interview.GetAreaQuestion(this.Identity);
            if (!areaQuestion.IsAnswered()) return;

            var questionAnswer = areaQuestion.GetAnswer().Value;
            this.answer = new Area(questionAnswer.Geometry, questionAnswer.MapName, questionAnswer.NumberOfPoints, 
                questionAnswer.AreaSize, questionAnswer.Length, questionAnswer.DistanceToEditor);
            this.UpdateLabels();
        }
    }
}
