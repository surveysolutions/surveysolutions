using System;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.UI.Shared.Android.Services
{
    internal class AndroidPreconditionsService : IInterviewPreconditionsService
    {
        public int? GetMaxAllowedInterviewsCount()
        {
            return null;
        }

        public int? GetInterviewsCountAllowedToCreateUntilLimitReached()
        {
            return null;
        }
    }
}
