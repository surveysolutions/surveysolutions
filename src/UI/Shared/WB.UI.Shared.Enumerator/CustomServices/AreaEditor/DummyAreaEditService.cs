using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Questionnaire.Documents;
using Area = WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.Area;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class DummyAreaEditService : IAreaEditService
    {
        public Task<AreaEditResult> EditAreaAsync(Area area, GeometryType? geometryType)
        {
            throw new NotImplementedException("This functionality is not available");
        }
    }
}
