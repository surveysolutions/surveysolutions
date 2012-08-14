using System;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.Survey
{
    public class SurveyGroupInputModel
    {
        private int _page = 1;

        private int _pageSize = 20;

        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        public string Id { get; set;}

        public Func<CompleteQuestionnaireBrowseItem, bool> Expression
        {
            get { return x => x.TemplateId == Id; }
        }

        public string QuestionnaireId { get; set; }

        public SurveyGroupInputModel(string id)
        {
            this.Id = id;
        }

        public SurveyGroupInputModel(string id, string questionnaireId)
        {
            this.Id = id;
            this.QuestionnaireId = questionnaireId;
        }
    }
}
