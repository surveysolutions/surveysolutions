namespace Questionnaire.Core.Web.Security
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;

    using Main.Core.Entities.SubEntities;

    /// <summary>
    /// The questionnaire authorize attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class QuestionnaireAuthorizeAttribute : FilterAttribute, IAuthorizationFilter
    {
        #region Fields

        /// <summary>
        /// The _accepted roles.
        /// </summary>
        private readonly string[] _acceptedRoles;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="acceptedRoles">
        /// The accepted roles.
        /// </param>
        public QuestionnaireAuthorizeAttribute(params UserRoles[] acceptedRoles)
        {
            var roles = new string[acceptedRoles.Length];
            for (int i = 0; i < roles.Length; i++)
            {
                roles[i] = acceptedRoles[i].ToString();
            }

            this._acceptedRoles = roles;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionnaireAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="acceptedRoles">
        /// The accepted roles.
        /// </param>
        public QuestionnaireAuthorizeAttribute(params string[] acceptedRoles)
        {
            this._acceptedRoles = acceptedRoles;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The on authorization.
        /// </summary>
        /// <param name="filterContext">
        /// The filter context.
        /// </param>
        /// <exception cref="HttpException">
        /// </exception>
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (this._acceptedRoles.Any(Roles.IsUserInRole))
            {
                return;
            }

            throw new HttpException(403, "No access to this method");
        }

        #endregion
    }
}