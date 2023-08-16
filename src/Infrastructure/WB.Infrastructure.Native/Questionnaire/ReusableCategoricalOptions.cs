using System;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Infrastructure.Native.Questionnaire
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
        public virtual string AttachmentName { get; set; }
    }
}
