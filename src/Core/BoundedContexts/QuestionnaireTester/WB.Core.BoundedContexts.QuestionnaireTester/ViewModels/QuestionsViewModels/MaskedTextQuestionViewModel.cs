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
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class MaskedTextQuestionViewModel : MvxNotifyPropertyChanged, IInterviewItemViewModel,
        ILiteEventBusEventHandler<TextQuestionAnswered>,
        ILiteEventBusEventHandler<QuestionsEnabled>,
        ILiteEventBusEventHandler<QuestionsDisabled>
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;
        private Identity questionIdentity;
        private Guid interviewId;

        public MaskedTextQuestionViewModel(
            ICommandService commandService, 
            IPrincipal principal, 
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }


        public void Init(string interviewId, Identity questionIdentity)
        {
            if (questionIdentity == null) throw new ArgumentNullException("questionIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.Get(interview.QuestionnaireId);

            var questionModel = (MaskedTextQuestionModel)questionnaire.Questions[questionIdentity.Id];
            var answerModel = interview.GetTextAnswerModel(questionIdentity);

            this.questionIdentity = questionIdentity;
            this.interviewId = interview.Id;
            this.Title = questionModel.Title;

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