using Android.Runtime;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.Platforms.Android.Views.Fragments;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    [Register("wb.ui.enumerator.activities.interview.CompleteInterviewFragment")]
    public class CompleteInterviewFragment : BaseFragment<CompleteInterviewViewModel>
    {
        protected override int ViewResourceId => Resource.Layout.interview_complete;

        /*public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            this.EnsureBindingContextIsSet(inflater);
            //base.OnCreateView(inflater, container, savedInstanceState);
            //var view = base.OnCreateView(inflater, container, savedInstanceState);
            var view = this.BindingInflate(ViewResourceId, container, false);
            
            var adapter = new CompleteInformationAdapter(Context, (IMvxAndroidBindingContext)base.BindingContext);
            //var expandableListView = view.FindViewById<MvxExpandableListView>(Resource.Id.tv_Complete_Groups_Information);
            var expandableListView = view.FindViewById<AutosizeExpandableListView>(Resource.Id.tv_Complete_Groups_Information);

            expandableListView.SetAdapter(adapter);

            return view;
        }*/
    }
}
