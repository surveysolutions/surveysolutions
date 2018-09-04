using System;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Implementation.Repositories;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class HistoryPreProcessor :
        ICommandPreProcessor<Questionnaire, ImportQuestionnaire>,
        ICommandPreProcessor<Questionnaire, CloneQuestionnaire>,
        ICommandPreProcessor<Questionnaire, CreateQuestionnaire>,
        ICommandPreProcessor<Questionnaire, UpdateQuestionnaire>,
        ICommandPreProcessor<Questionnaire, DeleteQuestionnaire>,
        ICommandPreProcessor<Questionnaire, AddSharedPersonToQuestionnaire>,
        ICommandPreProcessor<Questionnaire, RemoveSharedPersonFromQuestionnaire>,
        ICommandPreProcessor<Questionnaire, AddStaticText>,
        ICommandPreProcessor<Questionnaire, UpdateStaticText>,
        ICommandPreProcessor<Questionnaire, MoveStaticText>,
        ICommandPreProcessor<Questionnaire, DeleteStaticText>,
        ICommandPreProcessor<Questionnaire, AddVariable>,
        ICommandPreProcessor<Questionnaire, UpdateVariable>,
        ICommandPreProcessor<Questionnaire, MoveVariable>,
        ICommandPreProcessor<Questionnaire, DeleteVariable>,
        ICommandPreProcessor<Questionnaire, AddMacro>,
        ICommandPreProcessor<Questionnaire, UpdateMacro>,
        ICommandPreProcessor<Questionnaire, DeleteMacro>,
        ICommandPreProcessor<Questionnaire, AddOrUpdateAttachment>,
        ICommandPreProcessor<Questionnaire, DeleteAttachment>,
        ICommandPreProcessor<Questionnaire, AddOrUpdateTranslation>,
        ICommandPreProcessor<Questionnaire, DeleteTranslation>,
        ICommandPreProcessor<Questionnaire, SetDefaultTranslation>,
        ICommandPreProcessor<Questionnaire, AddGroup>,
        ICommandPreProcessor<Questionnaire, UpdateGroup>,
        ICommandPreProcessor<Questionnaire, MoveGroup>,
        ICommandPreProcessor<Questionnaire, DeleteGroup>,
        ICommandPreProcessor<Questionnaire, PasteAfter>,
        ICommandPreProcessor<Questionnaire, PasteInto>,
        ICommandPreProcessor<Questionnaire, AddDefaultTypeQuestion>,
        ICommandPreProcessor<Questionnaire, DeleteQuestion>,
        ICommandPreProcessor<Questionnaire, MoveQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateMultimediaQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateDateTimeQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateNumericQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateQRBarcodeQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateGpsCoordinatesQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateTextListQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateTextQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateMultiOptionQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateSingleOptionQuestion>,
        ICommandPreProcessor<Questionnaire, AddLookupTable>,
        ICommandPreProcessor<Questionnaire, UpdateLookupTable>,
        ICommandPreProcessor<Questionnaire, DeleteLookupTable>,
        ICommandPreProcessor<Questionnaire, ReplaceTextsCommand>,
        ICommandPreProcessor<Questionnaire, RevertVersionQuestionnaire>,
        ICommandPreProcessor<Questionnaire, UpdateAreaQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateAudioQuestion>,
        ICommandPreProcessor<Questionnaire, UpdateMetadata>
    {
        private readonly OriginalQuestionnaireStorage originalQuestionnaireStorage;
        private readonly IEntitySerializer<QuestionnaireDocument> serializer;

        public HistoryPreProcessor(
            OriginalQuestionnaireStorage originalQuestionnaireStorage,
            IEntitySerializer<QuestionnaireDocument> serializer
            )
        {
            this.originalQuestionnaireStorage = originalQuestionnaireStorage;
            this.serializer = serializer;
        }

        public void Process(Questionnaire aggregate, ImportQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        private void StoreOriginalDocument(Questionnaire aggregate)
        {
            this.originalQuestionnaireStorage.Push(this.serializer.Serialize(aggregate.QuestionnaireDocument));
        }

        public void Process(Questionnaire aggregate, CloneQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, CreateQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddSharedPersonToQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, RemoveSharedPersonFromQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddStaticText command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateStaticText command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, MoveStaticText command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteStaticText command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddVariable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateVariable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, MoveVariable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteVariable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddMacro command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateMacro command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteMacro command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddOrUpdateAttachment command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteAttachment command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddOrUpdateTranslation command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteTranslation command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, SetDefaultTranslation command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddGroup command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateGroup command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, MoveGroup command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteGroup command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, PasteAfter command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, PasteInto command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddDefaultTypeQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, MoveQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateMultimediaQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateDateTimeQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateNumericQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateQRBarcodeQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateGpsCoordinatesQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateTextListQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateTextQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateMultiOptionQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateSingleOptionQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, AddLookupTable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateLookupTable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, DeleteLookupTable command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, ReplaceTextsCommand command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, RevertVersionQuestionnaire command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateAreaQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateAudioQuestion command)
        {
            StoreOriginalDocument(aggregate);
        }

        public void Process(Questionnaire aggregate, UpdateMetadata command)
        {
            StoreOriginalDocument(aggregate);
        }
    }
}
