using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public interface ISampleUploadViewFactory
    {
        SampleUploadView Load(SampleUploadViewInputModel input);
    }

    public class SampleUploadViewFactory : ISampleUploadViewFactory
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;
        private readonly IQuestionnaireStorage questionnaireStorage;

        public SampleUploadViewFactory(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires, 
            IQuestionnaireExportStructureStorage questionnaireExportStructureStorage,
            IQuestionnaireStorage questionnaireStorage)
        {
            this.questionnaires = questionnaires;
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
            this.questionnaireStorage = questionnaireStorage;
        }

        public SampleUploadView Load(SampleUploadViewInputModel input)
        {
            var questionnaireId = new QuestionnaireIdentity(input.QuestionnaireId, input.Version);
            var questionnaire = this.questionnaires.GetById(questionnaireId.ToString());
            if (questionnaire == null)
                return null;

            var questionnaireExportStructure =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(questionnaireId);

            var topLevelOfQuestionnaire =
                questionnaireExportStructure?.HeaderToLevelMap.Values.FirstOrDefault(level => level.LevelScopeVector.Count == 0);
            if (topLevelOfQuestionnaire == null)
                return null;

            var preloadedQuestions = new List<FeaturedQuestionItem>();
            
            foreach (var featuredQuestionItem in questionnaire.FeaturedQuestions)
            {
                if(!topLevelOfQuestionnaire.HeaderItems.ContainsKey(featuredQuestionItem.Id))
                    continue;

                var questionExportedColumn = topLevelOfQuestionnaire.HeaderItems[featuredQuestionItem.Id];
                foreach (var column in questionExportedColumn.ColumnHeaders)
                {
                    preloadedQuestions.Add(new FeaturedQuestionItem(featuredQuestionItem.Id, featuredQuestionItem.Title, column.Name));
                }
            }

            var plainQuestionnaire = this.questionnaireStorage.GetQuestionnaire(questionnaireId, null);
            var hiddenQuestionVariables = plainQuestionnaire.GetHiddenQuestions()
                                                            .Select(x => plainQuestionnaire.GetQuestionVariableName(x))
                                                            .ToList();
            var rosterSizeQuestions = plainQuestionnaire.GetAllRosterSizeQuestions()
                                                            .Select(x => plainQuestionnaire.GetQuestionVariableName(x))
                                                            .ToList();

            return new SampleUploadView(input.QuestionnaireId, input.Version, preloadedQuestions, hiddenQuestionVariables, rosterSizeQuestions);
        }
    }
}
