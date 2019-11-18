using System;
using System.Collections.Generic;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Core.BoundedContexts.Designer.Verifier
{
    public class CategoriesVerifications : AbstractVerifier, IPartialVerifier
    {
        private ICategoriesService categoriesService;

        public CategoriesVerifications(ICategoriesService categoriesService)
        {
            this.categoriesService = categoriesService;
        }

        public IEnumerable<QuestionnaireVerificationMessage> Verify(MultiLanguageQuestionnaireDocument multiLanguageQuestionnaireDocument)
        {
            yield break;
        }
    }
}
