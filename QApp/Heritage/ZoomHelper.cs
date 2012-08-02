using System;
using System.Windows;
using System.Windows.Controls;

namespace QApp.Heritage {
    public static class ZoomHelper {
        #region Dependency Properties
        public static readonly DependencyProperty SupressZoomFactorProperty;
        public static readonly DependencyProperty ChangeOrientationZoomFactorProperty;

        static ZoomHelper() {
            Type ownerType = typeof(ZoomHelper);
            SupressZoomFactorProperty = DependencyProperty.RegisterAttached("SupressZoomFactor", typeof(int), ownerType, new PropertyMetadata(0));
            ChangeOrientationZoomFactorProperty = DependencyProperty.RegisterAttached("ChangeOrientationZoomFactor", typeof(int), ownerType, new PropertyMetadata(0));
        }

        #endregion

        public static int GetSupressZoomFactor(FrameworkElement fe) { return (int)fe.GetValue(SupressZoomFactorProperty); }
        public static void SetSupressZoomFactor(FrameworkElement fe, int v) { fe.SetValue(SupressZoomFactorProperty, v); }
        public static int GetChangeOrientationZoomFactor(StackPanel panel) { return (int)panel.GetValue(ChangeOrientationZoomFactorProperty); }
        public static void SetChangeOrientationZoomFactor(StackPanel panel, int v) { panel.SetValue(ChangeOrientationZoomFactorProperty, v); }
    }
}
