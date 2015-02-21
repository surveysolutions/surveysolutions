using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class QuestionnaireSharedPersonsFactory : IViewFactory<QuestionnaireSharedPersonsInputModel, QuestionnaireSharedPersons>
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireSharedPersons> _repository;
        public QuestionnaireSharedPersonsFactory(IReadSideKeyValueStorage<QuestionnaireSharedPersons> repository)
        {
            this._repository = repository;
        }

        public QuestionnaireSharedPersons Load(QuestionnaireSharedPersonsInputModel input)
        {
            return _repository.GetById(input.QuestionnaireId);
        }
    }
}