using System;
using System.Collections.Generic;
using System.Linq;

using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.View;
using Main.Core.View.Question;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class EditQuestionnaireView
    {
        public class NodeWithParent
        {
            public IComposite Node { get; set; }
            public Guid? ParentId { get; set; }
        }

        public EditQuestionnaireView(QuestionnaireDocument source)
        {
            CreatedBy = source.CreatedBy;
            CreationDate = source.CreationDate;
            LastEntryDate = source.LastEntryDate;
            PublicKey = source.PublicKey;
            IsPublic = source.IsPublic;
            Title = source.Title;

            var questions = new List<NodeWithParent>();
            var groups = new List<NodeWithParent>();

            var treeStack = new Stack<IGroup>();
            treeStack.Push(source);
            while (treeStack.Count > 0)
            {
                var node = treeStack.Pop();

                foreach (var child in node.Children)
                {
                    if (child is IGroup)
                    {
                        var nodeWithParent = new NodeWithParent
                        {
                            Node = child, ParentId = node.PublicKey
                        };
                        groups.Add(nodeWithParent);
                        treeStack.Push(child as IGroup);
                    }
                    else if (child is IQuestion)
                    {
                        questions.Add(new NodeWithParent
                        {
                            Node = child, ParentId = node.PublicKey
                        }); 
                    }
                }
            }

            Groups = groups.Select(@group => new GroupView(source, @group.Node as IGroup)).ToList();
            Questions = questions.Select(question => new QuestionView(question.Node as IQuestion)).ToList();
        }

        public List<GroupView> Groups { get; set; }

        public List<QuestionView> Questions { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastEntryDate { get; set; }

        public Guid? Parent { get; set; }

        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public bool IsPublic { get; set; }

        public List<QuestionView> GetAllQuestions()
        {
            return this.Questions;
        }
    }
}

