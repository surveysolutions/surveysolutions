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
    [CustomAuthorize(Roles = "Administrator")]
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
        [ValidateAntiForgeryToken]
        public ActionResult Delete(Guid id)
        {
            MembershipUser user = this.GetUser(id);
            if (user == null)
            {
                this.Error(string.Format("User \"{0}\" doesn't exist", id));
            }
            else
            {
                Membership.DeleteUser(user.UserName);
                this.Success(string.Format("User \"{0}\" successfully deleted", user.UserName));
            }

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
        public ViewResult Details(Guid id)
        {
            MembershipUser account = this.GetUser(id);
            return
                this.View(
                    new AccountViewModel
                        {
                            Id = account.ProviderUserKey.AsGuid(), 
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
        public ActionResult Edit(Guid id)
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
                            UserName = intUser.UserName,
                            UserId = intUser.ProviderUserKey.AsGuid()
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
                MembershipUser intUser = this.GetUser(user.UserId);
                if (intUser != null)
                {
                    Membership.UpdateUser(
                        new MembershipUser(
                            providerName: intUser.ProviderName,
                            name: intUser.UserName,
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
        public ViewResult Index(int? p, string sb, int? so, string f)
        {
            var page = p ?? 1;

            ViewBag.PageIndex = p;
            ViewBag.SortBy = sb;
            ViewBag.Filter = f;
            ViewBag.SortOrder = so;

            if (so.ToBool())
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
                                 Id = x.ProviderUserKey.AsGuid(),
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
        private MembershipUser GetUser(Guid id)
        {
            return Membership.GetUser(id);
        }

        #endregion
    }
}