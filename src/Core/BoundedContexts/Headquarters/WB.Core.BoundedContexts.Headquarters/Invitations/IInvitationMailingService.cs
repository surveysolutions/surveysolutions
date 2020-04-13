using System.Threading.Tasks;
using WB.Core.BoundedContexts.Headquarters.Assignments;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public interface IInvitationMailingService
    {
        Task SendInvitationAsync(int invitationId, Assignment assignment, string email = null);
        Task SendResumeAsync(int invitationId, Assignment assignment, string email);
    }
}