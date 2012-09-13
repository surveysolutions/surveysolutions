using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Web.Models
{
    public class FlowGraphClient
    {
        public FlowGraphClient()
        {
            Blocks = new List<FlowBlockClient>();
            Connections = new List<FlowConnectionClient>();
        }

        public List<FlowBlockClient> Blocks { get; set; }
        public List<FlowConnectionClient> Connections { get; set; }
        
        public Guid? ParentPublicKey { get; set; }

        public void Calc(string parentCondition)
        {
            foreach (var connection in Connections)
            {
                connection.SourceBlock = Blocks.Single(b => b.PublicKey == connection.Source);
            }
            foreach (var block in Blocks)
            {
                block.InputConnections = Connections.Where(c => c.Target == block.PublicKey).ToList();
                block.CalculateCondition(parentCondition);
            }
        }
    }

    public class FlowConnectionClient
    {
        public FlowConnectionClient()
        {
        }

        public Guid Source { get; set; }
        public Guid Target { get; set; }
        public string LabelText { get; set; }
        public string Condition { get; set; }
        public FlowBlockClient SourceBlock { get; set; }

        public FlowConnection Convert()
        {
            var t = this;
            return new FlowConnection
                       {
                           Condition = t.Condition,
                           LabelText = t.LabelText,
                           Source = t.Source,
                           Target = t.Target
                       };
        }
    }
    public class FlowBlockClient
    {
        public FlowBlockClient()
        {
            Graphs = new List<FlowGraphClient>();
            InputConnections = new List<FlowConnectionClient>();
        }
        public int Height { get; set; }
        public int Width { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public Guid PublicKey { get; set; }
        public string Condition { get; set; }
        public bool IsQuestion { get; set; }

        public List<FlowConnectionClient> InputConnections { get; set; }
        public List<FlowGraphClient> Graphs { get; set; }

        public void CalculateCondition(string parentCondition)
        {
            Condition = string.Empty;
            switch (InputConnections.Count)
            {
                case 0:
                    Condition = parentCondition;
                    break;
                case 1:
                    {
                        var input = InputConnections[0];
                        var andList = new List<string> { input.Condition, input.SourceBlock.Condition };
                        Condition = andList.Join(" and ");
                    }
                    break;
                default:
                    {
                        var orList = new List<string>();
                        foreach (var input in InputConnections)
                        {
                            var andList = new List<string>{input.Condition, input.SourceBlock.Condition};
                            orList.Add(andList.Join(" and "));
                        }
                        Condition = orList.Join(" or ");
                    }
                    break;
            }
            if (Graphs.Count>0)
            {
                foreach (var graph in Graphs)
                {
                    graph.Calc(Condition);
                }
            }
        }

        public FlowBlock Convert()
        {
            var t = this;
            return new FlowBlock
            {
                Height = t.Height,
                Width = t.Width,
                Top = t.Top,
                Left = t.Left,
                PublicKey = t.PublicKey
            };
        }
    }

    public static class ListHelpers
    {
        public static string Join(this List<string> list, string d)
        {
            var notEmpty = list.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            if (notEmpty.Count > 1)
                notEmpty = notEmpty.Select(s => "(" + s + ")").ToList();
            return string.Join(d, notEmpty.ToArray());
        }
    }
}

