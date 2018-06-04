using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Humanizer;
using Humanizer.Localisation;
using MvvmCross.Commands;
using MvvmCross.ViewModels;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.EventBus.Lite;
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

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPermissionsService permissions;
        private readonly IAudioDialog audioDialog;
        private readonly IAudioFileStorage audioFileStorage;
        private readonly IAudioService audioService;
        public AnsweringViewModel Answering { get; }

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
            this.audioDialog = audioDialog;
            this.audioFileStorage = audioFileStorage;
            this.audioService = audioService;

            this.audioService.OnPlaybackCompleted += (sender, args) =>
            {
                this.IsPlaying = false;
            };
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (value == isPlaying) return;
                isPlaying = value;
                RaisePropertyChanged(() => IsPlaying);
            }
        }

        public bool CanBePlayed
        {
            get => canBePlayed;
            private set
            {
                if (value == canBePlayed) return;
                canBePlayed = value;
                RaisePropertyChanged(() => CanBePlayed);
            }
        }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public Identity Identity => this.questionIdentity;

        private string answer;
        private bool isPlaying;
        private bool canBePlayed;

        public string Answer
        {
            get => this.answer;
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        public ICommand RecordAudioCommand => new MvxAsyncCommand(this.RecordAudioAsync);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);

        public IMvxCommand TogglePlayback => new MvxCommand(() =>
        {
            if (this.IsPlaying)
            {
                this.audioService.Stop();
                this.IsPlaying = false;
            }
            else
            {
                this.audioService.Play(this.interviewId, this.questionIdentity, this.GetAudioFileName());
                this.IsPlaying = true;
            }
        });
        
        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            this.questionIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.variableName = questionnaire.GetQuestionVariableName(entityIdentity.Id);

            var answerModel = interview.GetAudioQuestion(entityIdentity);
            if (answerModel.IsAnswered())
            {
                this.SetAnswer(answerModel.GetAnswer().Length);
            }

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        private async Task SendAnswerAsync()
        {
            var audioDuration = this.audioService.GetAudioRecordDuration();

            var command = new AnswerAudioQuestionCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                fileName: this.GetAudioFileName(),
                length: audioDuration);

            try
            {
                this.StoreAudioToPlainStorage();

                await this.Answering.SendAnswerQuestionCommandAsync(command);

                this.SetAnswer(audioDuration);

                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }

        private void SetAnswer(TimeSpan duration)
        {
            this.Answer = string.Format(UIResources.AudioQuestion_DurationFormat,
                duration.Humanize(maxUnit: TimeUnit.Minute, minUnit: TimeUnit.Second));
            
            this.CanBePlayed = this.audioFileStorage.GetInterviewBinaryData(this.interviewId, GetAudioFileName()) != null;
        }

        private async Task RecordAudioAsync()
        {
            if (this.audioService.IsRecording()) return;
            this.audioService.Stop();
            this.IsPlaying = false;

            try
            {
                await this.permissions.AssureHasPermission(Permission.Microphone);
                await this.permissions.AssureHasPermission(Permission.Storage);

                this.audioDialog.OnRecorded += this.AudioDialog_OnRecorded;
                this.audioDialog.OnCanelRecording += AudioDialog_OnCancel;
                this.audioDialog.ShowAndStartRecording(this.QuestionState.Header.Title.HtmlText);
            }
            catch (MissingPermissionsException e) when (e.Permission == Permission.Microphone)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .MissingPermissions_Microphone);
            }
            catch (MissingPermissionsException e) when (e.Permission == Permission.Storage)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources
                    .MissingPermissions_Storage);
            }
            catch (MissingPermissionsException e)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(e.Message);
            }
            catch (AudioException e) when (e.Type == AudioExceptionType.Io)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Audio_Io_Exception_Message);
            }
            catch (AudioException e) when (e.Type == AudioExceptionType.Unhandled)
            {
                this.QuestionState.Validity.MarkAnswerAsNotSavedWithMessage(UIResources.Audio_Unhandled_Exception_Message);
            }
        }

        private void AudioDialog_OnCancel(object sender, EventArgs e) => this.UnhandleDialog();

        private async void AudioDialog_OnRecorded(object sender, EventArgs e)
        {
            this.UnhandleDialog();

            if (this.QuestionState.Enablement.Enabled)
                await this.SendAnswerAsync();
        }

        private void UnhandleDialog()
        {
            this.audioDialog.OnRecorded -= this.AudioDialog_OnRecorded;
            this.audioDialog.OnCanelRecording -= this.AudioDialog_OnCancel;
        }

        private async Task RemoveAnswerAsync()
        {
            try
            {
                if(this.IsPlaying) this.TogglePlayback.Execute();

                await this.Answering.SendRemoveAnswerCommandAsync(
                    new RemoveAnswerCommand(this.interviewId,
                        this.principal.CurrentUserIdentity.UserId,
                        this.questionIdentity));
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

        private string GetAudioFileName() => $"{this.variableName}__{this.questionIdentity.RosterVector}.{this.audioService.GetAudioType()}";

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.liteEventRegistry.Unsubscribe(this);
        }
    }
}
