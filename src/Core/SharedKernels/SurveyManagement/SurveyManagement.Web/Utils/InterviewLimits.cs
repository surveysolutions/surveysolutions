using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveyManagement.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils
{
    public class InterviewLimits
    {
        private static ILimitedInterviewService LimitedInterviewService
        {
            get { return ServiceLocator.Current.GetInstance<ILimitedInterviewService>(); }
        }

        public static bool LimitsEnabled
        {
            get { return ServiceLocator.Current.GetAllInstances<ILimitedInterviewService>().Any(); }
        }

        public static bool PayAttention { get { return InterviewsLeft <= (LimitedInterviewService.Limit / 10); } }

        public static long InterviewsLeft { get { return LimitedInterviewService.Limit - LimitedInterviewService.CreatedInterviewCount; } }
    }
}
