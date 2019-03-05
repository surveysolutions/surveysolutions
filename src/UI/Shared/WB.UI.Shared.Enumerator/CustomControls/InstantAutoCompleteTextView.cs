using System;
using System.Collections;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using MvvmCross.Binding.Attributes;
using MvvmCross.Binding.Extensions;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    [Register("WB.UI.Shared.Enumerator.CustomControls.InstantAutoCompleteTextView")]
    public class InstantAutoCompleteTextView: AppCompatAutoCompleteTextView
    {
        private readonly object adapterLock = new object();

        public InstantAutoCompleteTextView(Context context, IAttributeSet attrs)
            : this(context, attrs, new MvxFilteringAdapter(context))
        {
        }

        public InstantAutoCompleteTextView(Context context, IAttributeSet attrs, MvxFilteringAdapter adapter)
            : base(context, attrs)
        {
            var itemTemplateId = MvxAttributeHelpers.ReadListItemTemplateId(context, attrs);
            adapter.ItemTemplateId = itemTemplateId;

            //temporary fix for KP-12574
            //Updating datasource for control brakes initial logic

            Func<object, string, bool> filterPredicate = (Func<object, string, bool>)((item, filterString) => false);
            //((OptionWithSearchTerm) item).Title.ToLowerInvariant().Contains(filterString.ToLowerInvariant()));

            adapter.FilterPredicate = filterPredicate;

            this.Adapter = adapter;

            // note - we shouldn't realy need both of these... but we do (ask Roma)
            this.ItemClick += this.OnItemClick;
            this.ItemSelected += OnItemSelected;

            this.EditorAction += this.OnEditorAction;
            this.Click += OnEditTextClick;
        }

        protected InstantAutoCompleteTextView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        private void OnEditorAction(object sender, EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done)
                return;

            this.ClearFocus();
            this.DismissDropDown();
            this.HideKeyboard();
        }

        private void OnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
            => this.OnItemSelected(itemClickEventArgs.Position);

        private void OnItemSelected(object sender, AdapterView.ItemSelectedEventArgs itemSelectedEventArgs)
            => this.OnItemSelected(itemSelectedEventArgs.Position);

        protected virtual void OnItemSelected(int position)
        {
            lock (adapterLock)
                this.SelectedObject = this.Adapter.GetRawItem(position);
            
            this.ClearFocus();
            this.HideKeyboard();
        }

        public new MvxFilteringAdapter Adapter
        {
            get => base.Adapter as MvxFilteringAdapter;
            set
            {
                var existing = this.Adapter;
                if (existing == value)
                    return;

                if (existing != null)
                    existing.PartialTextChanged -= AdapterOnPartialTextChanged;

                if (existing != null && value != null)
                {
                    value.ItemsSource = existing.ItemsSource;
                    value.ItemTemplateId = existing.ItemTemplateId;
                }

                if (value != null)
                    value.PartialTextChanged += AdapterOnPartialTextChanged;

                base.Adapter = value;
            }
        }

        private void AdapterOnPartialTextChanged(object sender, EventArgs eventArgs) 
            => this.FireChanged(PartialTextChanged);

        [MvxSetToNullAfterBinding]
        public IEnumerable ItemsSource
        {
            get => this.Adapter.ItemsSource;
            set
            {
                lock (adapterLock)
                    this.Adapter.ItemsSource = value;
            }
        }

        public int ItemTemplateId
        {
            get => this.Adapter.ItemTemplateId;
            set => this.Adapter.ItemTemplateId = value;
        }

        public string PartialText
        {
            get => this.Adapter.PartialText;
            set
            {
                if (this.Adapter.PartialText == value) return;

                lock (adapterLock)
                {
                    var adapter = base.Adapter;
                    base.Adapter = null;
                    this.SetText(value, true);
                    base.Adapter = adapter;
                }
            }
    }

        private object selectedObject;
        public object SelectedObject
        {
            get => this.selectedObject;
            private set
            {
                this.selectedObject = value;
                this.FireChanged(SelectedObjectChanged);
            }
        }

        public event EventHandler SelectedObjectChanged;
        public event EventHandler PartialTextChanged;

        private void FireChanged(EventHandler eventHandler) => eventHandler?.Invoke(this, EventArgs.Empty);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ItemClick -= this.OnItemClick;
                this.ItemSelected -= this.OnItemSelected;
                this.EditorAction -= this.OnEditorAction;
                this.Click -= this.OnEditTextClick;

                if (this.Adapter != null)
                {
                    this.Adapter.PartialTextChanged -= this.AdapterOnPartialTextChanged;
                }
            }
            base.Dispose(disposing);
        }

        public override bool EnoughToFilter() => true;
        
        protected override void OnFocusChanged(bool gainFocus, FocusSearchDirection direction, Rect previouslyFocusedRect)
        {
            base.OnFocusChanged(gainFocus, direction, previouslyFocusedRect);
            this.ShowDropDownIfFocused();
        }

        private void OnEditTextClick(object sender, EventArgs e)
            => this.ShowDropDownIfFocused();

        private void ShowDropDownIfFocused()
        {
            if (this.EnoughToFilter() && this.IsFocused && this.WindowVisibility == ViewStates.Visible)
            {
                this.ShowDropDown();
            }
        }

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();

            this.ShowDropDownIfFocused();
        }

        private void HideKeyboard()
        {
            var ims = (InputMethodManager)this.Context.GetSystemService(Context.InputMethodService);
            ims.HideSoftInputFromWindow(this.WindowToken, 0);
        }
    }

    public class MvxFilteringAdapterC : MvxAdapter, IFilterable, IJavaObject, IDisposable
    {
        private object _syncLock = new object();
        public Func<object, string, bool> DefaultFilterPredicate = (Func<object, string, bool>)((item, filterString) => item.ToString().ToLowerInvariant().Contains(filterString.ToLowerInvariant()));
        private string _partialText;
        private MvxReplaceableJavaContainer _javaContainer;

        public Func<object, string, bool> FilterPredicate { get; set; }

        protected virtual ValueTuple<int, IEnumerable> FilterValues(string constraint)
        {
            if (this.PartialText == constraint)
                return new ValueTuple<int, IEnumerable>(-1, (IEnumerable)null);
            this.PartialText = constraint;
            IEnumerable enumerable = this.ItemsSource.Filter((Func<object, bool>)(item => this.FilterPredicate(item, constraint)));
            return new ValueTuple<int, IEnumerable>(enumerable.Count(), enumerable);
        }

        public override IEnumerable ItemsSource
        {
            get
            {
                return base.ItemsSource;
            }
            set
            {
                lock (this._syncLock)
                    this.FilteredItemsSource = value;
                base.ItemsSource = value;
            }
        }

        private IEnumerable FilteredItemsSource { get; set; }

        public event EventHandler PartialTextChanged;

        public string PartialText
        {
            get
            {
                return this._partialText;
            }
            private set
            {
                this._partialText = value;
                this.FireConstraintChanged();
            }
        }

        private void FireConstraintChanged()
        {
            (this.Context as Activity)?.RunOnUiThread((System.Action)(() =>
            {
                // ISSUE: reference to a compiler-generated field
                EventHandler partialTextChanged = this.PartialTextChanged;
                if (partialTextChanged == null)
                    return;
                partialTextChanged((object)this, EventArgs.Empty);
            }));
        }

        public MvxFilteringAdapterC(Context context)
          : this(context, MvxAndroidBindingContextHelpers.Current())
        {
        }

        public MvxFilteringAdapterC(Context context, IMvxAndroidBindingContext bindingContext)
          : base(context, bindingContext)
        {
            this.ReturnSingleObjectFromGetItem = true;
            this.FilterPredicate = this.DefaultFilterPredicate;
            this.Filter = (Filter)new MvxFilteringAdapterC.MyFilter(this);
        }

        protected MvxFilteringAdapterC(IntPtr javaReference, JniHandleOwnership transfer)
          : base(javaReference, transfer)
        {
        }

        public bool ReturnSingleObjectFromGetItem { get; set; }

        public override Java.Lang.Object GetItem(int position)
        {
            if (!this.ReturnSingleObjectFromGetItem)
                return base.GetItem(position);
            if (this._javaContainer == null)
                this._javaContainer = new MvxReplaceableJavaContainer();
            this._javaContainer.Object = this.GetRawItem(position);
            return (Java.Lang.Object)this._javaContainer;
        }

        public override object GetRawItem(int position)
        {
            lock (this._syncLock)
            {
                IEnumerable filteredItemsSource = this.FilteredItemsSource;
                return filteredItemsSource != null ? filteredItemsSource.ElementAt(position) : (object)null;
            }
        }

        public override int GetPosition(object item)
        {
            lock (this._syncLock)
            {
                IEnumerable filteredItemsSource = this.FilteredItemsSource;
                return filteredItemsSource != null ? filteredItemsSource.GetPosition(item) : 0;
            }
        }

        public override int Count
        {
            get
            {
                lock (this._syncLock)
                {
                    IEnumerable filteredItemsSource = this.FilteredItemsSource;
                    return filteredItemsSource != null ? filteredItemsSource.Count() : 0;
                }
            }
        }

        public Filter Filter { get; set; }

        private class MyFilter : Filter
        {
            private readonly MvxFilteringAdapterC _owner;

            public MyFilter(MvxFilteringAdapterC owner)
            {
                this._owner = owner;
            }

            protected override Filter.FilterResults PerformFiltering(ICharSequence constraint)
            {
                ValueTuple<int, IEnumerable> valueTuple = this._owner.FilterValues(constraint == null ? string.Empty : constraint.ToString());
                int num = valueTuple.Item1;
                IEnumerable enumerable = valueTuple.Item2;
                if (num == -1)
                    return new Filter.FilterResults();
                return new Filter.FilterResults()
                {
                    Count = num,
                    Values = (Java.Lang.Object)new MvxReplaceableJavaContainer()
                    {
                        Object = (object)enumerable
                    }
                };
            }

            protected override void PublishResults(ICharSequence constraint, Filter.FilterResults results)
            {
                if (results == null || results.Count <= 0)
                    return;
                MvxReplaceableJavaContainer values = results.Values as MvxReplaceableJavaContainer;
                if (values == null)
                    return;
                lock (this._owner._syncLock)
                {
                    this._owner.FilteredItemsSource = values.Object as IEnumerable;
                    this._owner.NotifyDataSetChanged();
                }
            }
        }
    }
}
