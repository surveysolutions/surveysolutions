using System;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.UI;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.UI.Shared.Enumerator.CustomBindings.Models
{
    public class MediaAttachment : IMediaAttachment
    {
        public string ContentPath { get; set; }
        
        private WeakReference<PlayerView> view;
        public PlayerView View
        {
            get => view.TryGetTarget(out var target) ? target : null;
            set => view = new WeakReference<PlayerView>(value);
        }
        private WeakReference<SimpleExoPlayer> player;
        public SimpleExoPlayer Player
        {
            get => player.TryGetTarget(out var target) ? target : null;
            set => player = new WeakReference<SimpleExoPlayer>(value);
        }
    }
}
