using System;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class AreaQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel, 
        ILiteEventHandler<AnswersRemoved>,
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

        private ICommand saveAnswerCommand;
        public ICommand SaveAnswerCommand
        {
            get { return this.saveAnswerCommand ?? (this.saveAnswerCommand = new MvxCommand(this.SaveAnswer, () => !this.IsInProgress)); }
        }

        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IAreaEditService areaEditService;
        private readonly ILiteEventRegistry eventRegistry;

        private Identity questionIdentity;
        private Guid interviewId;
        private readonly QuestionStateViewModel<AreaQuestionAnswered> questionState;

        public AreaQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IAreaEditService areaEditService,
            ILiteEventRegistry eventRegistry,
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
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            var areaQuestion = interview.GetAreaQuestion(entityIdentity);
            if (areaQuestion.IsAnswered)
            {
                var answer = areaQuestion.GetAnswer().Value;
                this.Answer = new Area(answer.Geometry, answer.MapName, answer.AreaSize );
            }

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async void SaveAnswer()
        {
            this.IsInProgress = true;
            try
            {
                var answerArea = await this.areaEditService.EditAreaAsync(this.Answer);

                if (answerArea != null)
                {
                    var command = new AnswerAreaQuestionCommand(
                        interviewId: this.interviewId,
                        userId: this.userId,
                        questionId: this.questionIdentity.Id,
                        rosterVector: this.questionIdentity.RosterVector,
                        answerTime: DateTime.UtcNow,
                        geometry: answerArea.Geometry,
                        mapName: answerArea.MapName,
                        area : answerArea.Area);

                    await this.Answering.SendAnswerQuestionCommandAsync(command);
                    this.QuestionState.Validity.ExecutedWithoutExceptions();
                    this.Answer = new Area(answerArea.Geometry, answerArea.MapName, answerArea.Area);
                }
            }
            catch (MissingPermissionsException)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.MissingPermissions_Storage);
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
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
                            new RemoveAnswerCommand(this.interviewId,
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
    }
}