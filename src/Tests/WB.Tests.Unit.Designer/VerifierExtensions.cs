using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer
{
    internal static class VerifierExtensions
    {
        public static QuestionnaireVerificationMessage GetMessage(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
            => verificationMessages.SingleOrDefault(message
                => message.Code == code);

        public static QuestionnaireVerificationMessage GetWarning(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
            => verificationMessages.SingleOrDefault(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                && message.Code == code);

        public static IEnumerable<QuestionnaireVerificationMessage> GetWarnings(
      this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
      => verificationMessages.Where(message
          => message.MessageLevel == VerificationMessageLevel.Warning
          && message.Code == code);

        public static QuestionnaireVerificationMessage GetError(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
            => verificationMessages.SingleOrDefault(message
                => message.MessageLevel == VerificationMessageLevel.General
                && message.Code == code);

        public static QuestionnaireVerificationMessage GetCritical(
           this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
           => verificationMessages.SingleOrDefault(message
               => message.MessageLevel == VerificationMessageLevel.Critical
               && message.Code == code);
    }
}