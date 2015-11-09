﻿using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.CommandDeserialization
{
    internal class DesignerCommandDeserializer : CommandDeserializer
    {
        protected override Dictionary<string, Type> KnownCommandTypes
        {
            get
            {
                return new Dictionary<string, Type>
                {
                    { "UpdateQuestionnaire", typeof (UpdateQuestionnaireCommand) },
                    { "UpdateGroup", typeof (UpdateGroupCommand) },
                    { "AddGroup", typeof (AddGroupCommand) },
                    { "CloneGroupWithoutChildren", typeof (CloneGroupWithoutChildrenCommand) },
                    { "CloneGroup", typeof (CloneGroupCommand) },
                    { "CloneStaticText", typeof (CloneStaticTextCommand) },
                    { "DeleteGroup", typeof (DeleteGroupCommand) },
                    { "MoveGroup", typeof (MoveGroupCommand) },
                    { "AddDefaultTypeQuestion", typeof (AddDefaultTypeQuestionCommand) },
                    { "CloneQuestionById", typeof(CloneQuestionByIdCommand) },
                    { "DeleteQuestion", typeof (DeleteQuestionCommand) },
                    { "MoveQuestion", typeof (MoveQuestionCommand) },
                    { "AddSharedPersonToQuestionnaire", typeof (AddSharedPersonToQuestionnaireCommand) },
                    { "RemoveSharedPersonFromQuestionnaire", typeof (RemoveSharedPersonFromQuestionnaireCommand) },
                    //Update questions command
                    { "UpdateTextQuestion", typeof (UpdateTextQuestionCommand) },
                    { "UpdateNumericQuestion", typeof (UpdateNumericQuestionCommand) },
                    { "UpdateDateTimeQuestion", typeof (UpdateDateTimeQuestionCommand) },
                    { "UpdateTextListQuestion", typeof (UpdateTextListQuestionCommand) },
                    { "UpdateQRBarcodeQuestion", typeof (UpdateQRBarcodeQuestionCommand) },
                    { "UpdateMultimediaQuestion", typeof (UpdateMultimediaQuestionCommand) },
                    { "UpdateMultiOptionQuestion", typeof (UpdateMultiOptionQuestionCommand) },
                    { "UpdateSingleOptionQuestion", typeof (UpdateSingleOptionQuestionCommand) },
                    { "UpdateGpsCoordinatesQuestion", typeof (UpdateGpsCoordinatesQuestionCommand) },
                    { "UpdateFilteredComboboxOptions", typeof (UpdateFilteredComboboxOptionsCommand) },
                    //Static text commands
                    { "AddStaticText", typeof (AddStaticTextCommand) },
                    { "UpdateStaticText", typeof (UpdateStaticTextCommand) },
                    { "DeleteStaticText", typeof (DeleteStaticTextCommand) },
                    { "MoveStaticText", typeof (MoveStaticTextCommand) },
                    { "MigrateExpressionsToCSharp", typeof(MigrateExpressionsToCSharp)},

                    {"PasteItemAfter", typeof(PasteItemAfterCommand) },
                    {"PasteItemInto", typeof(PasteItemIntoCommand) },
                    //Macro commands
                    { "AddMacros", typeof (AddMacroCommand) },
                    { "UpdateMacros", typeof (UpdateMacroCommand) },
                    { "DeleteMacros", typeof (DeleteMacroCommand) }
                };
            }
        }
    }
}