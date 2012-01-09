using System.Collections.Generic;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Export
{
    public class CompleteQuestionnaireExportInputModel
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
        private int _pageSize = 20;


        public string QuestionnaryId
        {
            get { return _responsibleId; }
            set { _responsibleId = value; }
        }

        private string _responsibleId = "";


    }
}
