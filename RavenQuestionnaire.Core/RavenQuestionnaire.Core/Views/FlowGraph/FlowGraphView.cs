using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.Group
{
    public abstract class AbstractFlowGraphView
    {
        public AbstractFlowGraphView()
        {
            Blocks = new FlowBlock[] { };
            Connections = new FlowConnection[] { };
        }
        public AbstractFlowGraphView(string questionnaireId)
        {
            this.QuestionnaireId = questionnaireId;
        }
        protected AbstractFlowGraphView(IQuestionnaireDocument doc)
        {
            this.QuestionnaireId = doc.Id;
        }

        public string QuestionnaireId
        {
            get { return IdUtil.ParseId(_questionnaireId); }
            set { _questionnaireId = value; }
        }
        private string _questionnaireId;

        public FlowBlock[] Blocks { get; set; }

        public FlowConnection[] Connections { get; set; }
    }

    public abstract class FlowGraphView<TFlowBlock, TFlowConnection> : AbstractFlowGraphView
        where TFlowBlock : IFlowBlock
        where TFlowConnection : IFlowConnection
    {
        public FlowGraphView()
        {
        }

        public FlowGraphView(string questionnaireId)
            : base(questionnaireId)
        {
        }

        public FlowGraphView(IQuestionnaireDocument doc)
            : base(doc)
        {
        }
    }

    public class FlowGraphView : FlowGraphView<FlowBlock, FlowConnection>
    {
        public FlowGraphView()
        {
        }

        public FlowGraphView(string questionnaireId)
            : base(questionnaireId)
        {
        }

        public FlowGraphView(QuestionnaireDocument doc)
            : base(doc)
        {
            if (doc.FlowGraph == null)
            {

                var blocks = new List<FlowBlock>();
                var top = 0;
                foreach (var question in doc.Questions)
                {
                    blocks.Add(new FlowBlock(question) {Top = top});
                    top += 120;
                }
                this.Blocks = blocks.ToArray();
                this.Connections = new FlowConnection[0];
                return;
            }
            this.Blocks = doc.FlowGraph.Blocks.ToArray();
            this.Connections = doc.FlowGraph.Connections.ToArray();
        }
    }
}
