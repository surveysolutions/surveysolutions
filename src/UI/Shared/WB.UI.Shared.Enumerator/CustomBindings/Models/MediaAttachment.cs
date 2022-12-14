using System;
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
            try
            {
                player?.Release();
            }
            catch (ObjectDisposedException)
            {
            }
            
            player = null;
        }
        
        public SimpleExoPlayer Player
        {
            get => player;
            set
            {
                try
                {
                    player?.Release();
                }
                catch (ObjectDisposedException)
                {
                }
                
                //player?.Dispose();
                player = value;
            }
        }
    }
}
