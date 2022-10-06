using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.UI;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.UI.Shared.Enumerator.CustomBindings.Models
{
    public class MediaAttachment : IMediaAttachment
    {
        private SimpleExoPlayer player;
        public PlayerView View { get; set; }
        public string ContentPath { get; set; }

        public void Release()
        {
            //Player?.Stop();
            Player?.Release();
        }
        
        public SimpleExoPlayer Player
        {
            get => player;
            set
            {
                player?.Release();
                //player?.Dispose();
                player = value;
            }
        }
    }
}
