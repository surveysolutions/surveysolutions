#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using HotChocolate;
using HotChocolate.Resolvers;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnaireItemResolver
    {
        public IEnumerable<QuestionnaireCompositeItem> QuestionnaireItems(Guid id, 
            long version,
            string language,
            [Service] IUnitOfWork unitOfWork,
            [Service] IQuestionnaireStorage storage, 
            IResolverContext resolverContext)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(id, version);

            var questionnaire = storage.GetQuestionnaireOrThrow(questionnaireIdentity, language);
            var featured = questionnaire.GetPrefilledEntities().ToHashSet();

            IQueryable<QuestionnaireCompositeItem> compositeItem = unitOfWork.Session.Query<QuestionnaireCompositeItem>();

            var exposed = compositeItem
                .Where(x => x.IncludedInReportingAtUtc != null
                            && x.QuestionnaireIdentity == questionnaireIdentity.ToString())


                .Select(x => new {Id = x.EntityId, IncludedInReportingAtUtc = x.IncludedInReportingAtUtc})
                .ToDictionary(x=>x.Id, x=> x.IncludedInReportingAtUtc);
            
            resolverContext.ScopedContextData = resolverContext.ScopedContextData.SetItem("language", language);

            return from q in questionnaire.GetAllEntities()
                let entityType = EntityTypeHelper.GetEntityType(q, questionnaire)
                select new QuestionnaireCompositeItem
                {
                    Id = questionnaire.GetEntityIdMapValue(q),
                    EntityId = q,
                    EntityType = entityType,
                    Featured = featured.Contains(q),
                    StataExportCaption = entityType == EntityType.Question 
                        ? questionnaire.GetQuestionVariableName(q) 
                        : entityType == EntityType.Variable
                            ? questionnaire.GetVariableName(q) 
                            : null,
                    VariableLabel = entityType == EntityType.Question 
                        ? questionnaire.GetQuestionExportDescription(q)
                        : null,
                    QuestionType = entityType == EntityType.Question ? questionnaire.GetQuestionType(q) : (QuestionType?)null,
                    QuestionText = GetTitle(q, entityType, questionnaire),
                    QuestionScope = entityType == EntityType.Question ? questionnaire.GetQuestionScope(q) : (QuestionScope?)null,
                    VariableType = entityType == EntityType.Variable ? questionnaire.GetVariableVariableType(q): (VariableType?)null,
                    IncludedInReportingAtUtc= exposed.ContainsKey(q) ? exposed[q] : null
                };
        }

        private string? GetTitle(Guid id, EntityType entityType, IQuestionnaire questionnaire)
        {
            return entityType switch
            {
                EntityType.Question => questionnaire.GetQuestionTitle(id),
                EntityType.Variable => questionnaire.GetVariableLabel(id),
                _ => null
            };
        }
    }
}
