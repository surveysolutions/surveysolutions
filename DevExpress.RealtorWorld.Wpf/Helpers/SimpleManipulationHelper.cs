using System;
using System.Windows;
using System.Windows.Input;

using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using DevExpress.Xpf.Core.Native;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public interface ISimpleManupulationSupport {
        void ScrollBy(double x, double y, bool isMouseManipulation);
        void ScaleBy(double factor, bool isMouseManipulation);
        void FinishManipulation(bool isMouseManipulation);
        double DesiredDeseleration { get; }
    }
    public class SimpleManipulationHelper {
        Point lastPosition;
        FrameworkElement owner;
        bool manipulationInProgress;
        DependencyPropertyDescriptor isMouseManipulationEnabledDescriptor;

        public SimpleManipulationHelper(FrameworkElement owner) {
            this.owner = owner;
            PropertyDescriptor pd = TypeDescriptor.GetProperties(this.owner)["IsMouseManipulationEnabled"];
            this.isMouseManipulationEnabledDescriptor = pd == null ? null : DependencyPropertyDescriptor.FromProperty(pd);
            this.owner.Loaded += OnOwnerLoaded;
            this.owner.Unloaded += OnOwnerUnloaded;
            this.owner.ManipulationDelta += OnOwnerManipulationDelta;
            this.owner.ManipulationCompleted += OnOwnerManipulationCompleted;
            this.owner.ManipulationInertiaStarting += OnOwnerManipulationInertiaStarting;
        }
        void OnOwnerLoaded(object sender, RoutedEventArgs e) {
            if(this.isMouseManipulationEnabledDescriptor != null)
                this.isMouseManipulationEnabledDescriptor.AddValueChanged(this.owner, OnOwnerIsMouseManipulationEnabledChanged);
            OnOwnerIsMouseManipulationEnabledChanged(this.owner, EventArgs.Empty);
        }
        void OnOwnerUnloaded(object sender, RoutedEventArgs e) {
            if(this.isMouseManipulationEnabledDescriptor != null)
                this.isMouseManipulationEnabledDescriptor.RemoveValueChanged(this.owner, OnOwnerIsMouseManipulationEnabledChanged);
            UnsubscribeFromMouseEvents();
        }
        void OnOwnerIsMouseManipulationEnabledChanged(object sender, EventArgs e) {
            bool isManipulationEnabled = this.isMouseManipulationEnabledDescriptor == null ? false : (bool)this.isMouseManipulationEnabledDescriptor.GetValue(this.owner);
            if(isManipulationEnabled)
                SubscribeToMouseEvents();
            else
                UnsubscribeFromMouseEvents();
        }
        void SubscribeToMouseEvents() {
            this.owner.MouseLeftButtonDown += OnOwnerMouseLeftButtonDown;
            this.owner.MouseLeftButtonUp += OnOwnerMouseLeftButtonUp;
            this.owner.MouseMove += OnOwnerMouseMove;
            this.owner.MouseWheel += OnOwnerMouseWheel;
        }
        void UnsubscribeFromMouseEvents() {
            this.owner.MouseLeftButtonDown -= OnOwnerMouseLeftButtonDown;
            this.owner.MouseLeftButtonUp -= OnOwnerMouseLeftButtonUp;
            this.owner.MouseMove -= OnOwnerMouseMove;
            this.owner.MouseWheel -= OnOwnerMouseWheel;
        }
        void OnOwnerManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e) {
            ISimpleManupulationSupport sms = this.owner as ISimpleManupulationSupport;
            if(sms != null)
                e.TranslationBehavior.DesiredDeceleration = sms.DesiredDeseleration;
            e.Handled = true;
        }
        void OnOwnerManipulationCompleted(object sender, ManipulationCompletedEventArgs e) {
            e.Handled = true;
            ISimpleManupulationSupport sms = this.owner as ISimpleManupulationSupport;
            if(sms != null)
                sms.FinishManipulation(false);
            if(e.TotalManipulation.Translation.X == 0.0 && e.TotalManipulation.Translation.Y == 0.0 && e.TotalManipulation.Scale.X == 1.0 && e.TotalManipulation.Scale.Y == 1.0)
                RaiseClick();
        }
        void OnOwnerManipulationDelta(object sender, ManipulationDeltaEventArgs e) {
            ISimpleManupulationSupport sms = this.owner as ISimpleManupulationSupport;
            if(sms != null) {
                double sx = 1.0 + (e.DeltaManipulation.Scale.X - 1.0) / 1.0;
                double sy = 1.0 + (e.DeltaManipulation.Scale.Y - 1.0) / 1.0;
                double prec = 0.0005;
                bool b1 = Math.Abs(sx - 1.0) <= prec;
                bool b2 = Math.Abs(sy - 1.0) <= prec;
                if(!b1 || !b2)
                    sms.ScaleBy(sx, false);
                else
                    sms.ScrollBy(-e.DeltaManipulation.Translation.X, -e.DeltaManipulation.Translation.Y, false);
            }
            e.Handled = true;
        }
        void RaiseClick() {
            HitTestResult result = VisualTreeHelper.HitTest(this.owner, Mouse.GetPosition(this.owner));
            UIElement element = result == null ? null : result.VisualHit as UIElement;
            ButtonBase button = element == null ? null : LayoutHelper.FindParentObject<ButtonBase>(element);
            if(button == null) {
                if(element == null)
                    element = this.owner;
                MouseButtonEventArgs previewDown = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent, Source = this.owner };
                this.owner.RaiseEvent(previewDown);
                InputManager.Current.ProcessInput(previewDown);
                MouseButtonEventArgs down = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseDownEvent, Source = element };
                element.RaiseEvent(down);
                InputManager.Current.ProcessInput(down);
                MouseButtonEventArgs previewUp = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseUpEvent, Source = this.owner };
                this.owner.RaiseEvent(previewUp);
                InputManager.Current.ProcessInput(previewUp);
                MouseButtonEventArgs up = new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.MouseUpEvent, Source = element };
                element.RaiseEvent(up);
                InputManager.Current.ProcessInput(up);
            } else {
                RoutedEventArgs click = new RoutedEventArgs(ButtonBase.ClickEvent, button);
                button.RaiseEvent(click);
                ICommand command = button.Command;
                if(command != null && command.CanExecute(button.CommandParameter))
                    command.Execute(button.CommandParameter);
            }
        }
        void OnOwnerMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if(!this.owner.CaptureMouse()) return;
            e.Handled = true;
            this.lastPosition = e.GetPosition(this.owner);
            this.manipulationInProgress = true;
        }
        void OnOwnerMouseLeftButtonUp(object sender, MouseButtonEventArgs e) {
            if(!this.manipulationInProgress) return;
            e.Handled = true;
            this.manipulationInProgress = false;
            this.owner.ReleaseMouseCapture();
            ISimpleManupulationSupport sms = this.owner as ISimpleManupulationSupport;
            if(sms != null)
                sms.FinishManipulation(true);
        }
        void OnOwnerMouseMove(object sender, MouseEventArgs e) {
            if(!this.manipulationInProgress) return;
            Point newPosition = e.GetPosition(this.owner);
            ISimpleManupulationSupport sms = this.owner as ISimpleManupulationSupport;
            if(sms != null)
                sms.ScrollBy(this.lastPosition.X - newPosition.X, this.lastPosition.Y - newPosition.Y, true);
            this.lastPosition = newPosition;
        }
        void OnOwnerMouseWheel(object sender, MouseWheelEventArgs e) {
            if((Keyboard.Modifiers & ModifierKeys.Control) == 0) return;
            e.Handled = true;
            ISimpleManupulationSupport sms = this.owner as ISimpleManupulationSupport;
            if(sms != null)
                sms.ScaleBy((double)e.Delta, true);
        }
    }
}
