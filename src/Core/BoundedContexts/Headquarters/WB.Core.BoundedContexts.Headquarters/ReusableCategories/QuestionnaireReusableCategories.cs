using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Infrastructure.Native.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.ReusableCategories
{
    public class QuestionnaireReusableCategoriesAccessor : ICategories
    {
        private readonly QuestionnaireIdentity questionnaireId;
        private readonly IReusableCategoriesStorage reusableCategoriesStorage;

        public QuestionnaireReusableCategoriesAccessor(QuestionnaireIdentity questionnaireId, IReusableCategoriesStorage reusableCategoriesStorage)
        {
            this.questionnaireId = questionnaireId;
            this.reusableCategoriesStorage = reusableCategoriesStorage;
        }

        public List<CategoriesItem> GetCategories(Guid categoriesId)
        {
            return reusableCategoriesStorage.GetOptions(questionnaireId, categoriesId).ToList();
        }
    }
}
