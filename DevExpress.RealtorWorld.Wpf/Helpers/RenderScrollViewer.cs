using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Utils;


namespace DevExpress.RealtorWorld.Xpf.Helpers {
    [TemplatePart(Name = "ScrollContentControl", Type = typeof(RenderScrollContentControl))]
    public class RenderScrollViewer : ContentControl, ISimpleManupulationSupport {
        #region Dependency Properties
        public static readonly DependencyProperty DesiredDeselerationProperty;
        public static readonly DependencyProperty IsMouseManipulationEnabledProperty;
        static RenderScrollViewer() {
            Type ownerType = typeof(RenderScrollViewer);
            DesiredDeselerationProperty = DependencyProperty.Register("DesiredDeseleration", typeof(double), ownerType, new PropertyMetadata(0.001));
            IsMouseManipulationEnabledProperty = DependencyProperty.Register("IsMouseManipulationEnabled", typeof(bool), ownerType, new PropertyMetadata(false));
        }
        #endregion
        RenderScrollContentControl scrollContentControl;
        SimpleManipulationHelper smh;

        public RenderScrollViewer() {
            this.SetDefaultStyleKey(typeof(RenderScrollViewer));
            FocusHelper2.SetFocusable(this, false);
            this.smh = new SimpleManipulationHelper(this);
        }
        public double DesiredDeseleration { get { return (double)GetValue(DesiredDeselerationProperty); } set { SetValue(DesiredDeselerationProperty, value); } }
        public bool IsMouseManipulationEnabled { get { return (bool)GetValue(IsMouseManipulationEnabledProperty); } set { SetValue(IsMouseManipulationEnabledProperty, value); } }
        public double HorizontalOffset { get { return this.scrollContentControl == null ? 0.0 : this.scrollContentControl.HorizontalOffset; } }
        public double VerticalOffset { get { return this.scrollContentControl == null ? 0.0 : this.scrollContentControl.VerticalOffset; } }
        public double HorizontalRelative { get { return this.scrollContentControl == null ? 0.0 : this.scrollContentControl.HorizontalValue / this.scrollContentControl.HorizontalMaximum; } }
        public double VerticalRelative { get { return this.scrollContentControl == null ? 0.0 : this.scrollContentControl.VerticalValue / this.scrollContentControl.VerticalMaximum; } }
        public double ViewportWidth { get { return this.scrollContentControl == null ? 0.0 : this.scrollContentControl.ActualWidth; } }
        public double ViewportHeight { get { return this.scrollContentControl == null ? 0.0 : this.scrollContentControl.ActualHeight; } }
        public event EventHandler ScrollChanged;
        public event EventHandler ViewportSizeChanged;
        public void ScrollToHorizontalOffset(double x) {
            if(this.scrollContentControl != null)
                this.scrollContentControl.ScrollToHorizontalOffset(x);
        }
        public void ScrollToVerticalOffset(double y) {
            if(this.scrollContentControl != null)
                this.scrollContentControl.ScrollToVerticalOffset(y);
        }
        public void ScrollToHorizontalRelative(double x) {
            if(this.scrollContentControl != null)
                this.scrollContentControl.ScrollToHorizontalRelative(x);
        }
        public void ScrollToVerticalRelative(double y) {
            if(this.scrollContentControl != null)
                this.scrollContentControl.ScrollToVerticalRelative(y);
        }
        void OnScrollContentControlSizeChanged(object sender, SizeChangedEventArgs e) {
            if(ViewportSizeChanged != null)
                ViewportSizeChanged(this, EventArgs.Empty);
        }
        void OnScrollContentControlScrollChanged(object sender, EventArgs e) {
            if(ScrollChanged != null)
                ScrollChanged(this, EventArgs.Empty);
        }
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.scrollContentControl = (RenderScrollContentControl)GetTemplateChild("ScrollContentControl");
            if(this.scrollContentControl != null) {
                this.scrollContentControl.SizeChanged += OnScrollContentControlSizeChanged;
                this.scrollContentControl.ScrollChanged += OnScrollContentControlScrollChanged;
            }
        }
        #region ISimpleManupulationSupport
        public virtual void ScrollBy(double x, double y, bool isMouseManipulation) {
            ScrollToHorizontalOffset(HorizontalOffset + x);
            ScrollToVerticalOffset(VerticalOffset + y);
        }
        public virtual void ScaleBy(double factor, bool isMouseManipulation) { }
        public virtual void FinishManipulation(bool isMouseManipulation) { }
        #endregion
    }
}
