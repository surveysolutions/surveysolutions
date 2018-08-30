using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class CoverStateViewModel : GroupStateViewModel
    {
        public override void Init(string interviewId, Identity groupIdentity)
        {
            this.QuestionsCount = 0;
            this.SubgroupsCount = 0;
            this.AnsweredQuestionsCount = 0;
            this.InvalidAnswersCount = 0;

            this.SimpleStatus = SimpleGroupStatus.Other;
            this.Status = GroupStatus.Started;
        }

        public override void UpdateFromGroupModel()
        {
        }
    }
}
