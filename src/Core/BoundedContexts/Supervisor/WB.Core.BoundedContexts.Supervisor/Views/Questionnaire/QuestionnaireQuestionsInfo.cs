using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class QuestionnaireQuestionsInfo : IView
    {
        public Dictionary<Guid, string> QuestionIdToVariableMap { get; set; }
    }
}