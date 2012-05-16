using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;


namespace QApp.Helpers.Extensions
{
    public class ComboBoxDropDownCommand : ComboBox, ICommandSource
    {
        private static EventHandler canExecuteChangedHandler;

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command",
                                        typeof (ICommand), typeof (ComboBoxDropDownCommand),
                                        new PropertyMetadata((ICommand) null,
                                                             new PropertyChangedCallback(CommandChanged)));

        public ICommand Command
        {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }

        }

        public static readonly DependencyProperty CommandTargetProperty =
            DependencyProperty.Register("CommandTarget",
                                        typeof (IInputElement), typeof (ComboBoxDropDownCommand),
                                        new PropertyMetadata((IInputElement) null));

        public IInputElement CommandTarget
        {
            get { return (IInputElement) GetValue(CommandTargetProperty); }
            set { SetValue(CommandTargetProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter",
                                        typeof (object),
                                        typeof (ComboBoxDropDownCommand),
                                        new PropertyMetadata((object) null));

        public object CommandParameter
        {
            get { return (object) GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public ComboBoxDropDownCommand()
            : base()
        {
        }


        private static void CommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ComboBoxDropDownCommand cb = (ComboBoxDropDownCommand) d;
            cb.HookUpCommand((ICommand) e.OldValue, (ICommand) e.NewValue);
        }

        private void HookUpCommand(ICommand oldCommand, ICommand newCommand)
        {
            if (oldCommand != null)
                RemoveCommand(oldCommand, newCommand);
            AddCommand(oldCommand, newCommand);
        }

        private void RemoveCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = CanExecuteChanged;
            oldCommand.CanExecuteChanged -= handler;
        }

        private void AddCommand(ICommand oldCommand, ICommand newCommand)
        {
            EventHandler handler = new EventHandler(CanExecuteChanged);
            canExecuteChangedHandler = handler;
            if (newCommand != null)
                newCommand.CanExecuteChanged += canExecuteChangedHandler;
        }

        private void CanExecuteChanged(object sender, EventArgs e)
        {
            if (this.Command != null)
            {
                RoutedCommand command = this.Command as RoutedCommand;
                this.IsEnabled = command != null
                                     ? command.CanExecute(this.CommandParameter, this.CommandTarget)
                                     : Command.CanExecute(CommandParameter);
            }
        }

        protected override void OnSelectionChanged(SelectionChangedEventArgs e)
        {
            base.OnSelectionChanged(e);
            if (this.Command != null)
            {
                RoutedCommand command = this.Command as RoutedCommand;
                if (command != null)
                    command.Execute(this.CommandParameter, this.CommandTarget);
                else
                    if (this.CommandParameter != null)
                        ((ICommand)Command).Execute(CommandParameter);
                    else
                    {
                        var answer=(((ComboBox)(e.Source)).SelectionBoxItem) as RavenQuestionnaire.Core.Views.Answer.CompleteAnswerView;
                        if (answer!=null && string.IsNullOrEmpty(answer.ToString()) && !answer.Selected)
                            ((ICommand) Command).Execute(
                                ((System.Windows.Controls.Primitives.Selector) (e.Source)).SelectedValue);
                    }
            }
        }
    }
}