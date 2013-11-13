using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace WB.UI.Capi.Core
{
    public class AjustableListView : ListView
    {
        private ViewGroup.LayoutParams layoutParams;
        private int old_count = 0;

        public AjustableListView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        protected override void OnDraw(Canvas canvas)
        {
            if (this.Count != this.old_count)
            {
                this.old_count = this.Count;
                this.layoutParams = this.LayoutParameters;
                this.layoutParams.Height = this.Count * (this.old_count > 0 ? this.GetChildAt(0).Height : 0);
                this.LayoutParameters = this.layoutParams;
            }
            base.OnDraw(canvas);
        }
    }
}