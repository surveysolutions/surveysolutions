using Android.Media;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Com.Google.Android.Exoplayer2.Video;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Binding;
using Uri = Android.Net.Uri;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class ExoPlayerBinding : BaseBinding<StyledPlayerView, string>
    {
        public ExoPlayerBinding(StyledPlayerView view) : base(view)
        {
            
        }

        public override MvxBindingMode DefaultMode => MvxBindingMode.OneWay;

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                var player = Target?.Player;
                if (player != null)
                {
                    try
                    {
                        player.Stop();
                        player.Release();
                        player.Dispose();
                    }
                    catch (ObjectDisposedException) { }
                    
                    Target.Player = null;
                }
            }
            
            base.Dispose(isDisposing);
        }

        protected override void SetValueToView(StyledPlayerView view, string path)
        {
            // exit if there is no content path of file not exists
            if (path == null 
                || string.IsNullOrWhiteSpace(path) 
                || !System.IO.File.Exists(path))
            {
                return;
            }

            if (view.Player != null)
            {
                try
                {
                    view.Player.Stop();
                    view.Player.Release();
                    view.Player.Dispose();
                }
                catch (ObjectDisposedException) { }

                view.Player = null;
            }

            IExoPlayer.Builder exoPlayerBuilder = new IExoPlayer.Builder(view.Context);
            var exoPlayer = exoPlayerBuilder.Build();
            view.Player = exoPlayer;

            var dataSourceFactory = new DefaultDataSourceFactory(
                view.Context, Util.GetUserAgent(view.Context, "ExoPlayerInfo")
            );

            var uri = Uri.FromFile(new Java.IO.File(path));
            
            var mediaItem = MediaItem.FromUri(uri);
            var mediaSourceFactory = new ProgressiveMediaSource.Factory(dataSourceFactory, new DefaultExtractorsFactory());
            var mediaSource = mediaSourceFactory.CreateMediaSource(mediaItem);

            exoPlayer.SetMediaSource(mediaSource);
            exoPlayer.Prepare();

            // adjust video view height so that video take all horizontal space
            exoPlayer.SetVideoFrameMetadataListener(new VideoFrameMetadataListener(view, exoPlayer)); 

            exoPlayer.SeekTo(1);
        }
        
        private class VideoFrameMetadataListener : Java.Lang.Object, IVideoFrameMetadataListener
        {
            private StyledPlayerView playerView;
            private IExoPlayer exoPlayer;

            public VideoFrameMetadataListener(StyledPlayerView playerView, IExoPlayer exoPlayer)
            {
                this.playerView = playerView;
                this.exoPlayer = exoPlayer;
            }

            public async void OnVideoFrameAboutToBeRendered(long presentationTimeUs, long releaseTimeNs, Format format,
                MediaFormat mediaFormat)
            {
                var player = playerView;
                if (player == null)
                    return;

                var ePlayer = exoPlayer;
                if (ePlayer == null)
                    return;
                
                var mainThreadDispatcher = Mvx.IoCProvider.Resolve<IMvxMainThreadAsyncDispatcher>();

                await mainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
                {
                    var ratio = (float)format.Height / (float)format.Width / (float)format.PixelWidthHeightRatio;
                    player.SetMinimumHeight((int)(player.Width * ratio));
                    player.HideController();

                    try
                    {
                        ePlayer.ClearVideoFrameMetadataListener(this);
                    }
                    catch 
                    {
                        // ignore exception
                    }

                    playerView = null;
                    exoPlayer = null;
                });
            }
        }
    }

    public class ExoPlayerAudioAttachmentBinding : ExoPlayerBinding
    {
        public ExoPlayerAudioAttachmentBinding(StyledPlayerView view) : base(view)
        {

        }

        protected override void SetValueToView(StyledPlayerView view, string value)
        {
            base.SetValueToView(view, value);
            view.ControllerShowTimeoutMs = 0;
            view.ControllerHideOnTouch = false;
            view.ShowController();
        }
    }
}
