using System;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class SearchPostProcessors:
        ICommandPostProcessor<Questionnaire, ImportQuestionnaire>,
        ICommandPostProcessor<Questionnaire, CloneQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateQuestionnaire>,
        ICommandPostProcessor<Questionnaire, DeleteQuestionnaire>,
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
        ICommandPostProcessor<Questionnaire, ReplaceOptionsWithClassification>,
        ICommandPostProcessor<Questionnaire, UpdateFilteredComboboxOptions>,
        ICommandPostProcessor<Questionnaire, RevertVersionQuestionnaire>,
        ICommandPostProcessor<Questionnaire, UpdateAreaQuestion>,
        ICommandPostProcessor<Questionnaire, UpdateAudioQuestion>,
        ICommandPostProcessor<Questionnaire, ReplaceTextsCommand>
    {
        private IQuestionnaireSearchStorage SearchStorage =>
            ServiceLocator.Current.GetInstance<IQuestionnaireSearchStorage>();

        private IPlainKeyValueStorage<QuestionnaireDocument> QuestionnaireStorage =>
            ServiceLocator.Current.GetInstance<IPlainKeyValueStorage<QuestionnaireDocument>>();

        public void Process(Questionnaire aggregate, ImportQuestionnaire command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, CloneQuestionnaire command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, UpdateQuestionnaire command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            SearchStorage.RemoveAllEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, AddStaticText command)
        {
            UpdateEntity(command.QuestionnaireId, command.EntityId);
        }

        public void Process(Questionnaire aggregate, UpdateStaticText command)
        {
            UpdateEntity(command.QuestionnaireId, command.EntityId);
        }

        public void Process(Questionnaire aggregate, DeleteStaticText command)
        {
            SearchStorage.Remove(command.QuestionnaireId, command.EntityId);
        }

        public void Process(Questionnaire aggregate, AddOrUpdateTranslation command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, DeleteTranslation command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, AddGroup command)
        {
            UpdateEntity(command.QuestionnaireId, command.GroupId);
        }

        public void Process(Questionnaire aggregate, UpdateGroup command)
        {
            UpdateEntity(command.QuestionnaireId, command.GroupId);
        }

        public void Process(Questionnaire aggregate, DeleteGroup command)
        {
            SearchStorage.Remove(command.QuestionnaireId, command.GroupId);
        }

        public void Process(Questionnaire aggregate, PasteAfter command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, PasteInto command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, AddDefaultTypeQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, DeleteQuestion command)
        {
            SearchStorage.Remove(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateMultimediaQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateDateTimeQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateNumericQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateQRBarcodeQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateGpsCoordinatesQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateTextListQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateTextQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateMultiOptionQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateSingleOptionQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateCascadingComboboxOptions command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, ReplaceOptionsWithClassification command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateFilteredComboboxOptions command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, RevertVersionQuestionnaire command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        public void Process(Questionnaire aggregate, UpdateAreaQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, UpdateAudioQuestion command)
        {
            UpdateEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, ReplaceTextsCommand command)
        {
            RewriteQuestionnaireEntities(command.QuestionnaireId);
        }

        private void UpdateEntity(Guid questionnaireId, Guid entityId)
        {
            var questionnaireDocument = QuestionnaireStorage.GetById(questionnaireId.FormatGuid());
            if (!questionnaireDocument.IsPublic)
                return;

            var entity = questionnaireDocument.Find<IComposite>(entityId);
            SearchStorage.AddOrUpdateEntity(questionnaireId, entity);
        }

        private void RewriteQuestionnaireEntities(Guid questionnaireId)
        {
            SearchStorage.RemoveAllEntities(questionnaireId);

            var questionnaireDocument = QuestionnaireStorage.GetById(questionnaireId.FormatGuid());
            if (!questionnaireDocument.IsPublic)
                return;

            var entities = questionnaireDocument.Children.TreeToEnumerable(e => e.Children);
            foreach (var entity in entities)
            {
                SearchStorage.AddOrUpdateEntity(questionnaireId, entity);
            }
        }
    }
}
