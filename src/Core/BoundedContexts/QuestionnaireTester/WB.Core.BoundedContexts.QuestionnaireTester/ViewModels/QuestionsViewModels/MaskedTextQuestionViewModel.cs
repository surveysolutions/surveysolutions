using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MaskedTextQuestionViewModel : BaseInterviewItemViewModel,
        ILiteEventBusEventHandler<TextQuestionAnswered>,
        ILiteEventBusEventHandler<QuestionsEnabled>,
        ILiteEventBusEventHandler<QuestionsDisabled>
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private Identity identity;
        private InterviewModel interviewModel;
        private QuestionnaireModel questionnaireModel;

        public MaskedTextQuestionViewModel(ICommandService commandService, IPrincipal principal)
        {
            this.commandService = commandService;
            this.principal = principal;
        }

        public override void Init(Identity identity, InterviewModel interviewModel, QuestionnaireModel questionnaireModel)
        {
            if (identity == null) throw new ArgumentNullException("identity");
            if (interviewModel == null) throw new ArgumentNullException("interviewModel");
            if (questionnaireModel == null) throw new ArgumentNullException("questionnaireModel");

            this.identity = identity;
            this.interviewModel = interviewModel;
            this.questionnaireModel = questionnaireModel;

            var questionModel = (MaskedTextQuestionModel)this.questionnaireModel.Questions[this.identity.Id];
            var answerModel = this.interviewModel.GetTextAnswerModel(this.identity);

            Title = questionModel.Title;

            if (answerModel != null)
            {
                Answer = answerModel.Answer;
            }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { title = value; RaisePropertyChanged(() => Title); }
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(() => Answer); }
        }

        private bool enabled;
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; RaisePropertyChanged(() => Enabled); }
        }

        private bool isTextChanged = false;
        private IMvxCommand afterTextChangedCommand;
        public IMvxCommand AfterTextChangedCommand
        {
            get { return afterTextChangedCommand ?? (afterTextChangedCommand = new MvxCommand(MarkTextChangedAndTrySendAnswerTextQuestionCommand)); }
        }

        private bool isFocusChange = false;
        private IMvxCommand focusChangeCommand;
        public IMvxCommand FocusChangeCommand
        {
            get { return focusChangeCommand ?? (focusChangeCommand = new MvxCommand(MarkFocusChangedAndTrySendAnswerTextQuestionCommand)); }
        }

/*        private IMvxCommand answerTextQuestionCommand;
        public IMvxCommand AnswerTextQuestionCommand
        {
            get { return answerTextQuestionCommand ?? (answerTextQuestionCommand = new MvxCommand(SendAnswerTextQuestionCommandAfterEndEditing)); }
        }*/

        private void MarkTextChangedAndTrySendAnswerTextQuestionCommand()
        {
            isTextChanged = true;
            if (isFocusChange)
                TrySendAnswerTextQuestionCommand();
        }

        private void MarkFocusChangedAndTrySendAnswerTextQuestionCommand()
        {
            isFocusChange = true;
            if (isTextChanged)
                TrySendAnswerTextQuestionCommand();
        }

        private void TrySendAnswerTextQuestionCommand()
        {
            if (!isFocusChange || !isTextChanged)
                return;

            isFocusChange = false;
            isTextChanged = false;

            commandService.Execute(new AnswerTextQuestionCommand(
                interviewId: interviewModel.Id,
                userId: principal.CurrentUserIdentity.UserId,
                questionId: identity.Id,
                rosterVector: identity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: Answer
                ));
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId != identity.Id && @event.PropagationVector != identity.RosterVector)
                return;

            Answer = @event.Answer;
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Any(i => i.Id != identity.Id && i.RosterVector != identity.RosterVector))
                return;

            Enabled = true;
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Any(i => i.Id != identity.Id && i.RosterVector != identity.RosterVector))
                return;

            Enabled = false;
        }
    }
}