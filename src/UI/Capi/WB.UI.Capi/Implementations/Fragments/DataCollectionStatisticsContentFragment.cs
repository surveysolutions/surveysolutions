using System;
using CAPI.Android.Core.Model;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.Capi.Implementations.Fragments
{
    public class DataCollectionStatisticsContentFragment : StatisticsContentFragment
    {
        protected override InterviewViewModel GetInterviewViewModel(Guid interviewId)
        {
            return CapiApplication.LoadView<QuestionnaireScreenInput, InterviewViewModel>(new QuestionnaireScreenInput(interviewId));
        }

        protected override void PreCompleteAction()
        {
            base.PreCompleteAction();

            var logManipulator = CapiApplication.Kernel.Get<IChangeLogManipulator>();

            if (this.Model.Status == InterviewStatus.Completed)
            {
                CapiApplication.CommandService.Execute(
                    new RestartInterviewCommand(this.Model.PublicKey, CapiApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CreateOrReopenDraftRecord(this.Model.PublicKey);
            }
            else
            {
                CapiApplication.CommandService.Execute(
                    new CompleteInterviewCommand(this.Model.PublicKey, CapiApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CloseDraftRecord(this.Model.PublicKey);
            }

            this.Activity.Finish();
        }
    }
}