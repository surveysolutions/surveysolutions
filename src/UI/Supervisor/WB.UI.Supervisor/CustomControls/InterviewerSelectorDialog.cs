using System;
using Android.App;
using Android.Support.V7.App;
using Android.Views;
using MvvmCross.Base;
using MvvmCross.Droid.Support.V7.RecyclerView;
using MvvmCross.Platforms.Android;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Views;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard;
using WB.Core.BoundedContexts.Supervisor.ViewModel.Dashboard.Services;
using WB.Core.SharedKernels.Enumerator.Services;
using WB.Core.SharedKernels.Enumerator.Views;
using WB.UI.Shared.Enumerator.Activities.Dashboard;

namespace WB.UI.Supervisor.CustomControls
{
    public class InterviewerSelectorDialog : IInterviewerSelectorDialog
    {
        private readonly IInterviewViewModelFactory viewModelFactory;
        private readonly IMvxAndroidCurrentTopActivity topActivity;
        private MvxAndroidBindingContext modalDialogBindingContext;

        private Dialog modalDialog;
        private InterviewerSelectorDialogViewModel viewModel;

        public InterviewerSelectorDialog(IInterviewViewModelFactory viewModelFactory,
            IMvxAndroidCurrentTopActivity topActivity)
        {
            this.viewModelFactory = viewModelFactory;
            this.topActivity = topActivity;
        }


        public event EventHandler OnCanelSelection;
        public event EventHandler OnSelected;

        public void SelectInterviewer(AssignmentDocument assignment)
        {
            this.viewModel = this.viewModelFactory.GetNew<InterviewerSelectorDialogViewModel>();
            this.viewModel.Init();
            this.viewModel.Title = string.Format("Select responsible for assignment #{0}", assignment.Id);
            this.viewModel.OnCancel += ViewModel_OnCancel;
            this.viewModel.OnDone += ViewModel_OnDone;

            var parentActivity = this.topActivity.Activity;

            //keep ref to context not to be collected by GC
            this.modalDialogBindingContext = new MvxAndroidBindingContext(parentActivity,
                new MvxSimpleLayoutInflaterHolder(parentActivity.LayoutInflater), viewModel);

            var view = this.modalDialogBindingContext.BindingInflate(Resource.Layout.dashboard_interviewer_selector, null, false);
            var recyclerView = view.FindViewById<MvxRecyclerView>(Resource.Id.interviewers_list);
            recyclerView.HasFixedSize = true;
            //recyclerView.Adapter = new RecyclerViewAdapter(this.modalDialogBindingContext);

            this.modalDialog = new AppCompatDialog(parentActivity);
            this.modalDialog.Window.RequestFeature(WindowFeatures.NoTitle);
            this.modalDialog.SetCancelable(false);
            this.modalDialog.SetContentView(view);

            this.modalDialog.Show();
            this.modalDialog.Window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent);
            this.modalDialog.Window.SetBackgroundDrawableResource(Android.Resource.Color.Transparent);
        }

        private void ViewModel_OnDone(object sender, EventArgs e)
        {
            this.HideDialog();
            this.OnSelected?.Invoke(sender, EventArgs.Empty);
        }
        private void ViewModel_OnCancel(object sender, EventArgs e)
        {
            this.HideDialog();
            this.OnCanelSelection?.Invoke(sender, EventArgs.Empty);
        }

        private void HideDialog()
        {
            this.modalDialog.Dismiss();
            this.modalDialog.Dispose();
            this.modalDialog = null;

            this.viewModel.OnDone -= this.ViewModel_OnDone;
            this.viewModel.OnCancel -= this.ViewModel_OnCancel;
            this.viewModel.DisposeIfDisposable();
            this.viewModel = null;
        }
    }
}
