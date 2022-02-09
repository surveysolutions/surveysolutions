using System;
using System.Collections;
using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Text;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using MvvmCross.Binding.Attributes;
using MvvmCross.Platforms.Android.Binding.Views;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    [Register("WB.UI.Shared.Enumerator.CustomControls.InstantAutoCompleteTextView")]
    public class InstantAutoCompleteTextView: AppCompatAutoCompleteTextView
    {
        private FixedMvxFilteringAdapter adapter => base.Adapter as FixedMvxFilteringAdapter;

        public InstantAutoCompleteTextView(Context context, IAttributeSet attrs)
            : this(context, attrs, new FixedMvxFilteringAdapter(context))
        {
        }

        public InstantAutoCompleteTextView(Context context, IAttributeSet attrs, FixedMvxFilteringAdapter adapter)
            : base(context, attrs)
        {
            adapter.ItemTemplateId = MvxAttributeHelpers.ReadListItemTemplateId(context, attrs);
            adapter.PartialTextChanged += this.AdapterOnPartialTextChanged;
            base.Adapter = adapter;
            
            this.ItemClick += this.OnItemClick;
            this.ItemSelected += OnItemSelected;

            this.EditorAction += this.OnEditorAction;
            this.Click += OnEditTextClick;
        }

        public bool Loading
        {
            get => adapter.Loading;
            set => adapter.Loading = value;
        }

        public void DisableDefaultSearch()
        {
            adapter.FilterPredicate = (o, s) => true;
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
            this.SelectedObjectChanged?.Invoke(this, this.adapter.GetRawItem(position));
            
            this.ClearFocus();
            this.HideKeyboard();
        }

        private void AdapterOnPartialTextChanged(object sender, EventArgs eventArgs)
            => this.PartialTextChanged?.Invoke(this, this.adapter.PartialText);

        [MvxSetToNullAfterBinding]
        public IEnumerable ItemsSource
        {
            get => this.adapter.ItemsSource;
            set =>this.adapter.ItemsSource = value;
        }

        public int ItemTemplateId
        {
            get => this.adapter.ItemTemplateId;
            set => this.adapter.ItemTemplateId = value;
        }

        public void SetText(string text)
        {
            this.SetText(text, true);
            this.SetSelection(text?.Length ?? 0);
            this.PerformFiltering(text, 0);
        }

        public event EventHandler<object> SelectedObjectChanged;
        public event EventHandler<string> PartialTextChanged;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.ItemClick -= this.OnItemClick;
                this.ItemSelected -= this.OnItemSelected;
                this.EditorAction -= this.OnEditorAction;
                this.Click -= this.OnEditTextClick;

                if (this.adapter != null)
                {
                    this.adapter.PartialTextChanged -= this.AdapterOnPartialTextChanged;
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
}
