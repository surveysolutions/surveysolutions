using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Android.Net;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Java.IO;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.UI.Shared.Enumerator.CustomBindings.Models;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ExoPlayerBinding : BaseBinding<PlayerView, IMediaAttachment>
    {
        public ExoPlayerBinding(PlayerView view) : base(view)
        {
            
        }

        protected override void Dispose(bool isDisposing)
        {
            if (IsDisposed)
                return;

            base.Dispose(isDisposing);
            Target?.Player?.Release();
        }

        static readonly DefaultExtractorsFactory ExtractorsFactory = new DefaultExtractorsFactory();
        
        protected override void SetValueToView(PlayerView view, IMediaAttachment value)
        {
            var media = value as MediaAttachment;

            // exit if there is no content path of file not exists
            if (media == null 
                || string.IsNullOrWhiteSpace(value.ContentPath) 
                || !System.IO.File.Exists(value.ContentPath))
            {
                return;
            }

            // we don't want to rebind same player on same view
            if (media.View == view && media.Player != null && media.Player == view.Player) return;

            view.Player?.Stop();
            view.Player?.Release();
            
            var exoPlayer = ExoPlayerFactory.NewSimpleInstance(view.Context, new DefaultTrackSelector());
            
            var dataSourceFactory = new DefaultDataSourceFactory(
                view.Context, Util.GetUserAgent(view.Context, "ExoPlayerInfo")
            );

            var uri = Uri.FromFile(new File(value.ContentPath));

            exoPlayer.Prepare(new ExtractorMediaSource.Factory(dataSourceFactory)
                .SetExtractorsFactory(ExtractorsFactory)
                .CreateMediaSource(uri));

            // adjust video view height so that video take all horizontal space
            exoPlayer.RenderedFirstFrame += (sender, args) =>
            {
                var ratio = (float)exoPlayer.VideoFormat.Height / (float)exoPlayer.VideoFormat.Width / (float)exoPlayer.VideoFormat.PixelWidthHeightRatio;
                view.SetMinimumHeight((int)(view.Width * ratio));
                view.HideController();
            };

            exoPlayer.SeekTo(1);
            view.Player = exoPlayer;
            media.Player = exoPlayer;
            media.View = view;
        }
    }

    public class ExoPlayerAudioAttachmentBinding : ExoPlayerBinding
    {
        public ExoPlayerAudioAttachmentBinding(PlayerView view) : base(view)
        {

        }

        protected override void SetValueToView(PlayerView view, IMediaAttachment value)
        {
            base.SetValueToView(view, value);
            view.ControllerShowTimeoutMs = 0;
            view.ControllerHideOnTouch = false;
            view.ShowController();
        }
    }
}
