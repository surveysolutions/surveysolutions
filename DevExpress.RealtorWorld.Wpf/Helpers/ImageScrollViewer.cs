using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class ImageScrollViewer : RenderScrollViewer {
        #region Dependency Properties
        public static readonly DependencyProperty SourceProperty;
        public static readonly DependencyProperty ZoomProperty;
        public static readonly DependencyProperty MinimalZoomProperty;
        public static readonly DependencyProperty MaximalZoomProperty;
        static ImageScrollViewer() {
            Type ownerType = typeof(ImageScrollViewer);
            SourceProperty = DependencyProperty.Register("Source", typeof(ImageSource), ownerType, new PropertyMetadata(null, (d, e) => ((ImageScrollViewer)d).RaiseSourceChanged(e)));
            ZoomProperty = DependencyProperty.Register("Zoom", typeof(double), ownerType, new PropertyMetadata(1.0, (d, e) => ((ImageScrollViewer)d).RaiseZoomChanged(e)));
            MinimalZoomProperty = DependencyProperty.Register("MinimalZoom", typeof(double), ownerType, new PropertyMetadata(0.1));
            MaximalZoomProperty = DependencyProperty.Register("MaximalZoom", typeof(double), ownerType, new PropertyMetadata(1.0));
        }
        #endregion

        Image image;
        Size originalImageSize;
        Size currentImageSize;
        bool isMouseManipulation = false;
        bool isMouseOver;

        public ImageScrollViewer() {
            DefaultStyleKey = typeof(ImageScrollViewer);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsManipulationEnabled = true;
            IsMouseManipulationEnabled = true;
            System.Windows.Controls.ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            System.Windows.Controls.ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            SizeChanged += OnImageScrollViewerSizeChanged;
        }
        public ImageSource Source { get { return (ImageSource)GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }
        public double Zoom { get { return (double)GetValue(ZoomProperty); } set { SetValue(ZoomProperty, value); } }
        public double MinimalZoom { get { return (double)GetValue(MinimalZoomProperty); } set { SetValue(MinimalZoomProperty, value); } }
        public double MaximalZoom { get { return (double)GetValue(MaximalZoomProperty); } set { SetValue(MaximalZoomProperty, value); } }
        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            isMouseOver = true;
            GoToState(true);
        }
        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            isMouseOver = false;
            GoToState(true);
        }
        protected virtual void OnLoaded(object sender, RoutedEventArgs e) {
            GoToStateCore(false);
        }
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e) {
        }
        protected void GoToState(bool useTransitions) {
            if(IsLoaded)
                GoToStateCore(useTransitions);
        }
        protected virtual void GoToStateCore(bool useTransitions) {
            if(isMouseOver)
                VisualStateManager.GoToState(this, "MouseOver", useTransitions);
            else
                VisualStateManager.GoToState(this, "Normal", useTransitions);
        }

        public override void ScaleBy(double factor, bool isMouseManipulated) {
            if(isMouseManipulated) {
                factor = 1.0 + factor / this.currentImageSize.Width;
            }
            factor = Math.Min(1.0, Zoom * factor);
            if(factor > MaximalZoom) {
                factor = MaximalZoom;
            }
            if(factor < MinimalZoom) {
                factor = MinimalZoom;
            }
            isMouseManipulation = isMouseManipulated;
            Zoom = factor;
        }
        void OnImageScrollViewerSizeChanged(object sender, SizeChangedEventArgs e) {
            UpdateZoomBoundaries();
        }
        void ScaleImage(double factor) {
            double scaleFactorX = HorizontalRelative;
            double scaleFactorY = VerticalRelative;
            bool isHorizMin = double.IsNaN(scaleFactorX) || double.IsInfinity(scaleFactorX);
            bool isVertMin = double.IsNaN(scaleFactorY) || double.IsInfinity(scaleFactorY);
            ScaleImageCore(factor);
            UpdateLayout();
            if(isMouseManipulation) {
                isMouseManipulation = false;
                SetImageToCenter();
            } else {
                if(isHorizMin)
                    scaleFactorX = 0.5;
                if(isVertMin)
                    scaleFactorY = 0.5;
                ScrollToHorizontalRelative(scaleFactorX);
                ScrollToVerticalRelative(scaleFactorY);
            }
        }
        void SetImageToCenter() {
            if(this.image == null) return;
            ScrollToHorizontalRelative(0.5);
            ScrollToVerticalRelative(0.5);
        }
        void ScaleImageCore(double factor) {
            CalcCurrentImageSize(factor);
            ApplyCurrentImageSize();
        }
        void CalcCurrentImageSize(double factor) {
            int delta = 200;
            double newWidth = this.currentImageSize.Width * factor;
            double newHeight = this.currentImageSize.Height * factor;

            if(newWidth > this.originalImageSize.Width || newHeight > this.originalImageSize.Height) {
                this.currentImageSize.Width = this.originalImageSize.Width;
                this.currentImageSize.Height = this.originalImageSize.Height;
                return;
            }
            if(newWidth < delta) {
                newHeight = (delta / newWidth) * newHeight;
                newWidth = delta;
            }
            if(newHeight < delta) {
                newWidth = (delta / newHeight) * newWidth;
                newHeight = delta;
            }
            this.currentImageSize.Width = newWidth;
            this.currentImageSize.Height = newHeight;
        }
        void ApplyCurrentImageSize() {
            if(this.image == null) return;
            this.image.Width = this.currentImageSize.Width;
            this.image.Height = this.currentImageSize.Height;
        }
        void RaiseZoomChanged(DependencyPropertyChangedEventArgs e) {
            ScaleImage(((double)e.NewValue) / ((double)e.OldValue));
        }
        void RaiseSourceChanged(DependencyPropertyChangedEventArgs e) {
            this.image = new Image() { Stretch = Stretch.None };
            Content = this.image;
            this.image.SizeChanged += OnImageSizeChanged;
            this.image.Source = Source;
        }
        void OnImageSizeChanged(object sender, SizeChangedEventArgs e) {
            this.image.SizeChanged -= OnImageSizeChanged;
            this.currentImageSize = this.originalImageSize = new Size(this.image.ActualWidth, this.image.ActualHeight);
            UpdateZoomBoundaries();
            this.image.Width = this.currentImageSize.Width;
            this.image.Height = this.currentImageSize.Height;
            this.image.Stretch = Stretch.Uniform;
            Zoom = 1.0;
            BackgroundHelper.DoInBackground(null, SetImageToCenter);
        }
        void UpdateZoomBoundaries() {
            this.MinimalZoom = Math.Max(0.001, Math.Min(Math.Min((ActualHeight - 3) / originalImageSize.Height, (ActualWidth - 3) / originalImageSize.Width), 1.0));
        }
    }
}
