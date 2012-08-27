using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;


namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerGroupView
    {
        public string Order
        {
            get { return _order; }
            set { _order = value; }
        }
        private string _order = string.Empty;

        public int PageSize { get; private set; }

        public int Page { get; private set; }

        public int TotalCount { get; private set; }

        public string TemplateId { get; set; }

        public string Title { get; set; }

        public List<CompleteQuestionnaireBrowseItem> Items { get; set; }

        public Dictionary<Guid, string> HeaderFeaturedQuestions { get; set; }

        public InterviewerGroupView(string templateId, string title, List<CompleteQuestionnaireBrowseItem> items, string order, int page, int pageSize, int totalCount)
        {
            this.TemplateId = templateId;
            this.Title = title;
            this.Items = items;
            this.Order = order;
            this.Page = page;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            var helper = new Dictionary<Guid, string>();
            foreach (var question in items.SelectMany(completeQuestionnaireBrowseItem => completeQuestionnaireBrowseItem.FeaturedQuestions.Where(t => !string.IsNullOrEmpty(t.QuestionText)))
                .Where(question => !helper.ContainsKey(question.PublicKey)))
                helper.Add(question.PublicKey, question.QuestionText);
            this.HeaderFeaturedQuestions = helper;
        }
   }
}
