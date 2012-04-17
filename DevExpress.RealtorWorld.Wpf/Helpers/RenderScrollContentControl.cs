using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DevExpress.Xpf.Core;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public interface ISupportLayout {
        Size DesiredSize { get; }
        Size PerfectSize { get; }
        Size RenderSize { get; }
    }
    public class RenderScrollContentControl : ContentControl {
        const double Range = 100.0;
        #region Dependency Properties
        public static readonly DependencyProperty HorizontalScrollBarVisibilityProperty;
        public static readonly DependencyProperty VerticalScrollBarVisibilityProperty;
        public static readonly DependencyProperty ComputedHorizontalScrollBarIsEnabledProperty;
        public static readonly DependencyProperty ComputedVerticalScrollBarIsEnabledProperty;
        public static readonly DependencyProperty ComputedHorizontalScrollBarVisibilityProperty;
        public static readonly DependencyProperty ComputedVerticalScrollBarVisibilityProperty;
        public static readonly DependencyProperty ComputedBothScrollBarsVisiblityProperty;
        public static readonly DependencyProperty HorizontalMinimumProperty;
        public static readonly DependencyProperty VerticalMinimumProperty;
        public static readonly DependencyProperty HorizontalMaximumProperty;
        public static readonly DependencyProperty VerticalMaximumProperty;
        public static readonly DependencyProperty HorizontalViewportSizeProperty;
        public static readonly DependencyProperty VerticalViewportSizeProperty;
        public static readonly DependencyProperty HorizontalValueProperty;
        public static readonly DependencyProperty VerticalValueProperty;
        static RenderScrollContentControl() {
            Type ownerType = typeof(RenderScrollContentControl);
            HorizontalScrollBarVisibilityProperty = DependencyProperty.Register("HorizontalScrollBarVisibility", typeof(ScrollBarVisibility), ownerType, new PropertyMetadata(ScrollBarVisibility.Disabled, RaiseHorizontalScrollBarVisibilityChanged));
            VerticalScrollBarVisibilityProperty = DependencyProperty.Register("VerticalScrollBarVisibility", typeof(ScrollBarVisibility), ownerType, new PropertyMetadata(ScrollBarVisibility.Disabled, RaiseVerticalScrollBarVisibilityChanged));
            ComputedHorizontalScrollBarIsEnabledProperty = DependencyProperty.Register("ComputedHorizontalScrollBarIsEnabled", typeof(bool), ownerType, new PropertyMetadata(false));
            ComputedVerticalScrollBarIsEnabledProperty = DependencyProperty.Register("ComputedVerticalScrollBarIsEnabled", typeof(bool), ownerType, new PropertyMetadata(false));
            ComputedHorizontalScrollBarVisibilityProperty = DependencyProperty.Register("ComputedHorizontalScrollBarVisibility", typeof(Visibility), ownerType, new PropertyMetadata(Visibility.Collapsed, RaiseComputedHorizontalScrollBarVisibilityChanged));
            ComputedVerticalScrollBarVisibilityProperty = DependencyProperty.Register("ComputedVerticalScrollBarVisibility", typeof(Visibility), ownerType, new PropertyMetadata(Visibility.Collapsed, RaiseComputedVerticalScrollBarVisibilityChanged));
            ComputedBothScrollBarsVisiblityProperty = DependencyProperty.Register("ComputedBothScrollBarsVisiblity", typeof(Visibility), ownerType, new PropertyMetadata(Visibility.Collapsed));
            HorizontalMinimumProperty = DependencyProperty.Register("HorizontalMinimum", typeof(double), ownerType, new PropertyMetadata(0.0));
            VerticalMinimumProperty = DependencyProperty.Register("VerticalMinimum", typeof(double), ownerType, new PropertyMetadata(0.0));
            HorizontalMaximumProperty = DependencyProperty.Register("HorizontalMaximum", typeof(double), ownerType, new PropertyMetadata(0.0));
            VerticalMaximumProperty = DependencyProperty.Register("VerticalMaximum", typeof(double), ownerType, new PropertyMetadata(0.0));
            HorizontalViewportSizeProperty = DependencyProperty.Register("HorizontalViewportSize", typeof(double), ownerType, new PropertyMetadata(0.0));
            VerticalViewportSizeProperty = DependencyProperty.Register("VerticalViewportSize", typeof(double), ownerType, new PropertyMetadata(0.0));
            HorizontalValueProperty = DependencyProperty.Register("HorizontalValue", typeof(double), ownerType, new PropertyMetadata(0.0, RaiseHorizontalValueChanged));
            VerticalValueProperty = DependencyProperty.Register("VerticalValue", typeof(double), ownerType, new PropertyMetadata(0.0, RaiseVerticalValueChanged));
        }
        static void RaiseHorizontalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RenderScrollContentControl)d).RaiseHorizontalScrollBarVisibilityChanged(e);
        }
        static void RaiseVerticalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RenderScrollContentControl)d).RaiseVerticalScrollBarVisibilityChanged(e);
        }
        static void RaiseComputedHorizontalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RenderScrollContentControl)d).RaiseComputedHorizontalScrollBarVisibilityChanged(e);
        }
        static void RaiseComputedVerticalScrollBarVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RenderScrollContentControl)d).RaiseComputedVerticalScrollBarVisibilityChanged(e);
        }
        static void RaiseHorizontalValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RenderScrollContentControl)d).RaiseHorizontalValueChanged(e);
        }
        static void RaiseVerticalValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((RenderScrollContentControl)d).RaiseVerticalValueChanged(e);
        }
        #endregion

        public RenderScrollContentControl() {
            FocusHelper2.SetFocusable(this, false);
        }
        public ScrollBarVisibility HorizontalScrollBarVisibility { get { return (ScrollBarVisibility)GetValue(HorizontalScrollBarVisibilityProperty); } set { SetValue(HorizontalScrollBarVisibilityProperty, value); } }
        public ScrollBarVisibility VerticalScrollBarVisibility { get { return (ScrollBarVisibility)GetValue(VerticalScrollBarVisibilityProperty); } set { SetValue(VerticalScrollBarVisibilityProperty, value); } }
        public bool ComputedHorizontalScrollBarIsEnabled { get { return (bool)GetValue(ComputedHorizontalScrollBarIsEnabledProperty); } private set { SetValue(ComputedHorizontalScrollBarIsEnabledProperty, value); } }
        public bool ComputedVerticalScrollBarIsEnabled { get { return (bool)GetValue(ComputedVerticalScrollBarIsEnabledProperty); } private set { SetValue(ComputedVerticalScrollBarIsEnabledProperty, value); } }
        public Visibility ComputedHorizontalScrollBarVisibility { get { return (Visibility)GetValue(ComputedHorizontalScrollBarVisibilityProperty); } private set { SetValue(ComputedHorizontalScrollBarVisibilityProperty, value); } }
        public Visibility ComputedVerticalScrollBarVisibility { get { return (Visibility)GetValue(ComputedVerticalScrollBarVisibilityProperty); } private set { SetValue(ComputedVerticalScrollBarVisibilityProperty, value); } }
        public Visibility ComputedBothScrollBarsVisiblity { get { return (Visibility)GetValue(ComputedBothScrollBarsVisiblityProperty); } private set { SetValue(ComputedBothScrollBarsVisiblityProperty, value); } }
        public double HorizontalMinimum { get { return (double)GetValue(HorizontalMinimumProperty); } private set { SetValue(HorizontalMinimumProperty, value); } }
        public double VerticalMinimum { get { return (double)GetValue(VerticalMinimumProperty); } private set { SetValue(VerticalMinimumProperty, value); } }
        public double HorizontalMaximum { get { return (double)GetValue(HorizontalMaximumProperty); } private set { SetValue(HorizontalMaximumProperty, value); } }
        public double VerticalMaximum { get { return (double)GetValue(VerticalMaximumProperty); } private set { SetValue(VerticalMaximumProperty, value); } }
        public double HorizontalViewportSize { get { return (double)GetValue(HorizontalViewportSizeProperty); } private set { SetValue(HorizontalViewportSizeProperty, value); } }
        public double VerticalViewportSize { get { return (double)GetValue(VerticalViewportSizeProperty); } private set { SetValue(VerticalViewportSizeProperty, value); } }
        public double HorizontalValue { get { return (double)GetValue(HorizontalValueProperty); } set { SetValue(HorizontalValueProperty, value); } }
        public double VerticalValue { get { return (double)GetValue(VerticalValueProperty); } set { SetValue(VerticalValueProperty, value); } }
        public double HorizontalOffset { get; private set; }
        public double VerticalOffset { get; private set; }
        public event EventHandler ScrollChanged;
        public void ScrollToHorizontalOffset(double x) {
            UIElement child = Child;
            if(child == null) return;
            Size childSize = GetChildSize(child, RenderSize);
            double childWidth = Math.Max(childSize.Width, RenderSize.Width);
            double h = CalculateValueFromArrange(childWidth, RenderSize.Width, x, HorizontalScrollBarVisibility);
            if(h < 0.0)
                h = 0.0;
            if(h > HorizontalMaximum)
                h = HorizontalMaximum;
            HorizontalValue = h;
        }
        public void ScrollToVerticalOffset(double y) {
            UIElement child = Child;
            if(child == null) return;
            Size childSize = GetChildSize(child, RenderSize);
            double childHeight = Math.Max(childSize.Height, RenderSize.Height);
            double v = CalculateValueFromArrange(childHeight, RenderSize.Height, y, VerticalScrollBarVisibility);
            if(v < 0.0)
                v = 0.0;
            if(v > VerticalMaximum)
                v = VerticalMaximum;
            VerticalValue = v;
        }
        public void ScrollToHorizontalRelative(double x) {
            HorizontalValue = x * HorizontalMaximum;
        }
        public void ScrollToVerticalRelative(double y) {
            VerticalValue = y * VerticalMaximum;
        }
        protected override Size MeasureOverride(Size availableSize) {
            UIElement child = Child;
            if(child == null) return new Size(0.0, 0.0);
            double width = HorizontalScrollBarVisibility == ScrollBarVisibility.Disabled ? availableSize.Width : double.PositiveInfinity;
            double height = VerticalScrollBarVisibility == ScrollBarVisibility.Disabled ? availableSize.Height : double.PositiveInfinity;
            child.Measure(new Size(width, height));
            Size childSize = GetChildSize(child, availableSize);
            return new Size(Math.Min(childSize.Width, availableSize.Width), Math.Min(childSize.Height, availableSize.Height));
        }
        protected override Size ArrangeOverride(Size finalSize) {
            UIElement child = Child;
            if(child == null) return finalSize;
            Size childSize = GetChildSize(child, finalSize);
            childSize = new Size(Math.Max(childSize.Width, finalSize.Width), Math.Max(childSize.Height, finalSize.Height));
            child.Arrange(new Rect(0.0, 0.0, childSize.Width, childSize.Height));
            Clip = new RectangleGeometry() { Rect = new Rect(0.0, 0.0, finalSize.Width, finalSize.Height) };
            UpdateScroll();
            return finalSize;
        }
        void UpdateScroll() {
            UIElement child = Child;
            if(child == null) return;
            Size childSize = GetChildSize(child, RenderSize);
            childSize = new Size(Math.Max(childSize.Width, RenderSize.Width), Math.Max(childSize.Height, RenderSize.Height));
            bool enableScrolling;
            Visibility computedScrollBarVisibility;
            double maximum;
            double viewportSize;
            double x = CalculateArrange(childSize.Width, RenderSize.Width, HorizontalValue, HorizontalScrollBarVisibility, out enableScrolling, out computedScrollBarVisibility, out maximum, out viewportSize);
            ComputedHorizontalScrollBarIsEnabled = enableScrolling;
            ComputedHorizontalScrollBarVisibility = computedScrollBarVisibility;
            HorizontalMaximum = maximum;
            HorizontalViewportSize = viewportSize;
            double y = CalculateArrange(childSize.Height, RenderSize.Height, VerticalValue, VerticalScrollBarVisibility, out enableScrolling, out computedScrollBarVisibility, out maximum, out viewportSize);
            ComputedVerticalScrollBarIsEnabled = enableScrolling;
            ComputedVerticalScrollBarVisibility = computedScrollBarVisibility;
            VerticalMaximum = maximum;
            VerticalViewportSize = viewportSize;
            child.RenderTransform = new TranslateTransform() { X = -Math.Ceiling(x), Y = -Math.Ceiling(y) };
            UpdateOffsets(x, y);
        }
        void UpdateOffsets(double x, double y) {
            if(x == HorizontalOffset && y == VerticalOffset) return;
            HorizontalOffset = x;
            VerticalOffset = y;
            if(ScrollChanged != null)
                ScrollChanged(this, EventArgs.Empty);
        }
        UIElement Child { get { return VisualTreeHelper.GetChildrenCount(this) < 1 ? null : VisualTreeHelper.GetChild(this, 0) as UIElement; } }
        static Size GetChildSize(UIElement child, Size availableSize) {
            Size desiredSize = child.DesiredSize;
            ContentPresenter contentPresenter = child as ContentPresenter;
            ISupportLayout scalePanel = contentPresenter == null ? child as ISupportLayout : contentPresenter.Content as ISupportLayout;
            Size perfectSize = scalePanel == null ? desiredSize : scalePanel.PerfectSize;
            double width = perfectSize.Width > availableSize.Width ? desiredSize.Width : perfectSize.Width;
            double height = perfectSize.Height > availableSize.Height ? desiredSize.Height : perfectSize.Height;
            return new Size(width, height);
        }
        double CalculateArrange(double regularSize, double finalSize, double value, ScrollBarVisibility scrollBarVisibility, out bool enableScrolling, out Visibility computedScrollBarVisibility, out double maximum, out double viewportSize) {
            double arrange;
            if(scrollBarVisibility == ScrollBarVisibility.Disabled) {
                viewportSize = 0.0;
                enableScrolling = false;
                arrange = 0.0;
            } else {
                double k = Range / (regularSize - finalSize);
                viewportSize = k * finalSize;
                enableScrolling = !double.IsNaN(viewportSize) && !double.IsInfinity(viewportSize) && viewportSize > 0.0;
                if(!enableScrolling)
                    viewportSize = 0.0;
                arrange = enableScrolling ? value / k : 0.0;
            }
            computedScrollBarVisibility = ComputeScrollBarVisibility(scrollBarVisibility, enableScrolling);
            maximum = enableScrolling ? Range : 0.0;
            return arrange;
        }
        double CalculateValueFromArrange(double regularSize, double finalSize, double arrange, ScrollBarVisibility scrollBarVisibility) {
            if(scrollBarVisibility == ScrollBarVisibility.Disabled) return 0.0;
            double k = Range / (regularSize - finalSize);
            double viewportSize = k * finalSize;
            bool enableScrolling = !double.IsNaN(viewportSize) && !double.IsInfinity(viewportSize) && viewportSize > 0.0;
            return enableScrolling ? arrange * k : 0.0;
        }
        Visibility ComputeScrollBarVisibility(ScrollBarVisibility visibility, bool enableScrolling) {
            if(visibility == ScrollBarVisibility.Disabled || visibility == ScrollBarVisibility.Hidden) return Visibility.Collapsed;
            if(visibility == ScrollBarVisibility.Visible) return Visibility.Visible;
            return enableScrolling ? Visibility.Visible : Visibility.Collapsed;
        }
        void RaiseHorizontalScrollBarVisibilityChanged(DependencyPropertyChangedEventArgs e) { InvalidateMeasure(); }
        void RaiseVerticalScrollBarVisibilityChanged(DependencyPropertyChangedEventArgs e) { InvalidateMeasure(); }
        void RaiseHorizontalValueChanged(DependencyPropertyChangedEventArgs e) {
            UIElement child = Child;
            if(child == null) return;
            double newValue = (double)e.NewValue;
            Size childSize = GetChildSize(child, RenderSize);
            double childWidth = Math.Max(childSize.Width, RenderSize.Width);
            bool enableScrolling;
            Visibility computedScrollBarVisibility;
            double maximum;
            double viewportSize;
            UpdateOffsets(CalculateArrange(childWidth, RenderSize.Width, newValue, HorizontalScrollBarVisibility, out enableScrolling, out computedScrollBarVisibility, out maximum, out viewportSize), VerticalOffset);
            UpdateScroll();
        }
        void RaiseVerticalValueChanged(DependencyPropertyChangedEventArgs e) {
            UIElement child = Child;
            if(child == null) return;
            double newValue = (double)e.NewValue;
            Size childSize = GetChildSize(child, RenderSize);
            double childHeight = Math.Max(childSize.Height, RenderSize.Height);
            bool enableScrolling;
            Visibility computedScrollBarVisibility;
            double maximum;
            double viewportSize;
            UpdateOffsets(HorizontalOffset, CalculateArrange(childHeight, RenderSize.Height, newValue, VerticalScrollBarVisibility, out enableScrolling, out computedScrollBarVisibility, out maximum, out viewportSize));
            UpdateScroll();
        }
        void RaiseComputedHorizontalScrollBarVisibilityChanged(DependencyPropertyChangedEventArgs e) { UpdateComputedBothScrollBarsVisiblity(); }
        void RaiseComputedVerticalScrollBarVisibilityChanged(DependencyPropertyChangedEventArgs e) { UpdateComputedBothScrollBarsVisiblity(); }
        void UpdateComputedBothScrollBarsVisiblity() {
            ComputedBothScrollBarsVisiblity = ComputedHorizontalScrollBarVisibility == Visibility.Visible && ComputedVerticalScrollBarVisibility == Visibility.Visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
