﻿using System.Web.Security;

namespace WB.UI.Headquarters.Code
{
    public class IdentityManager : IIdentityManager
    {
        public string[] GetUsersInRole(string roleName)
        {
            return Roles.GetUsersInRole(roleName);
        }
    }
}