namespace WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership
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