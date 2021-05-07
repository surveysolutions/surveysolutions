using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class LookupTablesQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly QuestionnaireDocument questionnaire;
        private readonly IDesignerApi designerApi;
        private readonly ILogger logger;
        private readonly IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage;
        private readonly  Dictionary<Guid, QuestionnaireLookupTable> lookupTables = new Dictionary<Guid, QuestionnaireLookupTable>();

        public LookupTablesQuestionnaireImportStep(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire, IDesignerApi designerApi, IPlainKeyValueStorage<QuestionnaireLookupTable> lookupTablesStorage, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.questionnaire = questionnaire;
            this.designerApi = designerApi;
            this.lookupTablesStorage = lookupTablesStorage;
            this.logger = logger;
        }

        public bool IsNeedProcessing()
        {
            return questionnaire.LookupTables?.Count > 0;
        }

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            if (questionnaire.LookupTables != null && questionnaire.LookupTables.Count > 0)
            {
                int percentPerLookupTable = 100 / questionnaire.LookupTables.Count;
                int currentPercent = 0;

                foreach (var lookupId in questionnaire.LookupTables.Keys)
                {
                    this.logger.Debug($"Loading lookup table questionnaire {questionnaireIdentity}. Lookup id {lookupId}");
                    var lookupTable = await designerApi.GetLookupTables(questionnaire.PublicKey, lookupId);
                    lookupTables.Add(lookupId, lookupTable);

                    currentPercent += percentPerLookupTable;
                    progress.Report(currentPercent);
                }
            }
            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            foreach (var lookupTable in lookupTables)
            {
                this.logger.Debug($"Save lookup table questionnaire {questionnaireIdentity}. Lookup id {lookupTable.Key}");
                lookupTablesStorage.Store(lookupTable.Value, questionnaireIdentity, lookupTable.Key);
            }
            progress.Report(100);
        }
    }
}
