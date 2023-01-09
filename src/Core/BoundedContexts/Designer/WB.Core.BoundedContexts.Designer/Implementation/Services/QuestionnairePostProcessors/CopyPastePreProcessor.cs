using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class CopyPastePreProcessor :
        ICommandPreProcessor<Questionnaire, PasteAfter>,
        ICommandPreProcessor<Questionnaire, PasteInto>
    {
        private readonly IReusableCategoriesService reusableCategoriesService;

        public CopyPastePreProcessor(IReusableCategoriesService reusableCategoriesService)
        {
            this.reusableCategoriesService = reusableCategoriesService;
        }

        public void Process(Questionnaire aggregate, PasteAfter command)
            => this.CopyCategories(command.SourceDocument, command.SourceItemId, aggregate.Id);

        public void Process(Questionnaire aggregate, PasteInto command)
            => this.CopyCategories(command.SourceDocument, command.SourceItemId, aggregate.Id);

        private void CopyCategories(QuestionnaireDocument? sourceQuestionnaire, Guid sourceItemId, Guid targetQuestionnaireId)
        {
            if(sourceQuestionnaire == null)
                throw new QuestionnaireException(
                    DomainExceptionType.EntityNotFound,
                string.Format(ExceptionMessages.QuestionnaireCantBeFound, "unknown"));

            var categoricalQuestion = sourceQuestionnaire.Find<ICategoricalQuestion>(sourceItemId);
            if (categoricalQuestion?.CategoriesId != null)
                this.CloneCategories(sourceQuestionnaire.PublicKey, categoricalQuestion.CategoriesId.Value, targetQuestionnaireId);

            var group = sourceQuestionnaire.Find<IGroup>(sourceItemId);
            if (group != null)
                ((IComposite) @group).TreeToEnumerable(x => x.Children)
                    .OfType<ICategoricalQuestion>()
                    .Where(x => x.CategoriesId.HasValue)
                    .Select(x => x.CategoriesId!.Value)
                    .ToHashSet()
                    .ForEach(categoryId => CloneCategories(sourceQuestionnaire.PublicKey, categoryId, targetQuestionnaireId));
        }

        private void CloneCategories(Guid questionnaireId, Guid categoriesId, Guid targetQuestionnaireId)
        {
            if (this.reusableCategoriesService.GetCategoriesById(targetQuestionnaireId, categoriesId).Any()) return;

            this.reusableCategoriesService.CloneCategories(questionnaireId,
                categoriesId, targetQuestionnaireId, categoriesId);
        }
    }
}
