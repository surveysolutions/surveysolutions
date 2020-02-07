using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Main.Core.Documents;
using Polly;
using WB.Core.BoundedContexts.Headquarters.Designer;
using WB.Core.BoundedContexts.Headquarters.Services;
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
        private readonly Progress progress;
        private readonly IDesignerApi designerApi;
        private readonly ILogger logger;
        private readonly IPlainKeyValueStorage<QuestionnairePdf> pdfStorage;
        private readonly Dictionary<string, QuestionnairePdf> pdfFiles = new Dictionary<string, QuestionnairePdf>();

        public PdfQuestionnaireImportStep(QuestionnaireIdentity questionnaireIdentity, QuestionnaireDocument questionnaire, Progress progress, IDesignerApi designerApi, IPlainKeyValueStorage<QuestionnairePdf> pdfStorage, ILogger logger)
        {
            this.questionnaireIdentity = questionnaireIdentity;
            this.questionnaire = questionnaire;
            this.progress = progress;
            this.designerApi = designerApi;
            this.pdfStorage = pdfStorage;
            this.logger = logger;
        }

        public int GetPrecessStepsCount()
        {
            return (1 + (questionnaire.Translations?.Count ?? 0)) * 3;
        }

        public async Task DownloadFromDesignerAsync()
        {
            await RequestToGeneratePdf();
            await DownloadPdf();
        }

        private async Task RequestToGeneratePdf()
        {
            this.logger.Error($"Requesting pdf generator to start working for questionnaire {questionnaire.PublicKey}");

            await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId);
            progress.Current++;

            foreach (var questionnaireTranslation in questionnaire.Translations)
            {
                await designerApi.GetPdfStatus(questionnaire.PublicKey, questionnaireTranslation.Id);
                progress.Current++;
            }
        }

        private async Task DownloadPdf()
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

            this.logger.Error("Loading pdf for default language");

            var pdfFile = await designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId);
            progress.Current++;

            pdfFiles.Add(questionnaireIdentity.ToString(), new QuestionnairePdf { Content = pdfFile.Content });

            this.logger.Error($"PDF for questionnaire stored {questionnaireIdentity}");

            foreach (var translation in questionnaire.Translations)
            {
                this.logger.Error($"loading pdf for translation {translation}");

                await pdfRetry.ExecuteAsync(async () =>
                {
                    this.logger.Trace($"Waiting for pdf to be ready {questionnaireIdentity}");

                    return await designerApi.GetPdfStatus(questionnaireIdentity.QuestionnaireId, translation.Id);
                });

                var pdfTranslated = await designerApi.DownloadPdf(questionnaireIdentity.QuestionnaireId, translation.Id);
                progress.Current++;
                pdfFiles.Add($"{translation.Id.FormatGuid()}_{questionnaireIdentity}", new QuestionnairePdf { Content = pdfTranslated.Content });
                this.logger.Error($"PDF for questionnaire stored {questionnaireIdentity} translation {translation.Id}, {translation.Name}");
            }
        }

        public void SaveData()
        {
            foreach (var pdfFile in pdfFiles)
            {
                this.pdfStorage.Store(pdfFile.Value, pdfFile.Key);
                progress.Current++;
            }
        }
    }
}
