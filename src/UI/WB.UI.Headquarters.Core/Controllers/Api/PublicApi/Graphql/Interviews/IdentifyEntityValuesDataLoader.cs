using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using Microsoft.Extensions.DependencyInjection;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Interviews;

public class IdentifyEntityValuesDataLoader : BatchDataLoader<string, IReadOnlyList<IdentifyEntityValue>>
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

    protected override async Task<IReadOnlyDictionary<string, IReadOnlyList<IdentifyEntityValue>>> LoadBatchAsync(
        IReadOnlyList<string> keys, CancellationToken cancellationToken)
    {
        if (!unitOfWork.Session.IsOpen)
        {
            throw new InvalidOperationException("GraphQL: NHibernate session is closed before query execution.");
        }
        
        var questionAnswers = await unitOfWork.Session.Query<IdentifyEntityValue>()
            .Where(a => keys.Contains(a.InterviewSummary.SummaryId) && a.Identifying)
            .OrderBy(a => a.Position)
            .Fetch(q => q.Entity)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return questionAnswers
            .GroupBy(x => x.InterviewSummary.SummaryId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<IdentifyEntityValue>)g.ToList());
    }
}
