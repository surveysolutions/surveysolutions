using System.Collections.Generic;
using Main.Core.Entities;
using Main.Core.Utility;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Views
{
    public class QuestionnaireListInputModel
    {

        private List<OrderRequestItem> orders = new List<OrderRequestItem>();

        public QuestionnaireListInputModel()
        {
            this.Page = 1;
            this.PageSize = 20;
        }

        public string Order
        {
            get { return StringUtil.GetOrderRequestString(this.orders); }

            set { this.orders = StringUtil.ParseOrderRequestString(value); }
        }

        public List<OrderRequestItem> Orders
        {
            get { return this.orders; }

            set { this.orders = value; }
        }

        public int Page { get; set; }


        public int PageSize { get; set; }

    }
}