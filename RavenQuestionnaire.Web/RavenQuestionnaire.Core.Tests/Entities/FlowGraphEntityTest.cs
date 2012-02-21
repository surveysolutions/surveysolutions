#region

using System;
using System.Collections.Generic;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

#endregion

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class FlowGraphEntityTest
    {
        [Test]
        public void UpdateFlowGraph()
        {
            var innerDocument = new FlowGraphDocument();
            var graph = new FlowGraph(innerDocument);

            var g1 = Guid.NewGuid();
            var g2 = Guid.NewGuid();
            var g3 = Guid.NewGuid();

            var blocks = new List<FlowBlock>
                             {
                                 new FlowBlock(g1),
                                 new FlowBlock(g2),
                                 new FlowBlock(g3)
                             };
            var connections = new List<FlowConnection>
                                  {
                                      new FlowConnection(g1, g2),
                                      new FlowConnection(g2, g3)
                                  };
            graph.UpdateFlow(blocks, connections);

            Assert.AreEqual(blocks.Count, innerDocument.Blocks.Count);
            Assert.AreEqual(blocks[0].PublicKey, innerDocument.Blocks[0].PublicKey);
            Assert.AreEqual(blocks[1].PublicKey, innerDocument.Blocks[1].PublicKey);
            Assert.AreEqual(blocks[2].PublicKey, innerDocument.Blocks[2].PublicKey);

            Assert.AreEqual(connections.Count, innerDocument.Connections.Count);

            Assert.AreEqual(connections[0].Source, innerDocument.Connections[0].Source);
            Assert.AreEqual(connections[0].Target, innerDocument.Connections[0].Target);
            Assert.AreEqual(connections[1].Source, innerDocument.Connections[1].Source);
            Assert.AreEqual(connections[1].Target, innerDocument.Connections[1].Target);
        }
    }
}