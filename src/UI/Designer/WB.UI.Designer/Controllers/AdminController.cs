// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdministrationController.cs" company="">
//   
// </copyright>
// <summary>
//   The administration controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using Main.Core.Utility;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Extensions;
    using WB.UI.Designer.Models;

    using WebMatrix.WebData;

    /// <summary>
    /// The administration controller.
    /// </summary>
    [Authorize(Roles = "Administrator")]
    public class AdminController : AlertController
    {
        // GET: /Administration/

        // GET: /Administration/Create
        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Create()
        {
            return this.View(new RegisterModel());
        }

        // POST: /Administration/Create

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterModel model)
        {
            if (this.ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { model.Email }, false);
                    Roles.Provider.AddUsersToRoles(new[] { model.UserName }, new[] { UserHelper.USERROLENAME });

                    return this.RedirectToAction("Index");
                }
                catch (MembershipCreateUserException e)
                {
                    this.Error(e.StatusCode.ToErrorCode());
                }
            }

            return View(model);
        }

        // GET: /Administration/Edit/john

        // GET: /Administration/Delete/john

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Delete(string id)
        {
            MembershipUser user = this.GetUser(id);
            return this.View(
                new DeleteAccountModel { Id = user.UserName, UserName = user.UserName, Email = user.Email });
        }

        // POST: /Administration/Delete/john

        /// <summary>
        /// The delete confirmed.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            Membership.DeleteUser(id);

            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The details.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ViewResult"/>.
        /// </returns>
        public ViewResult Details(string id)
        {
            MembershipUser account = this.GetUser(id);
            return
                this.View(
                    new AccountViewModel
                        {
                            Id = account.UserName, 
                            CreationDate = account.CreationDate, 
                            Email = account.Email, 
                            IsApproved = account.IsApproved, 
                            IsLockedOut = account.IsLockedOut, 
                            LastLoginDate = account.LastLoginDate, 
                            UserName = account.UserName, 
                            IsOnline = account.IsOnline, 
                            LastActivityDate = account.LastActivityDate, 
                            LastLockoutDate = account.LastLockoutDate, 
                            PasswordQuestion = account.PasswordQuestion, 
                            LastPasswordChangedDate = account.LastPasswordChangedDate, 
                            Comment = account.Comment
                        });
        }

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        public ActionResult Edit(string id)
        {
            MembershipUser intUser = this.GetUser(id);
            return
                this.View(
                    new UpdateAccountModel
                        {
                            Comment = intUser.Comment, 
                            Email = intUser.Email, 
                            IsApproved = intUser.IsApproved, 
                            IsLockedOut = intUser.IsLockedOut, 
                            UserName = intUser.UserName
                        });
        }

        // POST: /Administration/Edit/john

        /// <summary>
        /// The edit.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="ActionResult"/>.
        /// </returns>
        [HttpPost]
        public ActionResult Edit(UpdateAccountModel user)
        {
            if (this.ModelState.IsValid)
            {
                MembershipUser intUser = Membership.GetUser(user.UserName);
                if (intUser != null)
                {
                    Membership.UpdateUser(
                        new MembershipUser(
                            providerName: intUser.ProviderName, 
                            name: user.UserName, 
                            providerUserKey: intUser.ProviderUserKey, 
                            email: user.Email, 
                            passwordQuestion: intUser.PasswordQuestion, 
                            comment: user.Comment, 
                            isApproved: user.IsApproved, 
                            isLockedOut: user.IsLockedOut, 
                            creationDate: intUser.CreationDate, 
                            lastLoginDate: intUser.LastLoginDate, 
                            lastActivityDate: intUser.LastActivityDate, 
                            lastPasswordChangedDate: intUser.LastPasswordChangedDate, 
                            lastLockoutDate: intUser.LastLockoutDate));
                }

                return this.RedirectToAction("Index");
            }
            else
            {
                return this.View();
            }
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <returns>
        /// The <see cref="ViewResult"/>.
        /// </returns>
        public ViewResult Index(int? p, string sb, bool? so, string f)
        {
            var page = p ?? 1;

            ViewBag.PageIndex = p;
            ViewBag.SortBy = sb;
            ViewBag.Filter = f;
            ViewBag.SortOrder = so;
            if (ViewBag.SortOrder != null && ViewBag.SortOrder)
            {
                sb = string.Format("{0} Desc", sb);
            }

            var users =
                Membership.GetAllUsers()
                          .OfType<MembershipUser>()
                          .Where(x => (!string.IsNullOrEmpty(f) && (x.UserName.Contains(f) || x.Email.Contains(f))) || string.IsNullOrEmpty(f))
                          .AsQueryable()
                          .OrderUsingSortExpression(sb ?? string.Empty);

            Func<MembershipUser, bool> editAction =
                (user) => !Roles.GetRolesForUser(user.UserName).Contains(UserHelper.ADMINROLENAME);

            var retVal =
                users.Skip((page - 1) * GlobalHelper.GridPageItemsCount)
                     .Take(GlobalHelper.GridPageItemsCount)
                     .Select(
                         x =>
                         new AccountListViewItemModel
                             {
                                 Id = x.UserName,
                                 UserName = x.UserName,
                                 Email = x.Email,
                                 CreationDate = x.CreationDate.ToUIString(),
                                 LastLoginDate = x.LastLoginDate.ToUIString(),
                                 IsApproved = x.IsApproved,
                                 IsLockedOut = x.IsLockedOut,
                                 CanEdit = editAction(x),
                                 CanDelete = editAction(x),
                                 CanPreview = editAction(x)
                             });
            return View(retVal.ToPagedList(page, GlobalHelper.GridPageItemsCount, users.Count()));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The get user.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="MembershipUser"/>.
        /// </returns>
        private MembershipUser GetUser(string id)
        {
            return Membership.GetUser(id);
        }

        #endregion
    }
}