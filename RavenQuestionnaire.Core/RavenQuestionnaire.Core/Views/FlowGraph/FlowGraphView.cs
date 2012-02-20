using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Group;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Views.Questionnaire;

namespace RavenQuestionnaire.Core.Views.FlowGraph
{
    public class FlowGraphView
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public FlowBlockViewItem Canvas { get; set; }
        public FlowConnection[] Connections { get; set; }

        public FlowGraphView()
        {
            Canvas = new FlowBlockViewItem { Id = "canvas" };
            Connections = new FlowConnection[] { };
        }

        public FlowGraphView(FlowGraphDocument flow, QuestionnaireView q)
            : this()
        {
            this.Id = IdUtil.ParseId(q.Id);
            this.Title = q.Title;

            var blocks = AddGroup(Canvas, q.Questions, q.Groups);

            var ids = blocks.Select(b => b.Id).Distinct().ToList();

            var connections = new List<FlowConnection>();

            if (flow != null && flow.Blocks.Count > 0)
            {
                foreach (var block in blocks)
                {
                    var fBlock = flow.Blocks.FirstOrDefault(b => b.QuestionId.ToString() == block.Id);
                    if (fBlock == null)
                    {
                        block.Left = 0;
                        block.Top = 0;
                    }
                    else
                    {
                        block.Width = fBlock.Width;
                        block.Height = fBlock.Height;
                        block.Left = fBlock.Left;
                        block.Top = fBlock.Top;
                    }
                }

                foreach (var flowConnection in flow.Connections)
                {
                    if (ids.Contains(flowConnection.Source.ToString()) &&
                    ids.Contains(flowConnection.Target.ToString()))
                    {
                        connections.Add(flowConnection);
                    }
                }

            }
            else
            {
                LayoutFlow(Canvas);
            }

            this.Connections = connections.ToArray();

            Canvas.Width = Math.Max(Canvas.InnerWidth, 900);
            Canvas.Height = Canvas.InnerHeight + 100;
        }

        private void LayoutFlow(FlowBlockViewItem parent)
        {
            var top = 50;
            var width = 0;
            foreach (var block in parent.Blocks)
            {
                block.Top = top;
                block.Left = 10;
                if (!block.IsQuestion)
                {
                    LayoutFlow(block);
                }
                top += block.Height + 20;
                width = Math.Max(width, block.Width + 20);
            }
            parent.Height = top + 50;
            parent.Width = width;
        }

        private static List<FlowBlockViewItem> AddGroup(FlowBlockViewItem parent, IEnumerable<AbstractQuestionView> questions, IEnumerable<GroupView> groups)
        {
            var list = new List<FlowBlockViewItem>();
            foreach (var b in questions.Select(question => new FlowBlockViewItem()
                                                               {
                                                                   Id = question.PublicKey.ToString(),
                                                                   Scope = parent.Id,
                                                                   Question = question.QuestionText
                                                               }))
            {
                parent.Blocks.Add(b);
                list.Add(b);
            }

            foreach (var group in groups)
            {
                var gBlock = new FlowBlockViewItem()
                                 {
                                     Id = group.PublicKey.ToString(),
                                     Scope = parent.Id,
                                     Title = group.GroupText
                                 };
                var blocks = AddGroup(gBlock, group.Questions, group.Groups);
                parent.Blocks.Add(gBlock);
                list.Add(gBlock);
                list.AddRange(blocks);
            }
            return list;
        }
    }
}
