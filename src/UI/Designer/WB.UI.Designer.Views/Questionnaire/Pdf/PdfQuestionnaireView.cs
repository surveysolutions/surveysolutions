using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.Infrastructure.ReadSide;
using System.Linq;

namespace WB.UI.Designer.Views.Questionnaire.Pdf
{
    public class PdfQuestionnaireView : IView
    {
        public PdfQuestionnaireView()
        {
            this.Groups = new List<PdfEntityView>();
        }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string Title { get; set; }

        public int ChaptersCount
        {
            get
            {
                return this.Groups.TreeToEnumerable().OfType<PdfGroupView>().Count(x => x.Parent == null);
            }
        }

        public int GroupsCount
        {
            get
            {
                return this.Groups.TreeToEnumerable().OfType<PdfGroupView>().Count(x => x.Parent != null);
            }
        }

        public int QuestionsCount
        {
            get
            {
                return this.Groups.TreeToEnumerable().OfType<PdfQuestionView>().Count();
            }
        }

        public int QuestionsWithConditionsCount
        {
            get
            {
                return this.Groups.TreeToEnumerable().OfType<PdfQuestionView>().Count(x => x.HasCodition);
            } 
        }

        public List<PdfEntityView> Groups { get; set; }

        internal void RemoveGroup(Guid groupId)
        {
            var item = this.Groups.TreeToEnumerable<PdfEntityView>().FirstOrDefault(x => x.Id == groupId);
            if (item != null)
            {
                item.Parent.Children.Remove(item);
            }
        }

        internal PdfGroupView GetGroup(Guid groupId)
        {
            return this.Groups.TreeToEnumerable().OfType<PdfGroupView>().FirstOrDefault(x => x.Id == groupId);
        }

        internal int GetEntityDepth(Guid? entityId)
        {
            var item = this.Groups.TreeToEnumerable().FirstOrDefault(x => x.Id == entityId);
            if (item != null)
            {
                int result = 1;
                var itemParent = item.Parent;
                while (itemParent != null)
                {
                    itemParent = itemParent.Parent;
                    result++;
                }

                return result;
            }

            return 0;
        }

        internal void AddGroup(PdfGroupView newGroup, Guid? parentId)
        {
            if (newGroup == null) throw new ArgumentNullException("newGroup");

            if (parentId.HasValue)
            {
                var pdfGroupView = this.GetGroup(parentId.Value);
                pdfGroupView.Children.Add(newGroup);
                newGroup.Parent = pdfGroupView;
            }
            else
            {
                this.Groups.Add(newGroup);
            }
        }

        internal void AddQuestion(PdfQuestionView newQuestion, Guid? groupPublicKey)
        {
            if (newQuestion == null) throw new ArgumentNullException("newQuestion");

            if (!groupPublicKey.HasValue)
            {
                throw new ArgumentNullException("groupPublicKey", string.Format("Question {0} cant be created not inside group", newQuestion.Id));
            }

            var group = this.GetGroup(groupPublicKey.Value);
            group.Children.Add(newQuestion);
            newQuestion.Parent = group;
        }

        public PdfQuestionView GetQuestion(Guid publicKey)
        {
            return this.Groups.TreeToEnumerable().OfType<PdfQuestionView>().FirstOrDefault(x => x.Id == publicKey);
        }

        public void RemoveQuestion(Guid questionId)
        {
            var item = this.Groups.TreeToEnumerable().FirstOrDefault(x => x.Id == questionId);
            if (item != null)
            {
                item.Parent.Children.Remove(item);
            }
        }

        public void FillFrom(QuestionnaireDocument source)
        {
            var children = source.Children ?? new List<IComposite>();

            foreach (var child in children)
            {
                var childGroup = child as IGroup;
                if (childGroup != null)
                {
                    var pdfGroupView = new PdfGroupView
                    {
                        Id = childGroup.PublicKey,
                        Title = childGroup.Title,
                        Depth = 1
                    };
                    this.AddGroup(pdfGroupView, null);
                    Fill(pdfGroupView, child);
                }
            }

            //var treeToEnumerable1 = source.Children.TreeToEnumerable1();
            //foreach (IComposite composite in treeToEnumerable1)
            //{
            //    if (composite is IQuestion)
            //    {
            //        var item = composite as IQuestion;
            //        Guid? parentId = item.GetParent() != null ? item.GetParent().PublicKey : (Guid?)null;
            //        if (parentId.HasValue)
            //        {
            //            this.AddQuestion(new PdfQuestionView
            //                {
            //                    Id = item.PublicKey,
            //                    Title = item.QuestionText,
            //                    Answers = (item.Answers ?? new List<IAnswer>()).Select(x => new PdfAnswerView
            //                        {
            //                            Title = x.AnswerText,
            //                            AnswerType = x.AnswerType,
            //                            AnswerValue = x.AnswerValue
            //                        }).ToList()
            //                }, parentId);
            //        }
            //    }
            //    if (composite is IGroup)
            //    {
            //        var item = composite as IGroup;
            //        Guid? parentId = item.GetParent() != null ? item.GetParent().PublicKey : (Guid?) null;
            //        this.AddGroup(new PdfGroupView {
            //            Id = item.PublicKey,
            //            Title = item.Title,
            //            Depth = this.GetEntityDepth(parentId) + 1
            //        }, parentId);
            //    }
            //}
        }

        private void Fill(PdfGroupView group, IComposite item)
        {
            if (item.Children != null)
            {
                foreach (var child in item.Children)
                {
                    var childQuestion = child as IQuestion;
                    var childGroup = child as IGroup;
                    if (childQuestion != null)
                    {
                        this.AddQuestion(new PdfQuestionView
                        {
                            Id = childQuestion.PublicKey,
                            Title = childQuestion.QuestionText,
                            Answers = (childQuestion.Answers ?? new List<IAnswer>()).Select(x => new PdfAnswerView
                            {
                                Title = x.AnswerText,
                                AnswerType = x.AnswerType,
                                AnswerValue = x.AnswerValue
                            }).ToList(),
                            HasCodition = !string.IsNullOrEmpty(childQuestion.ConditionExpression)
                        }, item.PublicKey);
                    }
                    if (childGroup != null)
                    {
                        var pdfGroupView = new PdfGroupView {
                            Id = childGroup.PublicKey, 
                            Title = childGroup.Title, 
                            Depth = this.GetEntityDepth(item.PublicKey) + 1
                        };
                        this.AddGroup(pdfGroupView, group.Id);
                        Fill(pdfGroupView, child);
                    }
                }
            }
        }
    }

    public static class Extensions
    {
        public static IEnumerable<T> TreeToEnumerable<T>(this IEnumerable<T> tree) where T : PdfEntityView
        {
            var groups = new Stack<T>(tree);

            while (groups.Count > 0)
            {
                var group = groups.Pop();

                yield return group;
                foreach (T childGroup in group.Children.OfType<T>())
                {
                    groups.Push(childGroup);
                }
            }
        }

        public static IEnumerable<T> TreeToEnumerable1<T>(this IEnumerable<T> tree) where T : IComposite
        {
            var groups = new Stack<T>(tree);

            while (groups.Count > 0)
            {
                var group = groups.Pop();

                yield return group;
                foreach (T childGroup in group.Children.OfType<T>())
                {
                    groups.Push(childGroup);
                }
            }
        }
    }
}