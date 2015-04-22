using System.Web.Security;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Code.Security
{
    public class CustomIdentity : System.Security.Principal.IIdentity
    {
        private readonly FormsAuthenticationTicket ticket;

        public CustomIdentity(FormsAuthenticationTicket ticket)
        {
            this.ticket = ticket;
            name = ticket.Name;
            isObserver = !string.IsNullOrEmpty(ticket.UserData);
            observerName = ticket.UserData;
        }

        public string AuthenticationType
        {
            get { return "Custom"; }
        }

        public bool IsAuthenticated
        {
            get { return true; }
        }

        private readonly string name;
        public string Name
        {
            get { return name; }
        }

        public FormsAuthenticationTicket Ticket
        {
            get { return ticket; }
        }

        private readonly bool isObserver = false;
        public bool IsObserver
        {
            get { return isObserver; }
        }

        private readonly string observerName;
        public string ObserverName
        {
            get
            {
                return observerName;
            }
        }
    }
}