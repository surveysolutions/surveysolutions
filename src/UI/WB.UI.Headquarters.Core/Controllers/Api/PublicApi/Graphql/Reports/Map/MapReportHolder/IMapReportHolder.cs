using System.Collections;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    public interface IMapReportHolder<T> : IMapReportHolder { }
    public interface IMapReportHolder
    {
        IList Nodes { get; }
    }
}
