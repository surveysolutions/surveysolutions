using System;
using System.IO;
using System.Threading.Tasks;
using MvvmCross.Platform;
using MvvmCross.Core.ViewModels;
using MvvmCross.Plugins.PictureChooser;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using Identity = WB.Core.SharedKernels.DataCollection.Identity;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class MultimedaQuestionViewModel : MvxNotifyPropertyChanged, 
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private readonly Guid userId;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IPlainInterviewFileStorage plainInterviewFileStorage;
        private readonly ILiteEventRegistry eventRegistry;
        private Guid interviewId;
        private Identity questionIdentity;
        private string variableName;
        private byte[] answer;

        public MultimedaQuestionViewModel(
            IPrincipal principal,
            IStatefulInterviewRepository interviewRepository,
            IPlainInterviewFileStorage plainInterviewFileStorage,
            ILiteEventRegistry eventRegistry,
            IQuestionnaireStorage questionnaireStorage,
            QuestionStateViewModel<PictureQuestionAnswered> questionStateViewModel,
            QuestionInstructionViewModel instructionViewModel,
            AnsweringViewModel answering)
        {
            this.userId = principal.CurrentUserIdentity.UserId;
            this.interviewRepository = interviewRepository;
            this.plainInterviewFileStorage = plainInterviewFileStorage;
            this.eventRegistry = eventRegistry;
            this.questionnaireStorage = questionnaireStorage;
            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
        }

        public AnsweringViewModel Answering { get; private set; }

        public byte[] Answer
        {
            get { return this.answer; }
            set
            {
                this.answer = value;
                this.RaisePropertyChanged();
            }
        }

        public Identity Identity => this.questionIdentity;
        public IMvxAsyncCommand RequestAnswerCommand => new MvxAsyncCommand(this.RequestAnswerAsync);
        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);

        public QuestionInstructionViewModel InstructionViewModel { get; }

        private readonly QuestionStateViewModel<PictureQuestionAnswered> questionState;
        public IQuestionStateViewModel QuestionState => this.questionState;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);
            this.questionIdentity = entityIdentity;

            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;

            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.variableName = questionnaire.GetQuestionVariableName(entityIdentity.Id);
            MultimediaAnswer multimediaAnswer = interview.GetMultimediaAnswer(entityIdentity);
            if (multimediaAnswer.IsAnswered)
            {
                this.Answer = this.plainInterviewFileStorage.GetInterviewBinaryData(this.interviewId, multimediaAnswer.PictureFileName);
            }

            this.eventRegistry.Subscribe(this, interviewId);
        }

        private async Task RequestAnswerAsync()
        {
            var pictureFileName = this.GetPictureFileName();

            var pictureChooserTask = Mvx.Resolve<IMvxPictureChooserTask>();
            using (Stream pictureStream = await pictureChooserTask.TakePictureAsync(400, 95))
            {
                if (pictureStream != null)
                {
                    this.StorePictureFile(pictureStream, pictureFileName);

                    var command = new AnswerPictureQuestionCommand(
                        this.interviewId,
                        this.userId,
                        this.questionIdentity.Id,
                        this.questionIdentity.RosterVector,
                        DateTime.UtcNow,
                        pictureFileName);

                    try
                    {
                        await this.Answering.SendAnswerQuestionCommandAsync(command);
                        this.Answer = this.plainInterviewFileStorage.GetInterviewBinaryData(this.interviewId, pictureFileName);
                        this.QuestionState.Validity.ExecutedWithoutExceptions();
                    }
                    catch (InterviewException ex)
                    {
                        this.plainInterviewFileStorage.RemoveInterviewBinaryData(this.interviewId, pictureFileName);
                        this.QuestionState.Validity.ProcessException(ex);
                    }
                }
            }
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.userId,
                        this.questionIdentity,
                        DateTime.UtcNow));
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException exception)
            {
                this.QuestionState.Validity.ProcessException(exception);
            }
        }

        public void Handle(AnswersRemoved @event)
        {
            foreach (var question in @event.Questions)
            {
                if (this.questionIdentity.Equals(question.Id, question.RosterVector))
                {
                    this.Answer = null;
                    this.plainInterviewFileStorage.RemoveInterviewBinaryData(this.interviewId, this.GetPictureFileName());
                }
            }
        }

        private void StorePictureFile(Stream pictureStream, string pictureFileName)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                pictureStream.CopyTo(ms);
                byte[] pictureBytes = ms.ToArray();
                this.plainInterviewFileStorage.StoreInterviewBinaryData(this.interviewId, pictureFileName, pictureBytes);
            }
        }

        private string GetPictureFileName() => $"{this.variableName}{string.Join("-", this.questionIdentity.RosterVector)}.jpg";

        public void Dispose()
        {
            this.eventRegistry.Unsubscribe(this);
            this.QuestionState.Dispose();
        }
    }
}