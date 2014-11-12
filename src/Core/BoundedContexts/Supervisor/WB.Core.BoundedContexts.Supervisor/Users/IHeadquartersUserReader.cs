using System;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveyManagement.Views.User;

namespace WB.Core.BoundedContexts.Supervisor.Users
{
    public interface IHeadquartersUserReader
    {
        Task<UserView> GetUserByUri(Uri headquartersUserUri);
    }
}