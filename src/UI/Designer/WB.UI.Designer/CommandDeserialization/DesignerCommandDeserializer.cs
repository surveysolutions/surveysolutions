using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
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
                        {"CloneGroupWithoutChildren", typeof (CloneGroupCommand)},
                        {"DeleteGroup", typeof (DeleteGroupCommand)},
                        {"MoveGroup", typeof (MoveGroupCommand)},
                        {"UpdateQuestion", typeof (UpdateQuestionCommand)},
                        {"AddQuestion", typeof (AddQuestionCommand)},
                        {"CloneQuestion", typeof (CloneQuestionCommand)},
                        {"DeleteQuestion", typeof (DeleteQuestionCommand)},
                        {"MoveQuestion", typeof (MoveQuestionCommand)},
                    };
            }

        }
    }
}
