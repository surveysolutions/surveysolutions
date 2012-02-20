using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Entities
{
    public class FlowGraph : IEntity<FlowGraphDocument>
    {
        private FlowGraphDocument innerDocument;

        //public string QuestionnaireId { get { return innerDocument; } }

        public FlowGraph(string questionnaireId)
        {
            var fId = IdUtil.CreateFlowGraphId(IdUtil.ParseId(questionnaireId));
            innerDocument = new FlowGraphDocument
                                {
                                    Id = fId,
                                    QuestionnaireDocumentId = questionnaireId
                                };
        }

        public FlowGraph(Questionnaire questionnaire)
            : this(questionnaire.QuestionnaireId)
        {
            //Create new from questionnaire
        }

        public FlowGraph(FlowGraphDocument innerDocument)
        {
            this.innerDocument = innerDocument;
        }

        public FlowGraphDocument GetInnerDocument()
        {
            return innerDocument;
        }

        public void UpdateFlow(List<FlowBlock> blocks, List<FlowConnection> connections)
        {
            this.innerDocument.Blocks = blocks;
            this.innerDocument.Connections = connections;
            this.innerDocument.LastEntryDate = DateTime.Now;
        }
    }
}