using System;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire
{
    public class ReferenceInfoByQuestion
    {
        public ReferenceInfoByQuestion(Guid scopeId, Guid referencedQuestionId, int lengthOfRosterVectorWhichNeedToBeExported)
        {
            this.ScopeId = scopeId;
            this.ReferencedQuestionId = referencedQuestionId;
            this.LengthOfRosterVectorWhichNeedToBeExported = lengthOfRosterVectorWhichNeedToBeExported;
        }

        public Guid ScopeId { get; private set; }
        public int LengthOfRosterVectorWhichNeedToBeExported { get; private set; }
        public Guid ReferencedQuestionId { get; private set; }
    }
}