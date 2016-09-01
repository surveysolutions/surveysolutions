namespace WB.UI.Shared.Web.Membership
{
    public interface IMembershipUserService
    {
        string ADMINROLENAME { get; }

        string USERROLENAME { get; }

        IMembershipWebServiceUser WebServiceUser { get; }

    
        IMembershipWebUser WebUser { get; }

        void Logout();
    }
}