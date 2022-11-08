#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
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
using InvalidOperationException = System.InvalidOperationException;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class AreaQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        IViewModelEventHandler<AnswersRemoved>,
        IViewModelEventHandler<AreaQuestionAnswered>,
        ICompositeQuestion,
        IDisposable
    {
        private Area? answer;
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
            AnsweringViewModel answering,
            IEnumeratorSettings settings)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.mapInteractionService = mapInteractionService;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.interviewId = string.Empty;
            Identity = new  Identity(Guid.Empty, RosterVector.Empty);
            this.lengthText = string.Empty;
            this.areaText = string.Empty;
            this.pointsText = string.Empty;
            this.userInteractionService = userInteractionService;

            this.questionnaireRepository = questionnaireRepository;

            this.Settings = settings;
        }

        public IEnumeratorSettings Settings { get; set; }

        public Guid Id { get; } = Guid.NewGuid();
        
        public Identity Identity { get; private set; }
        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        public AnsweringViewModel Answering { get; }

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
        public IMvxAsyncCommand SaveAnswerCommand => new MvxAsyncCommand(this.SaveAnswerAsync, () => !this.Answering.InProgress);

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.Identity = entityIdentity;
            this.interviewId = interviewId ?? throw new ArgumentNullException("interviewId");

            var interview = interviewRepository.Get(interviewId);
            if (interview == null)
                throw new InvalidOperationException($"Interview {interviewId} was not found");

            var questionnaire =
                this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            if (questionnaire == null)
                throw new InvalidOperationException($"Questionnaire {interview.QuestionnaireIdentity} for language {interview.Language} was not found");
            
            this.geometryType = questionnaire.GetQuestionGeometryType(entityIdentity.Id);
            this.requestedGeometryMode = questionnaire.GetQuestionGeometryMode(entityIdentity.Id);
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity, navigationState);
            this.eventRegistry.Subscribe(this, interviewId);

            UpdateSelfFromModel();
        }

        private void UpdateSelfFromModel()
        {
            var interview = interviewRepository.Get(interviewId);
            if (interview == null)
                throw new InvalidOperationException($"Interview {interviewId} was not found");
            
            var areaQuestion = interview.GetAreaQuestion(this.Identity);
            Area? answerValue = null;
            if (areaQuestion.IsAnswered())
            {
                var questionAnswer = areaQuestion.GetAnswer().Value;
                answerValue = new Area(questionAnswer.Geometry, questionAnswer.MapName, questionAnswer.NumberOfPoints, 
                    questionAnswer.AreaSize, questionAnswer.Length, questionAnswer.DistanceToEditor, 
                    questionAnswer.RequestedAccuracy, questionAnswer.RequestedFrequency);
            }

            SetAnswerAndUpdateLabels(answerValue);
        }

        private async Task SaveAnswerAsync()
        {
            this.Answering.StartInProgressIndicator();
            try
            {
                var interview = interviewRepository.GetOrThrow(interviewId);
                var questionnaire = questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);
                var question = interview.GetQuestion(this.Identity);

                var neighboringIds = questionnaire.IsNeighboringSupport(Identity.Id)
                    ? interview.GetNeighboringQuestionIdentities(Identity)
                    : Enumerable.Empty<Identity>();
                var neighbors = neighboringIds
                    .Select(qId =>
                    {
                        var nQuestion = interview.GetQuestion(qId);
                        var parentRosterInstance = nQuestion.Parent;
                        var areaQuestion = nQuestion.GetAsInterviewTreeAreaQuestion();
                
                        return new GeographyNeighbor
                        {
                            Id = qId.ToString(),
                            Title = parentRosterInstance.Title.Text,
                            Geometry = areaQuestion?.GetAnswer()?.Value?.Geometry
                        };
                    })
                    .Where(neighbor => neighbor.Geometry != null)
                    .ToArray();
                
                var answerArea = await this.mapInteractionService.EditAreaAsync(
                        new EditAreaArgs(
                            area: this.answer,
                            geometryType: geometryType,
                            requestedGeometryInputMode: requestedGeometryMode,
                            requestedAccuracy: requestedGeometryMode == GeometryInputMode.Manual ? null: Settings.GeographyQuestionAccuracyInMeters,
                            requestedFrequency: (requestedGeometryMode is GeometryInputMode.Manual or GeometryInputMode.Semiautomatic) ? null: Settings.GeographyQuestionPeriodInSeconds,
                            geographyNeighbors: neighbors,
                            title: question.Parent.Title.Text
                            ))
                    .ConfigureAwait(false);

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
                        numberOfPoints: answerArea.NumberOfPoints,
                        requestedAccuracy: answerArea.RequestedAccuracy,
                        requestedFrequency: answerArea.RequestedFrequency);

                    await this.Answering.SendQuestionCommandAsync(command);
                    await this.QuestionState.Validity.ExecutedWithoutExceptions();

                    var answerValue = new Area(answerArea.Geometry, answerArea.MapName, answerArea.NumberOfPoints, 
                        answerArea.Area, answerArea.Length, answerArea.DistanceToEditor, 
                        answerArea.RequestedAccuracy, answerArea.RequestedAccuracy);
                    SetAnswerAndUpdateLabels(answerValue);
                }
            }
            catch (InterviewException ex)
            {
                await this.QuestionState.Validity.ProcessException(ex);
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
                this.Answering.FinishInProgressIndicator();
            }
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendQuestionCommandAsync(
                    new RemoveAnswerCommand(
                        Guid.Parse(this.interviewId),
                        this.userId,
                        this.Identity));

                await this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                await this.QuestionState.Validity.ProcessException(exception);
            }
        }

        private void SetAnswerAndUpdateLabels(Area? answerValue)
        {
            this.answer = answerValue;

            this.HasArea = this.answer?.AreaSize > 0;
            this.HasLength = this.answer?.Length > 0;

            this.PointsText = this.answer == null 
                ? string.Empty 
                : string.Format(UIResources.AreaMap_PointsFormat, this.answer.NumberOfPoints);
            this.AreaText = this.answer == null 
                ? string.Empty 
                : string.Format(UIResources.AreaMap_AreaFormat, this.answer.AreaSize.HasValue 
                    ? this.answer.AreaSize.FormatDouble(2)
                    : string.Empty);
            this.LengthText = this.answer == null ? string.Empty : string.Format(
                this.geometryType == GeometryType.Polygon 
                    ? UIResources.AreaMap_PerimeterFormat 
                    : UIResources.AreaMap_LengthFormat, 
                this.answer.Length.HasValue
                    ? this.answer.Length.FormatDouble(2)
                    : string.Empty);
        }

        private bool isDisposed = false;
        private GeometryInputMode? requestedGeometryMode;

        public void Dispose()
        {
            if (isDisposed)
                return;
            
            isDisposed = true;
            
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
                    SetAnswerAndUpdateLabels(null);
                }
            }
        }
        
        //if view model is recreated on return from Geography activity
        //new instance will react on event
        public void Handle(AreaQuestionAnswered @event)
        {
            if (@event.QuestionId == this.Identity.Id &&
                @event.RosterVector.Identical(this.Identity.RosterVector))
            {
                this.UpdateSelfFromModel();
            }
        }
    }
}
