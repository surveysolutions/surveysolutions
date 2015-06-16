using System;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<TextQuestionAnswered>
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<TextQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        public TextQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionStateViewModel<TextQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
            this.Answering = answering;
            this.QuestionState = questionStateViewModel;
        }


        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            liteEventRegistry.Subscribe(this);

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            InitQuestionSettings();
            UpdateSelfFromModel();
        }

        private string mask;
        public string Mask
        {
            get { return mask; }
            private set { mask = value; RaisePropertyChanged(); }
        }

        public bool IsMaskedQuestionAnswered { get; set; }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set
            {
                if (answer != value)
                {
                    answer = value;
                    RaisePropertyChanged();
                }
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand<string>(SendAnswerTextQuestionCommand)); }
        }

        private async void SendAnswerTextQuestionCommand(string text)
        {
            Answer = text;

            if (!Mask.IsNullOrEmpty() && !this.IsMaskedQuestionAnswered)
            {
                this.QuestionState.Validity.MarkAnswerAsInvalidWithMessage(UIResources.Interview_Question_Text_MaskError);
                return;
            }

            var command = new AnswerTextQuestionCommand(
                interviewId: Guid.Parse(interviewId),
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: text);

            try
            {
                await this.Answering.SendAnswerQuestionCommand(command);
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void InitQuestionSettings()
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var textQuestionModel = questionnaire.GetTextQuestion(questionIdentity.Id);
            this.Mask = textQuestionModel.Mask;
        }

        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(interviewId);

            var answerModel = interview.GetTextAnswer(questionIdentity);
            if (answerModel != null)
            {
                this.Answer = answerModel.Answer;
            }
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId == questionIdentity.Id &&
                @event.PropagationVector.SequenceEqual(questionIdentity.RosterVector))
            {
                UpdateSelfFromModel();
            }
        }
    }
}