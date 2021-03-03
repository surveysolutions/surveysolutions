#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Types;
using NHibernate.Linq;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Reports.Map.MapReportHolder
{
    internal sealed class MapReportHolderResolver<TClrType, TSchemaType>
        where TClrType : class
        where TSchemaType : class, IType
    {
        private readonly IQueryable<TClrType> source;

        public MapReportHolderResolver(IQueryable<TClrType> source)
        {
            this.source = source;
        }

        public async Task<IMapReportHolder> ResolveAsync(CancellationToken cancellationToken)
        {
            var data = await this.source.ToListAsync(cancellationToken);
            return new MapReportHolder<TSchemaType>(data);
        }
    }
}
