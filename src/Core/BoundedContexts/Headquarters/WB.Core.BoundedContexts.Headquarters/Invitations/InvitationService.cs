using System.Threading;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class InvitationService : IInvitationService
    {
        private readonly IPlainStorageAccessor<Assignment> assignmentStorage;
        private readonly IPlainStorageAccessor<Invitation> invitationStorage;

        public InvitationService(
            IPlainStorageAccessor<Assignment> assignmentStorage, 
            IPlainStorageAccessor<Invitation> invitationStorage)
        {
            this.assignmentStorage = assignmentStorage;
            this.invitationStorage = invitationStorage;
        }

        public void CreateInvitationForWebInterview(Assignment assignment)
        {
            var hasEmail = !string.IsNullOrWhiteSpace(assignment.Email);
            var hasPassword = !string.IsNullOrWhiteSpace(assignment.Password);
            var isPrivateAssignment = (assignment.Quantity ?? 1) == 1;

            /*
            Quantity  Password 	    Email 	    
            1         empty 	    empty      -
           -1 	      empty 	    not empty  -
           -1 	      not empty 	not empty  -
           */
            if (isPrivateAssignment && !hasEmail && !hasPassword)
                return;

            if (!isPrivateAssignment && hasEmail)
                return;

            var assignmentId = assignment.Id;
            var invitation = new Invitation(assignmentId);
            var questionnaireHash = assignment.QuestionnaireId.GetHashCode();

            if (!isPrivateAssignment)
            {
                /*
                Quantity  Password 	    Email 	    
                -1 	      empty 	    empty      Public link, no password
                -1 	      not empty 	empty      Public link, with password
                */
                var token = TokenGenerator.Instance.Generate(questionnaireHash + 293 * assignmentId);
                invitation.SetToken(token);
            }
            else
            {
               /*
               Quantity  Password 	    Email 	    
               1 	      not empty 	empty      Public link, unique passwords. Token should be unique for all assignments
               1          empty 	    not empty  Private link, no password
               1 	      not empty 	not empty  Private link, with password
               */
               if (!hasEmail)
               {
                   
                   var token = TokenGenerator.Instance.Generate(questionnaireHash);
                   invitation.SetToken(token);
               }
               else
               {
                   var hash = questionnaireHash + assignment.Email.GetHashCode() + assignmentId;
                   var token = TokenGenerator.Instance.Generate(hash);
                   invitation.SetToken(token);
               }
            }

            invitationStorage.Store(invitation, null);
        }

        public int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return 0;
        }

        public int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity)
        {
            return 0;
        }
    }

    public interface IInvitationService
    {
        void CreateInvitationForWebInterview(Assignment assignment);
        int GetCountOfInvitations(QuestionnaireIdentity questionnaireIdentity);
        int GetCountOfNotSentInvitations(QuestionnaireIdentity questionnaireIdentity);
    }

    public sealed class TokenGenerator
    {
        private const string Encode_32_Chars = "XVNWRCK53MTFQP7A6HUBZ4GEID921SL8";

        private static readonly ThreadLocal<char[]> _charBufferThreadLocal =
            new ThreadLocal<char[]>(() => new char[13]);

        private TokenGenerator() { }

        /// <summary>
        /// Returns a single instance of the <see cref="TokenGenerator"/>.
        /// </summary>
        public static TokenGenerator Instance { get; } = new TokenGenerator();

        /// <summary>
        /// Returns an ID. e.g: <c>0HLHI1F5INOFA</c>
        /// </summary>
        public string Generate(long id) => GenerateImpl(id);

        private static string GenerateImpl(long id)
        {
            var buffer = _charBufferThreadLocal.Value;

            buffer[0] = Encode_32_Chars[(int)(id >> 60) & 31];
            buffer[1] = Encode_32_Chars[(int)(id >> 55) & 31];
            buffer[2] = Encode_32_Chars[(int)(id >> 50) & 31];
            buffer[3] = Encode_32_Chars[(int)(id >> 45) & 31];
            buffer[4] = Encode_32_Chars[(int)(id >> 40) & 31];
            buffer[5] = Encode_32_Chars[(int)(id >> 35) & 31];
            buffer[6] = Encode_32_Chars[(int)(id >> 30) & 31];
            buffer[7] = Encode_32_Chars[(int)(id >> 25) & 31];
            buffer[8] = Encode_32_Chars[(int)(id >> 20) & 31];
            buffer[9] = Encode_32_Chars[(int)(id >> 15) & 31];
            buffer[10] = Encode_32_Chars[(int)(id >> 10) & 31];
            buffer[11] = Encode_32_Chars[(int)(id >> 5) & 31];
            buffer[12] = Encode_32_Chars[(int)id & 31];

            return new string(buffer, 0, buffer.Length);
        }
    }
}
