using Android.Net;
using Com.Google.Android.Exoplayer2;
using Com.Google.Android.Exoplayer2.Extractor;
using Com.Google.Android.Exoplayer2.Source;
using Com.Google.Android.Exoplayer2.Trackselection;
using Com.Google.Android.Exoplayer2.UI;
using Com.Google.Android.Exoplayer2.Upstream;
using Com.Google.Android.Exoplayer2.Util;
using Java.IO;

namespace WB.UI.Shared.Enumerator.CustomBindings
{
    public class VideoViewPathBinding : BaseBinding<PlayerView, string>
    {
        public VideoViewPathBinding(PlayerView view) : base(view)
        {
          
        }

        protected override void SetValueToView(PlayerView view, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;

            var exoPlayer = ExoPlayerFactory.NewSimpleInstance(view.Context, new DefaultTrackSelector());

            var dataSourceFactory = new DefaultDataSourceFactory(
                view.Context, Util.GetUserAgent(view.Context, "ExoPlayerInfo")
            );

            var uri = Uri.FromFile(new File(value));

            exoPlayer.Prepare(new ExtractorMediaSource.Factory(dataSourceFactory)
                .SetExtractorsFactory(new DefaultExtractorsFactory())
                .CreateMediaSource(uri));
            
            exoPlayer.RenderedFirstFrame += (sender, args) =>
            {
                var ratio = (float) exoPlayer.VideoFormat.Height / (float)exoPlayer.VideoFormat.Width / (float)exoPlayer.VideoFormat.PixelWidthHeightRatio;
                view.SetMinimumHeight((int)(view.Width * ratio));
                view.HideController();
            };

            exoPlayer.SeekTo(1);
            view.Player = exoPlayer;
        }
    }
}
