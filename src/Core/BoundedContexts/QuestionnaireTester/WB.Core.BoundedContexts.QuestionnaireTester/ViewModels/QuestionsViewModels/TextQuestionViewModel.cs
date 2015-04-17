using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxViewModel,
        IEventBusEventHandler<TextQuestionAnswered>,
        IEventBusEventHandler<QuestionsEnabled>,
        IEventBusEventHandler<QuestionsDisabled>
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private NavObject navObject;

        public class NavObject
        {
            public Identity QuestionIdentity { get; set; }
            public InterviewModel InterviewModel { get; set; }
            public QuestionnaireDocument QuestionnaireDocument { get; set; }
        }

        public TextQuestionViewModel(ICommandService commandService, IPrincipal principal)
        {
            this.commandService = commandService;
            this.principal = principal;
        }

        public void Init(NavObject navObject)
        {
            if (navObject == null) 
                throw new ArgumentNullException("navObject");

            this.navObject = navObject;
            var parent = navObject.QuestionnaireDocument.GetParentById(navObject.QuestionIdentity.Id);
            TextQuestion textQuestion = parent.Find<TextQuestion>(navObject.QuestionIdentity.Id);
            var model = navObject.InterviewModel.GetTextQuestionModel(navObject.QuestionIdentity);

            Title = textQuestion.QuestionText;

            if (model != null)
            {
                Answer = model.Answer;
            }
        }


        public void Init(Identity questionIdentity, InterviewModel interviewModel, QuestionnaireDocument questionnaireDocument)
        {
            Init(new NavObject
            {
                QuestionIdentity = questionIdentity,
                InterviewModel = interviewModel,
                QuestionnaireDocument = questionnaireDocument
            });
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
                interviewId: navObject.InterviewModel.Id,
                userId: principal.CurrentUserIdentity.UserId,
                questionId: navObject.QuestionIdentity.Id,
                rosterVector: navObject.QuestionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: Answer
                ));
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId != navObject.QuestionIdentity.Id
                && @event.PropagationVector != navObject.QuestionIdentity.RosterVector)
                return;

            Answer = @event.Answer;
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Any(i => i.Id != navObject.QuestionIdentity.Id
                && i.RosterVector != navObject.QuestionIdentity.RosterVector))
                return;

            Enabled = true;
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Any(i => i.Id != navObject.QuestionIdentity.Id
                && i.RosterVector != navObject.QuestionIdentity.RosterVector))
                return;

            Enabled = false;
        }
    }
}