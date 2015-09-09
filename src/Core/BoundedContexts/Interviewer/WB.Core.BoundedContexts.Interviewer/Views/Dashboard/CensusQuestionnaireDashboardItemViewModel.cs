using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;

namespace WB.Core.BoundedContexts.Interviewer.Views.Dashboard
{
    public class CensusQuestionnaireDashboardItemViewModel : IDashboardItem
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository;

        public CensusQuestionnaireDashboardItemViewModel(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
        }

        public void Init(string id)
        {
            var questionnaire = this.questionnaireRepository.GetById(id);

            Title = questionnaire.Title;
        }

        public string Title { get; set; }
    }
}