using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using MvvmCross.Core.ViewModels;
using Plugin.Permissions.Abstractions;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
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
        private int bitRate;

        private readonly ILiteEventRegistry liteEventRegistry;
        private readonly IPermissionsService permissions;
        private readonly IAudioDialog audioDialog;
        private readonly IAudioFileStorage audioFileStorage;
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
            IAudioFileStorage audioFileStorage)
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
        }

        public IQuestionStateViewModel QuestionState => this.questionState;

        public QuestionInstructionViewModel InstructionViewModel { get; }

        public Identity Identity => this.questionIdentity;

        public void Init(string interviewId, Identity entityIdentity, NavigationState navigationState)
        {
            this.questionState.Init(interviewId, entityIdentity, navigationState);
            this.InstructionViewModel.Init(interviewId, entityIdentity);

            this.questionIdentity = entityIdentity;

            var interview = this.interviewRepository.Get(interviewId);
            this.interviewId = interview.Id;
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);
            this.variableName = questionnaire.GetQuestionVariableName(entityIdentity.Id);
            this.bitRate = (int)questionnaire.GetAudioQuality(entityIdentity.Id);
            
            //var answerModel = interview.GetAudioQuestion(entityIdentity);
            //if (answerModel.IsAnswered)
            //{
            //    this.SetToView(answerModel.GetAnswer().Value);
            //}

            this.liteEventRegistry.Subscribe(this, interviewId);
        }

        public ICommand RecordAudioCommand => new MvxAsyncCommand(this.RecordAudioAsync);

        public IMvxAsyncCommand RemoveAnswerCommand => new MvxAsyncCommand(this.RemoveAnswerAsync);

        private async Task SendAnswerAsync()
        {
            var command = new AnswerAudioQuestionCommand(
                interviewId: this.interviewId,
                userId: this.principal.CurrentUserIdentity.UserId,
                questionId: this.questionIdentity.Id,
                rosterVector: this.questionIdentity.RosterVector,
                answerTime: DateTime.UtcNow,
                fileName: this.GetAudioFileName(),
                length: TimeSpan.Zero);

            try
            {

                await this.Answering.SendAnswerQuestionCommandAsync(command);
                
                this.QuestionState.Validity.ExecutedWithoutExceptions();
            }
            catch (InterviewException ex)
            {
                this.QuestionState.Validity.ProcessException(ex);
            }
        }
        private async Task RecordAudioAsync()
        {
            try
            {
                await this.permissions.AssureHasPermission(Permission.Microphone).ConfigureAwait(false);
                await this.permissions.AssureHasPermission(Permission.Storage).ConfigureAwait(false);

                this.audioDialog.OnRecorded += AudioDialog_OnRecorded;
                this.audioDialog.ShowAndStartRecording(this.QuestionState.Header.Title.HtmlText, this.bitRate);
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

        private async void AudioDialog_OnRecorded(object sender, Stream audioStream)
        {
            this.audioDialog.OnRecorded -= AudioDialog_OnRecorded;

            using (var audioMemoryStream = new MemoryStream())
            {
                audioStream.CopyTo(audioMemoryStream);
                this.audioFileStorage.StoreInterviewBinaryData(this.interviewId, this.GetAudioFileName(), audioMemoryStream.ToArray());
            }

            await this.SendAnswerAsync();
        }

        private string answer;
        public string Answer
        {
            get => this.answer;
            set { this.answer = value; this.RaisePropertyChanged(); }
        }

        public void Dispose()
        {
            this.QuestionState.Dispose();
            this.liteEventRegistry.Unsubscribe(this); 
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

        private string GetAudioFileName() => $"{this.variableName}{string.Join("-", this.questionIdentity.RosterVector)}.3gpp";
    }
}