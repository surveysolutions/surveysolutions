using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire
{
    public class CompleteQuestionnaireBrowseInputModel
    {
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

        public List<OrderRequestItem> _orders = new List<OrderRequestItem>();

        public int Page
        {
            get { return _page; }
            set { _page = value; }
        }

        private int _page = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }
        private int _pageSize = 5;


        public int ResponsibleId
        {
            get { return _pageSize; }
            set { _pageSize = value; }
        }

        private string _responsibleId = "";


    }
}
