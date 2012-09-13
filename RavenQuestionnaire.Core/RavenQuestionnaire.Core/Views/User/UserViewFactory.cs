﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserViewFactory.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The user view factory.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace RavenQuestionnaire.Core.Views.User
{
    using System;
    using System.Linq;

    using Main.Core.Documents;

    using RavenQuestionnaire.Core.Denormalizers;

    /// <summary>
    /// The user view factory.
    /// </summary>
    public class UserViewFactory : IViewFactory<UserViewInputModel, UserView>
    {
        // remove when done
        /*  private IDocumentSession documentSession;
        public UserViewFactory(IDocumentSession documentSession)
        {
            this.documentSession = documentSession;
        }*/
        #region Fields

        /// <summary>
        /// The users.
        /// </summary>
        private readonly IDenormalizerStorage<UserDocument> users;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserViewFactory"/> class.
        /// </summary>
        /// <param name="users">
        /// The users.
        /// </param>
        public UserViewFactory(IDenormalizerStorage<UserDocument> users)
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
        /// The RavenQuestionnaire.Core.Views.User.UserView.
        /// </returns>
        public UserView Load(UserViewInputModel input)
        {
            UserDocument doc = null;

            if (input.UserId != Guid.Empty)
            {
                doc = this.users.Query().FirstOrDefault(u => u.PublicKey == input.UserId);
            }
            else if (!string.IsNullOrEmpty(input.UserName) && string.IsNullOrEmpty(input.Password))
            {
                doc =
                    this.users.Query().FirstOrDefault(
                        u => string.Compare(u.UserName, input.UserName, StringComparison.OrdinalIgnoreCase) == 0);
            }

            if (!string.IsNullOrEmpty(input.UserName) && !string.IsNullOrEmpty(input.Password))
            {
                doc = this.users.Query().FirstOrDefault(u => u.UserName == input.UserName);

                if (doc != null && doc.Password != input.Password)
                {
                    return null;
                }
            }

            if (doc == null || doc.IsDeleted)
            {
                return null;
            }

            return new UserView(
                doc.PublicKey, 
                doc.UserName, 
                doc.Password, 
                doc.Email, 
                doc.CreationDate, 
                doc.Roles, 
                doc.IsLocked, 
                doc.Supervisor, 
                doc.Location.Id);
        }

        #endregion
    }
}