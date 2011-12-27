using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class QuestionnaireAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        private readonly string[] _acceptedRoles;

        public QuestionnaireAuthorizeAttribute(params UserRoles[] acceptedRoles)
        {
            string[] roles= new string[acceptedRoles.Length];
            for (int i = 0; i < roles.Length; i++)
            {
                roles[i] = acceptedRoles[i].ToString();
            }
            _acceptedRoles = roles;
        }
        public QuestionnaireAuthorizeAttribute(params string[] acceptedRoles)
        {
            _acceptedRoles = acceptedRoles;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (_acceptedRoles.Any(Roles.IsUserInRole))
            {
                return;
            }
            throw new HttpException(403, "No access to this method");
        }
    }
}
