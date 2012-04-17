using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Charts;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class ChartAnimationHelper {
        #region Dependency Properties
        public static readonly DependencyProperty AnimateOnDataSourceChangedProperty;
        static ChartAnimationHelper() {
            Type ownerType = typeof(ChartAnimationHelper);
            AnimateOnDataSourceChangedProperty = DependencyProperty.RegisterAttached("AnimateOnDataSourceChanged", typeof(bool), ownerType, new PropertyMetadata(false, OnAnimateOnDataSourceChangedChanged));
        }
        #endregion
        public static bool GetAnimateOnDataSourceChanged(DependencyObject d) { return (bool)d.GetValue(AnimateOnDataSourceChangedProperty); }
        public static void SetAnimateOnDataSourceChanged(DependencyObject d, bool value) { d.SetValue(AnimateOnDataSourceChangedProperty, value); }
        static void OnAnimateOnDataSourceChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            ChartControl chart = d as ChartControl;
            if(chart == null) return;
            if((bool)e.NewValue) {
                chart.BoundDataChanged += OnBoundDataChanged;
                chart.EnableAnimation = true;
            } else {
                chart.BoundDataChanged -= OnBoundDataChanged;
            }
        }
        static void OnBoundDataChanged(object sender, RoutedEventArgs e) {
            ((ChartControl)sender).Animate();
        }
    }
}
