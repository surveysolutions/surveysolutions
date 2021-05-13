#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Linq;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    internal sealed class MapReportHolderResolver<GpsAnswerQuery>
    {
        private readonly IQueryable<GpsAnswerQuery> source;

        public MapReportHolderResolver(IQueryable<GpsAnswerQuery> source)
        {
            this.source = source;
        }

        public async Task<IMapReportHolder> ResolveAsync(CancellationToken cancellationToken)
        {
            var data = await this.source.ToListAsync(cancellationToken);
            return new MapReportHolder(data);
        }
    }
}
