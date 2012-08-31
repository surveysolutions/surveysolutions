using System.Web.Security;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Helpers
{
    using System;

    public class GlobalInfo 
    {
        public static UserLight GetCurrentUser()
        {
            MembershipUser currentUser = Membership.GetUser();
            if (currentUser == null)
                return null;
            
            //byte[] key = (byte[])currentUser.ProviderUserKey;
            return new UserLight((Guid)currentUser.ProviderUserKey, currentUser.UserName);
        }
    }
}
