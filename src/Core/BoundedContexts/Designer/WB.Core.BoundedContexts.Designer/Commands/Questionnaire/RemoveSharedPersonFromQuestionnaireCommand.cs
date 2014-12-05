﻿using System;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Base;

namespace WB.Core.BoundedContexts.Designer.Commands.Questionnaire
{
    [Serializable]
    public class RemoveSharedPersonFromQuestionnaireCommand : QuestionnaireCommand
    {
        public RemoveSharedPersonFromQuestionnaireCommand(Guid questionnaireId, Guid personId, string email, Guid responsibleId)
            : base(questionnaireId, responsibleId)
        {
            this.Email = email;
            this.PersonId = personId;
        }

        public Guid PersonId { get; set; }
        public string Email { get; private set; }
    }
}