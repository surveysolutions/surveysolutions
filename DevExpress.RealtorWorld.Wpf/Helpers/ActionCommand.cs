using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Collections.ObjectModel;
using DevExpress.Xpf.Grid;
using System.Windows.Markup;

namespace DevExpress.RealtorWorld.Xpf.Helpers {
    public class ActionCommandBase : ICommand {
        bool allowExecute = true;

        public ActionCommandBase(Action<object> action, object owner) {
            Action = action;
            Owner = owner;
        }
        public bool AllowExecute {
            get { return allowExecute; }
            protected set {
                allowExecute = value;
                RaiseAllowExecuteChanged();
            }
        }
        public Action<object> Action { get; private set; }
        protected object Owner { get; private set; }
        public event EventHandler CanExecuteChanged;
        public bool CanExecute(object parameter) { return AllowExecute; }
        public void Execute(object parameter) {
            if(Action != null)
                Action(parameter);
        }
        void RaiseAllowExecuteChanged() {
            if(CanExecuteChanged != null)
                CanExecuteChanged(this, EventArgs.Empty);
        }
    }
    public class SimpleActionCommand : ActionCommandBase {
        public SimpleActionCommand(Action<object> action, object owner) : base(action, owner) {}
        public SimpleActionCommand(Action<object> action) : this(action, null) { }
    }
    public class ExtendedActionCommandBase : ActionCommandBase {
        string allowExecutePropertyName;
        PropertyInfo allowExecuteProperty;

        public ExtendedActionCommandBase(Action<object> action, INotifyPropertyChanged owner, string allowExecuteProperty)
            : base(action, owner) {
            this.allowExecutePropertyName = allowExecuteProperty;
            if(Owner != null) {
                this.allowExecuteProperty = ReflectionHelper.GetPublicProperty(Owner.GetType(), this.allowExecutePropertyName);
                if(this.allowExecuteProperty == null)
                    throw new ArgumentOutOfRangeException("allowExecuteProperty");
                ((INotifyPropertyChanged)Owner).PropertyChanged += OnOwnerPropertyChanged;
            }
        }
        protected virtual void UpdateAllowExecute() {
            AllowExecute = Owner == null ? true : (bool)this.allowExecuteProperty.GetValue(Owner, null);
        }
        void OnOwnerPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if(e.PropertyName == this.allowExecutePropertyName)
                UpdateAllowExecute();
        }
    }
    public class ActionCommand : ExtendedActionCommandBase {
        public ActionCommand(Action<object> action, INotifyPropertyChanged owner, string allowExecuteProperty)
            : base(action, owner, allowExecuteProperty) {
            UpdateAllowExecute();
        }
    }
    public class ExtendedActionCommand : ExtendedActionCommandBase {
        Func<object, bool> allowExecuteCallback;
        object id;

        public ExtendedActionCommand(Action<object> action, INotifyPropertyChanged owner, string allowExecuteProperty, Func<object, bool> allowExecuteCallback, object id)
            : base(action, owner, allowExecuteProperty) {
            this.allowExecuteCallback = allowExecuteCallback;
            this.id = id;
            UpdateAllowExecute();
        }
        protected override void UpdateAllowExecute() {
            AllowExecute = this.allowExecuteCallback == null ? true : this.allowExecuteCallback(this.id);
        }
    }
}
