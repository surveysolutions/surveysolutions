using WB.UI.Designer.Providers.Roles;
using System;

namespace WB.UI.Designer.Models
{
    using WB.UI.Designer.Providers.Roles;

    public static class UserHelper
    {
        public static string ADMINROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
        public static string USERROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
    }
}