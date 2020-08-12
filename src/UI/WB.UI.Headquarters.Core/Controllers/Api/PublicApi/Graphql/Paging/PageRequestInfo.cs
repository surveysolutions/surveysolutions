namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    internal class PageRequestInfo
    {
        public int? Skip { get; set; }
        public int? Take { get; set; }
        public bool HasTotalCount { get; set; }
        public bool HasFilteredCount { get; set; }
    }
}
