using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Humanizer;
using Humanizer.Localisation;
using MvvmCross.Core.ViewModels;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.Questionnaire.Documents.Question;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class AudioQuestionViewModel :
        MvxNotifyPropertyChanged,
        IInterviewEntityViewModel,
        ILiteEventHandler<AnswersRemoved>,
        ICompositeQuestion,
        IDisposable
    {
        private readonly IPrincipal principal;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly QuestionStateViewModel<AudioQuestionAnswered> questionState;

        private Identity questionIdentity;
        private Guid interviewId;
        private string variableName;
        private int kbPerSec;

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPermissionsService permissions;
        private readonly IAudioDialog audioDialog;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioService audioService;
        private readonly IFileSystemAccessor fileSystemAccessor;
        public AnsweringViewModel Answering { get; private set; }

        public AudioQuestionViewModel(
            IPrincipal principal, 
            IStatefulInterviewRepository interviewRepository,
            IQuestionnaireStorage questionnaireStorage,
            QuestionStateViewModel<AudioQuestionAnswered> questionStateViewModel, 
            AnsweringViewModel answering,
            QuestionInstructionViewModel instructionViewModel, 
            ILiteEventRegistry liteEventRegistry,
            IPermissionsService permissions,
            IAudioDialog audioDialog,
            IAudioFileStorage audioFileStorage,
            IAudioService audioService)
        {
            this.principal = principal;
            this.interviewRepository = interviewRepository;
            this.questionnaireStorage = questionnaireStorage;

            this.questionState = questionStateViewModel;
            this.InstructionViewModel = instructionViewModel;
            this.Answering = answering;
            this.liteEventRegistry = liteEventRegistry;
            this.permissions = permissions;
            this.permissions = permissions;
            this.audioDialog = audioDialog;
            this.audioFileStorage = audioFileStorage;
            this.audioService = audioService;
        }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public Identity Identity => this.questionIdentity;

        private string answer;
        public string Answer
        {
            get => this.answer;
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        public ICommand RecordAudioCommand => new MvxAsyncCommand(this.RecordAudioAsync);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);


        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            this.questionIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.variableName = questionnaire.GetQuestionVariableName(entityIdentity.Id);
            this.kbPerSec = AudioQuality.DefaultBitRate;

            var answerModel = interview.GetAudioQuestion(entityIdentity);
            if (answerModel.IsAnswered)
                this.SetAnswer(answerModel.GetAnswer().Length);

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        private async Task SendAnswerAsync()
        {
            var audioDuration = this.audioService.GetDuration();

            var command = new AnswerAudioQuestionCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                fileName: this.GetAudioFileName(),
                length: audioDuration);

            try
            {

                await this.Answering.SendAnswerQuestionCommandAsync(command);

                this.StoreAudioToPlainStorage();
                this.SetAnswer(audioDuration);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void SetAnswer(TimeSpan duration)
            => this.Answer = string.Format(UIResources.AudioQuestion_DurationFormat,
                duration.Humanize(maxUnit: TimeUnit.Minute, minUnit: TimeUnit.Second));

        private async Task RecordAudioAsync()
        {
            try
            {
                await this.permissions.AssureHasPermission(Permission.Microphone).ConfigureAwait(false);
                await this.permissions.AssureHasPermission(Permission.Storage).ConfigureAwait(false);

                this.audioDialog.OnRecorded += AudioDialog_OnRecorded;
                this.audioDialog.ShowAndStartRecording(this.QuestionState.Header.Title.HtmlText, this.kbPerSec);
            }
            catch (MissingPermissionsException e)
            {
                switch (e.Permission)
                {
                    case Permission.Microphone:
                        this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                            .MissingPermissions_Microphone);
                        break;
                    case Permission.Storage:
                        this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                            .MissingPermissions_Storage);
                        break;
                    default:
                        this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(e.Message);
                        break;
                }
            }
        }

        private async void AudioDialog_OnRecorded(object sender, EventArgs e)
        {
            this.audioDialog.OnRecorded -= AudioDialog_OnRecorded;
            await this.SendAnswerAsync();
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
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
            if (!@event.Questions.Any(question => this.questionIdentity.Equals(question.Id, question.RosterVector)))
                return;

            this.Answer = null;
            this.audioFileStorage.RemoveInterviewBinaryData(this.interviewId, this.GetAudioFileName());
        }

        private void StoreAudioToPlainStorage()
        {
            var audioStream = this.audioService.GetLastRecord();

            using (var audioMemoryStream = new MemoryStream())
            {
                audioStream.CopyTo(audioMemoryStream);
                this.audioFileStorage.StoreInterviewBinaryData(this.interviewId, this.GetAudioFileName(),
                    audioMemoryStream.ToArray(), this.audioService.GetMimeType());
            }
        }

        private string GetAudioFileName() => $"{this.variableName}{string.Join("-", this.questionIdentity.RosterVector)}.{this.audioService.GetAudioType()}";

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.liteEventRegistry.Unsubscribe(this);
        }
    }
}