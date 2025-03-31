using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;

public class QuestionnaireCompositeItemDataLoader : BatchDataLoader<int, QuestionnaireCompositeItem>
{
    private readonly IUnitOfWork unitOfWork;

    public QuestionnaireCompositeItemDataLoader(
        IBatchScheduler batchScheduler,
        IUnitOfWork unitOfWork,
        DataLoaderOptions options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        this.unitOfWork = unitOfWork;
    }

    protected override bool AllowCachePropagation => false;

    protected override bool AllowBranching => true;

    protected override async Task<IReadOnlyDictionary<int, QuestionnaireCompositeItem>> LoadBatchAsync(
        IReadOnlyList<int> keys, CancellationToken cancellationToken)
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
}
