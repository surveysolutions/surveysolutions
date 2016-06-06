using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public interface IQuestionnaireSharedPersonsFactory
    {
        QuestionnaireSharedPersons Load(QuestionnaireSharedPersonsInputModel input);
    }

    public class QuestionnaireSharedPersonsFactory : IQuestionnaireSharedPersonsFactory
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