using System.Collections;
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    class PagedConnection<T> : ObjectType<IPagedConnection<T>>, IPagedConnection
        where T : class, IType
    {
        public PagedConnection()
            : base(descriptor => Configure(descriptor))
        {
        }

        public PagedConnection(long totalCount, long filteredCount, IList nodes)
        {
            TotalCount = totalCount;
            FilteredCount = filteredCount;
            Nodes = nodes;
        }

        public long TotalCount { get; }
        public long FilteredCount { get; }
        public IList Nodes { get; }

        private new static void Configure(IObjectTypeDescriptor<IPagedConnection<T>> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);

            descriptor.Field("nodes")
                .Description("A flattened list of the nodes.")
                .Type<NonNullType<ListType<T>>>()
                .Resolve(ctx =>
                    ctx.Parent<IPagedConnection>().Nodes);

            descriptor.Field("totalCount")
                .Type<NonNullType<IntType>>()
                .Description("Total count of nodes without filtering applied")
                .Resolve(x => x.Parent<IPagedConnection>().TotalCount);

            descriptor.Field("filteredCount")
                .Type<NonNullType<IntType>>()
                .Description("Filtered count of nodes without paging")
                .Resolve(x => x.Parent<IPagedConnection>().FilteredCount);
        }
    }
}
