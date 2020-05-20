using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class SearchPostProcessors:
        ICommandPostProcessor<Questionnaire, ImportQuestionnaire>,
        ICommandPostProcessor<Questionnaire, CloneQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateQuestionnaire>,
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>,
        ICommandPostProcessor<Questionnaire, RevertVersionQuestionnaire>,

        ICommandPostProcessor<Questionnaire, AddStaticText>,
        ICommandPostProcessor<Questionnaire, UpdateStaticText>,
        ICommandPostProcessor<Questionnaire, DeleteStaticText>,

        ICommandPostProcessor<Questionnaire, AddOrUpdateTranslation>,
        ICommandPostProcessor<Questionnaire, DeleteTranslation>,

        ICommandPostProcessor<Questionnaire, AddGroup>,
        ICommandPostProcessor<Questionnaire, UpdateGroup>,
        ICommandPostProcessor<Questionnaire, DeleteGroup>,

        ICommandPostProcessor<Questionnaire, PasteAfter>,
        ICommandPostProcessor<Questionnaire, PasteInto>,

        ICommandPostProcessor<Questionnaire, AddDefaultTypeQuestion>,
        ICommandPostProcessor<Questionnaire, DeleteQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMultimediaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateDateTimeQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateNumericQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateQRBarcodeQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateGpsCoordinatesQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateTextListQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateTextQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateMultiOptionQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateSingleOptionQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateCascadingComboboxOptions>,
        ICommandPostProcessor<Questionnaire, UpdateFilteredComboboxOptions>,
        ICommandPostProcessor<Questionnaire, UpdateAreaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateAudioQuestion>,
        ICommandPostProcessor<Questionnaire, ReplaceOptionsWithClassification>,

        ICommandPostProcessor<Questionnaire, ReplaceTextsCommand>,

        ICommandPostProcessor<Questionnaire, AddVariable>,
        ICommandPostProcessor<Questionnaire, UpdateVariable>,
        ICommandPostProcessor<Questionnaire, DeleteVariable>,

        ICommandPostProcessor<Questionnaire, AddOrUpdateCategories>,
        ICommandPostProcessor<Questionnaire, DeleteCategories>
    {
        private readonly IQuestionnaireSearchStorage searchStorage;

        public SearchPostProcessors(IQuestionnaireSearchStorage searchStorage)
        {
            this.searchStorage = searchStorage;
        }

        public void Process(Questionnaire aggregate, ImportQuestionnaire command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, CloneQuestionnaire command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateQuestionnaire command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            GetSearchStorage().RemoveAllEntities(aggregate.Id);
        }

        private IQuestionnaireSearchStorage GetSearchStorage()
        {
            return this.searchStorage;
        }

        public void Process(Questionnaire aggregate, AddStaticText command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.EntityId);
        }

        public void Process(Questionnaire aggregate, UpdateStaticText command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.EntityId);
        }

        public void Process(Questionnaire aggregate, DeleteStaticText command)
        {
            GetSearchStorage().Remove(aggregate.Id, command.EntityId);
        }

        public void Process(Questionnaire aggregate, AddOrUpdateTranslation command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, DeleteTranslation command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, AddGroup command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.GroupId);
        }

        public void Process(Questionnaire aggregate, UpdateGroup command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.GroupId);
        }

        public void Process(Questionnaire aggregate, DeleteGroup command)
        {
            GetSearchStorage().Remove(aggregate.Id, command.GroupId);
        }

        public void Process(Questionnaire aggregate, PasteAfter command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, PasteInto command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, AddDefaultTypeQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, DeleteQuestion command)
        {
            GetSearchStorage().Remove(aggregate.Id, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateMultimediaQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateDateTimeQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateNumericQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateQRBarcodeQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateGpsCoordinatesQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateTextListQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateTextQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateMultiOptionQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateSingleOptionQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateCascadingComboboxOptions command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, ReplaceOptionsWithClassification command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateFilteredComboboxOptions command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, RevertVersionQuestionnaire command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, UpdateAreaQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateAudioQuestion command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, ReplaceTextsCommand command)
        {
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        }

        public void Process(Questionnaire aggregate, AddVariable command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.EntityId);
        }

        public void Process(Questionnaire aggregate, UpdateVariable command)
        {
            UpdateEntity(aggregate.QuestionnaireDocument, command.EntityId);
        }

        public void Process(Questionnaire aggregate, DeleteVariable command)
        {
            GetSearchStorage().Remove(aggregate.Id, command.EntityId);
        }

        private void UpdateEntity(QuestionnaireDocument questionnaireDocument, Guid entityId)
        {
            if (!questionnaireDocument.IsPublic)
                return;

            var entity = questionnaireDocument.Find<IComposite>(entityId);
            if (entity == null)
                throw new InvalidOperationException("Entity was not found.");

            var title = GetEntityTitle(entity);
            if (!string.IsNullOrWhiteSpace(title))
            {
                GetSearchStorage().AddOrUpdateEntity(questionnaireDocument.PublicKey, entity);
            }
        }

        private void RewriteQuestionnaireEntities(QuestionnaireDocument questionnaireDocument)
        {
            var questionnaireSearchStorage = GetSearchStorage();
            questionnaireSearchStorage.RemoveAllEntities(questionnaireDocument.PublicKey);

            if (!questionnaireDocument.IsPublic)
                return;

            var entities = questionnaireDocument.Children.TreeToEnumerable(e => e.Children);
            foreach (var entity in entities)
            {
                var title = GetEntityTitle(entity);
                if (string.IsNullOrWhiteSpace(title))
                    continue;

                questionnaireSearchStorage.AddOrUpdateEntity(questionnaireDocument.PublicKey, entity);
            }
        }

        public static string? GetEntityTitle(IQuestionnaireEntity entity)
            => (entity as IQuestion)?.QuestionText
               ?? (entity as IStaticText)?.Text
               ?? (entity as IGroup)?.Title
               ?? (entity as IVariable)?.Label;

        public void Process(Questionnaire aggregate, AddOrUpdateCategories command) =>
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
        public void Process(Questionnaire aggregate, DeleteCategories command) =>
            RewriteQuestionnaireEntities(aggregate.QuestionnaireDocument);
    }
}
