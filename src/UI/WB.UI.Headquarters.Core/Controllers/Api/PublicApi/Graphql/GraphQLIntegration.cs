using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public static class GraphQLIntegration
    {
        public static IRequestExecutorBuilder GraphQLBuilder = null; 
        public static void AddGraphQL(this IServiceCollection services)
        {
            GraphQLBuilder = services
                .AddGraphQLServer()
                .UseField<WorkspaceGraphQlMiddleware>()
                .AddAuthorization()
                .SetPagingOptions(new PagingOptions(){MaxPageSize = 200})
                .AddQueryType<HeadquartersQuery>()
                .AddMutationType<HeadquartersMutations>()
                .AddConvention<INamingConventions>(new CompatibilityNamingConvention())
                .BindRuntimeType<string,CustomStringOperationFilterInput>()
                .AddFiltering()
                .AddSorting()
                .AddErrorFilter<GraphQLErrorFilter>();
        }
        public static IApplicationBuilder UseGraphQLApi(this IApplicationBuilder app)
        {
            return app.UseEndpoints(x => x.MapGraphQL());
            
            /*return app.UseGraphQLHttpPost(new HttpPostMiddlewareOptions
                 {
                     Path = "/graphql"
 
                 })
                 .UseGraphQLHttpGetSchema(new HttpGetSchemaMiddlewareOptions
                 {
                     Path = "/graphql/schema"
                 });*/
        }
    }
}
