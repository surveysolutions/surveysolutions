using System;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Headquarters.Models.Template
{
    public class QuestionnaireVerificationReferenceResponse
    {
        public QuestionnaireVerificationReferenceResponse(QuestionnaireVerificationReference reference, QuestionnaireDocument questionnaire)
        {
            this.Type = reference.Type.ToString();
            this.Id = reference.Id;
            if (questionnaire != null)
                this.Title = reference.Type == QuestionnaireVerificationReferenceType.Group
                    ? this.GetGroupTitle(questionnaire, this.Id)
                    : this.GetQuestionTitle(questionnaire, this.Id);
        }

        private string GetGroupTitle(QuestionnaireDocument questionnaire, Guid id)
        {
            var group = questionnaire.FirstOrDefault<IGroup>(g => g.PublicKey == id);
            if (group == null)
                return string.Empty;
            return group.Title;
        }

        private string GetQuestionTitle(QuestionnaireDocument questionnaire, Guid id)
        {
            var question = questionnaire.FirstOrDefault<IQuestion>(q => q.PublicKey == id);
            if (question == null)
                return string.Empty;
            return question.QuestionText;
        }

        public string Type { get; private set; }
        public string Title { get; private set; }
        public Guid Id { get; private set; }
    }
}