using System;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Groups
{
    public class InterviewStateViewModel : GroupStateViewModel
    {
        private readonly IInterviewStateCalculationStrategy interviewStateCalculationStrategy;

        public InterviewStateViewModel(IStatefulInterviewRepository interviewRepository, 
            IInterviewStateCalculationStrategy interviewStateCalculationStrategy,
            IQuestionnaireStorage questionnaireRepository)
            :base(interviewRepository, null, questionnaireRepository)
        {
            this.interviewStateCalculationStrategy = interviewStateCalculationStrategy;
        }

        public override void Init(string interviewId, Identity groupIdentity)
        {
            this.interviewId = interviewId;
            this.UpdateFromGroupModel();
        }

        public override void UpdateFromGroupModel()
        {           
            if (interviewId == null)
                return;
            
            IStatefulInterview interview = this.interviewRepository.Get(this.interviewId);
            var interviewSimpleStatus = this.interviewStateCalculationStrategy.GetInterviewSimpleStatus(interview);

            this.QuestionsCount = interviewSimpleStatus.ActiveQuestionCount;
            this.SubgroupsCount = 0;
            this.AnsweredQuestionsCount = interviewSimpleStatus.AnsweredQuestionsCount;
            this.InvalidAnswersCount = interview.CountInvalidEntitiesInInterview();
            this.AnsweredProgress = this.QuestionsCount == 0 
                ? 100 
                : (int)Math.Round((double)(this.AnsweredQuestionsCount * 100)/ this.QuestionsCount);
            this.SimpleStatus = interviewSimpleStatus.SimpleStatus;
            this.Status = interviewSimpleStatus.Status;
        }
        
        public bool HasCriticalFeature(string interviewId)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, null);
            return questionnaire.DoesSupportCriticality();
        }
    }
}
