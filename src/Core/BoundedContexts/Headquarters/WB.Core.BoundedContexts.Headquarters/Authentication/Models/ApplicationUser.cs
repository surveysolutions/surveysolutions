using System;
using System.Linq;
using System.Security.Claims;
using AspNet.Identity.RavenDB.Entities;
using Raven.Imports.Newtonsoft.Json;
using Raven.Imports.Newtonsoft.Json.Schema;
using WB.Core.GenericSubdomains.Utils;

namespace WB.Core.BoundedContexts.Headquarters.Authentication.Models
{
    public class ApplicationUser : RavenUser
    {
        private ApplicationUser() 
        {
        }

        public ApplicationUser(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            this.Id = id;
        }

        [JsonIgnore]
        public bool IsAdministrator
        {
            get
            {
                return this.Claims.Any(x => x.ClaimType == ClaimTypes.Role && x.ClaimValue == ApplicationRoles.Administrator);
            }

            set
            {
                this.SetHasRoleFlag(value, ApplicationRoles.Administrator);
            }
        }

        [JsonIgnore]
        public bool IsHeadquarter
        {
            get
            {
                return this.Claims.Any(x => x.ClaimType == ClaimTypes.Role && x.ClaimValue == ApplicationRoles.Headquarter);
            }
            set
            {
                this.SetHasRoleFlag(value, ApplicationRoles.Headquarter);
            }
        }

        private void SetHasRoleFlag(bool value, string roleName)
        {
            RavenUserClaim administratorClaim = this.FindRole(roleName);

            if (administratorClaim != null)
            {
                this.Claims.Remove(administratorClaim);
            }

            if (value)
            {
                this.AddRole(roleName);
            }
        }

        private void AddRole(string rolename)
        {
            this.Claims.Add(new RavenUserClaim(new Claim(ClaimTypes.Role, rolename)));
        }

        private RavenUserClaim FindRole(string roleName)
        {
            return this.Claims.FirstOrDefault(x => x.ClaimType == ClaimTypes.Role && x.ClaimValue == roleName);
        }
    }
}