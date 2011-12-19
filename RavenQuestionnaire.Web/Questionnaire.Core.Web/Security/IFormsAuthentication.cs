using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Questionnaire.Core.Web.Membership
{
    public interface IFormsAuthentication
    {
        void SignIn(string userName, bool rememberMe);
        void SignOut();
        string GetUserIdForCurrentUser();
    }
}
