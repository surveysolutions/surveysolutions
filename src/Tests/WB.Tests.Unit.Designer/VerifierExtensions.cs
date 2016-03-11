using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer
{
    internal static class VerifierExtensions
    {
        public static QuestionnaireVerificationMessage GetWarning(
            this IEnumerable<QuestionnaireVerificationMessage> verificationMessages, string code)
            => verificationMessages.Single(message
                => message.MessageLevel == VerificationMessageLevel.Warning
                && message.Code == code);
    }
}