using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Designer.Code.Implementation
{
    public class CommandPostprocessor : ICommandPostprocessor
    {
        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
        private readonly ILogger logger;
        private readonly IAttachmentService attachmentService;
        private readonly ILookupTableService lookupTableService;
        private readonly ITranslationsService translationsService;
        private readonly IQuestionnaireHistory questionnaireHistory;


        public CommandPostprocessor(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader, 
            ILogger logger, 
            IAttachmentService attachmentService, 
            ILookupTableService lookupTableService, 
            ITranslationsService translationsService,
            IQuestionnaireHistory questionnaireHistory)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.logger = logger;
            this.attachmentService = attachmentService;
            this.lookupTableService = lookupTableService;
            this.translationsService = translationsService;
            this.questionnaireHistory = questionnaireHistory;
        }

        public void ProcessCommandAfterExecution(ICommand command)
        {
            var questionnaireCommand = command as QuestionnaireCommand;
            if (questionnaireCommand == null) return;

            try
            {
                TypeSwitch.Do(command,
                    TypeSwitch.Case<DeleteQuestionnaire>(x => this.DeleteAccompanyingDataOnQuestionnaireRemove(x.QuestionnaireId)),
                    TypeSwitch.Case<DeleteAttachment>(x => this.attachmentService.Delete(x.AttachmentId)),
                    TypeSwitch.Case<DeleteLookupTable>(x => this.lookupTableService.DeleteLookupTableContent(x.QuestionnaireId, x.LookupTableId)),
                    TypeSwitch.Case<DeleteTranslation>(x => this.translationsService.Delete(x.QuestionnaireId, x.TranslationId)));

                this.questionnaireHistory.Write(questionnaireCommand);
            }
            catch (Exception exc)
            {
                logger.Error("Error on command post-processing", exc);
            }
        }

        

        private void DeleteAccompanyingDataOnQuestionnaireRemove(Guid questionnaireId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId.FormatGuid());

            foreach (var attachment in questionnaire.Attachments)
            {
                attachmentService.Delete(attachment.AttachmentId);
            }

            foreach (var lookupTable in questionnaire.LookupTables)
            {
                this.lookupTableService.DeleteLookupTableContent(questionnaireId, lookupTable.Key);
            }

            foreach (var translation in questionnaire.Translations)
            {
                this.translationsService.Delete(questionnaireId, translation.Id);
            }
        }
    }
}
