using System;
using System.Windows.Controls;

namespace QApp.Heritage {
    public class ModuleView : UserControl, IView {
        bool isVisible;

        public bool IsReady { get; private set; }
        public event EventHandler Ready;
        public event EventHandler Hide;
        public event EventHandler Clear;
        public new event EventHandler IsVisibleChanged;
        public new bool IsVisible {
            get { return isVisible; }
            set {
                if(isVisible == value) return;
                isVisible = value;
                if(IsVisibleChanged != null)
                    IsVisibleChanged(this, EventArgs.Empty);
            }
        }
        public void RaiseReady() {
            if(IsReady) return;
            IsReady = true;
            if(Ready != null)
                Ready(this, EventArgs.Empty);
        }
        public void OnHide() {
            if(Hide != null)
                Hide(this, EventArgs.Empty);
        }
        public void OnClear() {
            if(Clear != null)
                Clear(this, EventArgs.Empty);
        }
    }
}
