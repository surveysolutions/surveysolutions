using System.Collections.Generic;
using Ncqrs.Commanding;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Commands.Questionnaire.Flow
{
    //check and rewrite
    public class UpdateQuestionnaireFlowCommand : CommandBase
    {
        public UpdateQuestionnaireFlowCommand(string questionnaireId, List<FlowBlock> blocks,
                                              List<FlowConnection> connections, UserLight executor)
        {
            QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            FlowGraphId = IdUtil.CreateFlowGraphId(questionnaireId);
            Blocks = blocks;
            Connections = connections;
            Executor = executor;
        }

        public string QuestionnaireId { get; set; }
        public string FlowGraphId { get; set; }
        public List<FlowBlock> Blocks { get; set; }
        public List<FlowConnection> Connections { get; set; }

        #region ICommand Members

        public UserLight Executor { get; set; }

        #endregion
    }
}