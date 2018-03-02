using System;
using Main.Core.Entities.SubEntities;

namespace WB.Enumerator.Native.Questionnaire
{
    public class QuestionnaireCompositeItem
    {
        public virtual int Id { get; set; }
        public virtual string QuestionnaireIdentity { get; set; }
        public virtual Guid EntityId { get; set; }
        public virtual Guid? ParentId { get; set; }

        public virtual QuestionType? QuestionType { get; set; }
        public virtual bool? Featured { get; set; }
        public virtual QuestionScope? QuestionScope { get; set; }
        public virtual string Type { get; set; }
    }
}
