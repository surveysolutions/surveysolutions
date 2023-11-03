using Android.Content;
using Android.Runtime;
using AndroidX.Fragment.App;
using Java.Lang;
using MvvmCross.Platforms.Android.Views.Fragments;
using MvvmCross.ViewModels;
using Object = Java.Lang.Object;

namespace WB.UI.Interviewer.CustomControls
{
    public class MvxFragmentStatePagerAdapter : FragmentStatePagerAdapter
    {
        public void Release()
        {
            this.context = null;
        }

        private class ViewPagerItem
        {
            public Type Type { get; set; }
            public MvxFragment CachedFragment { get; set; }
            public MvxViewModel ViewModel { get; set; }
            public string TitlePropertyName { get; set; }
        }

        private Context context;
        private readonly List<ViewPagerItem> fragments = new List<ViewPagerItem>();

        public MvxFragmentStatePagerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public MvxFragmentStatePagerAdapter(Context context, AndroidX.Fragment.App.FragmentManager fm)
            : base(fm)
        {
            this.context = context;
        }

        public override AndroidX.Fragment.App.Fragment GetItem(int position)
        {
            if (position < 0 || position > this.fragments.Count - 1) return null;

            var bundle = new Bundle();
            bundle.PutInt("number", position);

            var cachedFragment = this.fragments[position].CachedFragment;

            if (cachedFragment != null) return cachedFragment;

            cachedFragment = (MvxFragment)AndroidX.Fragment.App.Fragment.Instantiate(this.context,
                this.FragmentJavaName(this.fragments[position].Type), bundle);

            cachedFragment.ViewModel = this.fragments[position].ViewModel;

            this.fragments[position].CachedFragment = cachedFragment;

            return cachedFragment;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(context == null) return;
            
            var vm = sender as MvxViewModel;
            var viewPagerItem = this.fragments.FirstOrDefault(x => x.ViewModel == vm);
            var titlePropertyName = viewPagerItem?.TitlePropertyName;

            if (e.PropertyName == titlePropertyName)
                this.NotifyDataSetChanged();
        }

        public override int Count => this.fragments.Count;

        public void InsertFragment(Type fragType, MvxViewModel model, string titlePropertyName, int position = -1)
        {
            if (position < 0 && this.fragments.Count == 0)
                position = 0;
            else if ((position < 0 || position > this.fragments.Count) && this.fragments.Count > 0)
                position = this.fragments.Count;

            this.fragments.Insert(position, new ViewPagerItem
            {
                Type = fragType,
                ViewModel = model,
                TitlePropertyName = titlePropertyName
            });
            model.PropertyChanged += this.ViewModel_PropertyChanged;

            this.NotifyDataSetChanged();
        }

        public void RemoveFragment(int position)
        {
            this.fragments[position].ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
            this.fragments[position].ViewModel = null;
            //this.fragments[position].CachedFragment.Dispose();
            this.fragments[position].CachedFragment = null;
            this.fragments.RemoveAt(position);

            this.NotifyDataSetChanged();
        }

        public void RemoveFragmentByViewModel(MvxViewModel viewModel)
            => this.RemoveFragment(this.fragments.FindIndex(x => x.ViewModel == viewModel));

        public override ICharSequence GetPageTitleFormatted(int position) => new Java.Lang.String(
            (string) this.fragments[position].ViewModel.GetType().GetProperty(this.fragments[position].TitlePropertyName)
                .GetValue(this.fragments[position].ViewModel, null) ?? "");

        protected virtual string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public bool HasFragmentForViewModel(MvxViewModel viewModel) 
            => this.fragments.Any(x => x.ViewModel == viewModel);

        public override int GetItemPosition(Object @object) => PositionNone;

        public void RemoveAllFragments()
        {
            for (var fragmentIndex = this.fragments.Count - 1; fragmentIndex >= 0; fragmentIndex--)
                this.RemoveFragment(fragmentIndex);
        }
    }
}
