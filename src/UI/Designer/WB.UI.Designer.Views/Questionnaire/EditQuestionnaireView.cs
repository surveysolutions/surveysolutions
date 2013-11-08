using System;
using System.Collections.Generic;
using System.Linq;

using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class EditQuestionnaireView
    {
        public class NodeWithParent
        {
            public IComposite Node { get; set; }
            public Guid? ParentId { get; set; }
            public int Level { get; set; }
        }

        public EditQuestionnaireView(QuestionnaireDocument source)
        {
            CreatedBy = source.CreatedBy;
            CreationDate = source.CreationDate;
            LastEntryDate = source.LastEntryDate;
            Id = source.PublicKey;
            IsPublic = source.IsPublic;
            Title = source.Title;

            var questions = new List<NodeWithParent>();
            var groups = new List<NodeWithParent>();

            var treeStack = new Stack<NodeWithParent>();
            treeStack.Push(new NodeWithParent { Node = source, Level = -1 });
            while (treeStack.Count > 0)
            {
                var node = treeStack.Pop();

                foreach (var child in node.Node.Children)
                {
                    if (child is IGroup)
                    {
                        var nodeWithParent = new NodeWithParent
                        {
                            Node = child,
                            ParentId = node.Node.PublicKey,
                            Level = node.Level + 1
                        };
                        groups.Add(nodeWithParent);
                        treeStack.Push(nodeWithParent);
                    }
                    else if (child is IQuestion)
                    {
                        questions.Add(new NodeWithParent
                        {
                            Node = child,
                            ParentId = node.Node.PublicKey
                        });
                    }
                }
            }
            Chapters = source.Children.Select(composite => new QuestionnaireEntityNode
            {
                Id = composite.PublicKey,
                Type = QuestionnaireEntityType.Group
            }).ToList();

            Groups = groups.Select(@group => new EditGroupView(@group.Node as IGroup, group.ParentId, group.Level)).ToList();
            Questions = questions.Select(question => new EditQuestionView(question.Node as IQuestion, question.ParentId)).ToList();
        }

        public List<QuestionnaireEntityNode> Chapters { get; set; }

        public List<EditGroupView> Groups { get; set; }

        public List<EditQuestionView> Questions { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public Guid? Parent { get; set; }

        public Guid Id { get; set; }

        public string Title { get; set; }

        public bool IsPublic { get; set; }
    }
}

