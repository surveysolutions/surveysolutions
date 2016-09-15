using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public interface IQuestionnaireSharedPersonsFactory
    {
        QuestionnaireSharedPersons Load(QuestionnaireSharedPersonsInputModel input);
    }

    public class QuestionnaireSharedPersonsFactory : IQuestionnaireSharedPersonsFactory
    {
        private readonly IPlainKeyValueStorage<QuestionnaireSharedPersons> _repository;
        public QuestionnaireSharedPersonsFactory(IPlainKeyValueStorage<QuestionnaireSharedPersons> repository)
        {
            this._repository = repository;
        }

        public QuestionnaireSharedPersons Load(QuestionnaireSharedPersonsInputModel input)
        {
            return _repository.GetById(input.QuestionnaireId.FormatGuid());
        }
    }
}