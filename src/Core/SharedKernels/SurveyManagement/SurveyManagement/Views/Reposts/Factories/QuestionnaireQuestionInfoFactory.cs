using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
{
    public class QuestionnaireQuestionInfoFactory : IViewFactory<QuestionnaireQuestionInfoInputModel, QuestionnaireQuestionInfoView>
    {
        private readonly IPlainQuestionnaireRepository plainQuestionnaireRepository;

        public QuestionnaireQuestionInfoFactory(IPlainQuestionnaireRepository plainQuestionnaireRepository)
        {
            this.plainQuestionnaireRepository = plainQuestionnaireRepository;
        }

        public QuestionnaireQuestionInfoView Load(QuestionnaireQuestionInfoInputModel input)
        {
            var questionnaire = this.plainQuestionnaireRepository.GetQuestionnaireDocument(input.QuestionnaireId, input.QuestionnaireVersion);

            if (questionnaire == null)
                return new QuestionnaireQuestionInfoView();

            var questionnaireQuestionInfoItems = questionnaire
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
