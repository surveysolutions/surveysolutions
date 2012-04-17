using System;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Commands;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Utils;


namespace DevExpress.RealtorWorld.Xpf.Helpers {
    [TemplatePart(Name = "PrevButton", Type = typeof(Control))]
    [TemplatePart(Name = "NextButton", Type = typeof(Control))]
    [TemplateVisualState(GroupName = "CommonStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "CommonStates", Name = "MouseOver")]
    public class ElementSlider : ItemsControl, ISimpleManupulationSupport {
        #region Dependency Properties
        public static readonly DependencyProperty DesiredDeselerationProperty;
        public static readonly DependencyProperty IsMouseManipulationEnabledProperty;
        static readonly DependencyProperty ItemWidthProperty;
        static readonly DependencyProperty HorizontalOffsetProperty;

        static ElementSlider() {
            Type ownerType = typeof(ElementSlider);
            DesiredDeselerationProperty = DependencyProperty.Register("DesiredDeseleration", typeof(double), ownerType, new PropertyMetadata(0.005));
            IsMouseManipulationEnabledProperty = DependencyProperty.Register("IsMouseManipulationEnabled", typeof(bool), ownerType, new PropertyMetadata(false));
            ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), ownerType, new PropertyMetadata(double.NaN, RaiseItemWidthChanged));
            HorizontalOffsetProperty = DependencyProperty.Register("HorizontalOffset", typeof(double), ownerType, new PropertyMetadata(0.0, RaiseHorizontalOffsetChanged));
        }
        static void RaiseItemWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ElementSlider)d).RaiseItemWidthChanged(e);
        }
        static void RaiseHorizontalOffsetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ElementSlider)d).RaiseHorizontalOffsetChanged(e);
        }
        #endregion

        const double AnimationDurationShowPrevNextElement = 500.0;
        const double AnimationDurationManipulationCompleted = 300.0;
        RenderScrollViewer scrollViewer;
        Storyboard currentAnimation;
        Control prevButton;
        Control nextButton;
        SimpleManipulationHelper smh;
        bool isMouseOver;

        public ElementSlider() {
            this.SetDefaultStyleKey(typeof(ElementSlider));
            FocusHelper2.SetFocusable(this, false);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            IsManipulationEnabled = true;
            IsMouseManipulationEnabled = true;
            ShowNextElementCommand = new DelegateCommand<object>(parameter => ShowNextElement(), parameter => { return CanShowNextElement(); });
            ShowPrevElementCommand = new DelegateCommand<object>(parameter => ShowPrevElement(), parameter => { return CanShowPrevElement(); });
            this.smh = new SimpleManipulationHelper(this);
        }
        public double DesiredDeseleration { get { return (double)GetValue(DesiredDeselerationProperty); } set { SetValue(DesiredDeselerationProperty, value); } }
        public bool IsMouseManipulationEnabled { get { return (bool)GetValue(IsMouseManipulationEnabledProperty); } set { SetValue(IsMouseManipulationEnabledProperty, value); } }
        public ICommand ShowNextElementCommand { get; private set; }
        public ICommand ShowPrevElementCommand { get; private set; }
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
        double ItemWidth { get { return (double)GetValue(ItemWidthProperty); } set { SetValue(ItemWidthProperty, value); } }
        double HorizontalOffset { get { return (double)GetValue(HorizontalOffsetProperty); } set { SetValue(HorizontalOffsetProperty, value); } }
        bool CanShowNextElement() {
            return this.scrollViewer != null && HorizontalOffset < (Items.Count - 1) * ItemWidth;
        }
        bool CanShowPrevElement() {
            return this.scrollViewer != null && HorizontalOffset >= ItemWidth;
        }
        void ShowNextElement() {
            AnimateScrollViewer(GetScrollViewerHorizontalOffsetAfterAnimation() + ItemWidth, AnimationDurationShowPrevNextElement);
        }
        void ShowPrevElement() {
            AnimateScrollViewer(GetScrollViewerHorizontalOffsetAfterAnimation() - ItemWidth, AnimationDurationShowPrevNextElement);
        }
        protected override void OnItemsChanged(NotifyCollectionChangedEventArgs e) {
            base.OnItemsChanged(e);
            StopAnimation();
            HorizontalOffset = 0.0;
            UpdateCommands();
        }
        double GetScrollViewerHorizontalOffsetAfterAnimation() {
            if(this.currentAnimation != null)
                return ((DoubleAnimation)this.currentAnimation.Children[0]).To.Value;
            else
                return HorizontalOffset;
        }
        void UpdateCommands() {
            ((DelegateCommand<object>)ShowNextElementCommand).RaiseCanExecuteChanged();
            ((DelegateCommand<object>)ShowPrevElementCommand).RaiseCanExecuteChanged();
        }
        protected override DependencyObject GetContainerForItemOverride() {
            ElementSliderItem container = new ElementSliderItem();
            container.HorizontalContentAlignment = HorizontalAlignment.Stretch;
            return container;
        }
        protected override void PrepareContainerForItemOverride(DependencyObject element, object item) {
            base.PrepareContainerForItemOverride(element, item);
            FrameworkElement container = (FrameworkElement)element;
            container.Width = ItemWidth;
        }
        void OnScrollViewerViewportSizeChanged(object sender, EventArgs e) {
            ItemWidth = this.scrollViewer.ViewportWidth;
        }
        void OnScrollViewerScrollChanged(object sender, EventArgs e) {
            HorizontalOffset = this.scrollViewer.HorizontalOffset;
            UpdateCommands();
        }
        void RaiseItemWidthChanged(DependencyPropertyChangedEventArgs e) {
            foreach(object item in Items) {
                FrameworkElement container = (FrameworkElement)ItemContainerGenerator.ContainerFromItem(item);
                if(container == null) continue;
                container.Width = ItemWidth;
            }
            UpdateCommands();
        }
        void RaiseHorizontalOffsetChanged(DependencyPropertyChangedEventArgs e) {
            if(this.scrollViewer == null) return;
            double newValue = (double)e.NewValue;
            if(newValue != this.scrollViewer.HorizontalOffset)
                this.scrollViewer.ScrollToHorizontalOffset(newValue);
        }
        public void ScrollBy(double x, double y, bool isMouseManipulation) {
            if(this.scrollViewer == null) return;
            this.scrollViewer.ScrollBy(x, y, isMouseManipulation);
        }
        public void ScaleBy(double factor, bool isMouseManipulation) {
            if(this.scrollViewer == null) return;
            this.scrollViewer.ScaleBy(factor, isMouseManipulation);
        }
        public void FinishManipulation(bool isMouseManipulation) {
            AnimateScrollViewer(Math.Round(ItemWidth * Math.Round(HorizontalOffset / ItemWidth)), AnimationDurationManipulationCompleted);
        }
        void StopAnimation() {
            if(currentAnimation == null) return;
            currentAnimation.SkipToFill();
            currentAnimation.Stop();
            currentAnimation = null;
        }
        void AnimateScrollViewer(double to, double duration) {
            StopAnimation();
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = HorizontalOffset;
            animation.To = to;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(duration));
            animation.FillBehavior = FillBehavior.HoldEnd;
            ExponentialEase fadeIn = new ExponentialEase() { EasingMode = EasingMode.EaseIn };
            animation.EasingFunction = fadeIn;
            Storyboard.SetTargetProperty(animation, new PropertyPath("HorizontalOffset"));
            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            Storyboard.SetTarget(storyboard, this);
            storyboard.Completed += delegate {
                currentAnimation = null;
                storyboard.Stop();
                HorizontalOffset = to;
                Dispatcher.BeginInvoke(new Action(() => {
                    UpdateCommands();
                }), null);
            };
            currentAnimation = storyboard;
            storyboard.Begin();
        }
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.scrollViewer = (RenderScrollViewer)GetTemplateChild("ScrollViewer");
            if(this.scrollViewer != null) {
                this.scrollViewer.ViewportSizeChanged += OnScrollViewerViewportSizeChanged;
                this.scrollViewer.ScrollChanged += OnScrollViewerScrollChanged;
                OnScrollViewerScrollChanged(this.scrollViewer, EventArgs.Empty);
            }
            this.prevButton = (Control)GetTemplateChild("PrevButton");
            this.nextButton = (Control)GetTemplateChild("NextButton");
        }
    }
    public class ElementSliderItem : ContentControl {
        public ElementSliderItem() {
            this.SetDefaultStyleKey(typeof(ElementSliderItem));
            FocusHelper2.SetFocusable(this, false);
        }
    }
}
