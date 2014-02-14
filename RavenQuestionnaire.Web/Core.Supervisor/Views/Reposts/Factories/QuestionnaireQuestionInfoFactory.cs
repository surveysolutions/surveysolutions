using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace Core.Supervisor.Views.Reposts.Factories
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
            var questionnaire = questionnaireStore.GetById(input.QuestionnaireId, input.QuestionnaireVersion);
            return new QuestionnaireQuestionInfoView()
            {
                Variables = questionnaire.Questionnaire.Find<IQuestion>(question => true).ToDictionary(x => x.PublicKey, x =>
                    new QuestionnaireQuestionInfoItem
                    {
                        Variable = x.StataExportCaption,
                        Type = x.QuestionType
                    })
            };
        }
    }
}
