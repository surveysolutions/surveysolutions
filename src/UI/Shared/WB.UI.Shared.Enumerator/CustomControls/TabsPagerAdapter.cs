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
    public override int ItemCount => tabs.Count;

    public override AndroidX.Fragment.App.Fragment CreateFragment(int position)
    {
        if (position < 0 || position > tabs.Count - 1) return null;
            
        var bundle = new Bundle();
        bundle.PutInt("number", position);

        var viewPagerItem = tabs[position];
            
        var fragment = (MvxFragment)AndroidX.Fragment.App.Fragment.Instantiate(context,
            this.FragmentJavaName(typeof(CompleteTabContentFragment)), bundle);
        fragment.ViewModel = viewPagerItem;

        return fragment;
    }
    
    protected virtual string FragmentJavaName(Type fragmentType)
    {
        return "wb.ui.enumerator.activities.interview.CompleteTabContentFragment"; 
    }
}
