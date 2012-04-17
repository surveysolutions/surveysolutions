using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using DevExpress.Xpf.Core;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class WaitScreenHelperBase {
        public static WaitScreenHelperBase Current { get; set; }

        #region Dependency Properties
        public static readonly DependencyProperty IsWaitScreenProperty;
        static WaitScreenHelperBase() {
            Type ownerType = typeof(WaitScreenHelperBase);
            IsWaitScreenProperty = DependencyProperty.RegisterAttached("IsWaitScreen", typeof(bool), ownerType, new PropertyMetadata(false, RaiseIsWaitScreenChanged));
        }
        #endregion
        static Dictionary<FrameworkElement, bool> screens = new Dictionary<FrameworkElement, bool>();
        static int waitsCount = 0;

        public static bool GetIsWaitScreen(FrameworkElement fe) { return (bool)fe.GetValue(IsWaitScreenProperty); }
        public static void SetIsWaitScreen(FrameworkElement fe, bool value) { fe.SetValue(IsWaitScreenProperty, value); }
        public void DoInBackground(Action backgroundAction, Action mainThreadAction) {
            EnableWaitScreens();
            BackgroundHelper.DoInBackground(backgroundAction, () => {
                if(mainThreadAction != null)
                    mainThreadAction();
                DisableWaitScreens();
            });
        }
        public void EnableWaitScreens() {
            if(++waitsCount == 1)
                EnableWaitScreensCore();
        }
        public void DisableWaitScreens() {
            if(--waitsCount <= 0) {
                waitsCount = 0;
                DisableWaitScreensCore();
            }
        }
        protected virtual void EnableWaitScreensCore() {
            Mouse.OverrideCursor = Cursors.Wait;
            foreach(FrameworkElement screen in screens.Keys)
                EnableScreen(screen);
        }
        protected virtual void DisableWaitScreensCore() {
            foreach(FrameworkElement screen in screens.Keys)
                DisableScreen(screen);
            Mouse.OverrideCursor = null;
        }
        static void RaiseIsWaitScreenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            FrameworkElement fe = (FrameworkElement)d;
            bool newValue = (bool)e.NewValue;
            if(newValue) {
                DisableScreen(fe);
                screens.Add(fe, true);
            } else {
                screens.Remove(fe);
                EnableScreen(fe);
            }
        }
        static void EnableScreen(FrameworkElement screen) {
            screen.Visibility = Visibility.Visible;
        }
        static void DisableScreen(FrameworkElement screen) {
            screen.Visibility = Visibility.Collapsed;
        }
    }
    public class WaitScreenHelper<TSplashScreen> : WaitScreenHelperBase where TSplashScreen : Window, ISplashScreen, new() {
        protected override void EnableWaitScreensCore() {
            ShowSplashScreen();
            base.EnableWaitScreensCore();
        }
        protected override void DisableWaitScreensCore() {
            base.DisableWaitScreensCore();
            HideSplashScreen();
        }
        void ShowSplashScreen() {
            if(!DXSplashScreen.IsActive)
                DXSplashScreen.Show<TSplashScreen>();
        }
        void HideSplashScreen() {
            if(DXSplashScreen.IsActive)
                DXSplashScreen.Close();
        }
    }
}
