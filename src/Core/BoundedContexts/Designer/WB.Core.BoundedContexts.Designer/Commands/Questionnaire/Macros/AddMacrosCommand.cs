using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros
{
    [Serializable]
    public class AddMacrosCommand : QuestionnaireEntityCommand
    {
        public AddMacrosCommand(Guid questionnaireId, Guid entityId, string text, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
        }
    }

    [Serializable]
    public class DeleteMacrosCommand : QuestionnaireEntityCommand
    {
        public DeleteMacrosCommand(Guid questionnaireId, Guid entityId, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
        }
    }

    [Serializable]
    public class UpdateMacrosCommand : QuestionnaireEntityCommand
    {
        public UpdateMacrosCommand(Guid questionnaireId, Guid entityId, string name, string expression, string description, Guid responsibleId)
            : base(responsibleId: responsibleId, questionnaireId: questionnaireId, entityId: entityId)
        {
            Name = name;
            Expression = expression;
            Description = description;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Expression { get; set; }
    }
}
