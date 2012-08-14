using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupView
    {
        public int PageSize { get; private set; }

        public int Page { get; private set;}

        public int TotalCount { get; private set; }

        public IEnumerable<SurveyGroupItem> Items { get; set; }

        public Dictionary<string, string> Headers { get; set; }

        public SurveyGroupView(int page, int pageSize, int totalCount, IEnumerable<CompleteQuestionnaireBrowseItem> items)
        {
            this.Page = page;
            this.TotalCount = totalCount;
            this.PageSize = pageSize;
            var header = new Dictionary<string, string>();
            foreach (var name in items
                    .SelectMany(item => item.FeaturedQuestions
                    .Select(l => new { PublicKey = l.PublicKey, Name = l.QuestionText })
                    .Where(name => !header.ContainsKey(name.PublicKey.ToString()))))
                header.Add(name);

            this.Items = items;

            var header = new Dictionary<string, string>();
            foreach (var name in questionnaires.Where(x => x.TemplateId == input.Id)
                    .SelectMany(item => item.FeaturedQuestions
                    .Select(l => new { PublicKey = l.PublicKey, Name = l.QuestionText })
                    .Where(name => !header.ContainsKey(name.PublicKey.ToString()))))
                header.Add(name);

            var collect = new List<SurveyGroupItem>();
            foreach (var item in questionnaires)
            {
                var surveyItem = new SurveyGroupItem(Guid.Parse(item.CompleteQuestionnaireId), item.QuestionnaireTitle, item.TemplateId, item.Status, item.Responsible);
                foreach (var nameField in header)
                {
                    var val = item.FeaturedQuestions.Where(t => t.PublicKey == nameField) as QuestionStatisticView;
                    surveyItem.FeatureadValue.Add(nameField.ToString(), (val != null ? val.AnswerValue : string.Empty));
                }
                collect.Add(surveyItem);
            }
        }
    }
}
