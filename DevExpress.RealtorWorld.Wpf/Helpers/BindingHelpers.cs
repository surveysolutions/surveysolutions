using System;
using System.Windows;
using DevExpress.Xpf.LayoutControl;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class TileSelectHelper {
        #region Dependency Properties
        public static readonly DependencyProperty IsSelectedProperty;
        public static readonly DependencyProperty SelectedValueProperty;
        public static readonly DependencyProperty ComparisonPropertyPathProperty;
        static TileSelectHelper() {
            Type ownerType = typeof(TileSelectHelper);
            IsSelectedProperty = DependencyProperty.RegisterAttached("IsSelected", typeof(bool), ownerType, new PropertyMetadata(false));
            SelectedValueProperty = DependencyProperty.RegisterAttached("SelectedValue", typeof(object), ownerType, new PropertyMetadata(null, OnValuablePropertyChanged));
            ComparisonPropertyPathProperty = DependencyProperty.RegisterAttached("ComparisonPropertyPath", typeof(string), ownerType, new PropertyMetadata(null, OnValuablePropertyChanged));
        }
        #endregion
        public static bool GetIsSelected(DependencyObject d) { return (bool)d.GetValue(IsSelectedProperty); }
        static void SetIsSelected(DependencyObject d, bool value) { d.SetValue(IsSelectedProperty, value); }
        public static object GetSelectedValue(DependencyObject d) { return d.GetValue(SelectedValueProperty); }
        public static void SetSelectedValue(DependencyObject d, object value) { d.SetValue(SelectedValueProperty, value); }
        public static string GetComparisonPropertyPath(DependencyObject d) { return (string)d.GetValue(ComparisonPropertyPathProperty); }
        public static void SetComparisonPropertyPath(DependencyObject d, string value) { d.SetValue(ComparisonPropertyPathProperty, value); }
        static void OnValuablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            Select(d as Tile, GetSelectedValue(d), GetComparisonPropertyPath(d));
        }
        static void Select(Tile tile, object selectedValue, string propertyPath) {
            if(tile == null || selectedValue == null) return;
            bool isSelected = selectedValue.Equals(string.IsNullOrEmpty(propertyPath) ? tile.Content : GetPropertyValue(tile.Content, propertyPath));
            SetIsSelected(tile, isSelected);
            if(isSelected) {
                tile.TryFindVisualParent<TileLayoutControl>().BringChildIntoView(tile, true);
            }
        }
        static object GetPropertyValue(object instance, string propertyPath) {
            string[] properties = propertyPath.Split('.');
            object currentValue = instance;
            foreach(string item in properties) {
                try {
                    currentValue = currentValue.GetType().GetProperty(item).GetValue(currentValue, null);
                } catch {
                    return null;
                }
            }
            return currentValue;
        }
    }
}
