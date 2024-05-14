using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Lang;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;

namespace WB.UI.Shared.Enumerator.CustomControls;

public class AutosizeExpandableListView : MvxExpandableListView
{
    public AutosizeExpandableListView(Context context, IAttributeSet attrs) 
        : base(context, attrs, new CompleteInformationAdapter(context))
    {
        
    }

    public AutosizeExpandableListView(Context context, IAttributeSet attrs, MvxExpandableListAdapter adapter) : base(context, attrs, adapter)
    {
        
    }

    protected AutosizeExpandableListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
        
    }
    
    
    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        int expandSpec = MeasureSpec.MakeMeasureSpec(Integer.MaxValue, MeasureSpecMode.AtMost);
        base.OnMeasure(widthMeasureSpec, expandSpec);

        LayoutParameters.Height = MeasuredHeight;
    }
}
