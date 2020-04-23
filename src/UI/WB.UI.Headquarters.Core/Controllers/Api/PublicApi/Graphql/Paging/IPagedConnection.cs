using System.Collections;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    public interface IPagedConnection
    {
        long TotalCount { get; }
        IList Nodes { get; }
    }
}
