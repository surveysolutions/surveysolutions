using System;
using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Android.Content;
using Android.Support.V7.Widget;
using Android.Views;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Exceptions;
using Cirrious.CrossCore.Platform;
using Cirrious.CrossCore.WeakSubscription;
using Cirrious.MvvmCross.Binding;
using Cirrious.MvvmCross.Binding.Attributes;
using Cirrious.MvvmCross.Binding.Droid.BindingContext;
using Cirrious.MvvmCross.Binding.ExtensionMethods;

namespace WB.UI.Tester.CustomControls
{
    public class MvxRecyclerViewAdapter : RecyclerView.Adapter, IMvxRecyclerViewAdapter
    {
        private readonly Context context;
        private readonly IMvxAndroidBindingContext bindingContext;
        private int itemTemplateId;
        private IEnumerable itemsSource;
        private IDisposable subscription;

        public MvxRecyclerViewAdapter(Context context)
            : this(context, MvxAndroidBindingContextHelpers.Current())
        { }

        public MvxRecyclerViewAdapter(Context context, IMvxAndroidBindingContext bindingContext)
        {
            this.context = context;
            this.bindingContext = bindingContext;

            if (this.bindingContext == null)
            {
                throw new MvxException(
                    "bindingContext is null during MvxAdapter creation - Adapter's should only be created when a specific binding context has been placed on the stack");
            }
        }

        protected Context Context
        {
            get { return this.context; }
        }

        protected IMvxAndroidBindingContext BindingContext
        {
            get { return this.bindingContext; }
        }

        public virtual int ItemTemplateId
        {
            get { return this.itemTemplateId; }
            set
            {
                if (this.itemTemplateId == value)
                    return;
                this.itemTemplateId = value;

                if (this.itemsSource != null)
                    this.NotifyDataSetChanged();
            }
        }

        public ICommand ItemClick { get; set; }
        public ICommand ItemLongClick { get; set; }

        [MvxSetToNullAfterBinding]
        public virtual IEnumerable ItemsSource
        {
            get { return this.itemsSource; }
            set { this.SetItemsSource(value); }
        }

        protected virtual void SetItemsSource(IEnumerable value)
        {
            if (Object.ReferenceEquals(this.itemsSource, value))
                return;

            if (this.subscription != null)
            {
                this.subscription.Dispose();
                this.subscription = null;
            }

            this.itemsSource = value;

            if (this.itemsSource != null && !(this.itemsSource is IList))
            {
                MvxBindingTrace.Trace(
                    MvxTraceLevel.Warning,
                    "Binding to IEnumerable rather than IList - this can be inefficient, especially for large lists");
            }

            var newObservable = this.itemsSource as INotifyCollectionChanged;
            if (newObservable != null)
                this.subscription = newObservable.WeakSubscribe(OnItemsSourceCollectionChanged);

            this.NotifyDataSetChanged();
        }

        protected virtual void OnItemsSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.NotifyDataSetChanged(e);
        }

        protected virtual void NotifyDataSetChanged(NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count == 1)
                        this.NotifyItemInserted(e.NewStartingIndex);
                    else
                        this.NotifyItemRangeInserted(e.NewStartingIndex, e.NewItems.Count);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    if (e.OldItems.Count == 1)
                        this.NotifyItemRemoved(e.OldStartingIndex);
                    else
                        this.NotifyItemRangeRemoved(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Move:
                    if (e.OldItems.Count == 1)
                        this.NotifyItemMoved(e.OldStartingIndex, e.NewStartingIndex);
                    else
                        this.NotifyDataSetChanged();
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.NotifyItemRangeChanged(e.OldStartingIndex, e.OldItems.Count);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    this.NotifyDataSetChanged();
                    break;
            }
        }

        public virtual new void NotifyDataSetChanged()
        {
            this.RealNotifyDataSetChanged();
        }

        protected virtual void RealNotifyDataSetChanged()
        {
            try
            {
                base.NotifyDataSetChanged();
            }
            catch (Exception exception)
            {
                Mvx.Warning("Exception masked during Adapter RealNotifyDataSetChanged {0}", exception.ToLongString());
            }
        }

        public override int ItemCount
        {
            get { return this.itemsSource.Count(); }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public virtual object GetRawItem(int position)
        {
            return this.itemsSource.ElementAt(position);
        }

        public virtual int GetPosition(object value)
        {
            return this.itemsSource.GetPosition(value);
        }

        public override void OnViewAttachedToWindow(Java.Lang.Object holder)
        {
            base.OnViewAttachedToWindow(holder);

            var viewHolder = (IMvxRecyclerViewViewHolder)holder;
            viewHolder.OnAttachedToWindow();
        }

        public override void OnViewDetachedFromWindow(Java.Lang.Object holder)
        {
            base.OnViewDetachedFromWindow(holder);

            var viewHolder = (IMvxRecyclerViewViewHolder)holder;
            viewHolder.OnDetachedFromWindow();
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            var bindingContext = this.CreateBindingContextForViewHolder();

            View view = this.InflateViewForHolder(parent, viewType, bindingContext);
            return new MvxRecyclerViewViewHolder(view, bindingContext)
            {
                Click = this.ItemClick,
                LongClick = this.ItemLongClick
            };
        }

        protected virtual IMvxAndroidBindingContext CreateBindingContextForViewHolder()
        {
            return new MvxAndroidBindingContext(this.context, this.bindingContext.LayoutInflater);
        }

        protected virtual View InflateViewForHolder(ViewGroup parent, int viewType, IMvxAndroidBindingContext bindingContext)
        {
            return bindingContext.BindingInflate(this.itemTemplateId, parent, false);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var source = this.GetRawItem(position);

            var mvxViewHolder = (MvxRecyclerViewViewHolder)holder;
            mvxViewHolder.DataContext = source;
        }
    }
}