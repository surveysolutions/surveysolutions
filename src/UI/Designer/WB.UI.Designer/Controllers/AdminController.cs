// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AdminController.cs" company="">
//   
// </copyright>
// <summary>
//   The administration controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using WB.UI.Designer.Code;

namespace WB.UI.Designer.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Security;

    using Main.Core.Utility;
    using Main.Core.View;

    using Ncqrs.Commanding.ServiceModel;

    using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
    using WB.UI.Designer.Extensions;
    using WB.UI.Designer.Models;

    using WebMatrix.WebData;

    /// <summary>
    ///     The administration controller.
    /// </summary>
    [CustomAuthorize(Roles = "Administrator")]
    public class AdminController : BaseController
    {
        #region Constructors and Destructors
        
        public AdminController(IViewRepository repository, ICommandService commandService, IUserHelper userHelper)
            : base(repository, commandService,userHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The create.
        /// </summary>
        /// <returns>
        ///     The <see cref="ActionResult" />.
        /// </returns>
        public ActionResult Create()
        {
            return this.View(new RegisterModel());
        }

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

            var questionnaires = QuestionnaireHelper.GetQuestionnairesByUserId(repository: this.Repository, userId: id);
            questionnaires.ToList().ForEach(
                x =>
                    {
                        x.CanEdit = false;
                        x.CanDelete = false;
                    });
            
            return
                this.View(
                    new AccountViewModel
                        {
                            Id = account.ProviderUserKey.AsGuid(),
                            CreationDate = account.CreationDate.ToUIString(),
                            Email = account.Email,
                            IsApproved = account.IsApproved,
                            IsLockedOut = account.IsLockedOut,
                            //IsOnline = account.IsOnline,
                            LastLoginDate = account.LastLoginDate.ToUIString(),
                            UserName = account.UserName,
                            //LastActivityDate = account.LastActivityDate.ToUIString(),
                            LastLockoutDate = account.LastLockoutDate.ToUIString(),
                            LastPasswordChangedDate = account.LastPasswordChangedDate.ToUIString(),
                            Comment = account.Comment ?? GlobalHelper.EmptyString,
                            Questionnaires = questionnaires
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
                            Id = id
                        });
        }

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
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UpdateAccountModel user)
        {
            if (this.ModelState.IsValid)
            {
                MembershipUser intUser = this.GetUser(user.Id);
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
        /// <param name="p">
        /// The p.
        /// </param>
        /// <param name="sb">
        /// The sb.
        /// </param>
        /// <param name="so">
        /// The so.
        /// </param>
        /// <param name="f">
        /// The f.
        /// </param>
        /// <returns>
        /// The <see cref="ViewResult"/>.
        /// </returns>
        public ViewResult Index(int? p, string sb, int? so, string f)
        {
            int page = p ?? 1;

            this.ViewBag.PageIndex = p;
            this.ViewBag.SortBy = sb;
            this.ViewBag.Filter = f;
            this.ViewBag.SortOrder = so;

            if (so.ToBool())
            {
                sb = string.Format("{0} Desc", sb);
            }

            IEnumerable<MembershipUser> users =
                Membership.GetAllUsers()
                          .OfType<MembershipUser>()
                          .Where(
                              x =>
                              (!string.IsNullOrEmpty(f) && (x.UserName.Contains(f) || x.Email.Contains(f)))
                              || string.IsNullOrEmpty(f))
                          .AsQueryable()
                          .OrderUsingSortExpression(sb ?? string.Empty);

            Func<MembershipUser, bool> editAction =
                (user) => !Roles.GetRolesForUser(user.UserName).Contains(UserHelper.ADMINROLENAME);

            IEnumerable<AccountListViewItemModel> retVal =
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
            return Membership.GetUser(id, false);
        }

        #endregion
    }
}