using Android.Views;
using AndroidX.RecyclerView.Widget;
using Com.Google.Android.Exoplayer2.UI;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.UI.Shared.Enumerator.Activities;
using Object = Java.Lang.Object;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class InterviewEntityAdapter : MvxRecyclerAdapter
    {
        public InterviewEntityAdapter(IMvxAndroidBindingContext bindingContext)
            : base(bindingContext)
        {
        }
        
        //temporary solution of System.MissingMethodException: No constructor found
        //on destroy activity 
        public InterviewEntityAdapter(System.IntPtr pointer, Android.Runtime.JniHandleOwnership ownership)
            :base(pointer, ownership) { }

        public override void OnViewRecycled(Object holder)
        {
            if (holder is RecyclerView.ViewHolder { ItemView: ViewGroup viewGroup })
            {
                TryReleasePlayer(viewGroup, Resource.Id.audio_player_view);
                TryReleasePlayer(viewGroup, Resource.Id.video_player_view);
            }
            
            base.OnViewRecycled(holder);
        }

        private void TryReleasePlayer(ViewGroup viewGroup, int playerId)
        {
            var playerView = viewGroup.FindViewById<StyledPlayerView>(playerId);
            var player = playerView?.Player;
            if (player != null)
            {
                player.Stop();
                player.Release();
                player.Dispose();
                playerView.Player = null;
            }
        }

        public override void OnViewDetachedFromWindow(Java.Lang.Object holder)
        {
            // we do this, because same bindings use focus as trigger, 
            // but in new version of MvvmCross focus event is raised after clear data in control
            bool isFocusedChildren = IsThereChildrenWithFocus(holder);
            if (isFocusedChildren) 
            {
                var topActivity = Plugin.CurrentActivity.CrossCurrentActivity.Current.Activity;
                topActivity.RemoveFocusFromEditText();
            }

            base.OnViewDetachedFromWindow(holder);
        }

        private bool IsThereChildrenWithFocus(Java.Lang.Object holder)
        {
            if (holder is RecyclerView.ViewHolder viewHolder)
                return IsThereChildrenWithFocus(viewHolder.ItemView);

            var view = holder as View;

            if (view == null)
                return false;

            if (view.IsFocused)
                return true;

            if (view is ViewGroup viewGroup)
                return IsThereChildrenWithFocus(viewGroup.FocusedChild);

            return false;
        }
    }
}
