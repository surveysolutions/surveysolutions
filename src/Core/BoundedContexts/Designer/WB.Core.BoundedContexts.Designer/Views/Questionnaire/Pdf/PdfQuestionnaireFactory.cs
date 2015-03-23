using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireFactory : IViewFactory<PdfQuestionnaireInputModel, PdfQuestionnaireView>
    {
        private readonly IReadSideKeyValueStorage<PdfQuestionnaireView> _repository;

        public PdfQuestionnaireFactory(IReadSideKeyValueStorage<PdfQuestionnaireView> repository)
        {
            _repository = repository;
        }

        public PdfQuestionnaireView Load(PdfQuestionnaireInputModel input)
        {
            return _repository.GetById(input.Id);
        }
    }
}
