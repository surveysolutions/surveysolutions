using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WB.Tools.EventsMigrator
{
    public static class ListBoxBehaviour
    {
        public static readonly DependencyProperty AutoCopyProperty = DependencyProperty.RegisterAttached("AutoCopy",
            typeof(bool), typeof(ListBoxBehaviour), new UIPropertyMetadata(AutoCopyChanged));

        public static bool GetAutoCopy(DependencyObject obj_)
        {
            return (bool)obj_.GetValue(AutoCopyProperty);
        }

        public static void SetAutoCopy(DependencyObject obj_, bool value_)
        {
            obj_.SetValue(AutoCopyProperty, value_);
        }

        private static void AutoCopyChanged(DependencyObject obj_, DependencyPropertyChangedEventArgs e_)
        {
            var listBox = obj_ as ListBox;
            if (listBox != null)
            {
                if ((bool)e_.NewValue)
                {
                    ExecutedRoutedEventHandler handler =
                        (sender_, arg_) =>
                        {
                            if (listBox.SelectedItem != null)
                            {
                                //Copy what ever your want here
                                Clipboard.SetDataObject(listBox.SelectedItem.ToString());
                            }
                        };

                    var command = new RoutedCommand("Copy", typeof(ListBox));
                    command.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control, "Copy"));
                    listBox.CommandBindings.Add(new CommandBinding(command, handler));
                }
            }
        }
    }
}