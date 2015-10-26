using Main.Core.Documents;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IQuestionnaireModelBuilder questionnaireModelBuilder;

        public QuestionnaireImportService(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository, 
            IPlainQuestionnaireRepository questionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor,
            IQuestionnaireModelBuilder questionnaireModelBuilder)
        {
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.questionnaireModelBuilder = questionnaireModelBuilder;
        }

        public void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument, string supportingAssembly)
        {
            QuestionnaireModel questionnaireModel = this.questionnaireModelBuilder.BuildQuestionnaireModel(questionnaireDocument);

            this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);
        }
    }
}
