#nullable enable
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Types;
using NHibernate.Linq;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    internal sealed class PagedConnectionResolver<TClrType, TSchemaType>
        where TClrType : class
        where TSchemaType : class, IType
    {
        private readonly IQueryable<TClrType>? unfilteredQuery;
        private readonly IQueryable<TClrType> source;
        private readonly PageRequestInfo pageRequestInfo;
        private const int MaxTakeValue = 100;

        
        public PagedConnectionResolver(IQueryable<TClrType>? unfilteredQuery, IQueryable<TClrType> source, PageRequestInfo pageRequestInfo)
        {
            this.unfilteredQuery = unfilteredQuery;
            this.source = source;
            this.pageRequestInfo = pageRequestInfo;
        }

        public async Task<IPagedConnection> ResolveAsync(CancellationToken cancellationToken)
        {
            var filteredCount = pageRequestInfo.HasFilteredCount ? await this.source.CountAsync(cancellationToken) : 0;
            var totalCount = pageRequestInfo.HasTotalCount ? await this.unfilteredQuery.CountAsync(cancellationToken) : 0;

            var query = this.source;
            
            if (this.pageRequestInfo.Skip.HasValue)
                query = query.Skip(this.pageRequestInfo.Skip.Value);

            var take = this.pageRequestInfo.Take is <= MaxTakeValue
                ? this.pageRequestInfo.Take.Value
                : MaxTakeValue;

            query = query.Take(take);
            
            var data = await query.ToListAsync(cancellationToken);

            return new PagedConnection<TSchemaType>(totalCount, filteredCount, data);
        }
    }
}
