using Android.Content;
using AndroidX.RecyclerView.Widget;
using AndroidX.ViewPager2.Adapter;
using MvvmCross.Platforms.Android.Views.Fragments;
using MvvmCross.ViewModels;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace WB.UI.Shared.Enumerator.CustomControls
{
    //Waiting for release of ViewPager2 v 1.1.x
    public class MvxFragmentStateAdapter : FragmentStateAdapter
    {
        private class ViewPagerItem
        {
            private static long counter;
            
            public ViewPagerItem()
            {
                this.Id = Interlocked.Increment(ref counter);
            }

            public long Id { get; set; }

            public Type Type { get; set; }
            public MvxViewModel ViewModel { get; set; }
            public string TitlePropertyName { get; set; }
        }

        private Context context;
        private List<ViewPagerItem> pagerItems {get; set;} = new List<ViewPagerItem>();
        
        public MvxFragmentStateAdapter(Context context, AndroidX.Fragment.App.FragmentManager fm, 
            AndroidX.Lifecycle.Lifecycle lifecycle)
            : base(fm, lifecycle)
        {
            this.context = context;
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var vm = sender as MvxViewModel;
            var viewPagerItem = this.pagerItems.FirstOrDefault(x => x.ViewModel == vm);
            var titlePropertyName = viewPagerItem?.TitlePropertyName;

            if (e.PropertyName == titlePropertyName)
                this.NotifyDataSetChanged();
        }

        public void InsertTab(Type fragType, MvxViewModel model, string titlePropertyName, int order)
        {
            var positionsToInsert = order > this.pagerItems.Count ? this.pagerItems.Count : order;
            
            model.PropertyChanged += this.ViewModel_PropertyChanged;
            
            this.pagerItems.Insert(positionsToInsert, new ViewPagerItem
            {
                Type = fragType,
                ViewModel = model,
                TitlePropertyName = titlePropertyName
            });
            NotifyDataSetChanged();
        }

        private void RemoveTab(int position)
        {
            this.pagerItems[position].ViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
            this.pagerItems.RemoveAt(position);
            
            NotifyDataSetChanged();
        }

        public void RemoveTabByViewModel(MvxViewModel viewModel)
            => this.RemoveTab(this.pagerItems.FindIndex(x => x.ViewModel == viewModel));
        
        protected virtual string FragmentJavaName(Type fragmentType)
        {
            var namespaceText = fragmentType.Namespace ?? "";
            if (namespaceText.Length > 0)
                namespaceText = namespaceText.ToLowerInvariant() + ".";
            return namespaceText + fragmentType.Name;
        }

        public bool HasFragmentForViewModel(MvxViewModel viewModel) 
            => this.pagerItems.Any(x => x.ViewModel == viewModel);

        public void RemoveAllFragments()
        {
            for (var fragmentIndex = this.pagerItems.Count - 1; fragmentIndex >= 0; fragmentIndex--)
                this.RemoveTab(fragmentIndex);
        }

        public override int ItemCount => this.pagerItems.Count; 
        public override Fragment CreateFragment(int position)
        {
            if (position < 0 || position > this.pagerItems.Count - 1) return null;
            
            var bundle = new Bundle();
            bundle.PutInt("number", position);

            var viewPagerItem = this.pagerItems[position];
            
            var fragment = (MvxFragment)AndroidX.Fragment.App.Fragment.Instantiate(this.context,
                this.FragmentJavaName(viewPagerItem.Type), bundle);
            fragment.ViewModel = viewPagerItem.ViewModel;

            return fragment;
        }
        
        public override long GetItemId(int position)
        {
            if (position < 0 || position >= pagerItems.Count)
            {
                return RecyclerView.NoId;
            }

            return pagerItems[position].Id;
        }

        public override bool ContainsItem(long itemId)
        {
            return pagerItems.Any(x => x.Id == itemId);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                context = null;
                pagerItems = null;
            }
            
            base.Dispose(disposing);
        }
    }
}
