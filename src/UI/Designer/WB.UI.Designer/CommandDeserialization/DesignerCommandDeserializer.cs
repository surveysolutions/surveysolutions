using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.CommandDeserialization
{
    internal class DesignerCommandDeserializer : CommandDeserializer
    {
        public DesignerCommandDeserializer(ILogger logger) : base(logger)
        {
        }

        protected override Dictionary<string, Type> KnownCommandTypes
        {
            get
            {
                return new Dictionary<string, Type>
                {
                    { "UpdateQuestionnaire", typeof (UpdateQuestionnaire) },
                    { "UpdateGroup", typeof (UpdateGroup) },
                    { "AddGroup", typeof (AddGroup) },
                    { "DeleteGroup", typeof (DeleteGroup) },
                    { "MoveGroup", typeof (MoveGroup) },
                    { "AddDefaultTypeQuestion", typeof (AddDefaultTypeQuestion) },
                    { "DeleteQuestion", typeof (DeleteQuestion) },
                    { "MoveQuestion", typeof (MoveQuestion) },
                    { "AddSharedPersonToQuestionnaire", typeof (AddSharedPersonToQuestionnaire) },
                    { "RemoveSharedPersonFromQuestionnaire", typeof (RemoveSharedPersonFromQuestionnaire) },
                    { "ReplaceTexts", typeof (ReplaceTextsCommand) },
                    //Update questions command
                    { "UpdateTextQuestion", typeof (UpdateTextQuestion) },
                    { "UpdateNumericQuestion", typeof (UpdateNumericQuestion) },
                    { "UpdateDateTimeQuestion", typeof (UpdateDateTimeQuestion) },
                    { "UpdateTextListQuestion", typeof (UpdateTextListQuestion) },
                    { "UpdateQRBarcodeQuestion", typeof (UpdateQRBarcodeQuestion) },
                    { "UpdateMultimediaQuestion", typeof (UpdateMultimediaQuestion) },
                    { "UpdateMultiOptionQuestion", typeof (UpdateMultiOptionQuestion) },
                    { "UpdateSingleOptionQuestion", typeof (UpdateSingleOptionQuestion) },
                    { "UpdateGpsCoordinatesQuestion", typeof (UpdateGpsCoordinatesQuestion) },
                    { "UpdateFilteredComboboxOptions", typeof (UpdateFilteredComboboxOptions) },
                    { "UpdateAreaQuestion", typeof (UpdateAreaQuestion) },
                    { "UpdateAudioQuestion", typeof (UpdateAudioQuestion) },
                    
                    //Static text commands
                    { "AddStaticText", typeof (AddStaticText) },
                    { "UpdateStaticText", typeof (UpdateStaticText) },
                    { "DeleteStaticText", typeof (DeleteStaticText) },
                    { "MoveStaticText", typeof (MoveStaticText) },

                    // Variables
                    { "AddVariable", typeof(AddVariable) },
                    { "UpdateVariable", typeof(UpdateVariable) },
                    { "DeleteVariable", typeof(DeleteVariable) },
                    { "MoveVariable", typeof(MoveVariable) },
                    
                    //obsolete
                    { "MigrateExpressionsToCSharp", typeof(MigrateExpressionsToCSharp)},

                    {"PasteAfter", typeof(PasteAfter) },
                    {"PasteInto", typeof(PasteInto) },
                    //Macro commands
                    { "AddMacro", typeof (AddMacro) },
                    { "UpdateMacro", typeof (UpdateMacro) },
                    { "DeleteMacro", typeof (DeleteMacro) },
                     //Lookup table commands
                    { "AddLookupTable", typeof (AddLookupTable) },
                    { "UpdateLookupTable", typeof (UpdateLookupTable) },
                    { "DeleteLookupTable", typeof (DeleteLookupTable) },
                     //Attachment commands
                    { "AddOrUpdateAttachment", typeof (AddOrUpdateAttachment) },
                    { "DeleteAttachment", typeof (DeleteAttachment) },
                     //Translation commands
                    { "AddOrUpdateTranslation", typeof (AddOrUpdateTranslation) },
                    { "DeleteTranslation", typeof (DeleteTranslation) },
                    { "SetDefaultTranslation", typeof (SetDefaultTranslation) },
                    // Metadata
                    { "UpdateMetadata", typeof (UpdateMetadata) },
                };
            }
        }
    }
}
