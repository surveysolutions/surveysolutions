using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.Infrastructure.CommandBus;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class ResourcesPostProcessor : 
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>,
        ICommandPostProcessor<Questionnaire, DeleteAttachment>,
        ICommandPostProcessor<Questionnaire, DeleteTranslation>,
        ICommandPostProcessor<Questionnaire, DeleteLookupTable>
    {
        private IAttachmentService attachmentService => ServiceLocator.Current.GetInstance<IAttachmentService>();
        private ILookupTableService lookupTableService => ServiceLocator.Current.GetInstance<ILookupTableService>();
        private ITranslationsService translationsService => ServiceLocator.Current.GetInstance<ITranslationsService>();

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            var questionnaire = aggregate.QuestionnaireDocument;

            foreach (var attachment in questionnaire.Attachments)
            {
                attachmentService.Delete(attachment.AttachmentId);
            }

            foreach (var lookupTable in questionnaire.LookupTables)
            {
                this.lookupTableService.DeleteLookupTableContent(command.QuestionnaireId, lookupTable.Key);
            }

            foreach (var translation in questionnaire.Translations)
            {
                this.translationsService.Delete(command.QuestionnaireId, translation.Id);
            }
        }

        public void Process(Questionnaire aggregate, DeleteAttachment command)
            => this.attachmentService.Delete(command.AttachmentId);

        public void Process(Questionnaire aggregate, DeleteTranslation command)
            => this.translationsService.Delete(command.QuestionnaireId, command.TranslationId);

        public void Process(Questionnaire aggregate, DeleteLookupTable command)
            => this.lookupTableService.DeleteLookupTableContent(command.QuestionnaireId, command.LookupTableId);
    }
}