using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

using DevExpress.Utils;

#if DEBUGTEST && !REALTORWORLDDEMO
using DevExpress.DemoData.Helpers;
using DevExpress.Xpf.Core;
using DevExpress.Xpf.Core.Tests;
#endif

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public partial class StartupPage : Page {
        StartupBase startup;
        public StartupPage() : this(null) { }
        public StartupPage(StartupBase startup) {
            this.startup = startup;
            InitializeComponent();
            if(this.startup == null)
                this.startup = StartupBase.MainStartup;
            this.startup.RootVisual = this;
            SizeChanged += OnSizeChanged;
            SetBinding(MinWidthProperty, new Binding("MinWidth") { Source = this.startup, Mode = BindingMode.TwoWay });
            SetBinding(MinHeightProperty, new Binding("MinHeight") { Source = this.startup, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this.startup.MainElement, FrameworkElement.MinWidthProperty, new Binding("MainElementMinWidth") { Source = this.startup, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(this.startup.MainElement, FrameworkElement.MinHeightProperty, new Binding("MainElementMinHeight") { Source = this.startup, Mode = BindingMode.TwoWay });
            this.startup.TitleChanged += OnStartupTitleChanged;
            OnStartupTitleChanged(this.startup, new DependencyPropertyChangedEventArgs());
            Content = this.startup.MainElement;
        }
        void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            this.startup.Width = e.NewSize.Width;
            this.startup.Height = e.NewSize.Height;
        }
        void OnStartupTitleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            WindowTitle = this.startup.Title;
        }
    }
    public partial class StartupUserControl : UserControl {
        StartupBase startup;
        public StartupUserControl() : this(null) { }
        public StartupUserControl(StartupBase startup) {
            this.startup = startup;
            if(this.startup == null)
                this.startup = StartupBase.MainStartup;
            this.startup.RootVisual = this;
            SizeChanged += OnSizeChanged;
            SetBinding(MinWidthProperty, new Binding("MinWidth") { Source = this.startup, Mode = BindingMode.TwoWay });
            SetBinding(MinHeightProperty, new Binding("MinHeight") { Source = this.startup, Mode = BindingMode.TwoWay });
            if(this.startup.MainElement != null) {
                BindingOperations.SetBinding(this.startup.MainElement, FrameworkElement.MinWidthProperty, new Binding("MainElementMinWidth") { Source = this.startup, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(this.startup.MainElement, FrameworkElement.MinHeightProperty, new Binding("MainElementMinHeight") { Source = this.startup, Mode = BindingMode.TwoWay });
            }
            this.startup.TitleChanged += OnStartupTitleChanged;
            OnStartupTitleChanged(this.startup, new DependencyPropertyChangedEventArgs());
            Content = this.startup.MainElement;
        }
        void OnSizeChanged(object sender, SizeChangedEventArgs e) {
            this.startup.Width = e.NewSize.Width;
            this.startup.Height = e.NewSize.Height;
        }
        void OnStartupTitleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            if(this.startup.Page != null)
                this.startup.Page.WindowTitle = this.startup.Title;
        }
    }
    public class StartupBase : DependencyObject {
        #region Dependency Properties
        public static readonly DependencyProperty ExitAtRequestProperty;
        public static readonly DependencyProperty TitleProperty;
        public static readonly DependencyProperty IconProperty;
        public static readonly DependencyProperty WidthProperty;
        public static readonly DependencyProperty HeightProperty;
        public static readonly DependencyProperty MinWidthProperty;
        public static readonly DependencyProperty MinHeightProperty;
        public static readonly DependencyProperty MainElementMinWidthProperty;
        public static readonly DependencyProperty MainElementMinHeightProperty;
        public static readonly DependencyProperty BookmarkProperty;
        static StartupBase() {
            RegisterPackScheme();
            Type ownerType = typeof(StartupBase);
            ExitAtRequestProperty = DependencyProperty.Register("ExitAtRequest", typeof(bool), ownerType, new PropertyMetadata(false));
            TitleProperty = DependencyProperty.Register("Title", typeof(string), ownerType, new PropertyMetadata(string.Empty,
                (d, e) => ((StartupBase)d).RaiseTitleChanged(e)));
            IconProperty = DependencyProperty.Register("Icon", typeof(ImageSource), ownerType, new PropertyMetadata(null));
            WidthProperty = DependencyProperty.Register("Width", typeof(double), ownerType, new PropertyMetadata(double.NaN));
            HeightProperty = DependencyProperty.Register("Height", typeof(double), ownerType, new PropertyMetadata(double.NaN));
            MinWidthProperty = DependencyProperty.Register("MinWidth", typeof(double), ownerType, new PropertyMetadata(0.0));
            MinHeightProperty = DependencyProperty.Register("MinHeight", typeof(double), ownerType, new PropertyMetadata(0.0));
            MainElementMinWidthProperty = DependencyProperty.Register("MainElementMinWidth", typeof(double), ownerType, new PropertyMetadata(0.0));
            MainElementMinHeightProperty = DependencyProperty.Register("MainElementMinHeight", typeof(double), ownerType, new PropertyMetadata(0.0));
            BookmarkProperty = DependencyProperty.Register("Bookmark", typeof(string), ownerType, new PropertyMetadata(string.Empty,
                (d, e) => ((StartupBase)d).RaiseBookmarkChanged(e)));
        }
        #endregion

        bool isBrowserHosted;
        bool exit = false;
        FrameworkElement rootVisual;
        StartupUserControl startupUserControl;

        public static StartupType Run<StartupType>(Application app) where StartupType : StartupBase, new() {
            return Run<StartupType>(app, true, true, null, null);
        }
        public static StartupType Run<StartupType>(Application app, bool run, bool isMain, UIElement page, object userData) where StartupType : StartupBase, new() {
            StartupType startup = new StartupType();
            startup.UserData = userData;
            startup.IsMain = isMain;
            startup.Page = page as Page;
            startup.Run(app, run);
            return startup;
        }
        public StartupBase() {
            SaveNavigationHistory = false;
            this.isBrowserHosted = BrowserInteropHelper.IsBrowserHosted;
            ExitRequested += OnExitRequested;
        }
        public bool ExitAtRequest { get { return (bool)GetValue(ExitAtRequestProperty); } set { SetValue(ExitAtRequestProperty, value); } }
        public string Title { get { return (string)GetValue(TitleProperty); } set { SetValue(TitleProperty, value); } }
        public ImageSource Icon { get { return (ImageSource)GetValue(IconProperty); } set { SetValue(IconProperty, value); } }
        public double Width { get { return (double)GetValue(WidthProperty); } set { SetValue(WidthProperty, value); } }
        public double Height { get { return (double)GetValue(HeightProperty); } set { SetValue(HeightProperty, value); } }
        public double MinWidth { get { return (double)GetValue(MinWidthProperty); } set { SetValue(MinWidthProperty, value); } }
        public double MinHeight { get { return (double)GetValue(MinHeightProperty); } set { SetValue(MinHeightProperty, value); } }
        public double MainElementMinWidth { get { return (double)GetValue(MainElementMinWidthProperty); } set { SetValue(MainElementMinWidthProperty, value); } }
        public double MainElementMinHeight { get { return (double)GetValue(MainElementMinHeightProperty); } set { SetValue(MainElementMinHeightProperty, value); } }
        public string Bookmark { get { return (string)GetValue(BookmarkProperty); } set { SetValue(BookmarkProperty, value); } }
        public bool SaveNavigationHistory { get; set; }
        public string[] Args { get; private set; }
        public object UserData { get; set; }
        public bool IsMain { get; private set; }
        public Application Application { get; private set; }
        public UIElement MainElement { get; private set; }
        public FrameworkElement RootVisual {
            get { return rootVisual; }
            internal set {
                if(rootVisual != null)
                    rootVisual.Loaded -= OnRootVisualLoaded;
                rootVisual = value;
                if(rootVisual != null)
                    rootVisual.Loaded += OnRootVisualLoaded;
            }
        }
        internal Page Page { get; set; }
        public event DependencyPropertyChangedEventHandler TitleChanged;
        public event EventHandler ExitRequested;
        public event DependencyPropertyChangedEventHandler BookmarkChanged;
        public void DoExit() {
            this.exit = true;
            if(!this.isBrowserHosted) {
                Window mainWindow = RootVisual as Window;
                if(mainWindow != null)
                    mainWindow.Close();
            }
        }
        protected virtual ResourceDictionary GetCommonResources() { return null; }
        protected virtual UIElement CreateMainElement() { return null; }
        protected virtual void OnExitRequested(object sender, EventArgs e) { }
        protected virtual bool DoStartup() {
            Args = GetArgs();
            return true;
        }
        protected virtual Window CreateMainWindow() { return new Window(); }
        protected virtual Application CreateApplication(Application app) {
            app = app == null ? new Application() : app;
            return app;
        }
        protected virtual void RaiseExitRequested() {
            if(ExitAtRequest) {
                DoExit();
            } else {
                if(ExitRequested != null)
                    ExitRequested(this, EventArgs.Empty);
            }
        }
        protected virtual void OnRootVisualLoaded(object sender, RoutedEventArgs e) { }
        internal static StartupBase MainStartup { get; private set; }
        void Run(Application app, bool run) {
            Application = IsMain ? CreateApplication(app) : app;
            if(Application != null)
                Application.Navigated += OnApplicationNavigated;
            if(!run) {
            if(!DoRun(false)) return;
                return;
            }            
            if(!DoStartup()) {
                if(IsMain && !this.isBrowserHosted && Application != null)
                    Application.Shutdown();
                return;
            }
            if(this.isBrowserHosted) {
                MainStartup = this;
                MainElement = CreateMainElement();
                if(Application != null)
                    Application.StartupUri = AssemblyHelper.GetResourceUri(typeof(StartupPage).Assembly, "Helpers/StartupBase.xaml");
            } else {
                Window mainWindow = CreateMainWindow();
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                mainWindow.SetBinding(Window.TitleProperty, new Binding("Title") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.SetBinding(Window.IconProperty, new Binding("Icon") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.SetBinding(Window.MinWidthProperty, new Binding("MinWidth") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.SetBinding(Window.MinHeightProperty, new Binding("MinHeight") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.SetBinding(Window.WidthProperty, new Binding("Width") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.SetBinding(Window.HeightProperty, new Binding("Height") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.Closing += OnMainWindowClosing;
                mainWindow.WindowState = WindowState.Maximized;
                RootVisual = mainWindow;
                MainElement = CreateMainElement();
                mainWindow.Content = MainElement;
                BindingOperations.SetBinding(MainElement, FrameworkElement.MinWidthProperty, new Binding("MainElementMinWidth") { Source = this, Mode = BindingMode.TwoWay });
                BindingOperations.SetBinding(MainElement, FrameworkElement.MinHeightProperty, new Binding("MainElementMinHeight") { Source = this, Mode = BindingMode.TwoWay });
                mainWindow.Show();
                if(IsMain && Application != null)
                    Application.Run();
            }
        }
        bool DoRun(bool assignRootVisual) {
            if(this.startupUserControl == null) {
                if(!DoStartup()) return false;
                MainElement = CreateMainElement();
                this.startupUserControl = new StartupUserControl(this);
            }
            return true;
        }
        void RaiseTitleChanged(DependencyPropertyChangedEventArgs e) {
            if(TitleChanged != null)
                TitleChanged(this, e);
        }
        void RaiseBookmarkChanged(DependencyPropertyChangedEventArgs e) {
            if(BookmarkChanged != null)
                BookmarkChanged(this, e);
        }
        string[] GetArgs() {
            if(BrowserInteropHelper.IsBrowserHosted) return new string[] { };
            string[] environmentArgs = Environment.GetCommandLineArgs();
            string[] args = new string[environmentArgs.Length - 1];
            for(int i = 0; i < args.Length; ++i)
                args[i] = environmentArgs[i + 1];
            return args;
        }
        void OnMainWindowClosing(object sender, CancelEventArgs e) {
            if(!this.exit) {
                e.Cancel = true;
                Dispatcher.BeginInvoke((Action)RaiseExitRequested, DispatcherPriority.Render);
            }
        }
        void OnApplicationNavigated(object sender, NavigationEventArgs e) {
            if(BrowserInteropHelper.IsBrowserHosted) {
                NavigationWindow navigationWindow = e.Navigator as NavigationWindow;
                if(navigationWindow != null)
                    navigationWindow.ShowsNavigationUI = false;
            }
        }
        static void RegisterPackScheme() {
            new System.Windows.Documents.FlowDocument();
        }
    }
}
