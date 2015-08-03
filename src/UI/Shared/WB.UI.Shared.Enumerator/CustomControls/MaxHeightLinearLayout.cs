using System;

using Android.Content;
using Android.Util;
using Android.Views;
using Android.Widget;
using WB.UI.Shared.Enumerator;

namespace WB.UI.Tester.CustomControls
{
    public sealed class MaxHeightLinearLayout: LinearLayout
    {
        int maxHeight = 0;

        public MaxHeightLinearLayout(Context context)
            : base(context)
        {
        }

        public MaxHeightLinearLayout(Context context, IAttributeSet attrs)
            : base(context, attrs)
        {
            if (!IsInEditMode)
            {
                Init(context, attrs);
            }
        }

        public MaxHeightLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            if (!IsInEditMode)
            {
                Init(context, attrs);
            }
        }

        public MaxHeightLinearLayout(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
            if (!IsInEditMode)
            {
                Init(context, attrs);
            }
        }

        void Init(Context context, IAttributeSet attrs)
        {
            if (attrs != null)
            {
                var styledAttrs = context.ObtainStyledAttributes(attrs, Resource.Styleable.MaxHeightScrollView);
                maxHeight = styledAttrs.GetDimensionPixelSize(Resource.Styleable.MaxHeightScrollView_maxHeight, 0);

                styledAttrs.Recycle();
            }
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            if (maxHeight > 0)
            {
                var innerHeight = this.GetInnerHeight();

                if (innerHeight != 0)
                {
                    var modeWidth = (int)MeasureSpec.GetMode(widthMeasureSpec);
                    var modeHeight = (int)MeasureSpec.GetMode(heightMeasureSpec);

                    var sizeWidth = MeasureSpec.GetSize(widthMeasureSpec);
                    var sizeHeight = Math.Min(this.maxHeight, innerHeight);

                    this.MeasureChildren(sizeWidth, modeWidth, sizeHeight, modeHeight);
                    this.SetMeasuredDimension(sizeWidth, sizeHeight);

                    return;
                }
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }

        void MeasureChildren(int sizeWidth, int modeWidth, int sizeHeight, int modeHeight)
        {
            var count = this.ChildCount;
            for (var i = 0; i < count; i++)
            {
                var child = this.GetChildAt(i);
                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                child.Measure(
                    MeasureSpec.MakeMeasureSpec(sizeWidth, (MeasureSpecMode)modeWidth == MeasureSpecMode.Exactly ? MeasureSpecMode.AtMost : (MeasureSpecMode)modeWidth),
                    MeasureSpec.MakeMeasureSpec(sizeHeight, (MeasureSpecMode)modeHeight == MeasureSpecMode.Exactly ? MeasureSpecMode.AtMost : (MeasureSpecMode)modeHeight));
            }
        }

        int GetInnerHeight()
        {
            var count = this.ChildCount;
            var innerHeight = 0;
            for (var i = 0; i < count; i++)
            {
                var child = this.GetChildAt(i);
                innerHeight += child.Height;
            }
            return innerHeight;
        }
    }
}