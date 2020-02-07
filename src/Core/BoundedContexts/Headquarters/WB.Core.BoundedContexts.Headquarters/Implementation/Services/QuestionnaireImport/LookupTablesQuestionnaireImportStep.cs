using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class LookupTablesQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly QuestionnaireDocument questionnaire;
        private readonly Progress progress;
        private readonly IDesignerApi designerApi;
        private readonly ILogger logger;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly  Dictionary<Guid, QuestionnaireLookupTable> lookupTables = new Dictionary<Guid, QuestionnaireLookupTable>();

        public LookupTablesQuestionnaireImportStep(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire, Progress progress, IDesignerApi designerApi, IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.questionnaire = questionnaire;
            this.progress = progress;
            this.designerApi = designerApi;
            this.lookupTablesStorage = lookupTablesStorage;
            this.logger = logger;
        }

        public int GetPrecessStepsCount()
        {
            return (questionnaire.LookupTables?.Count ?? 0) * 2;
        }

        public async Task DownloadFromDesignerAsync()
        {
            if (questionnaire.LookupTables == null)
                return;

            foreach (var lookupId in questionnaire.LookupTables.Keys)
            {
                this.logger.Debug($"Loading lookup table questionnaire {questionnaireIdentity}. Lookup id {lookupId}");
                var lookupTable = await designerApi.GetLookupTables(questionnaire.PublicKey, lookupId);
                lookupTables.Add(lookupId, lookupTable);
                progress.Current++;
            }
        }

        public void SaveData()
        {
            foreach (var lookupTable in lookupTables)
            {
                this.logger.Debug($"Save lookup table questionnaire {questionnaireIdentity}. Lookup id {lookupTable.Key}");
                lookupTablesStorage.Store(lookupTable.Value, questionnaireIdentity, lookupTable.Key);
                progress.Current++;
            }
        }
    }
}
