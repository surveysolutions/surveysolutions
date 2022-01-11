using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class ResourcesPostProcessor :
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>
    {
        private readonly IAttachmentService attachmentService;
        private readonly ILookupTableService lookupTableService;
        private readonly ITranslationsService translationsService;
        private readonly ICommentsService commentsService;
        private readonly ICategoriesService categoriesService;

        public ResourcesPostProcessor(IAttachmentService attachmentService,
            ILookupTableService lookupTableService,
            ITranslationsService translationsService,
            ICommentsService commentsService,
            ICategoriesService categoriesService)
        {
            this.attachmentService = attachmentService;
            this.lookupTableService = lookupTableService;
            this.translationsService = translationsService;
            this.commentsService = commentsService;
            this.categoriesService = categoriesService;
        }

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            this.translationsService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
            this.lookupTableService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
            this.attachmentService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
            this.commentsService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
            this.categoriesService.DeleteAllByQuestionnaireId(command.QuestionnaireId);
        }
    }
}
