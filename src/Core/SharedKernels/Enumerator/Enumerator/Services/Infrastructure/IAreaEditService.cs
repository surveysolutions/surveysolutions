﻿using System.Threading.Tasks;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;

namespace WB.Core.SharedKernels.Enumerator.Services.Infrastructure
{
    public interface IAreaEditService
    {
        Task<AreaEditResult> EditAreaAsync(Area area);
    }
}
