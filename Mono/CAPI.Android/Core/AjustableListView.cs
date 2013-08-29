using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Core
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
            if (this.Count != old_count)
            {
                old_count = Count;
                layoutParams = this.LayoutParameters;
                layoutParams.Height = Count * (old_count > 0 ? this.GetChildAt(0).Height : 0);
                this.LayoutParameters = layoutParams;
            }
            base.OnDraw(canvas);
        }
    }
}