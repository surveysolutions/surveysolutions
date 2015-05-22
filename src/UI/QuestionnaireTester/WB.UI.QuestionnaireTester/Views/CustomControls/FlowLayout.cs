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

namespace WB.UI.QuestionnaireTester.Views.CustomControls
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
            ReadStyleParameters(context, null);
        }

        public FlowLayout(Context context, IAttributeSet attributeSet)
            : this(context, attributeSet, new MvxAdapterWithChangedEvent(context))
        {
            ReadStyleParameters(context, attributeSet);
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
            var sizeWidth = MeasureSpec.GetSize(widthMeasureSpec) - PaddingRight - PaddingLeft;
            var sizeHeight = MeasureSpec.GetSize(heightMeasureSpec) - PaddingRight - PaddingLeft;

            var modeWidth = (int)MeasureSpec.GetMode(widthMeasureSpec);
            var modeHeight = (int)MeasureSpec.GetMode(heightMeasureSpec);

            int size;
            int mode;

            if (Orientation == Horizontal)
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

            var count = ChildCount;
            for (var i = 0; i < count; i++)
            {
                var child = GetChildAt(i);
                if (child.Visibility == ViewStates.Gone)
                {
                    continue;
                }

                child.Measure(
                        MeasureSpec.MakeMeasureSpec(sizeWidth, (MeasureSpecMode)modeWidth == MeasureSpecMode.Exactly ? MeasureSpecMode.AtMost : (MeasureSpecMode)modeWidth),
                        MeasureSpec.MakeMeasureSpec(sizeHeight, (MeasureSpecMode)modeHeight == MeasureSpecMode.Exactly ? MeasureSpecMode.AtMost : (MeasureSpecMode)modeHeight)
                );

                var lp = (LayoutParams)child.LayoutParameters;

                var hSpacing = GetHorizontalSpacing(lp);
                var vSpacing = GetVerticalSpacing(lp);

                var childWidth = child.MeasuredWidth;
                var childHeight = child.MeasuredHeight;

                int childLength;
                int childThickness;
                int spacingLength;
                int spacingThickness;

                if (Orientation == Horizontal)
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
                if (Orientation == Horizontal)
                {
                    posX = PaddingLeft + lineLength - childLength;
                    posY = PaddingTop + prevLinePosition;
                }
                else
                {
                    posX = PaddingLeft + prevLinePosition;
                    posY = PaddingTop + lineLength - childHeight;
                }
                lp.SetPosition(posX, posY);

                controlMaxLength = Math.Max(controlMaxLength, lineLength);
                controlMaxThickness = prevLinePosition + lineThickness;
            }

            if (Orientation == Horizontal)
            {
                SetMeasuredDimension(ResolveSize(controlMaxLength, widthMeasureSpec), ResolveSize(controlMaxThickness, heightMeasureSpec));
            }
            else
            {
                SetMeasuredDimension(ResolveSize(controlMaxThickness, widthMeasureSpec), ResolveSize(controlMaxLength, heightMeasureSpec));
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
                vSpacing = VerticalSpacing;
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
                hSpacing = HorizontalSpacing;
            }
            return hSpacing;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            var count = ChildCount;
            for (var i = 0; i < count; i++)
            {
                var child = GetChildAt(i);
                var lp = (LayoutParams)child.LayoutParameters;
                child.Layout(lp.X, lp.Y, lp.X + child.MeasuredWidth, lp.Y + child.MeasuredHeight);
            }
        }

        protected override bool DrawChild(Canvas canvas, View child, long drawingTime)
        {
            var more = base.DrawChild(canvas, child, drawingTime);
            DrawDebugInfo(canvas, child);
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
            return new LayoutParams(Context, attributeSet);
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
                HorizontalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_horizontalSpacing, 0);
                VerticalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_verticalSpacing, 0);
                Orientation = a.GetInteger(Resource.Styleable.FlowLayout_orientation, Horizontal);
                DebugDraw = a.GetBoolean(Resource.Styleable.FlowLayout_debugDraw, false);
            }
            finally
            {
                a.Recycle();
            }
        }

        private void DrawDebugInfo(Canvas canvas, View child)
        {
            if (!DebugDraw)
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
                if (HorizontalSpacing > 0)
                {
                    float x = child.Right;
                    var y = child.Top + child.Height / 2.0f;
                    canvas.DrawLine(x, y, x + HorizontalSpacing, y, layoutPaint);
                    canvas.DrawLine(x + HorizontalSpacing - 4.0f, y - 4.0f, x + HorizontalSpacing, y, layoutPaint);
                    canvas.DrawLine(x + HorizontalSpacing - 4.0f, y + 4.0f, x + HorizontalSpacing, y, layoutPaint);
                }

            if (lp.VerticalSpacing > 0)
            {
                var x = child.Left + child.Width / 2.0f;
                float y = child.Bottom;
                canvas.DrawLine(x, y, x, y + lp.VerticalSpacing, childPaint);
                canvas.DrawLine(x - 4.0f, y + lp.VerticalSpacing - 4.0f, x, y + lp.VerticalSpacing, childPaint);
                canvas.DrawLine(x + 4.0f, y + lp.VerticalSpacing - 4.0f, x, y + lp.VerticalSpacing, childPaint);
            }
            else if (VerticalSpacing > 0)
            {
                var x = child.Left + child.Width / 2.0f;
                float y = child.Bottom;
                canvas.DrawLine(x, y, x, y + VerticalSpacing, layoutPaint);
                canvas.DrawLine(x - 4.0f, y + VerticalSpacing - 4.0f, x, y + VerticalSpacing, layoutPaint);
                canvas.DrawLine(x + 4.0f, y + VerticalSpacing - 4.0f, x, y + VerticalSpacing, layoutPaint);
            }

            if (lp.NewLine)
            {
                if (Orientation == Horizontal)
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
                ReadStyleParameters(context, attributeSet);
            }

            public LayoutParams(int width, int height) : base(width, height) { }

            public LayoutParams(ViewGroup.LayoutParams layoutParams) : base(layoutParams) { }

            public bool HorizontalSpacingSpecified()
            {
                return HorizontalSpacing != NoSpacing;
            }

            public bool VerticalSpacingSpecified()
            {
                return VerticalSpacing != NoSpacing;
            }

            public void SetPosition(int x, int y)
            {
                X = x;
                Y = y;
            }

            private void ReadStyleParameters(Context context, IAttributeSet attributeSet)
            {
                var a = context.ObtainStyledAttributes(attributeSet, Resource.Styleable.FlowLayout_LayoutParams);
                try
                {
                    HorizontalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_LayoutParams_layout_horizontalSpacing, NoSpacing);
                    VerticalSpacing = a.GetDimensionPixelSize(Resource.Styleable.FlowLayout_LayoutParams_layout_verticalSpacing, NoSpacing);
                    NewLine = a.GetBoolean(Resource.Styleable.FlowLayout_LayoutParams_layout_newLine, false);
                }
                finally
                {
                    a.Recycle();
                }
            }
        }
    }
}
