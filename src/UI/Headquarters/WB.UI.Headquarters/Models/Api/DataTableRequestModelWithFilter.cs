using WB.Core.BoundedContexts.Headquarters.Views.User;

namespace WB.UI.Headquarters.Models.Api
{
    public class DataTableRequestWithFilter : DataTableRequest
    {
        public string SupervisorName { set; get; }
        public bool Archived { set; get; }
        public InterviewerFacet Facet { get; set; }
    }
}