using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.SurveyManagement.Views.Interviewer;

namespace WB.Core.SharedKernels.SurveyManagement.Views.User
{
    /// <summary>
    ///     The user list item.
    /// </summary>
    public class UserListItem : InterviewersItem
    {
        #region Constructors and Destructors

        public UserListItem(Guid id, string name, string email, DateTime creationDate, bool isLockedBySupervisor, bool isLockedByHQ, List<UserRoles> roles)
            : base(id, name, email, creationDate, isLockedBySupervisor, isLockedByHQ)
        {
            this.Roles = roles;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the roles.
        /// </summary>
        public List<UserRoles> Roles { get; set; }

        #endregion
    }
}