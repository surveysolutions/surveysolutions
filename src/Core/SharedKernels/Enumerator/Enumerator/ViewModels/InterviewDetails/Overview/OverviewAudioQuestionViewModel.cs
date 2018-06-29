using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MvvmCross.Commands;
using MvvmCross.WeakSubscription;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Overview
{
    public class OverviewAudioQuestionViewModel : OverviewQuestionViewModel, INotifyPropertyChanged
    {
        private readonly IAudioService audioService;
        private readonly string fileName;
        private bool isPlaying;
        private readonly Guid interviewId;
        private readonly Identity questionIdentity;

        public OverviewAudioQuestionViewModel(InterviewTreeQuestion treeQuestion,
            IAudioFileStorage audioFileStorage,
            IAudioService audioService,
            IUserInteractionService interactionService, 
            IStatefulInterview statefulInterview) : base(treeQuestion, statefulInterview, interactionService)
        {
            this.audioService = audioService;
            this.interviewId = Guid.Parse(treeQuestion.Tree.InterviewId);
            this.questionIdentity = treeQuestion.Identity;

            if (treeQuestion.IsAnswered())
            {
                var audioQuestion = treeQuestion.GetAsInterviewTreeAudioQuestion();
                fileName = audioQuestion.GetAnswer().FileName;

                this.Audio = audioFileStorage.GetInterviewBinaryData(interviewId,
                    fileName);
                this.audioService.WeakSubscribe<IAudioService, PlaybackCompletedEventArgs>(nameof(audioService.OnPlaybackCompleted), PlaybackCompleted);
            }
        }

        private void PlaybackCompleted(object sender, PlaybackCompletedEventArgs e)
        {
            this.IsPlaying = false;
        }

        public byte[] Audio { get; set; }

        public bool CanBePlayed => this.Audio != null;

        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                if (value == isPlaying) return;
                isPlaying = value;
                OnPropertyChanged();
            }
        }

        public IMvxCommand TogglePlayback => new MvxCommand(() =>
        {
            if (this.IsPlaying)
            {
                this.audioService.Stop();
                this.IsPlaying = false;
            }
            else
            {
                this.audioService.Play(this.interviewId, this.questionIdentity, fileName);
                this.IsPlaying = true;
            }
        }, () => CanBePlayed);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
