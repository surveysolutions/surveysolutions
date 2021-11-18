using System;
using System.Collections.Generic;
using WB.Core.SharedKernels.Enumerator.Utils;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Core.SharedKernels.Enumerator.Services
{
    public interface ICompositeCollectionInflationService
    {
        CompositeCollection<ICompositeEntity> GetInflatedCompositeCollection(
            string interviewId,
            IEnumerable<IInterviewEntityViewModel> newGroupItems);
    }
}
