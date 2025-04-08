#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenDonut;
using HotChocolate.Execution;
using Main.Core.Entities.SubEntities;
using NHibernate.Linq;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires;

public class QuestionnaireItemOptionsDataLoader : BatchDataLoader<int, IReadOnlyList<CategoricalOption>>
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IQuestionnaireStorage questionnaireStorage;
    private readonly IRequestContextAccessor requestContextAccessor;

    public QuestionnaireItemOptionsDataLoader(
        IBatchScheduler batchScheduler,
        IUnitOfWork unitOfWork,
        IQuestionnaireStorage questionnaireStorage,
        IRequestContextAccessor requestContextAccessor,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options ?? new DataLoaderOptions())
    {
        this.unitOfWork = unitOfWork;
        this.questionnaireStorage = questionnaireStorage;
        this.requestContextAccessor = requestContextAccessor;
    }

    protected override bool AllowCachePropagation => true;

    protected override bool AllowBranching => true;

    protected override async Task<IReadOnlyDictionary<int, IReadOnlyList<CategoricalOption>>> LoadBatchAsync(
        IReadOnlyList<int> keys, CancellationToken cancellationToken)
    {
        var questions = await unitOfWork.Session.Query<QuestionnaireCompositeItem>()
            .Where(q => keys.Contains(q.Id))
            .ToListAsync(cancellationToken);
                            
        if(!questions.Any()) 
            return new Dictionary<int, IReadOnlyList<CategoricalOption>>();
                        
        var context = requestContextAccessor.RequestContext;
        var language = context?.ContextData.TryGetValue("language", out var langObj) == true 
            ? langObj?.ToString() 
            : (string?)null;
                        
        var questionnaires =  questions.Select(q
            => (q.Id, q.EntityId,
                questionnaireStorage.GetQuestionnaire(
                    QuestionnaireIdentity.Parse(q.QuestionnaireIdentity), language))).ToList();

        IReadOnlyDictionary<int, IReadOnlyList<CategoricalOption>> dictionary = questionnaires
            .Where(q => q.Item3 != null)
            .ToDictionary<(int Id, Guid EntityId, IQuestionnaire?), int, IReadOnlyList<CategoricalOption>>(q => q.Id, 
                q =>
                {
                    if (q.Item3!.IsQuestion(q.EntityId))
                    {
                        var questionType = q.Item3!.GetQuestionType(q.EntityId);
                        if (questionType is QuestionType.SingleOption or QuestionType.MultyOption or QuestionType.Numeric)
                            return q.Item3!.GetOptionsForQuestion(q.EntityId, null, null, null).ToReadOnlyCollection();
                    }

                    return new List<CategoricalOption>();
                });
        return dictionary;
    }
}
