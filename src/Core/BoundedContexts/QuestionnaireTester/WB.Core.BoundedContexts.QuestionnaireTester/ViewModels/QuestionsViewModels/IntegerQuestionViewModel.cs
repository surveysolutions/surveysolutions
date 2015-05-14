using System;
using System.Linq;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.CrossCore;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class IntegerQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventBusEventHandler<NumericIntegerQuestionAnswered>,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;
        private Identity entityIdentity;
        private string interviewId;

        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }
        public CommentsViewModel Comments { get; private set; }

        public IntegerQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            ICommandService commandService, 
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            ValidityViewModel validity,
            EnablementViewModel enablement,
            CommentsViewModel comments)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            this.Validity = validity;
            this.Enablement = enablement;
            this.Comments = comments;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.entityIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.Header.Init(interviewId, entityIdentity);
            this.Validity.Init(interviewId, entityIdentity);
            this.Comments.Init(interviewId, entityIdentity);
            this.Enablement.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);

            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);
            var questionModel = (IntegerNumericQuestionModel)questionnaire.Questions[entityIdentity.Id];

            this.Answer = Monads.Maybe(() => answerModel.Answer);
            this.previousAnswer = Monads.Maybe(() => answerModel.Answer);
            this.isRosterSizeQuestion = questionModel.IsRosterSizeQuestion;
        }

        private bool isRosterSizeQuestion;

        private int? previousAnswer;

        private int? answer;
        public int? Answer
        {
            get { return answer; }
            private set { answer = value; RaisePropertyChanged(); }
        }

        private bool isAnswered;
        public bool IsAnswered
        {
            get { return isAnswered; }
            set { isAnswered = value; RaisePropertyChanged(); }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private async void SendAnswerTextQuestionCommand()
        {
            if (!Answer.HasValue) return;

            if (isRosterSizeQuestion && previousAnswer.HasValue && Answer < previousAnswer)
            {
                var amountOfRostersToRemove = previousAnswer - Math.Max(Answer.Value, 0);
                var message = string.Format(UIResources.Interview_Questions_AreYouSureYouWantToRemoveRowFromRoster, amountOfRostersToRemove);
                if (!(await Mvx.Resolve<IUserInteraction>().ConfirmAsync(message)))
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
                Validity.ExecutedWithoutExceptions();
                
            }
            catch (Exception ex)
            {
                Validity.ProcessException(ex);
            }
        }

        public void Handle(NumericIntegerQuestionAnswered @event)
        {
            if (@event.QuestionId != entityIdentity.Id || !@event.PropagationVector.SequenceEqual(entityIdentity.RosterVector))
                return;

            previousAnswer = Answer;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetIntegerNumericAnswer(entityIdentity);
            this.Answer = Monads.Maybe(() => answerModel.Answer);
            this.IsAnswered = interview.WasAnswered(entityIdentity);
        }

        public void Dispose()
        {
            liteEventRegistry.Unsubscribe(this);
        }
    }
}