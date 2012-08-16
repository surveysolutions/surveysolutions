using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupView
    {
        public string SurveyTitle { get; private set; }
        public int PageSize { get; private set; }

        public int Page { get; private set;}

        public int TotalCount { get; private set; }

        public List<SurveyGroupItem> Items { get; set; }

        public Dictionary<Guid, string> Headers { get; set; }

        public SurveyGroupView(int page, int pageSize, string surveyTitle, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            this.Items = new List<SurveyGroupItem>();
            this.Headers = new Dictionary<Guid, string>();
            foreach (var question in items.SelectMany(completeQuestionnaireBrowseItem => completeQuestionnaireBrowseItem.FeaturedQuestions).
                Where(question => !Headers.ContainsKey(question.PublicKey)))
            {
                Headers.Add(question.PublicKey, question.QuestionText);
            }
            foreach (var it in items)
            {
                Items.Add(new SurveyGroupItem(it, Headers));
            }
            SurveyTitle = surveyTitle;
        }
    }
}
