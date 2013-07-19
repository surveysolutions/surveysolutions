using Main.Core.Documents;

namespace Core.Supervisor.Views.Assignment
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Core.Supervisor.Views.Summary;

    using Main.Core.Entities;
    using Main.Core.Entities.SubEntities;
    using Main.Core.View.CompleteQuestionnaire;

    /// <summary>
    /// The survey group view.
    /// </summary>
    public class AssignmentView
    {
        public AssignmentView()
        {
            Order = string.Empty;
        }

        public AssignmentView(int page, int pageSize, int totalCount)
        {
            Order = string.Empty;
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
        }

        public void SetItems(IEnumerable<CompleteQuestionnaireBrowseItem> items)
        {
            this.Headers = new Dictionary<Guid, string>();
            this.Items = new List<AssignmentViewItem>();
            if (items == null)
            {
                return;
            }
            if (this.Template!=null)
            {
                foreach (var question in
                    items.SelectMany(
                        completeQuestionnaireBrowseItem => completeQuestionnaireBrowseItem.FeaturedQuestions).Where(
                            question => !this.Headers.ContainsKey(question.PublicKey)))
                {
                    this.Headers.Add(question.PublicKey, question.Title);
                }


                foreach (CompleteQuestionnaireBrowseItem it in items) 
                    this.Items.Add(new AssignmentViewItem(it, this.Headers));
            }
            else
            {
                this.Headers.Add(Guid.Empty, "Featured Questions");
                foreach (CompleteQuestionnaireBrowseItem it in items)
                    this.Items.Add(new AssignmentViewItem(it));
            }
        }

        public TemplateLight Template { get; set; }

        public SurveyStatus Status { get; set; }

        public UserLight User { get; set; }

        public Dictionary<Guid, string> Headers { get; set; }

        public List<AssignmentViewItem> Items { get; set; }

        public string Order { get; set; }

        public int Page { get; private set; }

        public int PageSize { get; private set; }

        public int TotalCount { get; set; }

        public IEnumerable<UserDocument> AssignableUsers { get; set; }
    }
}