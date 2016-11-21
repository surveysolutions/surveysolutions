using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    [Obsolete("KP-8160: Used only for migrations")]
    public class QuestionnaireSharedPersons
    {
        public Guid QuestionnaireId { get; private set; }
        public List<SharedPerson> SharedPersons { get; private set; }

        public QuestionnaireSharedPersons(Guid questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
            this.SharedPersons = new List<SharedPerson>();
        }
    }
}