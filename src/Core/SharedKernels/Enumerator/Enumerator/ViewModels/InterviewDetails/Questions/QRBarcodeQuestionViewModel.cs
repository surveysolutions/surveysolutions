using System;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class QRBarcodeQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
    {
        public QuestionStateViewModel<QRBarcodeQuestionAnswered> QuestionState { get; private set; }
        public AnsweringViewModel Answering { get; private set; }
        

        private bool isInProgress;
        public bool IsInProgress
        {
            get { return this.isInProgress; }
            set { this.isInProgress = value; this.RaisePropertyChanged(); }
        }

        private string answer;
        public string Answer
        {
            get { return this.answer; }
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        private IMvxCommand saveAnswerCommand;
        public IMvxCommand SaveAnswerCommand
        {
            get { return this.saveAnswerCommand ?? (this.saveAnswerCommand = new MvxCommand(this.SaveAnswer, () => !this.IsInProgress)); }
        }

        private readonly IUserIdentity userIdentity;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQRBarcodeScanService qrBarcodeScanService;
        private readonly IUserInteractionService userInteractionService;

        private Identity questionIdentity;
        private Guid interviewId;

        public QRBarcodeQuestionViewModel(
            IUserIdentity userIdentity,
            IStatefulInterviewRepository interviewRepository,
            IQRBarcodeScanService qrBarcodeScanService,
            IUserInteractionService userInteractionService,
            QuestionStateViewModel<QRBarcodeQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
            this.userIdentity = userIdentity;
            this.interviewRepository = interviewRepository;
            this.qrBarcodeScanService = qrBarcodeScanService;
            this.userInteractionService = userInteractionService;

            this.QuestionState = questionStateViewModel;
            this.Answering = answering;
        }

        public Identity Identity { get { return this.questionIdentity; } }

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            if(interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");

            var interview = this.interviewRepository.Get(interviewId);
            
            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.QuestionState.Init(interviewId, entityIdentity, navigationState);

            var answerModel = interview.GetQRBarcodeAnswer(entityIdentity);
            if (answerModel.IsAnswered)
            {
                this.Answer = answerModel.Answer;
            }
        }

        private async void SaveAnswer()
        {
            this.IsInProgress = true;

            try
            {
                var scanCode = await this.qrBarcodeScanService.ScanAsync();

                if (scanCode != null)
                {
                    var command = new AnswerQRBarcodeQuestionCommand(
                        interviewId: this.interviewId,
                        userId: this.userIdentity.UserId,
                        questionId: this.questionIdentity.Id,
                        rosterVector: this.questionIdentity.RosterVector,
                        answerTime: DateTime.UtcNow,
                        answer: scanCode.Code);

                    await this.Answering.SendAnswerQuestionCommandAsync(command);
                    this.QuestionState.Validity.ExecutedWithoutExceptions();
                    this.Answer = scanCode.Code;
                }
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
            finally
            {
                this.IsInProgress = false;
            }
        }
    }
}