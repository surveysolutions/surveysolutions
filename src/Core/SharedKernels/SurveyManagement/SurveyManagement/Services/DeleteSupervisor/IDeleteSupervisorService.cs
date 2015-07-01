using System;
using System.Threading.Tasks;

namespace WB.Core.SharedKernels.SurveyManagement.Services.DeleteSupervisor
{
    public interface IDeleteSupervisorService
    {
        void DeleteSupervisor(Guid supervisorId);
    }
}