using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class QuestionnaireQuestionInfoFactory : IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore;

        public QuestionnaireQuestionInfoFactory(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore)
        {
            this.questionnaireStore = questionnaireStore;
        }

        public QuestionnaireQuestionInfoView Load(QuestionnaireQuestionInfoInputModel input)
        {
            var questionnaire = this.questionnaireStore.AsVersioned().Get(input.QuestionnaireId.FormatGuid(), input.QuestionnaireVersion);

            if (questionnaire == null)
                return new QuestionnaireQuestionInfoView();

            var questionnaireQuestionInfoItems = questionnaire.Questionnaire
                .Find<IQuestion>(question => true)
                .Where(x => !input.QuestionType.HasValue || x.QuestionType == input.QuestionType.Value)
                .Select(x => new QuestionnaireQuestionInfoItem
                {
                    Variable = x.StataExportCaption,
                    Type = x.QuestionType,
                    Id = x.PublicKey
                })
                .ToList();

            return new QuestionnaireQuestionInfoView
            {
                Variables = questionnaireQuestionInfoItems
            };
        }
    }
}
