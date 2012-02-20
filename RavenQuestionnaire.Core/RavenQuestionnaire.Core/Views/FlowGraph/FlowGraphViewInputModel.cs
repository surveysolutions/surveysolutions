using System;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.FlowGraph
{
    public class FlowGraphViewInputModel
    {
        public FlowGraphViewInputModel(string questionnaireId)
        {
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            FlowGraphId = IdUtil.CreateFlowGraphId(IdUtil.ParseId(QuestionnaireId));
        }

        public string QuestionnaireId { get; set; }
        public string FlowGraphId { get; set; }
    }
}
