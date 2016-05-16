using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;
        private readonly IOptionsRepository optionsRepository;

        public QuestionnaireImportService(IPlainQuestionnaireRepository questionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor, 
            IOptionsRepository optionsRepository)
        {
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
            this.optionsRepository = optionsRepository;
        }

        public async Task ImportQuestionnaireAsync(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaireDocument, string supportingAssembly)
        {
            await this.optionsRepository.RemoveOptionsForQuestionnaireAsync(questionnaireIdentity);
            await this.optionsRepository.StoreQuestionOptionsForQuestionnaireAsync(questionnaireIdentity, questionnaireDocument);
            this.questionnaireRepository.StoreQuestionnaire(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, questionnaireDocument);
            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireIdentity.QuestionnaireId, questionnaireIdentity.Version, supportingAssembly);
        }
    }
}
