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
        ILiteEventBusEventHandler<TextQuestionAnswered>

    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainRepository<QuestionnaireModel> questionnaireRepository;
        private readonly IPlainRepository<InterviewModel> interviewRepository;
        private Identity entityIdentity;
        private string interviewId;

        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel ValidityViewModel { get; private set; }
        public EnablementViewModel EnablementViewModel { get; private set; }

        public TextQuestionViewModel(
            ICommandService commandService, 
            IPrincipal principal, 
            IPlainRepository<QuestionnaireModel> questionnaireRepository,
            IPlainRepository<InterviewModel> interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            ValidityViewModel validityViewModel,
            EnablementViewModel enablementViewModel)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            ValidityViewModel = validityViewModel;
            EnablementViewModel = enablementViewModel;
        }


        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            this.entityIdentity = entityIdentity;
            this.interviewId = interviewId;

            this.Header.Init(interviewId, entityIdentity);
            this.ValidityViewModel.Init(interviewId, entityIdentity);
            this.EnablementViewModel.Init(interviewId, entityIdentity);

            var interview = this.interviewRepository.Get(interviewId);

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
            private set { title = value; RaisePropertyChanged(() => Title); }
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            private set { answer = value; RaisePropertyChanged(() => Answer); }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private void SendAnswerTextQuestionCommand()
        {
            commandService.Execute(new AnswerTextQuestionCommand(
                interviewId: Guid.Parse(interviewId),
                userId: principal.CurrentUserIdentity.UserId,
                questionId: this.entityIdentity.Id,
                rosterVector: this.entityIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                answer: Answer
                ));
        }


        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(interviewId);

            var answerModel = interview.GetTextAnswerModel(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = answerModel.Answer;
            }
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId != entityIdentity.Id && @event.PropagationVector != entityIdentity.RosterVector)
                return;

            UpdateSelfFromModel();
        }
    }
}