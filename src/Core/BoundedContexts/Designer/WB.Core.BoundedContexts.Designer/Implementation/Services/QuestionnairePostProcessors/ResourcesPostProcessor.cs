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
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>
    {
        private IAttachmentService attachmentService => ServiceLocator.Current.GetInstance<IAttachmentService>();
        private ILookupTableService lookupTableService => ServiceLocator.Current.GetInstance<ILookupTableService>();
        private ITranslationsService translationsService => ServiceLocator.Current.GetInstance<ITranslationsService>();

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            this.translationsService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
            this.lookupTableService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
            this.attachmentService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
        }
    }
}