using System;
using System.Linq;

using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class TextQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventBusEventHandler<TextQuestionAnswered>,
        IDisposable
    {
        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;
        private Identity entityIdentity;
        private string interviewId;

        public QuestionHeaderViewModel Header { get; private set; }
        public ValidityViewModel Validity { get; private set; }
        public EnablementViewModel Enablement { get; private set; }

        public TextQuestionViewModel(
            ILiteEventRegistry liteEventRegistry,
            ICommandService commandService, 
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            QuestionHeaderViewModel questionHeaderViewModel,
            ValidityViewModel validity,
            EnablementViewModel enablement)
        {
            this.liteEventRegistry = liteEventRegistry;
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.Header = questionHeaderViewModel;
            Validity = validity;
            Enablement = enablement;
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
            this.Enablement.Init(interviewId, entityIdentity);

            UpdateSelfFromModel();
        }

        private string mask;
        public string Mask
        {
            get { return mask; }
            private set { mask = value; RaisePropertyChanged(); }
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            private set { answer = value; RaisePropertyChanged(); }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return valueChangeCommand ?? (valueChangeCommand = new MvxCommand(SendAnswerTextQuestionCommand)); }
        }

        private void SendAnswerTextQuestionCommand()
        {
            try
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
            catch (Exception)
            {
                Validity.MarkAsError();
            }
        }


        private void UpdateSelfFromModel()
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var textQuestionModel = questionnaire.Questions[entityIdentity.Id] as MaskedTextQuestionModel;
            if (textQuestionModel != null)
            {
                this.Mask = textQuestionModel.Mask;
            }

            var answerModel = interview.GetTextAnswerModel(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = answerModel.Answer;
            }
        }

        public void Handle(TextQuestionAnswered @event)
        {
            if (@event.QuestionId != entityIdentity.Id || @event.PropagationVector.SequenceEqual(entityIdentity.RosterVector))
                return;

            UpdateSelfFromModel();
        }

        public void Dispose()
        {
            liteEventRegistry.Unsubscribe(this);
        }
    }
}