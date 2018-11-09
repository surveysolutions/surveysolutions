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

            var traskSelector = new DefaultTrackSelector();

            var exoPlayer = ExoPlayerFactory.NewSimpleInstance(view.Context, traskSelector);
            
            string playerInfo = Util.GetUserAgent(view.Context, "ExoPlayerInfo");

            var dataSourceFactory = new DefaultDataSourceFactory(
                view.Context, playerInfo
            );

            var file = new File(value);
            var uri = Uri.FromFile(file);
            var mediaSource = new ExtractorMediaSource.Factory(dataSourceFactory)
                .SetExtractorsFactory(new DefaultExtractorsFactory())
                .CreateMediaSource(uri);

            exoPlayer.Prepare(mediaSource);
            //_player.PlayWhenReady = true;

            exoPlayer.SeekTo(1);

            view.Player = exoPlayer;
        }
    }
}
