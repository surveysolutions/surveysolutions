using System;
using System.Linq;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel,
        ILiteEventBusEventHandler<TextQuestionAnswered>,
        ILiteEventBusEventHandler<QuestionsEnabled>,
        ILiteEventBusEventHandler<QuestionsDisabled>
    {
        public QuestionHeaderViewModel Header { get; set; }

        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;
        private Identity questionIdentity;
        private Guid interviewId;

        public TextQuestionViewModel(
            ICommandService commandService, 
            IPrincipal principal, 
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
        }


        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Header.Init(interviewId, entityIdentity);

            var answerModel = interview.GetTextAnswerModel(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = answerModel.Answer;
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

        private IMvxCommand valueChangedCommand;
        public IMvxCommand ValueChangedCommand
        {
            get { return valueChangedCommand ?? (valueChangedCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private void SendAnswerTextQuestionCommand()
        {
            commandService.Execute(new AnswerTextQuestionCommand(
                interviewId: interviewId,
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: Answer
                ));
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId != questionIdentity.Id && @event.PropagationVector != questionIdentity.RosterVector)
                return;

            Answer = @event.Answer;
        }

        public void Handle(QuestionsEnabled @event)
        {
            if (@event.Questions.Any(i => i.Id != questionIdentity.Id && i.RosterVector != questionIdentity.RosterVector))
                return;

            Enabled = true;
        }

        public void Handle(QuestionsDisabled @event)
        {
            if (@event.Questions.Any(i => i.Id != questionIdentity.Id && i.RosterVector != questionIdentity.RosterVector))
                return;

            Enabled = false;
        }
    }
}