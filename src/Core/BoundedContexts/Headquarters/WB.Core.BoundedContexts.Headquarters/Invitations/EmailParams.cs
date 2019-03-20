using WB.Infrastructure.Native.Storage.Postgre.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Invitations
{
    public class EmailParams : IStorableEntity
    {
        public string Subject { get; set; }
        public string MainText { get; set; }
        public string PasswordDescription { get; set; }
        public string LinkText { get; set; }
        public string Password{ get; set; }
        public string Link{ get; set; }
        public string SurveyName{ get; set; }
        public string Address{ get; set; }
        public string SenderName{ get; set; }
        public int AssignmentId { get; set; }
        public int InvitationId { get; set; }
    }

    public class EmailParameters : EmailParams
    {
    }
}
