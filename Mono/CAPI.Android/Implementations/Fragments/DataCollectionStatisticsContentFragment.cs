using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using CAPI.Android.Core.Model;
using Ninject;
using WB.Core.BoundedContexts.Capi.Views.Statistics;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Shared.Android.Frames;

namespace CAPI.Android.Implementations.Fragments
{
    public class DataCollectionStatisticsContentFragment : StatisticsContentFragment
    {
        protected override StatisticsViewModel GetStatisticsViewModel(Guid questionnaireId)
        {
            return CapiApplication.LoadView<StatisticsInput, StatisticsViewModel>(new StatisticsInput(questionnaireId));
        }
        protected override void PreCompleteAction()
        {
            base.PreCompleteAction();

            var logManipulator = CapiApplication.Kernel.Get<IChangeLogManipulator>();

            if (this.Model.Status == InterviewStatus.Completed)
            {
                CapiApplication.CommandService.Execute(
                    new RestartInterviewCommand(this.Model.QuestionnaireId, CapiApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CreateOrReopenDraftRecord(this.Model.QuestionnaireId);
            }
            else
            {
                CapiApplication.CommandService.Execute(
                    new CompleteInterviewCommand(this.Model.QuestionnaireId, CapiApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CloseDraftRecord(this.Model.QuestionnaireId);
            }
        }
    }
}