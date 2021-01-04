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

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Graphql.Questionnaires
{
    public class QuestionnaireItemResolver
    {
        public IEnumerable<QuestionnaireCompositeItem> QuestionnaireItems(Guid id, 
            long version,
            string language,
            [Service] IQuestionnaireStorage storage, 
            [Service] IResolverContext resolverContext)
        {
            var questionnaire = storage.GetQuestionnaireOrThrow(new QuestionnaireIdentity(id, version), language);
            var featured = questionnaire.GetPrefilledEntities().ToHashSet();

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
                    QuestionScope = entityType == EntityType.Question ? questionnaire.GetQuestionScope(q) : (QuestionScope?)null
                };
        }

        private string? GetTitle(Guid id, EntityType entityType, IQuestionnaire questionnaire)
        {
            if (entityType == EntityType.Question) 
                return questionnaire.GetQuestionTitle(id);
            if (entityType == EntityType.Variable)
                return questionnaire.GetVariableLabel(id);
            return null;
        }
    }
}
