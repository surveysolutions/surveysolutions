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
            this.Groups = new List<PdfGroupView>
                {
                    new PdfGroupView
                        {
                            Title = "Group",
                            Children = new List<PdfEntityView>
                                {
                                    new PdfQuestionView {
                                        Title = "Question",
                                    }
                                }
                        }
                };
        }

        public string CreatedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public string Title { get; set; }

        public int ChaptersCount { get; set; }

        public int QuestionsCount { get; set; }

        public int QuestionsWithConditionsCount { get; set; }

        public int GroupsCount { get; set; }

        public List<PdfGroupView> Groups { get; set; }

        internal void RemoveGroup(Guid groupId)
        {
            throw new NotImplementedException();
        }

        internal PdfGroupView GetGroup(Guid groupId)
        {
            var groups = new Queue<PdfGroupView>(this.Groups);

            while (groups.Count > 0)
            {
                var group = groups.Dequeue();

                if (group.Id == groupId)
                    return group;

                foreach (PdfGroupView childGroup in group.Children.Where(child => child is PdfGroupView).Cast<PdfGroupView>())
                {
                    groups.Enqueue(childGroup);
                }
            }

            return null;
        }

        internal int GetEntityDepth(Guid? entityId)
        {
            if (entityId == null)
            {
                return 0;
            }

            return 1; // TODO
        }

        internal void AddGroup(PdfGroupView newGroup, Guid? parentId)
        {
            this.Groups.Add(newGroup); // TODO
        }
    }
}