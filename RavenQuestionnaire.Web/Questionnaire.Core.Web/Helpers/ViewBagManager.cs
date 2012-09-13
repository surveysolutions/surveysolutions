// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ViewBagManager.cs" company="">
//   
// </copyright>
// <summary>
//   The view bag manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Questionnaire.Core.Web.Helpers
{
    using System.Collections.Generic;
    using System.Linq;

    using RavenQuestionnaire.Core;
    using RavenQuestionnaire.Core.Views.User;

    /// <summary>
    /// The view bag manager.
    /// </summary>
    public class ViewBagManager : IBagManager
    {
        #region Public Methods and Operators

        /// <summary>
        /// The add users to bag.
        /// </summary>
        /// <param name="bag">
        /// The bag.
        /// </param>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public void AddUsersToBag(dynamic bag, IViewRepository viewRepository)
        {
            IEnumerable<UserBrowseItem> users =
                viewRepository.Load<UserBrowseInputModel, UserBrowseView>(new UserBrowseInputModel { PageSize = 300 }).
                    Items;
            List<UserBrowseItem> list = users.ToList();
            bag.Users = list;
        }

        #endregion
    }
}