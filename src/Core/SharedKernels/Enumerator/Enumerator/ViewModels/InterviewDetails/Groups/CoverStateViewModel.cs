using Main.Core.Documents;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class CoverStateViewModel : GroupStateViewModel
    {
        private bool isCoverPageSupported = false;

        public CoverStateViewModel(IStatefulInterviewRepository interviewRepository,
            IGroupStateCalculationStrategy groupStateCalculationStrategy,
            IQuestionnaireStorage questionnaireRepository)
            :base(interviewRepository, groupStateCalculationStrategy, questionnaireRepository)
        {
        }

        public override void Init(string interviewId, Identity groupIdentity)
        {
            IStatefulInterview interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaireDocument(interview.QuestionnaireIdentity);

            this.isCoverPageSupported = questionnaire.IsCoverPageSupported;
            this.interviewId = interviewId;
            this.group = isCoverPageSupported
                ? new Identity(questionnaire.CoverPageSectionId, RosterVector.Empty) 
                : groupIdentity;

            this.QuestionsCount = 0;
            this.SubgroupsCount = 0;
            this.AnsweredQuestionsCount = 0;
            this.InvalidAnswersCount = 0;

            this.SimpleStatus = SimpleGroupStatus.Other;
            this.Status = GroupStatus.Started;

            this.UpdateFromGroupModel();
        }

        public override void UpdateFromGroupModel()
        {
            if (!isCoverPageSupported)
                return;
            
            base.UpdateFromGroupModel();
        }
    }
}
