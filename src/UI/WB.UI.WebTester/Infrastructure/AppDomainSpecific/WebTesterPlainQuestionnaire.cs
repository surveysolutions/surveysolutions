using Main.Core.Documents;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.UI.WebTester.Infrastructure.AppDomainSpecific;

public class WebTesterPlainQuestionnaire : PlainQuestionnaire
{
    public WebTesterPlainQuestionnaire(QuestionnaireDocument document, long version, IQuestionOptionsRepository questionOptionsRepository, ISubstitutionService substitutionService, Translation? translation = null) : base(document, version, questionOptionsRepository, substitutionService, translation)
    {
    }
    
    public bool CanSaveScenario { get; set; }
}