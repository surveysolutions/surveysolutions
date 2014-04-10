using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.Numeric;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.QRBarcode;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question.TextList;
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
                        {"UpdateQuestionnaire", typeof (UpdateQuestionnaireCommand)},
                        {"UpdateGroup", typeof (UpdateGroupCommand)},
                        {"AddGroup", typeof (AddGroupCommand)},
                        {"CloneGroupWithoutChildren", typeof (CloneGroupWithoutChildrenCommand)},
                        {"DeleteGroup", typeof (DeleteGroupCommand)},
                        {"MoveGroup", typeof (MoveGroupCommand)},
                        {"UpdateQuestion", typeof (UpdateQuestionCommand)},
                        {"UpdateNumericQuestion", typeof (UpdateNumericQuestionCommand)},
                        {"UpdateTextListQuestion", typeof (UpdateTextListQuestionCommand)},
                        {"AddQuestion", typeof (AddQuestionCommand)},
                        {"AddNumericQuestion", typeof (AddNumericQuestionCommand)},
                        {"AddTextListQuestion", typeof (AddTextListQuestionCommand)},
                        {"CloneQuestion", typeof (CloneQuestionCommand)},
                        {"CloneNumericQuestion", typeof (CloneNumericQuestionCommand)},
                        {"CloneTextListQuestion", typeof (CloneTextListQuestionCommand)},
                        {"DeleteQuestion", typeof (DeleteQuestionCommand)},
                        {"MoveQuestion", typeof (MoveQuestionCommand)},
                        {"AddSharedPersonToQuestionnaire", typeof (AddSharedPersonToQuestionnaireCommand)},
                        {"RemoveSharedPersonFromQuestionnaire", typeof (RemoveSharedPersonFromQuestionnaireCommand)},
                        {"AddQRBarcodeQuestion", typeof (AddQRBarcodeQuestionCommand)},
                        {"UpdateQRBarcodeQuestion", typeof (UpdateQRBarcodeQuestionCommand)},
                        {"CloneQRBarcodeQuestion", typeof (CloneQRBarcodeQuestionCommand)},
                    };
            }

        }
    }
}
