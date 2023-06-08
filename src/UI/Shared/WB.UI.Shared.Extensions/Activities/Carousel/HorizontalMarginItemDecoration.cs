using Android.Content;
using Android.Graphics;
using Android.Views;
using AndroidX.RecyclerView.Widget;

namespace WB.UI.Shared.Extensions.Activities.Carousel;

public class HorizontalMarginItemDecoration : RecyclerView.ItemDecoration
{
    private readonly int horizontalMarginInPx;

    public HorizontalMarginItemDecoration(Context context, int horizontalMarginInDp)
    {
        this.horizontalMarginInPx = Convert.ToInt32(context.Resources.GetDimension(horizontalMarginInDp));
    }

    public override void GetItemOffsets(Rect outRect, View view, RecyclerView parent, RecyclerView.State state)
    {
        outRect.Right = horizontalMarginInPx;
        outRect.Left = horizontalMarginInPx;

        //base.GetItemOffsets(outRect, view, parent, state);
    }
}