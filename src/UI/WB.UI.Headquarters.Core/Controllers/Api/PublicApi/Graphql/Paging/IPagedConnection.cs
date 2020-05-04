using System.Collections;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    public interface IPagedConnection
    {
        long TotalCount { get; }
        long FilteredCount { get; }
        IList Nodes { get; }
    }
}
