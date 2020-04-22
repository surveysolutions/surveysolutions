using System.Collections;
using HotChocolate.Types;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Paging
{
    class PagedConnection<T> : ObjectType<IPagedConnection>, IPagedConnection
        where T : class, IType
    {
        public PagedConnection()
            : base(descriptor => Configure(descriptor))
        {
        }

        public PagedConnection(long totalCount, IList nodes)
        {
            TotalCount = totalCount;
            Nodes = nodes;
        }

        public long TotalCount { get; set; }
        public IList Nodes { get; set; }

        protected new static void Configure(IObjectTypeDescriptor<IPagedConnection> descriptor)
        {
            descriptor.BindFields(BindingBehavior.Explicit);

            descriptor.Field("nodes")
                .Description("A flattened list of the nodes.")
                .Type<ListType<T>>()
                .Resolver(ctx =>
                    ctx.Parent<IPagedConnection>().Nodes);

            descriptor.Field("totalCount")
                .Type<IntType>()
                .Resolver(x => x.Parent<IPagedConnection>().TotalCount);
        }
    }
}
