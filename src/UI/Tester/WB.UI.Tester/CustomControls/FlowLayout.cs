using System;
using System.Collections;
using System.Collections.Specialized;
using Android.Content;
using Android.Graphics;
using Android.Util;
using Android.Views;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.Views;

namespace WB.UI.QuestionnaireTester.CustomControls
{
    class FlowLayout : ViewGroup, IMvxWithChangeAdapter
    {
        public static int Horizontal = 0;
        public static int Vertical = 1;

        public int HorizontalSpacing = 0;
        public int VerticalSpacing = 0;
        public int Orientation = 0;
        public bool DebugDraw = false;

        public FlowLayout(Context context)
            : this(context, null)
        {
            this.ReadStyleParameters(context, null);
        }

        public FlowLayout(Context context, IAttributeSet attributeSet)
            : this(context, attributeSet, new MvxAdapterWithChangedEvent(context))
        {
            this.ReadStyleParameters(context, attributeSet);
        }

        public FlowLayout(Context context, IAttributeSet attrs, IMvxAdapterWithChangedEvent adapter)
            : base(context, attrs)
        {
            int num = MvxAttributeHelpers.ReadListItemTemplateId(context, attrs);
            if (adapter != null)
            {
                this.Adapter = adapter;
                this.Adapter.ItemTemplateId = num;
            }
            this.ChildViewRemoved += new EventHandler<ViewGroup.ChildViewRemovedEventArgs>(this.OnChildViewRemoved);
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var sizeWidth = MeasureSpec.GetSize(widthMeasureSpec) - this.PaddingRight - this.PaddingLeft;
            var sizeHeight = MeasureSpec.GetSize(heightMeasureSpec) - this.PaddingRight - this.PaddingLeft;

            var modeWidth = (int)MeasureSpec.GetMode(widthMeasureSpec);
            var modeHeight = (int)MeasureSpec.GetMode(heightMeasureSpec);

            int size;
            int mode;

            if (this.Orientation == Horizontal)
            {
                size = sizeWidth;
                mode = modeWidth;
            }
            else
            {
                size = sizeHeight;
                mode = modeHeight;
            }

            var lineThicknessWithSpacing = 0;
            var lineThickness = 0;
            var lineLengthWithSpacing = 0;

            var prevLinePosition = 0;

            var controlMaxLength = 0;
            var controlMaxThickness = 0;

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
                        MeasureSpec.MakeMeasureSpec(sizeHeight, (MeasureSpecMode)modeHeight == MeasureSpecMode.Exactly ? MeasureSpecMode.AtMost : (MeasureSpecMode)modeHeight)
                );

                var lp = (LayoutParams)child.LayoutParameters;

                var hSpacing = this.GetHorizontalSpacing(lp);
                var vSpacing = this.GetVerticalSpacing(lp);

                var childWidth = child.MeasuredWidth;
                var childHeight = child.MeasuredHeight;

                int childLength;
                int childThickness;
                int spacingLength;
                int spacingThickness;

                if (this.Orientation == Horizontal)
                {
                    childLength = childWidth;
                    childThickness = childHeight;
                    spacingLength = hSpacing;
                    spacingThickness = vSpacing;
                }
                else
                {
                    childLength = childHeight;
                    childThickness = childWidth;
                    spacingLength = vSpacing;
                    spacingThickness = hSpacing;
                }

                var lineLength = lineLengthWithSpacing + childLength;
                lineLengthWithSpacing = lineLength + spacingLength;

                var newLine = lp.NewLine || ((MeasureSpecMode)mode != MeasureSpecMode.Unspecified && lineLength > size);
                if (newLine)
                {
                    prevLinePosition = prevLinePosition + lineThicknessWithSpacing;

                    lineThickness = childThickness;
                    lineLength = childLength;
                    lineThicknessWithSpacing = childThickness + spacingThickness;
                    lineLengthWithSpacing = lineLength + spacingLength;
                }

                lineThicknessWithSpacing = Math.Max(lineThicknessWithSpacing, childThickness + spacingThickness);
                lineThickness = Math.Max(lineThickness, childThickness);

                int posX;
                int posY;
                if (this.Orientation == Horizontal)
                {
                    posX = this.PaddingLeft + lineLength - childLength;
                    posY = this.PaddingTop + prevLinePosition;
                }
                else
                {
                    posX = this.PaddingLeft + prevLinePosition;
                    posY = this.PaddingTop + lineLength - childHeight;
                }
                lp.SetPosition(posX, posY);

                controlMaxLength = Math.Max(controlMaxLength, lineLength);
                controlMaxThickness = prevLinePosition + lineThickness;
            }

