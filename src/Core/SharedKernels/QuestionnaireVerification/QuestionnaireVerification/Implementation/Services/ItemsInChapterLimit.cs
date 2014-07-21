using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.SharedKernels.QuestionnaireVerification.Implementation.Services
{
    internal partial class QuestionnaireVerifier
    {
        private static bool ChapterChildrenLimit(IGroup group)
        {
            if (group.GetParent() is IQuestionnaireDocument)
            {
                int count = group.Children.TreeToEnumerable((item) => item.Children).Count();
                return count > 500;
            }

            return false;
        }
    }
}