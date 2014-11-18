using System;
using Main.Core.Documents;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireViewFactory : IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>
    {
        private readonly IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage;

        public QuestionnaireViewFactory(IReadSideRepositoryReader<QuestionnaireDocument> questionnaireStorage)
        {
            this.questionnaireStorage = questionnaireStorage;
        }

        QuestionnaireView IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>.Load(QuestionnaireViewInputModel input)
        {
            var doc = GetQuestionnaireDocument(input);
            return doc == null ? null : new QuestionnaireView(doc);
        }
        
        private QuestionnaireDocument GetQuestionnaireDocument(QuestionnaireViewInputModel input)
        {
            try
            {
                return this.questionnaireStorage.GetById(input.QuestionnaireId);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }
}