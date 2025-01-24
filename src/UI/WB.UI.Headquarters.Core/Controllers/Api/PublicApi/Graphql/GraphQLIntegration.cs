using System;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Execution;
using HotChocolate.Execution.Configuration;
using HotChocolate.Types;
using HotChocolate.Types.Descriptors;
using HotChocolate.Types.Pagination;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Code.Workspaces;
using WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Filters;
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
            
            /*services.Configure<FormOptions>(options =>
            {
                // Set the limit to 512 MB
                options.MultipartBodyLengthLimit = 512 * 1024 * 1024;
            });*/
            
            return GetExecutorBuilder(services)
                .AddErrorFilter(x => new GraphQLErrorFilter(x.GetApplicationService<ILogger<GraphQLErrorFilter>>()))
                .AddDiagnosticEventListener(x => 
                    new GraphqlDiagnosticEventListener(x.GetApplicationService<ILogger<GraphqlDiagnosticEventListener>>()));
        }

        private static IRequestExecutorBuilder GetExecutorBuilder(IServiceCollection services)
        {
            services.AddHttpResponseFormatter<CompositeFormatter>();

            return services
                .AddGraphQLServer()
                .AllowIntrospection(true) 
                .ModifyOptions(o => o.EnableDirectiveIntrospection = true)
                .InitializeOnStartup()
                .ConfigureSchema(x=>
                {
                    x.Use<WorkspaceGraphQlMiddleware>();
                })
                .AddAuthorization()
                .ModifyPagingOptions(o => { o.MaxPageSize = 200; })
                .ModifyCostOptions(o => { o.EnforceCostLimits = false; })
                .AddQueryType(x => x.Name("HeadquartersQuery"))
                .AddType<AssignmentsQueryExtension>()
                .AddType<InterviewsQueryExtension>()
                .AddType<MapsQueryExtension>()
                .AddType<QuestionnairesQueryExtension>()
                .AddType<QuestionsQueryExtension>()
                .AddType<QuestionnaireItemsQueryExtension>()
                .AddType<UsersQueryExtension>()
                .AddType<MapReportQueryExtension>()
                .AddMutationType(x => x.Name("HeadquartersMutation"))
                .AddType<CalendarEventsMutationExtension>()
                .AddType<MapsMutationExtension>()
                .AddType<UploadType>()
                .AddFiltering<HqFilteringConventions>()
                .AddConvention<INamingConventions>(new CompatibilityNamingConvention())
                .BindRuntimeType<string, CustomStringOperationFilterInput>()
                .BindRuntimeType<IdentifyEntityValue, IdentifyEntityValueFilterInput>()
                .AddType<IdentifyEntityValueFilterInput>()
                .BindRuntimeType<QuestionnaireCompositeItem, QuestionnaireItemsFilterType>()
                .AddType<QuestionnaireItemsFilterType>()
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
            
            var options = new GraphQLServerOptions { EnableSchemaRequests = true };
            //options.Tool.Credentials = DefaultCredentials.Include;
            options.Tool.DisableTelemetry = true;
            
            return app.UseEndpoints(x => x.MapGraphQL().WithOptions(options));
        }
    }
}
