using System;
using System.Windows;
using System.Windows.Controls;
using DevExpress.Xpf.Grid;


namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class GridControlHelper {
        #region Dependency Properties
        public static readonly DependencyProperty ExpandAllGroupsProperty;
        static GridControlHelper() {
            Type ownerType = typeof(GridControlHelper);
            ExpandAllGroupsProperty = DependencyProperty.RegisterAttached("ExpandAllGroups", typeof(bool), ownerType, new PropertyMetadata(false, RaiseExpandAllGroupsChanged));
        }
        #endregion

        public static bool GetExpandAllGroups(GridControl grid) { return (bool)grid.GetValue(ExpandAllGroupsProperty); }
        public static void SetExpandAllGroups(GridControl grid, bool v) { grid.SetValue(ExpandAllGroupsProperty, v); }
        static void RaiseExpandAllGroupsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            GridControl grid = (GridControl)d;
            grid.EndGrouping += OnGridEndGrouping;
        }
        static void OnGridEndGrouping(object sender, RoutedEventArgs e) {
            GridControl grid = (GridControl)sender;
            grid.EndGrouping -= OnGridEndGrouping;
            if(GetExpandAllGroups(grid))
                grid.ExpandAllGroups();
        }
    }
}
