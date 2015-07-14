using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Infrastructure;
using WB.Core.BoundedContexts.Tester.Properties;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.Tester.ViewModels.Questions.State;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.Tester.ViewModels.Questions
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
            private set { mask = value; RaisePropertyChanged(); RaisePropertyChanged(() => Hint); }
        }

        public string Hint
        {
            get
            {
                if (this.Mask.IsNullOrEmpty()) 
                    return UIResources.TextQuestion_Hint;

                string maskHint = this.Mask.Replace('*', '_').Replace('#', '_').Replace('~', '_');
                return UIResources.TextQuestion_MaskHint.FormatString(maskHint);
            }
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
            if (answerModel.IsAnswered)
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