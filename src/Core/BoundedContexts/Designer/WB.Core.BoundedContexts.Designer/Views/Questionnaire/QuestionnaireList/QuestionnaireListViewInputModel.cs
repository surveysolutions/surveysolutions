using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList
{
    public class QuestionnaireListInputModel
    {
        public QuestionnaireListInputModel()
        {
            this.Orders = new List<OrderRequestItem>();
            this.PageSize = 20;
            this.Page = 1;
        }

        public string Order
        {
            get
            {
                return this.Orders.GetOrderRequestString();
            }

            set
            {
                this.Orders = value.ParseOrderRequestString();
            }
        }
        public IEnumerable<OrderRequestItem> Orders { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public bool IsAdminMode { get; set; }
        public Guid ViewerId { get; set; }
        public bool IsPublic { get; set; }
        public string Filter { get; set; }
    }
}