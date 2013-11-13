using System;
//using CAPI.Android.Core.Model;
using Ninject;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.BoundedContexts.Capi.Views.Statistics;
using WB.UI.Shared.Android.Frames;

namespace WB.UI.QuestionnaireTester.Implementations.Fragments
{
    public class TesterStatisticsContentFragment : StatisticsContentFragment
    {
        protected override StatisticsViewModel GetStatisticsViewModel(Guid questionnaireId)
        {
            return CapiTesterApplication.LoadView<StatisticsInput, StatisticsViewModel>(new StatisticsInput(questionnaireId));
        }
        protected override void PreCompleteAction()
        {
            base.PreCompleteAction();

           /* var logManipulator = CapiTesterApplication.Kernel.Get<IChangeLogManipulator>();

            if (this.Model.Status == InterviewStatus.Completed)
            {
                CapiTesterApplication.CommandService.Execute(
                    new RestartInterviewCommand(this.Model.QuestionnaireId, CapiTesterApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CreateOrReopenDraftRecord(this.Model.QuestionnaireId);
            }
            else
            {
                CapiTesterApplication.CommandService.Execute(
                    new CompleteInterviewCommand(this.Model.QuestionnaireId, CapiTesterApplication.Membership.CurrentUser.Id, this.etComments.Text));

                logManipulator.CloseDraftRecord(this.Model.QuestionnaireId);
            }*/
        }
    }
}