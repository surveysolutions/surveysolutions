using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.ReusableCategories
{
    public class ReusableCategoricalOptions
    {
        public virtual int Id { get; set; }
        public virtual QuestionnaireIdentity QuestionnaireId { get; set; }
        public virtual Guid CategoriesId { get; set; }
        public virtual int Value { get; set; }
        public virtual int? ParentValue { get; set; }
        public virtual string Text { get; set; }
        public virtual int SortIndex { get; set; }
    }
}
