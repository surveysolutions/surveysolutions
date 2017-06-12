using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.UI.Shared.Enumerator.CustomServices.AreaEditor
{
    public class DummyAreaEditService : IAreaEditService
    {
        public Task<AreaEditResult> EditAreaAsync(Area area)
        {
            throw new NotImplementedException("This functionality is not available");
        }
    }
}