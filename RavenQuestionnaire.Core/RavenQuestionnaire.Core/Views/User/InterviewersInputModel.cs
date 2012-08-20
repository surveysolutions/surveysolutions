using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Views.User
{
    public class InterviewersInputModel
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

        public UserLight Supervisor { get; set; }
        public bool AllSubordinateUsers { get; set; }
    }
}