using System;
using System.Security.Claims;

namespace WB.UI.Designer.Services
{
    /// <summary>
    /// Helpers for extracting and validating claims from delegated JWT principals.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Returns <c>true</c> when the principal carries a <c>questionnaire_id</c> claim
        /// whose value matches <paramref name="questionnaireId"/>.
        /// </summary>
        public static bool HasMatchingQuestionnaireId(this ClaimsPrincipal user, Guid questionnaireId)
        {
            var claim = user.FindFirst(JwtTokenService.QuestionnaireIdClaimType);
            return claim != null
                   && Guid.TryParse(claim.Value, out var tokenQuestionnaireId)
                   && tokenQuestionnaireId == questionnaireId;
        }
    }
}
