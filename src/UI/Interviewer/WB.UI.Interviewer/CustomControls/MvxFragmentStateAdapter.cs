using Android.Content;
using Android.Runtime;
using AndroidX.ViewPager2.Adapter;
using MvvmCross.Platforms.Android.Views.Fragments;
using MvvmCross.ViewModels;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace WB.UI.Interviewer.CustomControls
{
    public class MvxFragmentStateAdapter : FragmentStateAdapter
    {
        private class ViewPagerItem
        {
            public Type Type { get; set; }
            public MvxFragment CachedFragment { get; set; }
            public MvxViewModel ViewModel { get; set; }
            public string TitlePropertyName { get; set; }
        }

        private readonly Context _context;
        private readonly List<ViewPagerItem> _fragments = new List<ViewPagerItem>();

        public MvxFragmentStateAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public MvxFragmentStateAdapter(Context context, AndroidX.Fragment.App.FragmentManager fm, 
            AndroidX.Lifecycle.Lifecycle lifecycle)
            : base(fm, lifecycle)
        {
            this._context = context;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var vm = sender as MvxViewModel;
            var viewPagerItem = this._fragments.FirstOrDefault(x => x.ViewModel == vm);
            var titlePropertyName = viewPagerItem?.TitlePropertyName;

            if (e.PropertyName == titlePropertyName)
                this.NotifyDataSetChanged();
        }

        public void InsertFragment(Type fragType, MvxViewModel model, string titlePropertyName, int position = -1)
        {
            if (position < 0 && this._fragments.Count == 0)
                position = 0;
            else if ((position < 0 || position > this._fragments.Count) && this._fragments.Count > 0)
                position = this._fragments.Count;

            this._fragments.Insert(position, new ViewPagerItem
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
            this._fragments[position].ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
            this._fragments.RemoveAt(position);

            this.NotifyDataSetChanged();
        }

        public void RemoveFragmentByViewModel(MvxViewModel viewModel)
            => this.RemoveFragment(this._fragments.FindIndex(x => x.ViewModel == viewModel));
        
        protected virtual string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public bool HasFragmentForViewModel(MvxViewModel viewModel) 
            => this._fragments.Any(x => x.ViewModel == viewModel);

        public void RemoveAllFragments()
        {
            for (var fragmentIndex = this._fragments.Count - 1; fragmentIndex >= 0; fragmentIndex--)
                this.RemoveFragment(fragmentIndex);
        }

        public override int ItemCount => this._fragments.Count; 
        public override Fragment CreateFragment(int position)
        {
            if (position < 0 || position > this._fragments.Count - 1) return null;
            
            var bundle = new Bundle();
            bundle.PutInt("number", position);
            
            var cachedFragment = this._fragments[position].CachedFragment;
            
            if (cachedFragment != null) return cachedFragment;
            
            cachedFragment = (MvxFragment)AndroidX.Fragment.App.Fragment.Instantiate(this._context,
                this.FragmentJavaName(this._fragments[position].Type), bundle);
            
            cachedFragment.ViewModel = this._fragments[position].ViewModel;
            
            this._fragments[position].CachedFragment = cachedFragment;
            
            return cachedFragment;
        }
    }
}
