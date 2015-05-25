using System;
using System.Globalization;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class DateTimeQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        private readonly ICommandService commandService;
        private readonly IPrincipal principal;
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        private Identity questionIdentity;
        private string interviewId;

        public QuestionStateViewModel<DateTimeQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }


        public DateTimeQuestionViewModel(ICommandService commandService,
            IPrincipal principal,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository,
            QuestionStateViewModel<DateTimeQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.commandService = commandService;
            this.principal = principal;
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            this.questionIdentity = entityIdentity;
            this.interviewId = interviewId;

            var interview = this.interviewRepository.Get(interviewId);
            var answerModel = interview.GetDateTimeAnswer(entityIdentity);
            if (answerModel != null && answerModel.IsAnswered)
            {
                SetToView(answerModel.Answer);
            }
        }

        public IMvxCommand AnswerCommand
        {
            get { return new MvxCommand<DateTime>(SendAnswerCommand); }
        }

        private async void SendAnswerCommand(DateTime answerValue)
        {
            try
            {
                var command = new AnswerDateTimeQuestionCommand(
                    interviewId: Guid.Parse(interviewId),
                    userId: principal.CurrentUserIdentity.UserId,
                    questionId: this.questionIdentity.Id,
                    rosterVector: this.questionIdentity.RosterVector,
                    answerTime: DateTime.UtcNow,
                    answer: answerValue
                    );
                await this.Answering.SendAnswerQuestionCommand(command);
                SetToView(answerValue);
                QuestionState.ExecutedAnswerCommandWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                QuestionState.ProcessAnswerCommandException(ex);
            }
        }

        private void SetToView(DateTime answerValue)
        {
            Answer = answerValue.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(); }
        }
    }
}