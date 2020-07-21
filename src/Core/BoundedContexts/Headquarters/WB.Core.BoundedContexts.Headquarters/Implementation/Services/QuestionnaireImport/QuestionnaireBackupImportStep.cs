using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Implementation;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services.QuestionnaireImport
{
    public class QuestionnaireBackupImportStep : IQuestionnaireImportStep
    {
        private readonly IDesignerApi designerApi;
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage;
        private readonly ILogger logger;
        private readonly RestFile backupFile;

        public QuestionnaireBackupImportStep(QuestionnaireIdentity questionnaireIdentity,
            IDesignerApi designerApi, 
            IPlainKeyValueStorage<QuestionnaireBackup> questionnaireBackupStorage, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.designerApi = designerApi;
            this.questionnaireBackupStorage = questionnaireBackupStorage;
            this.logger = logger;
        }

        public bool IsNeedProcessing() => true;
        

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            logger.Verbose($"Downloading Questionnaire Backup: {questionnaireIdentity.QuestionnaireId} ver.{questionnaireIdentity.Version}");

            var backupFile = await designerApi.DownloadQuestionnaireBackup(questionnaireIdentity.QuestionnaireId);

            progress.Report(100);
        }

        public void SaveData(IProgress<int> progress)
        {
            if (backupFile != null)
            {
                this.questionnaireBackupStorage.Store(new QuestionnaireBackup { Content = backupFile.Content }, questionnaireIdentity.ToString());
            }

            progress.Report(100);
        }
    }
}
