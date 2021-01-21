using System;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Factories
{
    public class QuestionnaireBrowseInputModel
    {
        public QuestionnaireBrowseInputModel()
        {
            this.Orders = new List<OrderRequestItem>();
            this.Page = 1;
        }

        public string Order
        {
            get => this.Orders.GetOrderRequestString();

            set => this.Orders = value.ParseOrderRequestString();
        }

        public IEnumerable<OrderRequestItem> Orders { get; set; }

        public int Page { get; set; }

        public int? PageSize { get; set; }

        public bool? IsAdminMode { get; set; }

        public Guid CreatedBy { get; set; }

        public bool IsOnlyOwnerItems { get; set; }

        public string SearchFor { get; set; }

        public bool? OnlyCensus { get; set; }

        public Guid? QuestionnaireId { get; set; }

        public long? Version { get; set; }
    }
}
