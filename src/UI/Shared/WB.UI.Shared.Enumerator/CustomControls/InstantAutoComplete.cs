using System;
using System.Collections;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using MvvmCross.Binding.Attributes;
using MvvmCross.Binding.Droid.Views;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    [Register("WB.UI.Shared.Enumerator.CustomControls.InstantAutoCompleteTextView")]
    public class InstantAutoCompleteTextView: AppCompatAutoCompleteTextView
    {
        public InstantAutoCompleteTextView(Context context, IAttributeSet attrs)
            : this(context, attrs, new MvxFilteringAdapter(context))
        {
            // note - we shouldn't realy need both of these... but we do
            ItemClick += OnItemClick;
            ItemSelected += OnItemSelected;
        }

        public InstantAutoCompleteTextView(Context context, IAttributeSet attrs, MvxFilteringAdapter adapter)
            : base(context, attrs)
        {
            var itemTemplateId = MvxAttributeHelpers.ReadListItemTemplateId(context, attrs);
            adapter.ItemTemplateId = itemTemplateId;
            Adapter = adapter;
            ItemClick += OnItemClick;
            EditorAction += this.OnEditorAction;
        }

        protected InstantAutoCompleteTextView(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        private void OnEditorAction(object sender, EditorActionEventArgs e)
        {
            if (e.ActionId != ImeAction.Done)
                return;

            this.SelectedObject = null;
            this.PartialText = string.Empty;

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
            this.SelectedObject = Adapter.GetRawItem(position);
            
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
            get => Adapter.ItemsSource;
            set => this.Adapter.ItemsSource = value;
        }

        public int ItemTemplateId
        {
            get => Adapter.ItemTemplateId;
            set => this.Adapter.ItemTemplateId = value;
        }

        public string PartialText
        {
            get => this.Adapter.PartialText;
            set
            {
                if (this.Adapter.PartialText == value) return;

                var adapter = base.Adapter;
                base.Adapter = null;
                this.SetText(value, true);
                base.Adapter = adapter;
            }
    }

        private object _selectedObject;
        public object SelectedObject
        {
            get => _selectedObject;
            private set
            {
                if (_selectedObject == value)
                    return;

                _selectedObject = value;
                FireChanged(SelectedObjectChanged);
            }
        }

        public event EventHandler SelectedObjectChanged;
        public event EventHandler PartialTextChanged;

        private void FireChanged(EventHandler eventHandler) => eventHandler?.Invoke(this, EventArgs.Empty);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ItemClick -= this.OnItemClick;
                ItemSelected -= this.OnItemSelected;
                EditorAction -= this.OnEditorAction;

                if (Adapter != null)
                {
                    Adapter.PartialTextChanged -= AdapterOnPartialTextChanged;
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
}