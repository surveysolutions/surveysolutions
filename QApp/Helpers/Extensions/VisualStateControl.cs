using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DevExpress.Xpf.Core;

namespace QApp.Helpers.Extensions {
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Pressed", GroupName = "CommonStates")]
    public class VisualStateControl : ContentControl {
        bool isPressed;
        bool isMouseOver;

        public VisualStateControl() {
            FocusHelper2.SetFocusable(this, false);
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }
        protected override void OnMouseEnter(MouseEventArgs e) {
            base.OnMouseEnter(e);
            isMouseOver = true;
            GoToState(true);
        }
        protected override void OnMouseLeave(MouseEventArgs e) {
            base.OnMouseLeave(e);
            isPressed = false;
            isMouseOver = false;
            GoToState(true);
        }
        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonDown(e);
            isPressed = true;
            GoToState(true);
        }
        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e) {
            base.OnMouseLeftButtonUp(e);
            isPressed = false;
            GoToState(true);
        }
        protected virtual void OnLoaded(object sender, RoutedEventArgs e) {
            GoToStateCore(false);
        }
        protected virtual void OnUnloaded(object sender, RoutedEventArgs e) {
        }
        protected void GoToState(bool useTransitions) {
            if(IsLoaded)
                GoToStateCore(useTransitions);
        }
        protected virtual void GoToStateCore(bool useTransitions) {
            if(isPressed)
                VisualStateManager.GoToState(this, "Pressed", useTransitions);
            else if(isMouseOver)
                VisualStateManager.GoToState(this, "MouseOver", useTransitions);
            else
                VisualStateManager.GoToState(this, "Normal", useTransitions);
        }
    }
}
