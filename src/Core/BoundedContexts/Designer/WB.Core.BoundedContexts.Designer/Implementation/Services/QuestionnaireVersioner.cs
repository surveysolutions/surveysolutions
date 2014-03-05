using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;

namespace WB.Core.BoundedContexts.Designer.Implementation.Services
{
    internal class QuestionnaireVersioner : IQuestionnaireVersioner
    {
        private static readonly QuestionnaireVersion version_1_6_1 = new QuestionnaireVersion(1, 6, 1);
        private static readonly QuestionnaireVersion version_1_6_2 = new QuestionnaireVersion(1, 6, 2);

        public QuestionnaireVersion GetVersion(QuestionnaireDocument questionnaire)
        {
            var version = new QuestionnaireVersion(1, 6, 0);
            questionnaire.ConnectChildrenWithParent();

            int qrBarcodeQuestionCount = GetQrBarcodeQuestionsCount(questionnaire);
            if (qrBarcodeQuestionCount > 0 && version < version_1_6_1)
                version = version_1_6_1;

            int nestedRostersCount = GetNestedRostersCount(questionnaire);
            if (nestedRostersCount > 0 && version < version_1_6_2) 
                version = version_1_6_2;

            return version;
        }

        private int GetNestedRostersCount(QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IGroup>(x => x.IsRoster).Count(x => GetAllParentGroups(questionnaire, x).Count(y => y.IsRoster) > 0);
        }

        private IEnumerable<IGroup> GetAllParentGroups(QuestionnaireDocument questionnaire, IComposite entity)
        {
            var currentParent = (IGroup)entity.GetParent();

            while (currentParent != null)
            {
                yield return currentParent;

                currentParent = (IGroup)currentParent.GetParent();
            }
        }

        private static int GetQrBarcodeQuestionsCount(QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<QRBarcodeQuestion>(x => true).Count();
        }
    }
}