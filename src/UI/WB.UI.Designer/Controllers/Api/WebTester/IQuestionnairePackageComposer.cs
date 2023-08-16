using System;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.UI.Designer.Api.WebTester
{
    public interface IQuestionnairePackageComposer
    {
        Questionnaire? ComposeQuestionnaire(Guid questionnaireId);
    }
}
