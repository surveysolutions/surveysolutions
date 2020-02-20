using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
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
        private readonly ICategoriesService categoriesService;

        public CopyPastePreProcessor(ICategoriesService categoriesService)
        {
            this.categoriesService = categoriesService;
        }

        public void Process(Questionnaire aggregate, PasteAfter command)
            => this.MoveCategories(command.SourceDocument, command.SourceItemId, aggregate.Id);

        public void Process(Questionnaire aggregate, PasteInto command)
            => this.MoveCategories(command.SourceDocument, command.SourceItemId, aggregate.Id);

        private void MoveCategories(QuestionnaireDocument sourceQuestionnaire, Guid sourceItemId, Guid targetQuestionnaireId)
        {
            var categoricalQuestion = sourceQuestionnaire.Find<ICategoricalQuestion>(sourceItemId);
            if (categoricalQuestion != null)
                this.MoveCategories(sourceQuestionnaire, categoricalQuestion, targetQuestionnaireId);

            var group = sourceQuestionnaire.Find<IGroup>(sourceItemId);
            ((IComposite) @group)?.TreeToEnumerable(x => x.Children).OfType<ICategoricalQuestion>()
                .ForEach(x => MoveCategories(sourceQuestionnaire, x, targetQuestionnaireId));
        }

        private void MoveCategories(QuestionnaireDocument sourceQuestionnaire, ICategoricalQuestion categoricalQuestion, Guid targetQuestionnaireId)
        {
            if (categoricalQuestion?.CategoriesId == null) return;

            if (this.categoriesService.GetCategoriesById(targetQuestionnaireId, categoricalQuestion.CategoriesId.Value)
                .Any()) return;

            this.categoriesService.CloneCategories(sourceQuestionnaire.PublicKey,
                categoricalQuestion.CategoriesId.Value, targetQuestionnaireId, categoricalQuestion.CategoriesId.Value);
        }
    }
}
