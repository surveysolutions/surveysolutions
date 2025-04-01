using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using Microsoft.AspNetCore.Http;
using NHibernate.Linq;
using StackExchange.Exceptional;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;

public class QuestionnaireCompositeItemDataLoader : BatchDataLoader<int, QuestionnaireCompositeItem>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly HttpContextAccessor httpContextAccessor;

    public QuestionnaireCompositeItemDataLoader(
        IBatchScheduler batchScheduler,
        IUnitOfWork unitOfWork,
        DataLoaderOptions options = null,
        HttpContextAccessor httpContextAccessor = null)
        : base(batchScheduler, options ?? new DataLoaderOptions(){ MaxBatchSize = 0 })
    {
        this.unitOfWork = unitOfWork;
        this.httpContextAccessor = httpContextAccessor;
    }

    protected override bool AllowCachePropagation => false;

    protected override bool AllowBranching => true;

    protected override async Task<IReadOnlyDictionary<int, QuestionnaireCompositeItem>> LoadBatchAsync(
        IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        try
        {
            if (!unitOfWork.Session.IsOpen)
            {
                throw new InvalidOperationException("GraphQL: session is closed before query execution.");
            }

            var compositeItems = await unitOfWork.Session.Query<QuestionnaireCompositeItem>()
                .Where(q => keys.Contains(q.Id))
                .ToListAsync(cancellationToken);
        
            return compositeItems.ToDictionary(x => x.Id);
        }
        catch (Exception e)
        {
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
                e.Log(httpContext);
            throw new Exception("TEST: fail to read query entity", e);
        }
    }
}
