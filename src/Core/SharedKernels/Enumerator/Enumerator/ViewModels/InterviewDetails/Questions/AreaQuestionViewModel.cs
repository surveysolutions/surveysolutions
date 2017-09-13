using System;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
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

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class AreaQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        ILiteEventHandler<AnswersRemoved>,
        ILiteEventHandler<AreaQuestionAnswered>,
        ICompositeQuestion,
        IDisposable
    {
        public IQuestionStateViewModel QuestionState
        {
            get { return this.questionState; }
        }

        public QuestionInstructionViewModel InstructionViewModel { get; set; }
        public AnsweringViewModel Answering { get; private set; }
        
        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; this.RaisePropertyChanged(); }
        }

        private Area answer;
        public Area Answer
        {
            get { return this.answer; }
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        private IMvxAsyncCommand saveAnswerCommand;
        public IMvxAsyncCommand SaveAnswerCommand
        {
            get { return this.saveAnswerCommand ?? (this.saveAnswerCommand = new MvxAsyncCommand(this.SaveAnswer, () => !this.IsInProgress)); }
        }

        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAreaEditService areaEditService;
        private readonly ILiteEventRegistry eventRegistry;

        private Identity questionIdentity;
        private string interviewId;
        private readonly QuestionStateViewModel<AreaQuestionAnswered> questionState;
        private IUserInteractionService userInteractionService;

        public AreaQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IAreaEditService areaEditService,
            ILiteEventRegistry eventRegistry,
            IUserInteractionService userInteractionService,
            QuestionStateViewModel<AreaQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.areaEditService = areaEditService;
            this.eventRegistry = eventRegistry;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.userInteractionService = userInteractionService;
        }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            this.UpdateSelfFromModel();
            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async Task SaveAnswer()
        {
            this.IsInProgress = true;
            try
            {
                var answerArea = await this.areaEditService.EditAreaAsync(this.Answer);

                if (answerArea != null)
                {
                    var command = new AnswerAreaQuestionCommand(
                        interviewId: Guid.Parse(this.interviewId),
                        userId: this.userId,
                        questionId: this.questionIdentity.Id,
                        rosterVector: this.questionIdentity.RosterVector,
                        answerTime: DateTime.UtcNow,
                        geometry: answerArea.Geometry,
                        mapName: answerArea.MapName,
                        area: answerArea.Area,
                        coordinates:answerArea.Coordinates,
                        length: answerArea.Length,
                        distanceToEditor: answerArea.DistanceToEditor);

                    await this.Answering.SendAnswerQuestionCommandAsync(command);
                    this.QuestionState.Validity.ExecutedWithoutExceptions();
                    this.Answer = new Area(answerArea.Geometry, answerArea.MapName, answerArea.Area, answerArea.Length,
                        answerArea.DistanceToEditor);
                }
            }
      
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
            catch (Exception e) when (e.InnerException is MissingPermissionsException)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.MissingPermissions_Storage);
            }
            catch (Exception e) when (e.InnerException is NotImplementedException)
            {
                userInteractionService.ShowToast(UIResources.Version_Not_Supports);
            }
            catch (Exception e) when (e.InnerException is NotSupportedException)
            {
                userInteractionService.ShowToast(UIResources.Device_Does_Not_Support);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }

        public ICommand RemoveAnswerCommand
        {
            get
            {
                return new MvxCommand(async () =>
                {
                    try
                    {
                        await this.Answering.SendRemoveAnswerCommandAsync(
                            new RemoveAnswerCommand(
                                Guid.Parse(this.interviewId),
                                this.userId, 
                                this.questionIdentity,
                                DateTime.UtcNow));

                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                    }
                    catch (InterviewException exception)
                    {
                        this.QuestionState.Validity.ProcessException(exception);
                    }
                });
            }
        }

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = null;
                }
            }
        }

        public void Handle(AreaQuestionAnswered @event)
        {
            if (@event.QuestionId == this.questionIdentity.Id &&
                @event.RosterVector.Identical(this.questionIdentity.RosterVector))
            {
                this.UpdateSelfFromModel();
            }
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(interviewId);
            var areaQuestion = interview.GetAreaQuestion(this.questionIdentity);
            if (areaQuestion.IsAnswered())
            {
                var questionAnswer = areaQuestion.GetAnswer().Value;
                this.Answer = new Area(questionAnswer.Geometry, questionAnswer.MapName, questionAnswer.AreaSize, questionAnswer.Length, questionAnswer.DistanceToEditor);
            }
        }
    }
}