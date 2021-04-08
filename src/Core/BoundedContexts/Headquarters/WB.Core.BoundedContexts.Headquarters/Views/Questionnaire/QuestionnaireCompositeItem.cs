using System;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Headquarters.Views.Questionnaire
{
    public class QuestionnaireCompositeItem : IView
    {
        public virtual int Id { get; set; }
        public virtual string QuestionnaireIdentity { get; set; }
        public virtual Guid EntityId { get; set; }
        public virtual Guid? ParentId { get; set; }

        public virtual QuestionType? QuestionType { get; set; }
        public virtual bool? Featured { get; set; }
        public virtual QuestionScope? QuestionScope { get; set; }
        public virtual EntityType EntityType { get; set; }
        public virtual bool IsFilteredCombobox { get; set; }
        public virtual Guid? CascadeFromQuestionId { get; set; }
        public virtual Guid? LinkedToRosterId { get; set; }
        public virtual Guid? LinkedToQuestionId { get; set; }
        public virtual string StataExportCaption { get; set; }
        public virtual string QuestionText { get; set; }
        public virtual string VariableLabel { get; set; }

        public virtual VariableType? VariableType { get; set; }
        public virtual DateTime? IncludedInReportingAtUtc { get; set; }
    }

    public class QuestionnaireCompositeItemAnswer
    {
        public virtual int EntityId { get; set; }
        public virtual string Text { get; set; }
        public virtual string Value { get; set; }
        public virtual string Parent { get; set; }
        public virtual decimal? AnswerCode { get; set; }
        public virtual decimal? ParentCode { get; set; }
    }
}
