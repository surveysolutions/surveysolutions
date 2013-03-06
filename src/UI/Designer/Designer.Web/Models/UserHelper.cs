using Designer.Web.Providers.Roles;
using System;

namespace Designer.Web.Models
{
    public static class UserHelper
    {
        public static string ADMINROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.Administrator);
        public static string USERROLENAME = Enum.GetName(typeof(SimpleRoleEnum), SimpleRoleEnum.User);
    }
}