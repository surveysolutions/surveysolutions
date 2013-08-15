using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons
{
    public class QuestionnaireSharedPersons : IView
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