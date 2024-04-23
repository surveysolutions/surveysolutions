using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Java.Lang;
using MvvmCross.Platforms.Android.Binding.Views;

namespace WB.UI.Shared.Enumerator.CustomControls;

public class AutosizeExpandableListView : MvxExpandableListView
{
    public AutosizeExpandableListView(Context context, IAttributeSet attrs) : base(context, attrs)
    {
        Initialize();
    }

    public AutosizeExpandableListView(Context context, IAttributeSet attrs, MvxExpandableListAdapter adapter) : base(context, attrs, adapter)
    {
        Initialize();
    }

    protected AutosizeExpandableListView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
    {
        Initialize();
    }

    public override bool CollapseGroup(int groupPos)
    {
        ManageIndicatorVisibility();
        return base.CollapseGroup(groupPos);
    }

    public override bool ExpandGroup(int groupPos)
    {
        ManageIndicatorVisibility();
        return base.ExpandGroup(groupPos);
    }
    
    private void ManageIndicatorVisibility()
    {
        var adapter = ExpandableListAdapter;
        int groupCount = adapter.GroupCount;
        for (int i = 0; i < groupCount; i++)
        {
            if (adapter.GetChildrenCount(i) == 0)
            {
                // Hide the indicator if no children
                SetGroupIndicator(null);
            }
            else
            {
                // Show the indicator
                SetGroupIndicator(Resources.GetDrawable(Resource.Drawable.expand_list_indicator));
            }
        }
    }

    public override void SetIndicatorBounds(int left, int right)
    {
        
        base.SetIndicatorBounds(left, right);
    }

    /*protected void OnCreate(Bundle savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        SetContentView(Resource.Layout.YourLayout);

        var expandableListView = FindViewById<ExpandableListView>(Resource.Id.expandableListView);
        int leftPadding = (int) (25 * Resources.DisplayMetrics.Density);  // 25dp to pixel
        int rightPadding = leftPadding + (int) (40 * Resources.DisplayMetrics.Density); // Width of the indicator

        // Adjust this logic based on your layout direction
        expandableListView.SetIndicatorBoundsRelative(leftPadding, rightPadding);
    }*/

    public override void SetIndicatorBoundsRelative(int start, int end)
    {
        base.SetIndicatorBoundsRelative(start, end);
    }

    /*protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        int expandSpec = MeasureSpec.MakeMeasureSpec(Integer.MaxValue >> 2, MeasureSpecMode.AtMost);
        base.OnMeasure(widthMeasureSpec, expandSpec);
    }*/
    
    protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        // HACK! TAKE THAT ANDROID!
        //if (IsExpanded)
        {
            // Calculate entire height by providing a very large height hint.
            // But do not use the highest 2 bits of this integer; those are
            // reserved for the MeasureSpec mode.
            int expandSpec = MeasureSpec.MakeMeasureSpec(Integer.MaxValue, MeasureSpecMode.AtMost);
            base.OnMeasure(widthMeasureSpec, expandSpec);

            //var parameters = LayoutParameters;
            LayoutParameters.Height = MeasuredHeight;
        }
        /*else
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }*/
    }
    
    private void Initialize()
    {
        //SetIndicatorBounds(10, 10);
        //SetIndicatorBoundsRelative(1, 1);

        this.SetOnGroupExpandListener(new GroupExpandListener(this));
        this.SetOnGroupCollapseListener(new GroupCollapseListener(this));
    }

    private class GroupExpandListener : Java.Lang.Object, IOnGroupExpandListener
    {
        private readonly AutosizeExpandableListView listView;

        public GroupExpandListener(AutosizeExpandableListView listView)
        {
            this.listView = listView;
        }

        public void OnGroupExpand(int groupPosition)
        {
            listView.SetListViewHeightBasedOnChildren();
        }
    }
    
    private class GroupCollapseListener : Java.Lang.Object, IOnGroupCollapseListener
    {
        private readonly AutosizeExpandableListView listView;

        public GroupCollapseListener(AutosizeExpandableListView listView)
        {
            this.listView = listView;
        }

        public void OnGroupCollapse(int groupPosition)
        {
            listView.SetListViewHeightBasedOnChildren();
        }
    }

    public void SetListViewHeightBasedOnChildren()
    {
        var listAdapter = this.ExpandableListAdapter;
        int totalHeight = 0;
        int desiredWidth = MeasureSpec.MakeMeasureSpec(this.Width, MeasureSpecMode.Exactly);
        for (int i = 0; i < listAdapter.GroupCount; i++)
        {
            View groupItem = listAdapter.GetGroupView(i, this.IsGroupExpanded(i), null, this);
            groupItem.Measure(desiredWidth, MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
            totalHeight += groupItem.MeasuredHeight;

            if (this.IsGroupExpanded(i))
            {
                for (int j = 0; j < listAdapter.GetChildrenCount(i); j++)
                {
                    View listItem = listAdapter.GetChildView(i, j, false, null, this);
                    //listItem.Measure(desiredWidth, MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));
                    //listItem.Layout(0, 0, listItem.MeasuredWidth, listItem.MeasuredHeight);
                    int childHeightSpec = MeasureSpec.MakeMeasureSpec(Integer.MaxValue >> 2, MeasureSpecMode.AtMost);
                    //listItem.Measure(desiredWidth, childHeightSpec);
                    totalHeight += listItem.MeasuredHeight;
                }
            }
        }
        ViewGroup.LayoutParams param = this.LayoutParameters;
        param.Height = totalHeight + (this.DividerHeight * (listAdapter.GroupCount - 1));
        this.LayoutParameters = param;
        this.RequestLayout();
    }

    public override void SetGroupIndicator(Drawable groupIndicator)
    {
        base.SetGroupIndicator(groupIndicator);
    }

    /*protected override void OnGroupCollapse(int groupPosition)
    {
       base.OnGroupCollapse(groupPosition);
       ManageIndicatorVisibility();
    }

    protected override void OnGroupExpand(int groupPosition)
    {
       base.OnGroupExpand(groupPosition);
       ManageIndicatorVisibility();
    }

    private void ManageIndicatorVisibility()
    {
       int groupCount = Adapter.GroupCount;
       for (int i = 0; i < groupCount; i++)
       {
           if (Adapter.GetChildrenCount(i) == 0)
           {
               // Hide the indicator if no children
               SetGroupIndicator(null);
           }
           else
           {
               // Show the indicator
               SetGroupIndicator(Resources.GetDrawable(Resource.Drawable.your_indicator));
           }
        }
    }*/
}
