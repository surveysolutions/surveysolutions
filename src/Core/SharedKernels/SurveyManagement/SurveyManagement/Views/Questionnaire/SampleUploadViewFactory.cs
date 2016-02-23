using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Mapping;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class SampleUploadViewFactory : IViewFactory<SampleUploadViewInputModel, SampleUploadView>
    {
        private readonly IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires;
        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;

        public SampleUploadViewFactory(IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaires,
            IQuestionnaireProjectionsRepository questionnaireProjectionsRepository)
        {
            this.questionnaires = questionnaires;
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
        }

        public SampleUploadView Load(SampleUploadViewInputModel input)
        {
            var questionnaire = this.questionnaires.AsVersioned().Get(input.QuestionnaireId.FormatGuid(), input.Version);
            if (questionnaire == null)
                return null;

            var questionnaireExportStructure =
                this.questionnaireProjectionsRepository.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(input.QuestionnaireId, input.Version));

            if (questionnaireExportStructure == null)
                return null;

            var topLevelOfQuestionnaire =
                questionnaireExportStructure.HeaderToLevelMap.Values.FirstOrDefault(
                    level => level.LevelScopeVector.Count == 0);
            if (topLevelOfQuestionnaire == null)
                return null;

            var columnListToPreload = new List<FeaturedQuestionItem>();
            
            foreach (var featuredQuestionItem in questionnaire.FeaturedQuestions)
            {
                if(!topLevelOfQuestionnaire.HeaderItems.ContainsKey(featuredQuestionItem.Id))
                    continue;

                var questionExportedColumn = topLevelOfQuestionnaire.HeaderItems[featuredQuestionItem.Id];
                foreach (var columnName in questionExportedColumn.ColumnNames)
                {
                    columnListToPreload.Add(new FeaturedQuestionItem(featuredQuestionItem.Id, featuredQuestionItem.Title,
                        columnName));
                }
            }
            return new SampleUploadView(input.QuestionnaireId, input.Version, columnListToPreload);
        }
    }
}
