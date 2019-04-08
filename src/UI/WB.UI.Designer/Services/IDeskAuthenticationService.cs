using System;

namespace WB.UI.Designer.Services
{
    public interface IDeskAuthenticationService
    {
        string GetReturnUrl(Guid userId, string userName, string userEmail, DateTime expiryDate);
    }
}