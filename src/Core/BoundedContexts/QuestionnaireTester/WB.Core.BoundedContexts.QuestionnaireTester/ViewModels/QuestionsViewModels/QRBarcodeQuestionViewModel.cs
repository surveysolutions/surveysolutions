using System;
using System.Threading.Tasks;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Properties;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionStateViewModels;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels
{
    public class QrBarcodeQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        public QuestionStateViewModel<QRBarcodeQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return isInProgress; }
            set { isInProgress = value; RaisePropertyChanged(); }
        }

        private string answer;
        public string Answer
        {
            get { return answer; }
            set { answer = value; RaisePropertyChanged(); }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return saveAnswerCommand ?? (saveAnswerCommand = new MvxCommand(SaveAnswer, () => !this.IsInProgress)); }
        }

        private readonly ICommandService commandService;
        private readonly IUserIdentity userIdentity;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly IQrBarcodeScanService qrBarcodeScanService;
        private readonly IUserInteraction userInteraction;

        private Identity questionIdentity;
        private Guid interviewId;

        public QrBarcodeQuestionViewModel(ICommandService commandService, 
            IUserIdentity userIdentity,
            IStatefullInterviewRepository interviewRepository,
            IQrBarcodeScanService qrBarcodeScanService,
            IUserInteraction userInteraction,
            QuestionStateViewModel<QRBarcodeQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.commandService = commandService;
            this.userIdentity = userIdentity;
            this.interviewRepository = interviewRepository;
            this.qrBarcodeScanService = qrBarcodeScanService;
            this.userInteraction = userInteraction;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var answerModel = interview.GetQRBarcodeAnswer(entityIdentity);
            if (answerModel != null)
            {
                this.Answer = answerModel.Answer;
            }
        }

        private async void SaveAnswer()
        {
            this.IsInProgress = true;

            var hasException = false;
            try
            {
                var scanCode = await this.qrBarcodeScanService.ScanAsync();

                var command = new AnswerQRBarcodeQuestionCommand(
                    interviewId: this.interviewId,
                    userId: this.userIdentity.UserId,
                    questionId: this.questionIdentity.Id,
                    rosterVector: this.questionIdentity.RosterVector,
                    answerTime: DateTime.UtcNow,
                    answer: scanCode.Code);

                this.Answer = scanCode.Code;

                await this.Answering.SendAnswerQuestionCommand(command);
                QuestionState.ExecutedAnswerCommandWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                hasException = true;
                QuestionState.ProcessAnswerCommandException(ex);
            }

            if (hasException)
            {
                await TryGetQbBarcodeAgainAsync();
            }
        }

        private async Task TryGetQbBarcodeAgainAsync()
        {
            if (await this.userInteraction.ConfirmAsync(
                message: UIResources.Interview_GeoLocation_Confirm_NoLocation,
                title: UIResources.ConfirmationText,
                okButton: UIResources.ConfirmationTryAgainText,
                cancelButton: UIResources.ConfirmationCancelText))
            {
                this.SaveAnswer();
            }
        }
    }
}