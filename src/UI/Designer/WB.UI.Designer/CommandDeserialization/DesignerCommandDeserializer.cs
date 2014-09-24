using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.DateTime;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.GpsCoordinates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Mulimedia;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.MultiOption;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Numeric;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.QRBarcode;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.SingleOption;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Text;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList;
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
                    { "DeleteGroup", typeof (DeleteGroupCommand) },
                    { "MoveGroup", typeof (MoveGroupCommand) },
                    { "UpdateQuestion", typeof (UpdateQuestionCommand) },
                    { "AddQuestion", typeof (AddQuestionCommand) },
                    { "AddNumericQuestion", typeof (AddNumericQuestionCommand) },
                    { "AddTextListQuestion", typeof (AddTextListQuestionCommand) },
                    { "CloneQuestion", typeof (CloneQuestionCommand) },
                    { "CloneQuestionById", typeof(CloneQuestionByIdCommand) },
                    { "CloneNumericQuestion", typeof (CloneNumericQuestionCommand) },
                    { "CloneTextListQuestion", typeof (CloneTextListQuestionCommand) },
                    { "DeleteQuestion", typeof (DeleteQuestionCommand) },
                    { "MoveQuestion", typeof (MoveQuestionCommand) },
                    { "AddSharedPersonToQuestionnaire", typeof (AddSharedPersonToQuestionnaireCommand) },
                    { "RemoveSharedPersonFromQuestionnaire", typeof (RemoveSharedPersonFromQuestionnaireCommand) },
                    { "AddQRBarcodeQuestion", typeof (AddQRBarcodeQuestionCommand) },
                    { "CloneQRBarcodeQuestion", typeof (CloneQRBarcodeQuestionCommand) },
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
                    { "CloneStaticText", typeof (CloneStaticTextCommand) },
                    { "DeleteStaticText", typeof (DeleteStaticTextCommand) },
                    { "MoveStaticText", typeof (MoveStaticTextCommand) },
                };
            }

        }
    }
}
