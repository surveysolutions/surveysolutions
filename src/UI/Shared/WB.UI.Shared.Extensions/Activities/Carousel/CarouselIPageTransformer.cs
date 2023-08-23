using Android.Content;
using Android.Views;
using AndroidX.ViewPager2.Widget;

namespace WB.UI.Shared.Extensions.Activities.Carousel;

public class CarouselIPageTransformer : Java.Lang.Object, ViewPager2.IPageTransformer
{
    private readonly float pageTranslationX;

    public CarouselIPageTransformer(Context context)
    {
        var nextItemVisiblePx = context.Resources.GetDimension(Resource.Dimension.carousel_next_item_visible);
        var currentItemHorizontalMarginPx = context.Resources.GetDimension(Resource.Dimension.carousel_current_item_horizontal_margin);
        pageTranslationX = nextItemVisiblePx + currentItemHorizontalMarginPx;
    }
    public void TransformPage(View page, float position)
    {
        page.TranslationY = position <= 0 ? -pageTranslationX * position : pageTranslationX * position;
        page.TranslationX = -pageTranslationX * position;
        page.ScaleY = 1 - (0.25f * Math.Abs(position));
        page.Alpha = 0.25f + (1 - Math.Abs(position));
        
        //page.Alpha = 0.50f + (1 - Math.Abs(position));
    }
}
