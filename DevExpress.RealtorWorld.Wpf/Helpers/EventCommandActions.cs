using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Input;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class CommandAction<T> : TriggerAction<T> where T : DependencyObject {
        #region Dependency Properties
        public static readonly DependencyProperty CommandProperty;
        public static readonly DependencyProperty CommandParameterProperty;
        static CommandAction() {
            Type ownerType = typeof(CommandAction<T>);
            CommandProperty = DependencyProperty.Register("Command", typeof(ICommand), ownerType, new PropertyMetadata(null));
            CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), ownerType, new PropertyMetadata(null));
        }
        #endregion
        public ICommand Command { get { return (ICommand)GetValue(CommandProperty); } set { SetValue(CommandProperty, value); } }
        public object CommandParameter { get { return GetValue(CommandParameterProperty); } set { SetValue(CommandParameterProperty, value); } }
        protected override void Invoke(object parameter) {
            if(Command != null && Command.CanExecute(CommandParameter)) {
                Command.Execute(CommandParameter);
            }
        }
    }
    public class KeyEventAction : CommandAction<UIElement> {
        public Key Key { get; set; }
        protected override void Invoke(object parameter) {
            KeyEventArgs e = (KeyEventArgs)parameter;
            if(Command != null && Command.CanExecute(CommandParameter) && e.Key == Key)
                Command.Execute(CommandParameter);
        }
    }
}
