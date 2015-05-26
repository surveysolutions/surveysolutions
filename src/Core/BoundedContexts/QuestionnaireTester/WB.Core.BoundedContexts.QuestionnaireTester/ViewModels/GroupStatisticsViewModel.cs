using System.Linq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;


namespace WB.Core.BoundedContexts.QuestionnaireTester.ViewModels
{     
    public class GroupStatistics
    {
        public int QuestionsCount { get; set; }
        public int SubgroupsCount { get; set; }
        public int AnsweredQuestionsCount { get; set; }
        public int UnansweredQuestionsCount { get; set; }
        public int InvalidAnswersCount { get; set; }
    }

    public class GroupStatisticsViewModel
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;
        private readonly IStatefullInterviewRepository interviewRepository;

        public GroupStatisticsViewModel(IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository,
            IStatefullInterviewRepository interviewRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;
        }

        public GroupStatistics GetStatistics(string interviewId, Identity groupIdentity)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetById(interview.QuestionnaireId);

            var groupModel = questionnaire.GroupsWithoutNestedChildren[groupIdentity.Id];
            var groupEntities = groupModel.Children;

            var groupModelTypes = new[] { typeof(GroupModel), typeof(RosterModel) };
            var nonQuestionModelTypes = groupModelTypes.Concat(new[] { typeof(StaticTextModel) });

            var groupQuestions = groupEntities.Where(x => !nonQuestionModelTypes.Contains(x.ModelType)).ToList();

            var questionsCount = groupQuestions.Count();
            var subgroupsCount = groupEntities.Count(x => groupModelTypes.Contains(x.ModelType));
            var answeredQuestionsCount = groupQuestions.Count(question =>
                interview.WasAnswered(new Identity(question.Id, groupIdentity.RosterVector)));

            var invalidAnswersCount = 0;
            var unansweredQuestionsCount = questionsCount - answeredQuestionsCount;

            return new GroupStatistics()
            {
                AnsweredQuestionsCount = answeredQuestionsCount,
                UnansweredQuestionsCount = unansweredQuestionsCount,
                InvalidAnswersCount = invalidAnswersCount,
                QuestionsCount = questionsCount,
                SubgroupsCount = subgroupsCount,
            };
        } 
    }
}