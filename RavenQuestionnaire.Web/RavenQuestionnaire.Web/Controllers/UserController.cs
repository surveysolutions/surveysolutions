// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserController.cs" company="World bank">
//   2012
// </copyright>
// <summary>
//   The user controller.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace RavenQuestionnaire.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;

    using Main.Core.Commands.User;
    using Main.Core.Entities.SubEntities;
    using Main.Core.Utility;
    using Main.Core.View;
    using Main.Core.View.User;

    using Ncqrs;
    using Ncqrs.Commanding.ServiceModel;

    using Questionnaire.Core.Web.Security;

    /// <summary>
    /// The user controller.
    /// </summary>
    [QuestionnaireAuthorize(UserRoles.Administrator)]
    public class UserController : Controller
    {
        #region Constants and Fields

        /// <summary>
        /// The view repository.
        /// </summary>
        private readonly IViewRepository viewRepository;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UserController"/> class.
        /// </summary>
        /// <param name="viewRepository">
        /// The view repository.
        /// </param>
        public UserController(IViewRepository viewRepository)
        {
            this.viewRepository = viewRepository;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <returns>
        /// </returns>
        public ActionResult Create()
        {
            this.AddSupervisorListToViewBag();
            this.AddLocationsListToViewBag();
            return this.View("Manage", UserView.New());
        }

        /// <summary>
        /// The delete.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult Delete(string id)
        {
            // commandInvoker.Execute(new DeleteUserCommand(id, GlobalInfo.GetCurrentUser()));
            return this.RedirectToAction("Index");
        }

        /// <summary>
        /// The index.
        /// </summary>
        /// <param name="input">
        /// The input.
        /// </param>
        /// <returns>
        /// </returns>
        public ActionResult Index(UserBrowseInputModel input)
        {
            UserBrowseView model = this.viewRepository.Load<UserBrowseInputModel, UserBrowseView>(input);
            return View(model);
        }

        /// <summary>
        /// The manage.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// </returns>
        /// <exception cref="HttpException">
        /// </exception>
        public ActionResult Manage(Guid id)
        {
            if (id == null || id == Guid.Empty)
            {
                throw new HttpException(404, "Invalid quesry string parameters");
            }

            UserView model = this.viewRepository.Load<UserViewInputModel, UserView>(new UserViewInputModel(id));
            this.AddSupervisorListToViewBag();
            this.AddLocationsListToViewBag();
            return View(model);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <returns>
        /// </returns>
        [HttpPost]
        public ActionResult Save(UserView model)
        {
            if (this.ModelState.IsValid)
            {
                if (model.PublicKey == Guid.Empty)
                {
                    Guid publicKey = Guid.NewGuid();

                    if (model.Supervisor.Id != Guid.Empty)
                    {
                        UserView super =
                            this.viewRepository.Load<UserViewInputModel, UserView>(
                                new UserViewInputModel(model.Supervisor.Id));
                        model.Supervisor.Name = super.UserName;
                        model.Supervisor.Id = super.PublicKey;
                    }

                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(
                        new CreateUserCommand(
                            publicKey, 
                            model.UserName, 
                            SimpleHash.ComputeHash(model.Password), 
                            model.Email, 
                            new[] { model.PrimaryRole }, 
                            model.IsLocked, 
                            model.Supervisor));
                }
                else
                {
                    var commandService = NcqrsEnvironment.Get<ICommandService>();
                    commandService.Execute(
                        new ChangeUserCommand(model.PublicKey, model.Email, new[] { model.PrimaryRole }, model.IsLocked));
                }

                return this.RedirectToAction("Index");
            }

            return View("Manage", model);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The add locations list to view bag.
        /// </summary>
        protected void AddLocationsListToViewBag()
        {
            /*var locations =
              viewRepository.Load<LocationBrowseInputModel, LocationBrowseView>(new LocationBrowseInputModel() 
                            { PageSize = 100 }).Items;

            ViewBag.AllLocations = locations;*/
        }

        /// <summary>
        /// The add supervisor list to view bag.
        /// </summary>
        protected void AddSupervisorListToViewBag()
        {
            IEnumerable<UserBrowseItem> supervisors =
                this.viewRepository.Load<UserBrowseInputModel, UserBrowseView>(
                    new UserBrowseInputModel(UserRoles.Supervisor) { PageSize = 100 }).Items;
            List<UserBrowseItem> list = supervisors.ToList();
            list.Insert(0, new UserBrowseItem(Guid.Empty, string.Empty, null, DateTime.MinValue, false, null, null));
            this.ViewBag.Supervisors = list;
        }

        #endregion
    }
}