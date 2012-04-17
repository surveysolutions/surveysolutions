using System;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Media;
using DevExpress.Utils;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class WebBrowserCreator : Grid {
        static readonly TimeSpan Time = new TimeSpan(0, 0, 0, 0, 200);

        #region Dependency Properties
        public static readonly DependencyProperty SourceProperty;
        public static readonly DependencyProperty ShowBrowserProperty;
        static WebBrowserCreator() {
            Type ownerType = typeof(WebBrowserCreator);
            SourceProperty = DependencyProperty.Register("Source", typeof(Uri), ownerType, new PropertyMetadata(null, RaiseSourceChanged));
            ShowBrowserProperty = DependencyProperty.Register("ShowBrowser", typeof(bool), ownerType, new PropertyMetadata(false, RaiseShowBrowserChanged));
        }
        static void RaiseSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WebBrowserCreator)d).RaiseSourceChanged(e);
        }
        static void RaiseShowBrowserChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ((WebBrowserCreator)d).RaiseShowBrowserChanged(e);
        }
        #endregion

        WebBrowser browser;
        //Image loadingImage;

        public WebBrowserCreator() {
            Background = new SolidColorBrush(Colors.White);
            //this.loadingImage = new Image() { Stretch = Stretch.None, Source = ImageSourceHelper.GetImageSource(AssemblyHelper.GetResourceUri(typeof(WebBrowserCreator).Assembly, "Images/Loading.png")) };
            //Children.Add(this.loadingImage);
        }
        public Uri Source { get { return (Uri)GetValue(SourceProperty); } set { SetValue(SourceProperty, value); } }
        public bool ShowBrowser { get { return (bool)GetValue(ShowBrowserProperty); } set { SetValue(ShowBrowserProperty, value); } }
        void DoShowBrowser() {
            this.browser = new WebBrowser();
            this.browser.Visibility = Visibility.Collapsed;
            Children.Add(this.browser);
            this.browser.LoadCompleted += OnBrowserLoadCompleted;
            UpdateBrowserSource();
        }
        void DoHideBrowser() {
            Children.Remove(this.browser);
            this.browser = null;
        }
        void OnBrowserLoadCompleted(object sender, NavigationEventArgs e) {
            this.browser.LoadCompleted -= OnBrowserLoadCompleted;
            this.browser.Visibility = Visibility.Visible;
            //Children.Remove(this.loadingImage);
        }
        void RaiseSourceChanged(DependencyPropertyChangedEventArgs e) {
            UpdateBrowserSource();
        }
        void UpdateBrowserSource() {
            if(this.browser != null)
                this.browser.Source = Source;
        }
        void RaiseShowBrowserChanged(DependencyPropertyChangedEventArgs e) {
            bool newValue = (bool)e.NewValue;
            if(newValue)
                DoShowBrowser();
            else
                DoHideBrowser();
        }
    }
}
