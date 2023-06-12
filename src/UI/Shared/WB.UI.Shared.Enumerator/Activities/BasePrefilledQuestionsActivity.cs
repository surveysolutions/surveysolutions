using Android.OS;
using AndroidX.RecyclerView.Widget;
using MvvmCross.DroidX.RecyclerView;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.UI.Shared.Enumerator.CustomControls;

namespace WB.UI.Shared.Enumerator.Activities
{
    public abstract class BasePrefilledQuestionsActivity<TViewModel> : SingleInterviewActivity<TViewModel>
        where TViewModel : BasePrefilledQuestionsViewModel
    {
        protected override int ViewResourceId => Resource.Layout.prefilled_questions;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var listOfInterviewQuestionsAndGroups = this.FindViewById<MvxRecyclerView>(Resource.Id.interviewEntitiesList);
            var layoutManager = new LinearLayoutManager(this);
            
            listOfInterviewQuestionsAndGroups?.SetItemAnimator(null);
            
            listOfInterviewQuestionsAndGroups.SetLayoutManager(layoutManager);
            listOfInterviewQuestionsAndGroups.HasFixedSize = true;
            listOfInterviewQuestionsAndGroups.Adapter = new InterviewEntityAdapter((IMvxAndroidBindingContext)this.BindingContext);
        }
    }
}
