using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using AndroidX.ConstraintLayout.Helper.Widget;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Com.Google.Android.Exoplayer2.Video;
using Java.IO;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.WeakSubscription;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.UI.Shared.Enumerator.CustomBindings.Models;
using Uri = Android.Net.Uri;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ExoPlayerBinding : BaseBinding<PlayerView, IMediaAttachment>
    {
        public ExoPlayerBinding(PlayerView view) : base(view)
        {
            
        }

        private IDisposable metadataEventSubscription;

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.metadataEventSubscription?.Dispose();
                this.metadataEventSubscription = null;
                
                if (Target?.Player != null)
                {
                    Target.Player.Release();
                    Target.Player.Dispose();
                    Target.Player = null;
                }
            }
            
            base.Dispose(isDisposing);
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

            if (view.Player != null)
            {
                view.Player.Stop();
                view.Player.Release();
                view.Player.Dispose();
                view.Player = null;
            }

            var dataSourceFactory = new DefaultDataSourceFactory(
                view.Context, Util.GetUserAgent(view.Context, "ExoPlayerInfo")
            );

            var uri = Uri.FromFile(new File(value.ContentPath));

            var mediaSourceFactory = new ProgressiveMediaSource.Factory(dataSourceFactory, ExtractorsFactory);
            var mediaSource = mediaSourceFactory.CreateMediaSource(MediaItem.FromUri(uri));

            SimpleExoPlayer.Builder exoPlayer = new SimpleExoPlayer.Builder(view.Context);
            var simpleExoPlayer = exoPlayer.Build();
            simpleExoPlayer.SetMediaSource(mediaSource);
            simpleExoPlayer.Prepare();

            // adjust video view height so that video take all horizontal space
            metadataEventSubscription = simpleExoPlayer.WeakSubscribe<SimpleExoPlayer, VideoFrameMetadataEventArgs>(
                nameof(simpleExoPlayer.VideoFrameMetadata),
                this.HandleVideoFrameMetadata);

            simpleExoPlayer.SeekTo(1);
            view.Player = simpleExoPlayer;
            media.Player = simpleExoPlayer;
            media.View = view;
        }

        private async void HandleVideoFrameMetadata(object sender, VideoFrameMetadataEventArgs args)
        {
            var mainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();

            await mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                var view = Target;
                var ratio = (float)args.P2.Height / (float)args.P2.Width / (float)args.P2.PixelWidthHeightRatio;
                view.SetMinimumHeight((int)(view.Width * ratio));
                view.HideController();
                
                this.metadataEventSubscription?.Dispose();
                this.metadataEventSubscription = null;
            });
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
