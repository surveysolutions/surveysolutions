using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class StoryboardProperty {
        public static DependencyProperty Register(string name, Type ownerType, object defaultValue) {
            return Register(name, ownerType, defaultValue, null);
        }
        public static DependencyProperty Register(string name, Type ownerType, object defaultValue, PropertyChangedCallback propertyChangedCallback) {
            return DependencyProperty.Register(name, typeof(Storyboard), ownerType, new PropertyMetadata(defaultValue, propertyChangedCallback
                , CoerceStoryboard
            ));
        }
        static object CoerceStoryboard(DependencyObject d, object source) {
            Storyboard sb = (Storyboard)source;
            return sb == null ? null : sb.Clone();
        }
    }
}
