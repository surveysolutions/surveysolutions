using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.Verification;

namespace Web.Supervisor.Models
{
    public class QuestionnaireVerificationResponse
    {
        public QuestionnaireVerificationResponse(bool isSuccess)
        {
            this.IsSuccess = isSuccess;
        }

        public void SetErrorsForQuestionnaire(QuestionnaireVerificationError[] errors, QuestionnaireDocument questionnaire)
        {
            Errors = errors.Select(error => new QuestionnaireVerificationErrorResponse(error, questionnaire)).ToArray();
        }

        public bool IsSuccess { get; private set; }
        public QuestionnaireVerificationErrorResponse[] Errors { get; private set; }
    }

    public class QuestionnaireVerificationErrorResponse
    {
        public QuestionnaireVerificationErrorResponse(QuestionnaireVerificationError error, QuestionnaireDocument questionnaire)
        {
            Code = error.Code;
            Message = error.Message;
            References = error.References.Select(reference => new QuestionnaireVerificationReferenceResponse(reference, questionnaire));
        }
        public string Code { get; private set; }
        public string Message { get; private set; }
        public IEnumerable<QuestionnaireVerificationReferenceResponse> References { get; private set; }
    }

    public class QuestionnaireVerificationReferenceResponse
    {
        public QuestionnaireVerificationReferenceResponse(QuestionnaireVerificationReference reference, QuestionnaireDocument questionnaire)
        {
            Type = reference.Type.ToString();
            Id = reference.Id;
            if (questionnaire != null)
                Title = reference.Type == QuestionnaireVerificationReferenceType.Group
                    ? GetGroupTitle(questionnaire, Id)
                    : GetQuestionTitle(questionnaire, Id);
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