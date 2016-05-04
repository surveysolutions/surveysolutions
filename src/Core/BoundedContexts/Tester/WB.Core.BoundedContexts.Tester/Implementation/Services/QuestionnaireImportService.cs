using Main.Core.Documents;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;

        public QuestionnaireImportService(IPlainQuestionnaireRepository questionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
        }

        public void ImportQuestionnaire(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument, string supportingAssembly)
        {
            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);
        }
    }
}
