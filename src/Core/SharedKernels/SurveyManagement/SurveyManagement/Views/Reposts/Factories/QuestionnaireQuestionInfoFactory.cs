using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class QuestionnaireQuestionInfoFactory : IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView>
    {
        private readonly IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore;

        public QuestionnaireQuestionInfoFactory(IVersionedReadSideRepositoryReader<QuestionnaireDocumentVersioned> questionnaireStore)
        {
            this.questionnaireStore = questionnaireStore;
        }

        public QuestionnaireQuestionInfoView Load(QuestionnaireQuestionInfoInputModel input)
        {
            var questionnaire = this.questionnaireStore.GetById(input.QuestionnaireId, input.QuestionnaireVersion);

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
