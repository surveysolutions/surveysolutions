using System.Configuration.Provider;
using System.Web;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using ICSharpCode.SharpZipLib.Zip;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Account;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Extensions;
using WebMatrix.WebData;

namespace WB.UI.Designer.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminController : BaseController
    {
        private readonly ILogger logger;
        private readonly IStringCompressor zipUtils;
        private readonly ISerializer serializer;
        private readonly ICommandService commandService;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;

        public AdminController(
            ILogger logger,
            IStringCompressor zipUtils,
            ICommandService commandService,
            IQuestionnaireViewFactory questionnaireViewFactory,
            ISerializer serializer, 
            IAccountListViewFactory accountListViewFactory, 
            ILookupTableService lookupTableService,
            IAttachmentService attachmentService,
            ITranslationsService translationsService)
            : base()
        {
            this.logger = logger;
            this.zipUtils = zipUtils;
            this.commandService = commandService;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.serializer = serializer;
            this.accountListViewFactory = accountListViewFactory;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
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
                try
                {
                    var document =
                        this.zipUtils.DecompressGZip<QuestionnaireDocumentWithLookUpTables>(CreateStreamCopy(uploadFile.InputStream));
                    if (document != null)
                    {
                        var command = new ImportQuestionnaire(this.UserHelper.WebUser.UserId, document.QuestionnaireDocument);

                        this.commandService.Execute(command);
                        foreach (var lookupTable in document.LookupTables)
                        {
                            this.lookupTableService.SaveLookupTableContent(document.QuestionnaireDocument.PublicKey,
                                lookupTable.Key,
                                lookupTable.Value);
                        }
                        return this.RedirectToAction("Index", "Questionnaire");
                    }
                }
                catch (JsonSerializationException)
                {
                    var document = this.zipUtils.DecompressGZip<QuestionnaireDocument>(CreateStreamCopy(uploadFile.InputStream));
                    if (document != null)
                    {
                        var command = new ImportQuestionnaire(this.UserHelper.WebUser.UserId, document);
                        this.commandService.Execute(command);
                        return this.RedirectToAction("Index", "Questionnaire");
                    }
                }
            }
            else
            {
                this.Error("Uploaded file is empty");
            }

            return this.Import();
        }

        

        private MemoryStream CreateStreamCopy(Stream originalStream)
        {
            MemoryStream copyStream = new MemoryStream();
            originalStream.Position = 0;
            originalStream.CopyTo(copyStream);
            copyStream.Position = 0;
            return copyStream;
        }

        [HttpGet]
        [Obsolete("Remove after 5.9+ version released")]
        public FileStreamResult Export(Guid id)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            var questionnaireDocumentWithLookUpTables = new QuestionnaireDocumentWithLookUpTables()
            {
                QuestionnaireDocument = questionnaireView.Source,
                LookupTables = this.lookupTableService.GetQuestionnairesLookupTables(id)
            };
            return new FileStreamResult(this.zipUtils.Compress(this.serializer.Serialize(questionnaireDocumentWithLookUpTables)), "application/octet-stream")
            {
                FileDownloadName = $"{questionnaireView.Title.ToValidFileName()}.tmpl"
            };
        }

        [HttpGet]
        public FileStreamResult BackupQuestionnaire(Guid id)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
                return null;

            var questionnaireDocument = questionnaireView.Source;

            string questionnaireJson = this.serializer.Serialize(questionnaireDocument);

            var output = new MemoryStream();

            ZipOutputStream zipStream = new ZipOutputStream(output);

            string questionnaireFolderName = $"{questionnaireView.Title.ToValidFileName()} ({id.FormatGuid()})";

            zipStream.PutTextFileEntry($"{questionnaireFolderName}/{questionnaireView.Title.ToValidFileName()}.json", questionnaireJson);

            for (int attachmentIndex = 0; attachmentIndex < questionnaireDocument.Attachments.Count; attachmentIndex++)
            {
                try
                {
                    Attachment attachmentReference = questionnaireDocument.Attachments[attachmentIndex];
                    
                    var attachmentContent = this.attachmentService.GetContent(attachmentReference.ContentId);

                    if (attachmentContent?.Content != null)
                    {
                        var attachmentMeta = this.attachmentService.GetAttachmentMeta(attachmentReference.AttachmentId);

                        zipStream.PutFileEntry($"{questionnaireFolderName}/Attachments/{attachmentReference.AttachmentId.FormatGuid()}/{attachmentMeta?.FileName ?? "unknown-file-name"}", attachmentContent.Content);
                        zipStream.PutTextFileEntry($"{questionnaireFolderName}/Attachments/{attachmentReference.AttachmentId.FormatGuid()}/Content-Type.txt", attachmentContent.ContentType);
                    }
                    else
                    {
                        zipStream.PutTextFileEntry(
                            $"{questionnaireFolderName}/Attachments/Invalid/missing attachment #{attachmentIndex + 1} ({attachmentReference.AttachmentId.FormatGuid()}).txt",
                            $"Attachment '{attachmentReference.Name}' is missing.");
                    }
                }
                catch (Exception exception)
                {
                    this.logger.Warn($"Failed to backup attachment #{attachmentIndex + 1} from questionnaire '{questionnaireView.Title}' ({questionnaireFolderName}).", exception);
                    zipStream.PutTextFileEntry(
                        $"{questionnaireFolderName}/Attachments/Invalid/broken attachment #{attachmentIndex + 1}.txt",
                        $"Failed to backup attachment. See error below.{Environment.NewLine}{exception}");
                }
            }

            Dictionary<Guid, string> lookupTables = this.lookupTableService.GetQuestionnairesLookupTables(id);

            foreach (KeyValuePair<Guid, string> lookupTable in lookupTables)
            {
                zipStream.PutTextFileEntry($"{questionnaireFolderName}/Lookup Tables/{lookupTable.Key.FormatGuid()}.txt", lookupTable.Value);
            }

            foreach (var translation in questionnaireDocument.Translations)
            {
                TranslationFile excelFile = this.translationsService.GetAsExcelFile(id, translation.Id);
                zipStream.PutFileEntry($"{questionnaireFolderName}/Translations/{translation.Id.FormatGuid()}.xlsx", excelFile.ContentAsExcelFile);
            }

            zipStream.Finish();

            output.Seek(0, SeekOrigin.Begin);

            return new FileStreamResult(output, "application/zip")
            {
                FileDownloadName = $"{questionnaireView.Title.ToValidFileName()}.zip"
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
                    WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { Email = model.Email, FullName = model.FullName }, false);
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
            MembershipUser user = Membership.GetUser(id, false);
            if (user == null)
            {
                this.Error($"User \"{id}\" doesn't exist");
            }
            else
            {
                Membership.DeleteUser(user.UserName);
                this.Success($"User \"{user.UserName}\" successfully deleted");
            }

            return this.RedirectToAction("Index");
        }

        public ViewResult Details(Guid id)
        {
            DesignerMembershipUser account = (DesignerMembershipUser)Membership.GetUser(id, false);

            var ownedQuestionnaires = this.questionnaireHelper.GetMyQuestionnairesByViewerId(viewerId: id,
                    isAdmin: this.UserHelper.WebUser.IsAdmin, folderId: null);

            var sharedQuestionnaires = this.questionnaireHelper.GetSharedQuestionnairesByViewerId(viewerId: id,
                isAdmin: this.UserHelper.WebUser.IsAdmin, folderId: null);

            ownedQuestionnaires.ToList().ForEach(x =>
            {
                x.CanEdit = false;
                x.CanDelete = false;
            });

            sharedQuestionnaires.ToList().ForEach(x =>
            {
                x.CanEdit = false;
                x.CanDelete = false;
            });

            return this.View(new AccountViewModel
            {
                Id = account.ProviderUserKey.AsGuid(),
                CreationDate = account.CreationDate.ToUIString(),
                Email = account.Email,
                IsApproved = account.IsApproved,
                IsLockedOut = account.IsLockedOut,
                CanImportOnHq = account.CanImportOnHq,
                LastLoginDate = account.LastLoginDate.ToUIString(),
                UserName = account.UserName,
                LastLockoutDate = account.LastLockoutDate.ToUIString(),
                LastPasswordChangedDate = account.LastPasswordChangedDate.ToUIString(),
                Comment = account.Comment ?? GlobalHelper.EmptyString,
                OwnedQuestionnaires = ownedQuestionnaires,
                SharedQuestionnaires = sharedQuestionnaires,
                FullName = account.FullName
            });
        }

        public ActionResult Edit(Guid id)
        {
            DesignerMembershipUser intUser = (DesignerMembershipUser)Membership.GetUser(id, false);
            return
                this.View(
                    new UpdateAccountModel
                        {
                            Email = intUser.Email, 
                            IsApproved = intUser.IsApproved, 
                            IsLockedOut = intUser.IsLockedOut, 
                            UserName = intUser.UserName, 
                            Id = id,
                            CanImportOnHq = intUser.CanImportOnHq,
                            FullName = intUser.FullName
                        });
        }

     
        
    }
}
