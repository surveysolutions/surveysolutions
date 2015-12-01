using WB.Core.BoundedContexts.Interviewer.Views;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure.Storage;

namespace WB.Core.BoundedContexts.Interviewer.Implementation.Services
{
    public class QuestionnaireModelKeyValueStorage : IPlainKeyValueStorage<QuestionnaireModel>
    {
        private readonly IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository;
        public QuestionnaireModelKeyValueStorage(IAsyncPlainStorage<QuestionnaireModelView> questionnaireModelViewRepository)
        {
            this.questionnaireModelViewRepository = questionnaireModelViewRepository;
        }

        public QuestionnaireModel GetById(string id)
        {
            return this.questionnaireModelViewRepository.GetByIdAsync(id).Result.Model;
        }

        public void Remove(string id)
        {
            throw new System.NotImplementedException();
        }

        public void Store(QuestionnaireModel view, string id)
        {
            throw new System.NotImplementedException();
        }
    }
}