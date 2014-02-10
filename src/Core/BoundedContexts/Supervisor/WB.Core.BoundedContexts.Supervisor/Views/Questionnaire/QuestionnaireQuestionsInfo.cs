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
        public QuestionnaireQuestionsInfo(QuestionnaireDocument questionnaire)
        {
            this.GuidToVariableMap = questionnaire.Find<IQuestion>(question => true)
                .ToDictionary(x => x.PublicKey, x => x.StataExportCaption);
        }

        public Dictionary<Guid, string> GuidToVariableMap { get; set; }
    }
}