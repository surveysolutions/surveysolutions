using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;

namespace WB.Enumerator.Native.Questionnaire
{
    public class CategoriesInstance : CategoriesDto
    {
        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }

        public virtual CategoriesInstance Clone() => new CategoriesInstance
        {
            QuestionnaireId = this.QuestionnaireId,
            CategoriesId = this.CategoriesId,
            Id = this.Id,
            ParentId = this.ParentId,
            Text = this.Text
        };
    }
}
