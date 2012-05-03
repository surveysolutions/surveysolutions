using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public static class ListExtensions {
        public static List<T> Filter<T>(this IList list, Predicate<T> predicate) {
            return new List<T>(GetFilteredItems<T>(list, predicate));
        }
        public static T FindFirst<T>(this IList list, Predicate<T> predicate) {
            foreach(T item in list) {
                if(predicate(item)) {
                    return item;
                }
            }
            return default(T);
        }
        static IEnumerable<T> GetFilteredItems<T>(this IList list, Predicate<T> predicate) {
            foreach(T item in list) {
                if(predicate(item)) {
                    yield return item;
                }
            }
        }
    }
    public static class DependencyObjectExtensions {
        public static T TryFindVisualParent<T>(this DependencyObject d) where T : DependencyObject {
            DependencyObject parent = VisualTreeHelper.GetParent(d);
            return parent != null && (parent is ListBox || parent is T) ? (T)parent : parent.TryFindVisualParent<T>();
        }
    }
    public static class ReflectionHelper {
        public static PropertyInfo GetPublicProperty(Type type, string propertyName, Type propertyType = null) {
            return type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public, null, propertyType, new Type[] { }, null);
        }
    }
    public class FormatValue {
        public object Value { get; set; }
        public string Text { get; set; }
    }
}
