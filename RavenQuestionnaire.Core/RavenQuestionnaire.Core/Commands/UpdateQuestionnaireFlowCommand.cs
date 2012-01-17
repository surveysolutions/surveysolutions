using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Answer;

namespace RavenQuestionnaire.Core.Commands
{
    public class UpdateQuestionnaireFlowCommand : ICommand
    {
        public string QuestionnaireId { get; set; }
        public List<FlowBlock> Blocks { get; set; }
        public List<FlowConnection> Connections { get; set; }
        public UserLight Executor { get; set; }

        public UpdateQuestionnaireFlowCommand(string questionnaireId, List<FlowBlock> blocks, List<FlowConnection> connections, UserLight executor)
        {
            this.QuestionnaireId = IdUtil.CreateQuestionnaireId(questionnaireId);
            this.Blocks = blocks;
            this.Connections = connections;
            this.Executor = executor;
        }
    }
}
