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
        private static readonly QuestionnaireVersion version_2_2_0 = new QuestionnaireVersion(2, 2, 0);
        private static readonly QuestionnaireVersion version_3 = new QuestionnaireVersion(3, 0, 0);


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

            int maskQuestionCount = GetQuestionsWithMaskCount(questionnaire);
            int staticTextCount = GetStaticTextCount(questionnaire);
            int groupWithVariableNameCount = GetGroupWithVariableNameCount(questionnaire);
            if ((maskQuestionCount > 0 || groupWithVariableNameCount > 0 || staticTextCount > 0) && version < version_2_2_0)
                version = version_2_2_0;

            int filteredComboboxQuestionsCount = GetFilteredComboboxQuestionsCount(questionnaire);
            if (filteredComboboxQuestionsCount > 0 && version < version_3)
                version = version_3;

            return version;
        }

        private int GetFilteredComboboxQuestionsCount(QuestionnaireDocument questionnaire)
        {
            return
                questionnaire.Find<SingleQuestion>(
                    question => question.IsFilteredCombobox.HasValue && question.IsFilteredCombobox.Value).Count();
        }

        private int GetStaticTextCount(QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IStaticText>(x => true).Count();
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

        private static int GetQuestionsWithMaskCount(QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<TextQuestion>(x => !string.IsNullOrEmpty(x.Mask)).Count();
        }

        private static int GetGroupWithVariableNameCount(QuestionnaireDocument questionnaire)
        {
            return questionnaire.Find<IGroup>(x => !string.IsNullOrEmpty(x.VariableName)).Count();
        }
    }
}