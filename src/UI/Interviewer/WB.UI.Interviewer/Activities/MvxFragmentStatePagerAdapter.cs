using System;
using System.Collections.Generic;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Java.Lang;
using MvvmCross.Core.ViewModels;
using MvvmCross.Droid.Support.V4;
using Object = Java.Lang.Object;

namespace WB.UI.Interviewer.Activities
{
    public class MvxFragmentStatePagerAdapter : FragmentStatePagerAdapter
    {
        private class ViewPagerItem
        {
            public Type Type { get; set; }
            public MvxFragment CachedFragment { get; set; }
            public MvxViewModel ViewModel { get; set; }
            public string TitlePropertyName { get; set; }
        }

        private readonly Context _context;
        private readonly ViewPager _viewPager;
        private readonly List<ViewPagerItem> _fragments = new List<ViewPagerItem>();

        public MvxFragmentStatePagerAdapter(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer) { }

        public MvxFragmentStatePagerAdapter(Context context, ViewPager pager, FragmentManager fm)
            : base(fm)
        {
            _context = context;
            _viewPager = pager;
        }

        public override Fragment GetItem(int position)
        {
            if (position < 0 || position > _fragments.Count - 1) return null;

            var bundle = new Bundle();
            bundle.PutInt("number", position);

            if (this._fragments[position].CachedFragment != null)
                return this._fragments[position].CachedFragment;
            
            _fragments[position].CachedFragment = (MvxFragment)Fragment.Instantiate(_context,
                FragmentJavaName(_fragments[position].Type), bundle);

            _fragments[position].CachedFragment.ViewModel = _fragments[position].ViewModel;
            _fragments[position].ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            return _fragments[position].CachedFragment;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            this.NotifyDataSetChanged();
        }

        public override int Count => _fragments.Count;

        public void AddFragment(Type fragType, MvxViewModel model, string titlePropertyName, int position = -1)
        {
            if (position < 0 && _fragments.Count == 0)
                position = 0;
            else if (position < 0 && _fragments.Count > 0)
                position = _fragments.Count;

            _fragments.Add(new ViewPagerItem
            {
                Type = fragType,
                ViewModel = model,
                TitlePropertyName = titlePropertyName
            });

            NotifyDataSetChanged();
        }

        public void RemoveFragment(int position)
        {
            if (_fragments[position].CachedFragment != null)
                DestroyItem(null, position, _fragments[position].CachedFragment);

            _fragments[position].ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
            _fragments.RemoveAt(position);

            NotifyDataSetChanged();

            _viewPager.SetCurrentItem(position - 1, true);
        }

        public void RemoveFragmentByViewModel(MvxViewModel viewModel)
            => this.RemoveFragment(this._fragments.FindIndex(x => x.ViewModel == viewModel));

        public override ICharSequence GetPageTitleFormatted(int position) => new Java.Lang.String(
            (string) _fragments[position].ViewModel.GetType().GetProperty(_fragments[position].TitlePropertyName)
                .GetValue(_fragments[position].ViewModel, null) ?? "");

        protected virtual string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public bool HasFragmentForViewModel(MvxViewModel viewModel) 
            => this._fragments.Any(x => x.ViewModel == viewModel);

        public override int GetItemPosition(Object @object) => PositionNone;
    }
}