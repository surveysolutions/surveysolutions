using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;

public class IdentifyEntityValuesDataLoader : BatchDataLoader<int, IReadOnlyList<IdentifyEntityValue>>
{
    private readonly IUnitOfWork unitOfWork;

    public IdentifyEntityValuesDataLoader(
        IBatchScheduler batchScheduler, 
        IUnitOfWork unitOfWork, 
        DataLoaderOptions options = null) 
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        this.unitOfWork = unitOfWork;
    }

    protected override bool AllowCachePropagation => false;

    protected override bool AllowBranching => true;

    protected override async Task<IReadOnlyDictionary<int, IReadOnlyList<IdentifyEntityValue>>> LoadBatchAsync(
        IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        if (!unitOfWork.Session.IsOpen)
        {
            throw new InvalidOperationException("GraphQL: session is closed before query execution.");
        }
        
        var questionAnswers = await unitOfWork.Session.Query<IdentifyEntityValue>()
            .Where(a => keys.Contains(a.InterviewSummary.Id) && a.Identifying)
            .OrderBy(a => a.Position)
            .Fetch(q => q.Entity)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    
        IReadOnlyDictionary<int, IReadOnlyList<IdentifyEntityValue>> answers = questionAnswers
            .GroupBy(x => x.InterviewSummary.Id)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<IdentifyEntityValue>)g.ToList());
        return answers;
    }
}
