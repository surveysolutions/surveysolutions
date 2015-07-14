using System;
using Chance.MvvmCross.Plugins.UserInteraction;
using Cirrious.MvvmCross.ViewModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.InterviewEntities;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions.State;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.Questions
{
    public class QRBarcodeQuestionViewModel : MvxNotifyPropertyChanged, IInterviewEntityViewModel
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

        private readonly IUserIdentity userIdentity;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQrBarcodeScanService qrBarcodeScanService;
        private readonly IUserInteraction userInteraction;

        private Identity questionIdentity;
        private Guid interviewId;

        public QRBarcodeQuestionViewModel(
            IUserIdentity userIdentity,
            IStatefulInterviewRepository interviewRepository,
            IQrBarcodeScanService qrBarcodeScanService,
            IUserInteraction userInteraction,
            QuestionStateViewModel<QRBarcodeQuestionAnswered> questionStateViewModel,
            AnsweringViewModel answering)
        {
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

                    await this.Answering.SendAnswerQuestionCommand(command);
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