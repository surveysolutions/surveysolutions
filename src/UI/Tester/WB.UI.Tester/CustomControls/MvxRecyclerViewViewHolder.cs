using System;
using System.Windows.Input;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.MvvmCross.Binding.BindingContext;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;

namespace WB.UI.Tester.CustomControls
{
    public class MvxRecyclerViewViewHolder : RecyclerView.ViewHolder, IMvxRecyclerViewViewHolder
    {
        private object cachedDataContext;

        public MvxRecyclerViewViewHolder(View itemView, IMvxAndroidBindingContext bindingContext)
            : base(itemView)
        {
            this.bindingContext = bindingContext;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ClearAllBindings();
                this.cachedDataContext = null;
            }

            base.Dispose(disposing);
        }

        protected IMvxAndroidBindingContext AndroidBindingContext
        {
            get { return this.bindingContext; }
        }

        public object DataContext
        {
            get { return this.bindingContext.DataContext; }
            set { this.bindingContext.DataContext = value; }
        }

        public void OnAttachedToWindow()
        {
            if (this.cachedDataContext != null && this.DataContext == null)
                this.DataContext = this.cachedDataContext;
        }

        public void OnDetachedFromWindow()
        {
            this.cachedDataContext = this.DataContext;
            this.DataContext = null;
        }

        private readonly IMvxAndroidBindingContext bindingContext;
        public IMvxBindingContext BindingContext
        {
            get { return this.bindingContext; }
            set { throw new NotImplementedException("BindingContext is readonly in the list item"); }
        }

        private ICommand click;
        public ICommand Click
        {
            get { return this.click; }
            set { this.click = value; if (this.click != null) this.EnsureClickOverloaded(); }
        }

        private bool clickOverloaded = false;
        private void EnsureClickOverloaded()
        {
            if (this.clickOverloaded)
                return;
            this.clickOverloaded = true;
            this.ItemView.Click += (sender, args) => this.ExecuteCommandOnItem(this.Click);
        }

        private ICommand longClick;
        public ICommand LongClick
        {
            get { return this.longClick; }
            set { this.longClick = value; if (this.longClick != null) this.EnsureLongClickOverloaded(); }
        }

        private bool longClickOverloaded = false;
        private void EnsureLongClickOverloaded()
        {
            if (this.longClickOverloaded)
                return;
            this.longClickOverloaded = true;
            this.ItemView.LongClick += (sender, args) => this.ExecuteCommandOnItem(this.LongClick);
        }

        protected virtual void ExecuteCommandOnItem(ICommand command)
        {
            if (command == null)
                return;

            var item = this.DataContext;
            if (item == null)
                return;

            if (!command.CanExecute(item))
                return;

            command.Execute(item);
        }
    }
}