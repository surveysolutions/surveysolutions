using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    internal struct EntityVerificationResult<TReferencedEntity>
        where TReferencedEntity : class, IQuestionnaireEntity
    {
        public bool HasErrors { get; set; }
        public IEnumerable<TReferencedEntity> ReferencedEntities { get; set; }
    }

    internal static class EntityVerificationResult
    {
        public static EntityVerificationResult<IQuestionnaireEntity> NoProblems()
            => new EntityVerificationResult<IQuestionnaireEntity> { HasErrors = false };

        public static EntityVerificationResult<TReferencedEntity> NoProblems<TReferencedEntity>()
            where TReferencedEntity : class, IQuestionnaireEntity
            => new EntityVerificationResult<TReferencedEntity> { HasErrors = false };

        public static EntityVerificationResult<TReferencedEntity> Problems<TReferencedEntity>(IEnumerable<TReferencedEntity> entities)
            where TReferencedEntity : class, IQuestionnaireEntity
            => new EntityVerificationResult<TReferencedEntity> { HasErrors = true, ReferencedEntities = entities };

        public static EntityVerificationResult<IQuestionnaireEntity> Problems(IQuestionnaireEntity entity, IEnumerable<IQuestionnaireEntity> entities)
            => Problems(entity.ToEnumerable().Concat(entities));
    }
}