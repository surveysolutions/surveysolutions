using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects.Verification;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVerifier : IQuestionnaireVerifier
    {
        public IEnumerable<QuestionnaireVerificationError> Verify(QuestionnaireDocument questionnaire)
        {
            if (NoQuestionsExist(questionnaire))
                return new[] { new QuestionnaireVerificationError("WB0001", VerificationMessages.WB0001_NoQuestions) };

            var errorList = new List<QuestionnaireVerificationError>();
            errorList.AddRange(this.GetListOfErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(questionnaire));
            return errorList;
        }

        private bool NoQuestionsExist(QuestionnaireDocument questionnaire)
        {
            return !questionnaire.Find<IQuestion>(_ => true).Any();
        }

        private IEnumerable<QuestionnaireVerificationError> GetListOfErrorsByPropagatingQuestionsThatHasNoAssociatedGroups(
            QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IAutoPropagateQuestion>(question => question.Triggers.Count == 0).Select(
                question =>
                    new QuestionnaireVerificationError("WB0008", VerificationMessages.WB0008_PropagatingQuestionHasNoAssociatedGroups,
                        new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question, question.PublicKey)));
        }
    }
}