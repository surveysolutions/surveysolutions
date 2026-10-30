using System.Runtime.CompilerServices;
using System.Threading;
using Android.Content;
using AndroidX.Fragment.App;
using AndroidX.ViewPager2.Adapter;
using MvvmCross.Platforms.Android.Views.Fragments;
using MvvmCross.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.Activities;
using Fragment = Android.App.Fragment;

namespace WB.UI.Shared.Enumerator.CustomControls;


public class TabsPagerAdapter(Context context, AndroidX.Fragment.App.FragmentManager fm, 
    AndroidX.Lifecycle.Lifecycle lifecycle, IList<TabViewModel> tabs) : FragmentStateAdapter(fm, lifecycle)
{
    private static readonly ConditionalWeakTable<TabViewModel, ItemId> ItemIds = new();
    private static long nextItemId;

    public override int ItemCount => tabs?.Count ?? 0;

    public override long GetItemId(int position) => GetItemId(tabs[position]);

    public override bool ContainsItem(long itemId) => tabs?.Any(tab => GetItemId(tab) == itemId) ?? false;

    public override AndroidX.Fragment.App.Fragment CreateFragment(int position)
    {
        if (position < 0 || position > tabs.Count - 1) 
            throw new ArgumentOutOfRangeException(nameof(position));
            
        var bundle = new Bundle();
        bundle.PutInt("number", position);

        var viewPagerItem = tabs[position];
            
        var fragment = (MvxFragment)AndroidX.Fragment.App.Fragment.Instantiate(context,
            this.FragmentJavaName(typeof(CompleteTabContentFragment)), bundle);
        fragment.ViewModel = viewPagerItem;

        return fragment;
    }
    
    protected virtual string FragmentJavaName(Type fragmentType) 
        => Java.Lang.Class.FromType(fragmentType).Name;

    private static long GetItemId(TabViewModel tab) =>
        ItemIds.GetValue(tab, _ => new ItemId(Interlocked.Increment(ref nextItemId))).Value;

    private sealed record ItemId(long Value);
}
