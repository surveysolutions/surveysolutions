using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Main.Core.Entities.SubEntities;
using Resources;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.User;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.SurveyManagement.Web.Controllers;
using WB.Core.SharedKernels.SurveyManagement.Web.Filters;
using WB.Core.SharedKernels.SurveyManagement.Web.Models;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Observer")]
    [ValidateInput(false)]
    public class HeadquartersController : TeamController
    {
        public HeadquartersController(ICommandService commandService, 
                              IGlobalInfoProvider globalInfo, 
                              ILogger logger,
                              IUserViewFactory userViewFactory,
                              IPasswordHasher passwordHasher)
            : base(commandService, globalInfo, logger, userViewFactory, passwordHasher)
        {
            
        }

        public ActionResult Create()
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            return this.View(new UserModel());
        }

        [HttpPost]
        [PreventDoubleSubmit]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        [ObserverNotAllowed]
        public ActionResult Create(UserModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            if (this.ModelState.IsValid)
            {
                try
                {
                    this.CreateHeadquarters(model);
                }
                catch (Exception e)
                {
                    this.Logger.Error(e.Message, e);
                    this.Error(e.Message);
                    return this.View(model);
                }

                this.Success(HQ.UserWasCreated);
                return this.RedirectToAction("Index");
            }

            return this.View(model);
        }

        [Authorize(Roles = "Administrator, Observer")]
        public ActionResult Index()
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            return this.View();
        }

        [Authorize(Roles = "Administrator")]
        public ActionResult Edit(Guid id)
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            var user = this.GetUserById(id);

            if(user == null) throw new HttpException(404, string.Empty);

            return this.View(new UserEditModel()
                {
                    Id = user.PublicKey,
                    Email = user.Email,
                    IsLocked = user.IsLockedByHQ,
                    UserName = user.UserName,
                    PersonName = user.PersonName,
                    PhoneNumber = user.PhoneNumber
                });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrator")]
        [ObserverNotAllowed]
        public ActionResult Edit(UserEditModel model)
        {
            this.ViewBag.ActivePage = MenuItem.Headquarters;

            if (this.ModelState.IsValid)
            {
                var user = this.GetUserById(model.Id);
                if (user == null)
                {
                    this.Error(HQ.UserNotExists);
                    return this.View(model);
                }

                bool isAdmin = Roles.IsUserInRole(user.UserName, UserRoles.Administrator.ToString());

                if (isAdmin)
                    this.Error(HQ.NoPermission);
                else
                {
                    this.UpdateAccount(user: user, editModel: model);

                    this.Success(string.Format(HQ.UserWasUpdatedFormat, user.UserName));

                    return this.RedirectToAction("Index");
                }

            }
            
            return this.View(model);
        }
    }
}