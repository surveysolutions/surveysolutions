using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
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

        public string Order
        {
            get { return StringUtil.GetOrderRequestString(_orders); }
            set { _orders = StringUtil.ParseOrderRequestString(value); }
        }

        public List<OrderRequestItem> Orders
        {
            get { return _orders; }
            set { _orders = value; }
        }

        private List<OrderRequestItem> _orders = new List<OrderRequestItem>();

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

        public SurveyGroupInputModel(string id, int page, int pageSize, List<OrderRequestItem> orders)
        {
            this.Id = id;
            this.Page = page;
            this.PageSize = pageSize;
            this.Orders = orders;
        }

    }
}
