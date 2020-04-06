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
        private readonly IQueryable<TClrType> source;
        private readonly PageDetails pageDetails;

        public PagedConnectionResolver(IQueryable<TClrType> source, PageDetails pageDetails)
        {
            this.source = source;
            this.pageDetails = pageDetails;
        }

        public async Task<IPagedConnection> ResolveAsync(CancellationToken cancellationToken)
        {
            var count = await this.source.CountAsync(cancellationToken);

            var data = await this.source
                .Skip(this.pageDetails.Skip)
                .Take(this.pageDetails.Take)
                .ToListAsync(cancellationToken);

            return new PagedConnection<TSchemaType>(count, data);
        }
    }
}
