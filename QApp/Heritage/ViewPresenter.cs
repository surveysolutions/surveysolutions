using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Markup;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace QApp.Heritage {
    public interface IView {
        bool IsReady { get; }
        event EventHandler Ready;
        bool IsVisible { get; set; }
        void OnHide();
        void OnClear();
    }
    [ContentProperty("Content")]
    public class ViewPresenter : Control {
        #region Dependency Properties
        public static readonly DependencyProperty ContentProperty;
        public static readonly DependencyProperty DefaultStoryboardProperty;
        public static readonly DependencyProperty StoryboardProperty;
        public static readonly DependencyProperty StoryboardSelectorProperty;
        public static readonly DependencyProperty OldContentProperty;
        public static readonly DependencyProperty NewContentProperty;
        public static readonly DependencyProperty OldContentTranslateXProperty;
        public static readonly DependencyProperty NewContentTranslateXProperty;
        static ViewPresenter() {
            Type ownerType = typeof(ViewPresenter);
            ContentProperty = DependencyProperty.Register("Content", typeof(object), ownerType, new PropertyMetadata(null, RaiseContentChanged));
            DefaultStoryboardProperty = DevExpress.RealtorWorld.Xpf.Helpers.StoryboardProperty.Register("DefaultStoryboard", ownerType, null);
            StoryboardProperty = DependencyProperty.Register("Storyboard", typeof(string), ownerType, new PropertyMetadata(string.Empty));
            StoryboardSelectorProperty = DependencyProperty.Register("StoryboardSelector", typeof(Func<object, string>), ownerType, new PropertyMetadata(null));
            OldContentProperty = DependencyProperty.Register("OldContent", typeof(ContentPresenter), ownerType, new PropertyMetadata(null));
            NewContentProperty = DependencyProperty.Register("NewContent", typeof(ContentPresenter), ownerType, new PropertyMetadata(null));
            OldContentTranslateXProperty = DependencyProperty.Register("OldContentTranslateX", typeof(double), ownerType, new PropertyMetadata(0.0, RaiseOldContentTranslateXChanged));
            NewContentTranslateXProperty = DependencyProperty.Register("NewContentTranslateX", typeof(double), ownerType, new PropertyMetadata(0.0, RaiseNewContentTranslateXChanged));
        }
        static void RaiseContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ViewPresenter)d).RaiseContentChanged(e.OldValue, e.NewValue);
        }
        static void RaiseOldContentTranslateXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ViewPresenter)d).RaiseOldContentTranslateXChanged();
        }
        static void RaiseNewContentTranslateXChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((ViewPresenter)d).RaiseNewContentTranslateXChanged();
        }
        #endregion
        bool animationInProgress = false;
        Grid grid;
        Storyboard storyboard;
        bool contentChanged = false;
        ContentPresenter root;

        public ViewPresenter() {
            this.DefaultStyleKey = typeof(ViewPresenter);
            SizeChanged += OnSizeChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        public object Content { get { return GetValue(ContentProperty); } set { SetValue(ContentProperty, value); } }
        public Storyboard DefaultStoryboard { get { return (Storyboard)GetValue(DefaultStoryboardProperty); } set { SetValue(DefaultStoryboardProperty, value); } }
        public string Storyboard { get { return (string)GetValue(StoryboardProperty); } set { SetValue(StoryboardProperty, value); } }
        public Func<object, string> StoryboardSelector { get { return (Func<object, string>)GetValue(StoryboardSelectorProperty); } set { SetValue(StoryboardSelectorProperty, value); } }
        public ContentPresenter OldContent { get { return (ContentPresenter)GetValue(OldContentProperty); } private set { SetValue(OldContentProperty, value); } }
        public ContentPresenter NewContent { get { return (ContentPresenter)GetValue(NewContentProperty); } private set { SetValue(NewContentProperty, value); } }
        public double OldContentTranslateX { get { return (double)GetValue(OldContentTranslateXProperty); } set { SetValue(OldContentTranslateXProperty, value); } }
        public double NewContentTranslateX { get { return (double)GetValue(NewContentTranslateXProperty); } set { SetValue(NewContentTranslateXProperty, value); } }
        TranslateTransform OldContentTranslate { get { return ((TransformGroup)OldContent.RenderTransform).Children[1] as TranslateTransform; } }
        TranslateTransform NewContentTranslate { get { return ((TransformGroup)NewContent.RenderTransform).Children[1] as TranslateTransform; } }
        protected virtual void SubscribeToReady(object view, EventHandler handler) {
            IView v = view as IView;
            if(v != null)
                v.Ready += handler;
        }
        protected virtual void UnsubscribeFromReady(object view, EventHandler handler) {
            IView v = view as IView;
            if(v != null)
                v.Ready -= handler;
        }
        protected virtual bool IsReady(object view) {
            IView v = view as IView;
            return v == null ? true : v.IsReady;
        }
        protected virtual void SetIsVisible(object view, bool value) {
            IView v = view as IView;
            if(v != null)
                v.IsVisible = value;
        }
        protected virtual void OnHide(object view) {
            IView v = view as IView;
            if(v != null)
                v.OnHide();
        }
        protected virtual void OnClear(object view) {
            IView v = view as IView;
            if(v != null)
                v.OnClear();
        }
        protected virtual void OnLoaded(object sender, RoutedEventArgs e) {
            BuildVisualTree();
        }
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e) {
            ClearVisualTree();
        }
        void BuildVisualTree() {
            if(this.grid == null) {
                this.grid = new Grid();
                this.root.Content = this.grid;
            }
            if(this.grid.Children.Count == 0) {
                if(OldContent != null)
                    this.grid.Children.Add(OldContent);
                if(NewContent != null)
                    this.grid.Children.Add(NewContent);
            }
        }
        void ClearVisualTree() {
            if(this.root != null)
                this.root.Content = null;
            if(this.grid != null)
                this.grid.Children.Clear();
            this.grid = null;
        }
        void SetStoryboard(object content) {
            if(string.IsNullOrEmpty(Storyboard) && StoryboardSelector == null) {
                this.storyboard = DefaultStoryboard;
            } else {
                string name = string.IsNullOrEmpty(Storyboard) ? StoryboardSelector(content) : Storyboard;
                this.storyboard = Resources[name] as Storyboard;
            }
        }
        void RaiseContentChanged(object oldValue, object newValue) {
            if(this.animationInProgress) {
                this.contentChanged = true;
                return;
            }
            this.animationInProgress = true;
            if(OldContent != null && OldContent.Content != null) {
                SetIsVisible(OldContent.Content, false);
                OnHide(OldContent.Content);
            }
            NewContent = new ContentPresenter() { Content = newValue, RenderTransformOrigin = new Point(0.5, 0.5), Opacity = 0.0 };
            Canvas.SetZIndex(NewContent, 5);
            InitTransform(NewContent);
            if(this.grid != null)
                this.grid.Children.Add(NewContent);
            SetStoryboard(newValue);
            if(OldContent == null || this.storyboard == null) {
                NewContent.Opacity = 1.0;
                FinishContentChanging();
                return;
            }
            System.Windows.Media.Animation.Storyboard.SetTarget(this.storyboard, this);
            this.storyboard.Completed += OnStoryboardCompleted;
            SubscribeToReady(newValue, OnNewValueReady);
            if(IsReady(newValue))
                OnNewValueReady(newValue, null);
        }
        void OnStoryboardCompleted(object sender, EventArgs e) {
            NewContent.Opacity = 1.0;
            this.storyboard.Completed -= OnStoryboardCompleted;
            this.storyboard.Stop();
            this.storyboard = null;
            FinishContentChanging();
        }
        void OnNewValueReady(object sender, EventArgs e) {
            UnsubscribeFromReady(sender, OnNewValueReady);
            this.storyboard.Begin();
        }
        void FinishContentChanging() {
            if(OldContent != null) {
                if(this.grid != null)
                    this.grid.Children.Remove(OldContent);
                if(OldContent.Content != null)
                    OnClear(OldContent.Content);
                OldContent.Content = null;
                
            }
            OldContent = NewContent;
            NewContent = null;
            Canvas.SetZIndex(OldContent, 10);
            InitTransform(OldContent);
            if(OldContent != null && OldContent.Content != null)
                SetIsVisible(OldContent.Content, true);
            this.animationInProgress = false;
            if(this.contentChanged) {
                this.contentChanged = false;
                RaiseContentChanged(null, Content);
            }
        }
        void InitTransform(FrameworkElement fe) {
            TransformGroup transform = new TransformGroup();
            transform.Children.Add(new ScaleTransform());
            transform.Children.Add(new TranslateTransform());
            fe.RenderTransform = transform;
        }
        void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            Clip = new RectangleGeometry() { Rect = new Rect(0.0, 0.0, ActualWidth, ActualHeight) };
            if(OldContent != null)
                RaiseOldContentTranslateXChanged();
            if(NewContent != null)
                RaiseNewContentTranslateXChanged();
        }
        void RaiseOldContentTranslateXChanged() {
            if(!this.animationInProgress) return;
            OldContentTranslate.X = OldContentTranslateX * ActualWidth;
        }
        void RaiseNewContentTranslateXChanged() {
            if(!this.animationInProgress) return;
            NewContentTranslate.X = NewContentTranslateX * ActualWidth;
        }
        public override void OnApplyTemplate() {
            base.OnApplyTemplate();
            this.root = (ContentPresenter)GetTemplateChild("Root");
            BuildVisualTree();
        }
    }
}
