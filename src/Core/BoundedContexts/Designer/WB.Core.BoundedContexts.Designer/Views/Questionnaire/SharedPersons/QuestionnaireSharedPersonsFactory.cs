using System.Linq;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class QuestionnaireSharedPersonsFactory : IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>
    {
        private readonly IQueryableReadSideRepositoryReader<QuestionnaireSharedPersons> _repository;
        public QuestionnaireSharedPersonsFactory(IQueryableReadSideRepositoryReader<QuestionnaireSharedPersons> repository)
        {
            this._repository = repository;
        }

        public QuestionnaireSharedPersons Load(QuestionnaireSharedPersonsInputModel input)
        {
            return _repository.GetById(input.QuestionnaireId);
        }
    }
}