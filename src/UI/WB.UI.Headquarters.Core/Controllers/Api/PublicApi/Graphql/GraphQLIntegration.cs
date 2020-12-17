using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public static class GraphQLIntegration
    {
        public static void AddGraphQL(this IServiceCollection services)
        {
            GetExecutorBuilder(services)
                .AddErrorFilter<GraphQLErrorFilter>()
                .AddDiagnosticEventListener(x => 
                    new GraphqlDiagnosticEventListener(x.GetApplicationService<ILogger<GraphqlDiagnosticEventListener>>()));
        }

        private static IRequestExecutorBuilder GetExecutorBuilder(IServiceCollection services)
        {
            return services
                .AddGraphQLServer()
                .UseField<WorkspaceGraphQlMiddleware>()
                .AddAuthorization()
                .SetPagingOptions(new PagingOptions(){MaxPageSize = 200})
                .AddQueryType<HeadquartersQuery>()
                .AddMutationType<HeadquartersMutations>()
                .AddConvention<INamingConventions>(new CompatibilityNamingConvention())
                .BindRuntimeType<string,CustomStringOperationFilterInput>()
                .AddFiltering()
                .AddSorting();
        }

        public static async Task<ISchema> GetSchema(IServiceCollection services)
        {
            return await GetExecutorBuilder(services).BuildSchemaAsync();
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
