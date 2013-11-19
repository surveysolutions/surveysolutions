using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireFactory : IViewFactory<PdfQuestionnaireInputModel, PdfQuestionnaireView>
    {
        private readonly IReadSideRepositoryReader<PdfQuestionnaireView> _repository;

        public PdfQuestionnaireFactory(IReadSideRepositoryReader<PdfQuestionnaireView> repository)
        {
            _repository = repository;
        }

        public PdfQuestionnaireView Load(PdfQuestionnaireInputModel input)
        {
            return _repository.GetById(input.Id);
        }
    }
}
