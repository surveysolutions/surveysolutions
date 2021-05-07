using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using Polly;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Synchronization.Designer;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class PdfQuestionnaireImportStep : IQuestionnaireImportStep
    {
        private readonly QuestionnaireIdentity questionnaireIdentity;
        private readonly QuestionnaireDocument questionnaire;
        private readonly IDesignerApi designerApi;
        private readonly ILogger logger;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly Dictionary<string, QuestionnairePdf> pdfFiles = new Dictionary<string, QuestionnairePdf>();

        public PdfQuestionnaireImportStep(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire, IDesignerApi designerApi, IPlainKeyValueStorage<QuestionnairePdf> pdfStorage, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.questionnaire = questionnaire;
            this.designerApi = designerApi;
            this.pdfStorage = pdfStorage;
            this.logger = logger;
        }

        public bool IsNeedProcessing() => true;

        public async Task DownloadFromDesignerAsync(IProgress<int> progress)
        {
            int stepsCount = ((questionnaire.Translations?.Count ?? 0) + 1) * 2;
            int percentPerStep = 100 / stepsCount;
            int currentPercent = 0;

            void IncrementProgress()
            {
                currentPercent += percentPerStep;
                progress.Report(currentPercent);
            }

            await RequestToGeneratePdf(IncrementProgress);
            await DownloadPdf(IncrementProgress);
            progress.Report(100);
        }

        private async Task RequestToGeneratePdf(Action incrementProgress)
        {
            this.logger.Info($"Requesting pdf generator to start working for questionnaire {questionnaire.PublicKey}");

            await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId);
            incrementProgress.Invoke();

            foreach (var questionnaireTranslation in questionnaire.Translations)
            {
                await designerApi.GetPdfStatus(questionnaire.PublicKey, questionnaireTranslation.Id);
                incrementProgress.Invoke();
            }
        }

        private async Task DownloadPdf(Action incrementProgress)
        {
            logger.Verbose($"DownloadPdf: {questionnaire.Title}({questionnaire.PublicKey} rev.{questionnaire.Revision})");

            var pdfRetry = Policy
                .HandleResult<PdfStatus>(x => x.ReadyForDownload == false && x.CanRetry != true)
                .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

            await pdfRetry.ExecuteAsync(async () =>
            {
                this.logger.Trace($"Waiting for pdf to be ready {questionnaireIdentity}");
                return await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId);
            });

            this.logger.Info("Loading pdf for default language");

            var pdfFile = await designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId);
            incrementProgress.Invoke();

            pdfFiles.Add(questionnaireIdentity.ToString(), new QuestionnairePdf { Content = pdfFile.Content });

            this.logger.Debug($"PDF for questionnaire stored {questionnaireIdentity}");

            foreach (var translation in questionnaire.Translations)
            {
                this.logger.Info($"loading pdf for translation {translation}");

                await pdfRetry.ExecuteAsync(async () =>
                {
                    this.logger.Trace($"Waiting for pdf to be ready {questionnaireIdentity}");

                    return await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId, translation.Id);
                });

                var pdfTranslated = await designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId, translation.Id);
                incrementProgress.Invoke();

                pdfFiles.Add($"{translation.Id.FormatGuid()}_{questionnaireIdentity}", new QuestionnairePdf { Content = pdfTranslated.Content });
                this.logger.Debug($"PDF for questionnaire stored {questionnaireIdentity} translation {translation.Id}, {translation.Name}");
            }
        }

        public void SaveData(IProgress<int> progress)
        {
            foreach (var pdfFile in pdfFiles)
            {
                this.pdfStorage.Store(pdfFile.Value, pdfFile.Key);
            }

            progress.Report(100);
        }
    }
}
