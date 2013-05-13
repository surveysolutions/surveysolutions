// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserViewFactory.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Core.Supervisor.Views.User
{
    using System;
    using System.Linq;

    using Main.Core.Documents;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.DenormalizerStorage;

    /// <summary>
    ///     The user view factory.
    /// </summary>
    internal class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        #region Fields

        /// <summary>
        ///     The users.
        /// </summary>
        private readonly IQueryableDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public UserViewFactory(IQueryableDenormalizerStorage<UserDocument> users)
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
        /// The <see cref="UserView"/>.
        /// </returns>
        public UserView Load(UserViewInputModel input)
        {
            Func<UserDocument, bool> query = (x) => false;
            if (input.PublicKey != null)
            {
                query = (x) => x.PublicKey.Equals(input.PublicKey);
            }
            else if (!string.IsNullOrEmpty(input.UserName))
            {
                query = (x) => x.UserName.Compare(input.UserName);
            }
            else if (!string.IsNullOrEmpty(input.UserEmail))
            {
                query = (x) => x.Email.Compare(input.UserEmail);
            }

            return
                this.users.Query(_ => _
                    .Where(query)
                    .Select(
                        x =>
                        new UserView
                            {
                                CreationDate = x.CreationDate, 
                                UserName = x.UserName, 
                                Email = x.Email, 
                                IsDeleted = x.IsDeleted, 
                                IsLocked = x.IsLocked, 
                                PublicKey = x.PublicKey, 
                                Roles = x.Roles, 
                                Location = x.Location, 
                                Password = x.Password, 
                                Supervisor = x.Supervisor
                            })
                    .FirstOrDefault());
        }

        #endregion
    }
}