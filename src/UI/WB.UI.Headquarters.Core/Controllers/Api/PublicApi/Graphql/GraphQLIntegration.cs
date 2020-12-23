using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Data.Filters;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Filters;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.FIlters;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Mutations;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Queries;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql
{
    public static class GraphQLIntegration
    {
        public static IRequestExecutorBuilder AddGraphQL(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            return GetExecutorBuilder(services)
                .AddErrorFilter<GraphQLErrorFilter>()
                .AddDiagnosticEventListener(x => 
                    new GraphqlDiagnosticEventListener(x.GetApplicationService<ILogger<GraphqlDiagnosticEventListener>>()));
        }

        private static IRequestExecutorBuilder GetExecutorBuilder(IServiceCollection services)
        {
            return services
                .AddGraphQLServer()
                .ConfigureSchema(x=>x.Use<WorkspaceGraphQlMiddleware>())
                .AddAuthorization()
                .SetPagingOptions(new PagingOptions(){MaxPageSize = 200})
                .AddQueryType(x => x.Name("HeadquartersQuery"))
                .AddType<AssignmentsQueryExtension>()
                .AddType<InterviewsQueryExtension>()
                .AddType<MapsQueryExtension>()
                .AddType<QuestionnairesQueryExtension>()
                .AddType<QuestionsQueryExtension>()
                .AddType<QuestionnaireItemsQueryExtension>()
                .AddType<UsersQueryExtension>()
                .AddMutationType(x => x.Name("HeadquartersMutation"))
                .AddType<CalendarEventsMutationExtension>()
                .AddType<MapsMutationExtension>()
                .AddFiltering()
                .AddConvention<INamingConventions>(new CompatibilityNamingConvention())
                .BindRuntimeType<string, CustomStringOperationFilterInput>()
                .BindRuntimeType<IdentifyEntityValue, IdentifyEntityValueFilterInput>()
                .BindRuntimeType<QuestionnaireCompositeItem, QuestionnaireItemsFilterType>()
                .AddSorting();
        }

        public static async Task<ISchema> GetSchema(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            
            return await GetExecutorBuilder(services)
                .BuildSchemaAsync();
        }

        public static IApplicationBuilder UseGraphQLApi(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }
            
            var options = new GraphQLServerOptions {EnableSchemaRequests = true};
            options.Tool.Credentials = DefaultCredentials.Include;
            
            return app.UseEndpoints(x => x.MapGraphQL().WithOptions(options));
        }
    }
}
