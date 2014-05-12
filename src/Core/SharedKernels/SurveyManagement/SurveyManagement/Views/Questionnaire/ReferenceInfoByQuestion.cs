using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class ReferenceInfoByQuestion
    {
        public ReferenceInfoByQuestion(Guid referencedQuestionId, ValueVector<Guid> referencedQuestionRosterScope, ValueVector<Guid> linkedQuestionRosterScope)
        {
            this.ReferencedQuestionRosterScope = referencedQuestionRosterScope;
            this.ReferencedQuestionId = referencedQuestionId;
            this.LinkedQuestionRosterScope = linkedQuestionRosterScope;
        }
        public Guid ReferencedQuestionId { get; private set; }

        public ValueVector<Guid> ReferencedQuestionRosterScope { get; private set; }
        public ValueVector<Guid> LinkedQuestionRosterScope { get; private set; }
    }
}