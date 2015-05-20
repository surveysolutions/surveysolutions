using System;
using System.Linq;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class IntegerQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<NumericIntegerQuestionAnswered>
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IUserInteraction userInteraction;
        private Identity entityIdentity;
        private string interviewId;

        public QuestionStateViewModel<NumericIntegerQuestionAnswered> QuestionState { get; private set; }

        private bool isRosterSizeQuestion;

        private int? previousAnswer;

        private int? answer;
        public int? Answer
        {
            get { return answer; }
            private set { answer = value; RaisePropertyChanged(); }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        public IntegerQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            ICommandService commandService, 
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionStateViewModel<NumericIntegerQuestionAnswered> questionStateViewModel, 
            IUserInteraction userInteraction)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.userInteraction = userInteraction;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.entityIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (IntegerNumericQuestionModel)questionnaire.Questions[entityIdentity.Id];

            this.Answer = Monads.Maybe(() => answerModel.Answer);
            this.previousAnswer = Monads.Maybe(() => answerModel.Answer);
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
        }

        private async void SendAnswerTextQuestionCommand()
        {
            if (!Answer.HasValue) return;

            if (isRosterSizeQuestion && previousAnswer.HasValue && Answer < previousAnswer)
            {
                var amountOfRostersToRemove = previousAnswer - Math.Max(Answer.Value, 0);
                var message = string.Format(UIResources.Interview_Questions_RemoveRowFromRosterMessage, amountOfRostersToRemove);
                if (!(await userInteraction.ConfirmAsync(message)))
                {
                    Answer = previousAnswer;
                    return;
                }
            }

            try
            {
                commandService.Execute(new AnswerNumericIntegerQuestionCommand(
                    interviewId: Guid.Parse(interviewId),
                    userId: principal.CurrentUserIdentity.UserId,
                    questionId: this.entityIdentity.Id,
                    rosterVector: this.entityIdentity.RosterVector,
                    answerTime: DateTime.UtcNow,
                    answer: Answer.Value
                    ));
                QuestionState.ExecutedAnswerCommandWithoutExceptions();
                
            }
            catch (InterviewException ex)
            {
                QuestionState.ProcessAnswerCommandException(ex);
            }
        }

        private IMvxCommand showCommentEditorCommand;
        public IMvxCommand ShowCommentEditorCommand
        {
            get { return showCommentEditorCommand ?? (showCommentEditorCommand = new MvxCommand(ShowCommentsCommand)); }
        }

        private void ShowCommentsCommand()
        {
            QuestionState.ShowCommentInEditor();
        }

        public void Handle(NumericIntegerQuestionAnswered @event)
        {
            if (@event.QuestionId != entityIdentity.Id || !@event.PropagationVector.SequenceEqual(entityIdentity.RosterVector))
                return;

            previousAnswer = Answer;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);
            this.Answer = Monads.Maybe(() => answerModel.Answer);
        }
    }
}