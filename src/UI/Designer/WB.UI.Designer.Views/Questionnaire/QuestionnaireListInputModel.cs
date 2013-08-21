using System;
using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Utility;

namespace WB.UI.Designer.Views.Questionnaire
{
    public class QuestionnaireListViewInputModel
    {
        public QuestionnaireListViewInputModel()
        {
            this.Orders = new List<OrderRequestItem>();
            this.PageSize = 20;
            this.Page = 1;
        }

        public string Order
        {
            get
            {
                return StringUtil.GetOrderRequestString(this.Orders);
            }

            set
            {
                this.Orders = StringUtil.ParseOrderRequestString(value);
            }
        }
        public List<OrderRequestItem> Orders { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool IsAdminMode { get; set; }
        public Guid ViewerId { get; set; }
        public bool IsPublic { get; set; }
        public string Filter { get; set; }
    }
}