            if (this.Orientation == Horizontal)
            {
                this.SetMeasuredDimension(ResolveSize(controlMaxLength, widthMeasureSpec), ResolveSize(controlMaxThickness, heightMeasureSpec));
            }
            else
            {
                this.SetMeasuredDimension(ResolveSize(controlMaxThickness, widthMeasureSpec), ResolveSize(controlMaxLength, heightMeasureSpec));
            }
        }

        private int GetVerticalSpacing(LayoutParams lp)
        {
            int vSpacing;
            if (lp.VerticalSpacingSpecified())
            {
                vSpacing = lp.VerticalSpacing;
            }
            else
            {
                vSpacing = this.VerticalSpacing;
            }
            return vSpacing;
        }

        private int GetHorizontalSpacing(LayoutParams lp)
        {
            int hSpacing;
            if (lp.HorizontalSpacingSpecified())
            {
                hSpacing = lp.HorizontalSpacing;
            }
            else
            {
                hSpacing = this.HorizontalSpacing;
            }
            return hSpacing;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var count = this.ChildCount;
            for (var i = 0; i < count; i++)
            {
                var child = this.GetChildAt(i);
                var lp = (LayoutParams)child.LayoutParameters;
                child.Layout(lp.X, lp.Y, lp.X + child.MeasuredWidth, lp.Y + child.MeasuredHeight);
            }
        }

        protected override bool DrawChild(Canvas canvas, View child, long drawingTime)
        {
            var more = base.DrawChild(canvas, child, drawingTime);
            this.DrawDebugInfo(canvas, child);
            return more;
        }

        protected override bool CheckLayoutParams(ViewGroup.LayoutParams p)
        {
            return p is LayoutParams;
        }

        protected override ViewGroup.LayoutParams GenerateDefaultLayoutParams()
        {
            return new LayoutParams(ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent);
        }

        public override ViewGroup.LayoutParams GenerateLayoutParams(IAttributeSet attributeSet)
        {
            return new LayoutParams(this.Context, attributeSet);
        }

        private IMvxAdapterWithChangedEvent _adapter;

        public IMvxAdapterWithChangedEvent Adapter
        {
            get
            {
                return this._adapter;
            }
            protected set
            {
                IMvxAdapterWithChangedEvent withChangedEvent = this._adapter;
                if (withChangedEvent == value)
                    return;
                if (withChangedEvent != null)
                {
                    withChangedEvent.DataSetChanged -= new EventHandler<NotifyCollectionChangedEventArgs>(this.AdapterOnDataSetChanged);
                    if (value != null)
                    {
                        value.ItemsSource = withChangedEvent.ItemsSource;
                        value.ItemTemplateId = withChangedEvent.ItemTemplateId;
                    }
                }
                this._adapter = value;
                if (this._adapter != null)
                    this._adapter.DataSetChanged += new EventHandler<NotifyCollectionChangedEventArgs>(this.AdapterOnDataSetChanged);
                if (this._adapter != null)
                    return;
                MvxBindingTrace.Warning("Setting Adapter to null is not recommended - you amy lose ItemsSource binding when doing this");
            }
        }

        [MvxSetToNullAfterBinding]
        public IEnumerable ItemsSource
        {
            get
            {
                return this.Adapter.ItemsSource;
            }
            set
            {
                this.Adapter.ItemsSource = value;
            }
        }

        public int ItemTemplateId
        {
            get
            {
                return this.Adapter.ItemTemplateId;
            }
            set
            {
                this.Adapter.ItemTemplateId = value;
            }
        }

        public void AdapterOnDataSetChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            this.UpdateDataSetFromChange(sender, eventArgs);
        }

        private void OnChildViewRemoved(object sender, ViewGroup.ChildViewRemovedEventArgs childViewRemovedEventArgs)
        {
            IMvxBindingContextOwner owner = childViewRemovedEventArgs.Child as IMvxBindingContextOwner;
            if (owner == null)
                return;
            MvxBindingContextOwnerExtensions.ClearAllBindings(owner);
        }

        protected override ViewGroup.LayoutParams GenerateLayoutParams(ViewGroup.LayoutParams p)
        {
            return new LayoutParams(p);
        }

        private void ReadStyleParameters(Context context, IAttributeSet attributeSet)
        {
            var a = context.ObtainStyledAttributes(attributeSet, Resource.Styleable.FlowLayout);
            try
            {
                this.HorizontalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_horizontalSpacing, 0);
                this.VerticalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_verticalSpacing, 0);
                this.Orientation = a.GetInteger(Resource.Styleable.FlowLayout_orientation, Horizontal);
                this.DebugDraw = a.GetBoolean(Resource.Styleable.FlowLayout_debugDraw, false);
            }
            finally
            {
                a.Recycle();
            }
        }

        private void DrawDebugInfo(Canvas canvas, View child)
        {
            if (!this.DebugDraw)
            {
                return;
            }

            var childPaint = CreatePaint(new Color(255, 255, 0, 255));
            var layoutPaint = CreatePaint(new Color(0, 255, 0, 255));
            var newLinePaint = CreatePaint(new Color(255, 0, 0, 255));

            var lp = (LayoutParams)child.LayoutParameters;

            if (lp.HorizontalSpacing > 0)
            {
                float x = child.Right;
                var y = child.Top + child.Height / 2.0f;
                canvas.DrawLine(x, y, x + lp.HorizontalSpacing, y, childPaint);
                canvas.DrawLine(x + lp.HorizontalSpacing - 4.0f, y - 4.0f, x + lp.HorizontalSpacing, y, childPaint);
                canvas.DrawLine(x + lp.HorizontalSpacing - 4.0f, y + 4.0f, x + lp.HorizontalSpacing, y, childPaint);
            }
            else
                if (this.HorizontalSpacing > 0)
                {
                    float x = child.Right;
                    var y = child.Top + child.Height / 2.0f;
                    canvas.DrawLine(x, y, x + this.HorizontalSpacing, y, layoutPaint);
                    canvas.DrawLine(x + this.HorizontalSpacing - 4.0f, y - 4.0f, x + this.HorizontalSpacing, y, layoutPaint);
                    canvas.DrawLine(x + this.HorizontalSpacing - 4.0f, y + 4.0f, x + this.HorizontalSpacing, y, layoutPaint);
                }

            if (lp.VerticalSpacing > 0)
            {
                var x = child.Left + child.Width / 2.0f;
                float y = child.Bottom;
                canvas.DrawLine(x, y, x, y + lp.VerticalSpacing, childPaint);
                canvas.DrawLine(x - 4.0f, y + lp.VerticalSpacing - 4.0f, x, y + lp.VerticalSpacing, childPaint);
                canvas.DrawLine(x + 4.0f, y + lp.VerticalSpacing - 4.0f, x, y + lp.VerticalSpacing, childPaint);
            }
            else if (this.VerticalSpacing > 0)
            {
                var x = child.Left + child.Width / 2.0f;
                float y = child.Bottom;
                canvas.DrawLine(x, y, x, y + this.VerticalSpacing, layoutPaint);
                canvas.DrawLine(x - 4.0f, y + this.VerticalSpacing - 4.0f, x, y + this.VerticalSpacing, layoutPaint);
                canvas.DrawLine(x + 4.0f, y + this.VerticalSpacing - 4.0f, x, y + this.VerticalSpacing, layoutPaint);
            }

            if (lp.NewLine)
            {
                if (this.Orientation == Horizontal)
                {
                    float x = child.Left;
                    var y = child.Top + child.Height / 2.0f;
                    canvas.DrawLine(x, y - 6.0f, x, y + 6.0f, newLinePaint);
                }
                else
                {
                    var x = child.Left + child.Width / 2.0f;
                    float y = child.Top;
                    canvas.DrawLine(x - 6.0f, y, x + 6.0f, y, newLinePaint);
                }
            }
        }

        private static Paint CreatePaint(Color color)
        {
            var paint = new Paint
                            {
                                AntiAlias = true,
                                Color = color,
                                StrokeWidth = 2.0f
                            };
            return paint;
        }

        public new class LayoutParams : ViewGroup.LayoutParams       //Java class WAS static
        {
            private const int NoSpacing = -1;

            public int X;
            public int Y;
            public int HorizontalSpacing = NoSpacing;
            public int VerticalSpacing = NoSpacing;
            public bool NewLine = false;

            public LayoutParams(Context context, IAttributeSet attributeSet) :
                base(context, attributeSet)
            {
                this.ReadStyleParameters(context, attributeSet);
            }

            public LayoutParams(int width, int height) : base(width, height) { }

            public LayoutParams(ViewGroup.LayoutParams layoutParams) : base(layoutParams) { }

            public bool HorizontalSpacingSpecified()
            {
                return this.HorizontalSpacing != NoSpacing;
            }

            public bool VerticalSpacingSpecified()
            {
                return this.VerticalSpacing != NoSpacing;
            }

            public void SetPosition(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            private void ReadStyleParameters(Context context, IAttributeSet attributeSet)
            {
                var a = context.ObtainStyledAttributes(attributeSet, Resource.Styleable.FlowLayout_LayoutParams);
                try
                {
                    this.HorizontalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_LayoutParams_layout_horizontalSpacing, NoSpacing);
                    this.VerticalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_LayoutParams_layout_verticalSpacing, NoSpacing);
                    this.NewLine = a.GetBoolean(Resource.Styleable.FlowLayout_LayoutParams_layout_newLine, false);
                }
                finally
                {
                    a.Recycle();
                }
            }
        }
    }
}
