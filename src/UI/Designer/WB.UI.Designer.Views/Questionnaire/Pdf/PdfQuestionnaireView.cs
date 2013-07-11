using System;
using System.Collections.Generic;
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
            if (!groupPublicKey.HasValue) throw new ArgumentNullException("groupPublicKey", "Question cant be created not inside group");

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
    }
}