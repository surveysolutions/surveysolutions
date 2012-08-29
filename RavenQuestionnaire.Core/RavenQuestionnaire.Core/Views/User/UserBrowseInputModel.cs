// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserBrowseInputModel.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The user browse input model.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Core.Views.User
{
    using System;

    using RavenQuestionnaire.Core.Documents;
    using RavenQuestionnaire.Core.Entities.SubEntities;
    using RavenQuestionnaire.Core.Utility;

    /// <summary>
    /// The user browse input model.
    /// </summary>
    public class UserBrowseInputModel
    {
        #region Fields

        /// <summary>
        /// The _page.
        /// </summary>
        private int _page = 1;

        /// <summary>
        /// The _page size.
        /// </summary>
        private int _pageSize = 20;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBrowseInputModel"/> class.
        /// </summary>
        public UserBrowseInputModel()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserBrowseInputModel"/> class.
        /// </summary>
        /// <param name="role">
        /// The role.
        /// </param>
        public UserBrowseInputModel(UserRoles role)
        {
            this.Role = role;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Func<UserDocument, bool> Expression
        {
            get
            {
                if (!string.IsNullOrEmpty(this.LocationId))
                {
                    string locatianOriginalId = IdUtil.CreateLocationId(this.LocationId);
                    return e => !e.IsDeleted && e.Location.Id == locatianOriginalId;
                }

                if (!this.Role.HasValue)
                {
                    return e => !e.IsDeleted;
                }

                return e => !e.IsDeleted && e.Roles.Contains(this.Role.Value);
            }
        }

        /// <summary>
        /// Gets or sets the location id.
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Gets or sets the page.
        /// </summary>
        public int Page
        {
            get
            {
                return this._page;
            }

            set
            {
                this._page = value;
            }
        }

        /// <summary>
        /// Gets or sets the page size.
        /// </summary>
        public int PageSize
        {
            get
            {
                return this._pageSize;
            }

            set
            {
                this._pageSize = value;
            }
        }

        /// <summary>
        /// Gets the role.
        /// </summary>
        public UserRoles? Role { get; private set; }

        #endregion
    }
}