using System;
using System.Collections;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCross.Binding.Extensions;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using WB.Core.SharedKernels.Enumerator.Properties;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    public class FixedMvxFilteringAdapter
        : MvxAdapter, IFilterable
    {
        private readonly object _syncLock = new object();

        private class MyFilter : Filter
        {
            private readonly FixedMvxFilteringAdapter owner;

            public MyFilter(FixedMvxFilteringAdapter owner)
            {
                this.owner = owner;
            }

            #region Overrides of Filter

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                lock (owner._syncLock)
                {
                    var filteredValues = owner.FilterValues(constraint?.ToString());

                    return new FilterResults
                    {
                        Count = filteredValues?.Count() ?? 0,
                        Values = new MvxReplaceableJavaContainer { Object = filteredValues }
                    };
                }
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                if (results == null || results.Count == 0) return;
                if (!(results.Values is MvxReplaceableJavaContainer items)) return;

                lock (owner._syncLock)
                {
                    owner.FilteredItemsSource = items.Object as IEnumerable;
                    owner.NotifyDataSetChanged();
                }
            }

            #endregion Overrides of Filter
        }

        public Func<object, string, bool> DefaultFilterPredicate = (item, filterString) => string.IsNullOrEmpty(filterString) || item.ToString().ToLowerInvariant().Contains(filterString.ToLowerInvariant());
        public Func<object, string, bool> FilterPredicate { get; set; }

        protected virtual IEnumerable FilterValues(string constraint)
        {
            if (PartialText != constraint)
                PartialText = constraint;

            return ItemsSource.Filter(item => FilterPredicate(item, constraint));
        }

        public override bool AreAllItemsEnabled() => false;

        public override bool IsEnabled(int position) => !Loading;

        public override IEnumerable ItemsSource
        {
            get => base.ItemsSource;
            set
            {
                lock (_syncLock)
                {
                    FilteredItemsSource = value;
                }
                base.ItemsSource = value;
            }
        }

        private IEnumerable FilteredItemsSource { get; set; }
        private IEnumerable LoadingItemsSource => new[] { new {Title = UIResources.Loading} };

        public event EventHandler PartialTextChanged;

        private string partialText;
        public string PartialText
        {
            get => partialText;
            private set
            {
                partialText = value;
                FireConstraintChanged();
            }
        }

        private bool loading;
        public bool Loading
        {
            get => loading;
            set
            {
                if (loading != value)
                {
                    loading = value;
                    ItemsSource = value
                        ? LoadingItemsSource
                        : FilteredItemsSource;
                    NotifyDataSetChanged();
                }
            }
        }

        private void FireConstraintChanged()
        {
            var activity = Context as Activity;

            activity?.RunOnUiThread(() =>
            {
                PartialTextChanged?.Invoke(this, EventArgs.Empty);
            });
        }

        public FixedMvxFilteringAdapter(Context context) : this(context, MvxAndroidBindingContextHelpers.Current())
        {
        }

        public FixedMvxFilteringAdapter(Context context, IMvxAndroidBindingContext bindingContext) : base(context, bindingContext)
        {
            ReturnSingleObjectFromGetItem = true;
            FilterPredicate = DefaultFilterPredicate;
            Filter = new MyFilter(this);
        }

        protected FixedMvxFilteringAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        public bool ReturnSingleObjectFromGetItem { get; set; }

        private MvxReplaceableJavaContainer _javaContainer;

        public override Java.Lang.Object GetItem(int position)
        {
            // for autocomplete views we need to return something other than null here
            // - see @JonPryor's answer in http://stackoverflow.com/questions/13842864/why-does-the-gref-go-too-high-when-i-put-a-mvxbindablespinner-in-a-mvxbindableli/13995199#comment19319057_13995199
            // - and see problem report in https://github.com/slodge/MvvmCross/issues/145
            // - obviously this solution is not good for general Java code!
            if (ReturnSingleObjectFromGetItem)
            {
                if (_javaContainer == null)
                    _javaContainer = new MvxReplaceableJavaContainer();
                _javaContainer.Object = GetRawItem(position);
                return _javaContainer;
            }

            return base.GetItem(position);
        }

        public override object GetRawItem(int position)
        {
            lock (_syncLock)
            {
                var element = FilteredItemsSource?.ElementAt(position);
                return element;
            }
        }

        public override int GetPosition(object item)
        {
            lock (_syncLock)
            {
                var pos = FilteredItemsSource?.GetPosition(item) ?? 0;
                return pos;
            }
        }

        public override int Count
        {
            get
            {
                lock (_syncLock)
                {
                    return FilteredItemsSource?.Count() ?? 0;
                }
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            if (Loading)
                return this.GetBindableView(convertView, null, parent,
                    Resource.Layout.interview_question_filtered_single_option_loading);
            return base.GetView(position, convertView, parent);
        }

        #region Implementation of IFilterable

        public Filter Filter { get; set; }

        #endregion Implementation of IFilterable
    }
}
