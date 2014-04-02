using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.UI.Headquarters.Models
{
    public class QuestionnaireVerificationResponse
    {
        public QuestionnaireVerificationResponse(bool isSuccess, string questionnaireTitle = null)
        {
            this.IsSuccess = isSuccess;
            this.QuestionnaireTitle = questionnaireTitle;
        }

        public void SetErrorsForQuestionnaire(QuestionnaireVerificationError[] errors, QuestionnaireDocument questionnaire)
        {
            this.Errors = errors.Select(error => new QuestionnaireVerificationErrorResponse(error, questionnaire)).ToArray();
        }

        public bool IsSuccess { get; private set; }
        public string QuestionnaireTitle { get; private set; }
        public QuestionnaireVerificationErrorResponse[] Errors { get; private set; }
        public string ImportError { get; set; }
    }

    public class QuestionnaireVerificationErrorResponse
    {
        public QuestionnaireVerificationErrorResponse(QuestionnaireVerificationError error, QuestionnaireDocument questionnaire)
        {
            this.Code = error.Code;
            this.Message = error.Message;
            this.References = error.References.Select(reference => new QuestionnaireVerificationReferenceResponse(reference, questionnaire));
        }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<QuestionnaireVerificationReferenceResponse> References { get; private set; }
    }

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