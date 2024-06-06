using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class CriticalityLevelRequest
    {
        public CriticalityLevel CriticalityLevel { get; set; }
    }
}
