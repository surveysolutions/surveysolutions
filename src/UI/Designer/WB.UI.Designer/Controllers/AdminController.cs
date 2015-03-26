using System.Configuration.Provider;
using System.Web;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Extensions;
using WB.UI.Shared.Web.Membership;
using WebMatrix.WebData;

namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize(Roles = "Administrator")]
    public class AdminController : BaseController
    {
        private readonly IQuestionnaireHelper questionnaireHelper;
        private readonly ILogger logger;
        private readonly IStringCompressor zipUtils;
        private readonly ICommandService commandService;
        private readonly IQuestionnaireExportService exportService;
        private readonly IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory;

        public AdminController(
            IMembershipUserService userHelper,
            IQuestionnaireHelper questionnaireHelper,
            ILogger logger, 
            IStringCompressor zipUtils, 
            ICommandService commandService, 
            IQuestionnaireExportService exportService, 
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory)
            : base(userHelper)
        {
            this.questionnaireHelper = questionnaireHelper;
            this.logger = logger;
            this.zipUtils = zipUtils;
            this.commandService = commandService;
            this.exportService = exportService;
            this.questionnaireViewFactory = questionnaireViewFactory;
        }

        [HttpGet]
        public ActionResult Import()
        {
            return this.View();
        }

        [HttpPost]
        public ActionResult Import(HttpPostedFileBase uploadFile)
        {
            uploadFile = uploadFile ?? this.Request.Files[0];

            if (uploadFile != null && uploadFile.ContentLength > 0)
            {
                var document = this.zipUtils.DecompressGZip<IQuestionnaireDocument>(uploadFile.InputStream);
                if (document != null)
                {
                    this.commandService.Execute(new ImportQuestionnaireCommand(this.UserHelper.WebUser.UserId, document));
                    return this.RedirectToAction("Index", "Questionnaire");
                }
            }
            else
            {
                this.Error("Uploaded file is empty");
            }

            return this.Import();
        }


        [HttpGet]
        public FileStreamResult Export(Guid id)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            var templateInfo = this.exportService.GetQuestionnaireTemplateInfo(questionnaireView.Source);

            if (templateInfo == null || string.IsNullOrEmpty(templateInfo.Source))
            {
                return null;
            }

            return new FileStreamResult(this.zipUtils.Compress(templateInfo.Source), "application/octet-stream")
            {
                FileDownloadName = string.Format("{0}.tmpl", templateInfo.Title.ToValidFileName())
            };
        }

        public ActionResult Create()
        {
            return this.View(new RegisterModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterModel model)
        {
            if (this.ModelState.IsValid)
            {
                try
                {
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { model.Email }, false);
                    Roles.Provider.AddUsersToRoles(new[] { model.UserName }, new[] { this.UserHelper.USERROLENAME });

                    return this.RedirectToAction("Index");
                }
                catch (MembershipCreateUserException e)
                {
                    this.Error(e.StatusCode.ToErrorCode());
                }
            }

            return View(model);
        }

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

        public ViewResult Details(Guid id)
        {
            MembershipUser account = this.GetUser(id);

            var questionnaires = this.questionnaireHelper.GetQuestionnairesByViewerId(viewerId: id);
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
                            LastLoginDate = account.LastLoginDate.ToUIString(),
                            UserName = account.UserName,
                            LastLockoutDate = account.LastLockoutDate.ToUIString(),
                            LastPasswordChangedDate = account.LastPasswordChangedDate.ToUIString(),
                            Comment = account.Comment ?? GlobalHelper.EmptyString,
                            Questionnaires = questionnaires
                        });
        }

        public ActionResult Edit(Guid id)
        {
            MembershipUser intUser = this.GetUser(id);
            return
                this.View(
                    new UpdateAccountModel
                        {
                            Email = intUser.Email, 
                            IsApproved = intUser.IsApproved, 
                            IsLockedOut = intUser.IsLockedOut, 
                            UserName = intUser.UserName, 
                            Id = id
                        });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(UpdateAccountModel user)
        {
            if (this.ModelState.IsValid)
            {
                MembershipUser intUser = this.GetUser(user.Id);
                if (intUser != null)
                {
                    try
                    {
                        Membership.UpdateUser(
                            new MembershipUser(
                                providerName: intUser.ProviderName,
                                name: intUser.UserName,
                                providerUserKey: intUser.ProviderUserKey,
                                email: user.Email,
                                passwordQuestion: intUser.PasswordQuestion,
                                comment: null,
                                isApproved: user.IsApproved,
                                isLockedOut: user.IsLockedOut,
                                creationDate: intUser.CreationDate,
                                lastLoginDate: intUser.LastLoginDate,
                                lastActivityDate: intUser.LastActivityDate,
                                lastPasswordChangedDate: intUser.LastPasswordChangedDate,
                                lastLockoutDate: intUser.LastLockoutDate));

                        return this.RedirectToAction("Index");
                    }
                    catch (ProviderException e)
                    {
                        this.Error(e.Message);
                    }
                    catch (Exception e)
                    {
                        logger.Error("User update exception", e);
                        this.Error("Could not update user information. Please, try again later.");
                    }
                }
            }

            return this.View(user);
        }

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
                (user) => !Roles.GetRolesForUser(user.UserName).Contains(this.UserHelper.ADMINROLENAME);

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
                                 CreationDate = x.CreationDate, 
                                 IsApproved = x.IsApproved, 
                                 IsLockedOut = x.IsLockedOut, 
                                 CanEdit = editAction(x), 
                                 CanOpen = false,
                                 CanDelete = false, 
                                 CanPreview = editAction(x)
                             });
            return View(retVal.ToPagedList(page, GlobalHelper.GridPageItemsCount, users.Count()));
        }

        private MembershipUser GetUser(Guid id)
        {
            return Membership.GetUser(id, false);
        }
    }
}