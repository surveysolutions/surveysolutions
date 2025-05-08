using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IInvitationMailingService
    {
        Task<string> SendInvitationAsync(int invitationId, Assignment assignment, string email = null);
        Task<string> SendResumeAsync(int invitationId, Assignment assignment, string email);
    }
}
