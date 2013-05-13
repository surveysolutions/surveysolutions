// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserListViewFactory.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Core.Supervisor.Views.User
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    /// <summary>
    ///     The user list view factory.
    /// </summary>
    public class UserListViewFactory : IViewFactory<UserListViewInputModel, UserListView>
    {
        #region Fields

        /// <summary>
        ///     The accounts.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserListViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public UserListViewFactory(IQueryableDenormalizerStorage<UserDocument> users)
        {
            this.users = users;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The load.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// The <see cref="UserListView"/>.
        /// </returns>
        public UserListView Load(UserListViewInputModel input)
        {
            var hasRole = input.Role != UserRoles.Undefined;

            Func<UserDocument, bool> query = (x) => false;

            if (hasRole)
            {
                query = (x) => x.Roles.Contains(input.Role);
            }

            query = query.AndAlso(x => !x.IsDeleted);

            return this.users.Query(queryable =>
            {
                var queryResult =
                    queryable.Where(query).AsQueryable().OrderUsingSortExpression(input.Order);

                var retVal = queryResult.Skip((input.Page - 1) * input.PageSize)
                                                .Take(input.PageSize)
                                                .Select(
                                                    x =>
                                                    new UserListItem
                                                        {
                                                            PublicKey = x.PublicKey,
                                                            CreationDate = x.CreationDate, 
                                                            Email = x.Email, 
                                                            IsLocked = x.IsLocked, 
                                                            UserName = x.UserName, 
                                                            Roles = x.Roles
                                                        });

                return new UserListView(input.Page, input.PageSize, queryResult.Count(), retVal.ToList(), input.Order);
            });
        }

        #endregion
    }
}