using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class ReferenceInfoByQuestion
    {
        public ReferenceInfoByQuestion(Guid referencedQuestionId, Guid[] referencedQuestionRosterScope, Guid[] linkedQuestionRosterScope)
        {
            this.ReferencedQuestionRosterScope = referencedQuestionRosterScope;
            this.ReferencedQuestionId = referencedQuestionId;
            this.LinkedQuestionRosterScope = linkedQuestionRosterScope;
        }
        public Guid ReferencedQuestionId { get; private set; }

        public Guid[] ReferencedQuestionRosterScope { get; private set; }
        public Guid[] LinkedQuestionRosterScope { get; private set; }
    }
}