using System.Collections.Generic;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;

namespace WB.Core.SharedKernels.DataCollection.WebApi;

public class QuestionnairesSettingsApiView
{
    public List<QuestionnaireIdentity> SwitchableToWeb { set; get; }
        
    public List<QuestionnaireCriticalityLevel> CriticalityLevel  { set; get; }
}

public class QuestionnaireCriticalityLevel
{
    public QuestionnaireIdentity Questionnaire { get; set; }
    public CriticalityLevel? CriticalityLevel { get; set; }
}
