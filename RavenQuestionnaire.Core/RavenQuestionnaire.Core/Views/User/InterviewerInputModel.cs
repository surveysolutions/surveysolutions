using System;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewerInputModel
    {
        public InterviewerInputModel(string id) { UserId = id; }

        public int Page { 
            get { return _page; }
            set { _page = value; }
        }

        private int _page = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        private int _pageSize = 10;

        public string UserId { get; set; }

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

        public Func<CompleteQuestionnaireBrowseItem, bool> Expression
        {
            get
            {
                return q => (q.Responsible == null ? false : q.Responsible.Id == UserId);
            }
        }

        public string TemplateId { get; set; }
    }
}
