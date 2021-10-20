using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Designer.BootstrapSupport.HtmlHelpers;
using WB.UI.Designer.Code.ImportExport;
using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Code
{
    public class QuestionnaireHelper : IQuestionnaireHelper
    {
        private readonly IQuestionnaireListViewFactory viewFactory;
        private readonly IQuestionnaireViewFactory questionnaireViewFactory;
        private readonly ISerializer serializer;
        private readonly IAttachmentService attachmentService;
        private readonly ILookupTableService lookupTableService;
        private readonly ITranslationsService translationsService;
        private readonly ICategoriesService categoriesService;
        private readonly ILogger<QuestionnaireHelper> logger;
        private readonly IFileSystemAccessor fileSystemAccessor;
        private readonly DesignerDbContext questionnaireChangeItemStorage;
        private readonly IImportExportQuestionnaireService importExportQuestionnaireService;

        public QuestionnaireHelper(
            IQuestionnaireListViewFactory viewFactory,
            IQuestionnaireViewFactory questionnaireViewFactory, 
            ISerializer serializer, 
            IAttachmentService attachmentService, 
            ILookupTableService lookupTableService, 
            ITranslationsService translationsService, 
            ICategoriesService categoriesService, 
            ILogger<QuestionnaireHelper> logger, 
            IFileSystemAccessor fileSystemAccessor,
            DesignerDbContext questionnaireChangeItemStorage,
            IImportExportQuestionnaireService importExportQuestionnaireService)
        {
            this.viewFactory = viewFactory;
            this.questionnaireViewFactory = questionnaireViewFactory;
            this.serializer = serializer;
            this.attachmentService = attachmentService;
            this.lookupTableService = lookupTableService;
            this.translationsService = translationsService;
            this.categoriesService = categoriesService;
            this.logger = logger;
            this.fileSystemAccessor = fileSystemAccessor;
            this.questionnaireChangeItemStorage = questionnaireChangeItemStorage;
            this.importExportQuestionnaireService = importExportQuestionnaireService;
        }

        public IPagedList<QuestionnaireListViewModel> GetQuestionnaires(Guid viewerId, bool isAdmin, QuestionnairesType type, Guid? folderId,
            int? pageIndex = null, string? sortBy = null, int? sortOrder = null, string? searchFor = null)
        {
            QuestionnaireListView model = this.viewFactory.LoadFoldersAndQuestionnaires(new QuestionnaireListInputModel
            {
                ViewerId = viewerId,
                Type = type,
                IsAdminMode = isAdmin,
                Page = pageIndex ?? 1,
                PageSize = GlobalHelper.GridPageItemsCount,
                Order = sortBy,
                SearchFor = searchFor,
                FolderId = folderId
            });

            var showPublic = type == QuestionnairesType.Public;
            var locations = showPublic ? GetLocations(model.Items) : new Dictionary<Guid, string>();

            return model.Items.Select(x =>
                {
                    if (x is QuestionnaireListViewItem item)
                    {
                        var questLocation = item.Folder != null && locations.ContainsKey(item.Folder.PublicId) 
                                            ? locations[item.Folder.PublicId] 
                                            : null;
                        return this.GetQuestionnaire(item, viewerId, isAdmin, showPublic, questLocation);
                    }

                    var folderLocation = locations.ContainsKey(x.PublicId) ? locations[x.PublicId] : null;
                    return this.GetFolder((QuestionnaireListViewFolder)x, showPublic, folderLocation);
                })
                .ToPagedList(page: model.Page, pageSize: model.PageSize, totalCount: model.TotalCount);
        }

        private Dictionary<Guid, string> GetLocations(IEnumerable<IQuestionnaireListItem> modelItems)
        {
            HashSet<QuestionnaireListViewFolder> folders = new HashSet<QuestionnaireListViewFolder>();

            foreach (var modelItem in modelItems)
            {
                if (modelItem is QuestionnaireListViewItem item && item.Folder != null)
                    folders.Add(item.Folder);
                if (modelItem is QuestionnaireListViewFolder folder)
                    folders.Add(folder);
            }

            var locations = viewFactory.LoadFoldersLocation(folders);

            return locations.ToDictionary(k => k.PublicId, v => v.Location);
        }

        public IPagedList<QuestionnaireListViewModel> GetMyQuestionnairesByViewerId(Guid viewerId, bool isAdmin, Guid? folderId = null)
            => GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin, type: QuestionnairesType.My, folderId: folderId);

        public IPagedList<QuestionnaireListViewModel> GetSharedQuestionnairesByViewerId(Guid viewerId, bool isAdmin, Guid? folderId)
            => this.GetQuestionnaires(viewerId: viewerId, isAdmin: isAdmin, type: QuestionnairesType.Shared, folderId: folderId);

        private QuestionnaireListViewModel GetQuestionnaire(QuestionnaireListViewItem x, Guid viewerId, 
            bool isAdmin, bool showPublic, string? location)
            => new QuestionnaireListViewModel
            {
                Id = x.PublicId.FormatGuid(),
                CreationDate = x.CreationDate,
                LastEntryDate = x.LastEntryDate,
                Title = x.Title,
                IsDeleted = x.IsDeleted,
                IsPublic = showPublic,
                CanDelete = x.CreatedBy == viewerId && !x.IsDeleted,
                CanExport = true,
                CanCopy = true,
                CanAssignFolder = showPublic && isAdmin,
                CanOpen = (showPublic || x.CreatedBy == viewerId || x.SharedPersons.Any(s => s.UserId == viewerId)) && !x.IsDeleted,
                CanSynchronize = isAdmin,
                CanExportToPdf = true,
                CanExportToHtml = true,
                Location = location != null
                           ? x.Title + "\r\n" + @QuestionnaireController.Location + QuestionnaireController.PublicQuestionnaires + " / " + location
                           : null,
                Owner = x.CreatedBy == null
                    ? GlobalHelper.EmptyString
                    : (x.CreatedBy == viewerId ? QuestionnaireController.You : x.CreatorName)
            };

        private QuestionnaireListViewModel GetFolder(QuestionnaireListViewFolder x, bool showPublic, string? location)
            => new QuestionnaireListViewModel
            {
                Id = x.PublicId.FormatGuid(),
                IsFolder = true,
                CreationDate = x.CreateDate,
                LastEntryDate = x.CreateDate,
                Title = x.Title,
                IsPublic = showPublic,
                CanDelete = false,
                CanCopy = false,
                CanExport = false,
                CanOpen = showPublic,
                CanEdit = false,
                CanSynchronize = false,
                CanExportToPdf = false,
                CanExportToHtml = false,
                Location = location != null ? QuestionnaireController.Location + QuestionnaireController.PublicQuestionnaires + " / " + location : null,
                Owner = GlobalHelper.EmptyString
            };


        public Stream? GetBackupQuestionnaire(Guid id, out string questionnaireFileName)
        {
            var questionnaireView = questionnaireViewFactory.Load(new QuestionnaireViewInputModel(id));
            if (questionnaireView == null)
            {
                questionnaireFileName = String.Empty;
                return null;
            }

            questionnaireFileName = fileSystemAccessor.MakeValidFileName(questionnaireView.Title);
            
            var questionnaireDocument = questionnaireView.Source;
            
            var maxSequenceByQuestionnaire = this.questionnaireChangeItemStorage.QuestionnaireChangeRecords
                .Where(y => y.QuestionnaireId == id.FormatGuid())
                .Select(y => (int?)y.Sequence)
                .Max();
            
            questionnaireDocument.Revision = maxSequenceByQuestionnaire ?? 0;
            //string questionnaireJson = this.serializer.Serialize(questionnaireDocument);
            var questionnaireJson = importExportQuestionnaireService.Export(questionnaireDocument);

            var output = new MemoryStream();

            ZipOutputStream zipStream = new ZipOutputStream(output);

            zipStream.PutTextFileEntry($"document.json", questionnaireJson);

            for (int attachmentIndex = 0; attachmentIndex < questionnaireDocument.Attachments.Count; attachmentIndex++)
            {
                try
                {
                    Attachment attachmentReference = questionnaireDocument.Attachments[attachmentIndex];

                    var attachmentContent = this.attachmentService.GetContent(attachmentReference.ContentId);

                    if (attachmentContent?.Content != null)
                    {
                        var attachmentMeta = this.attachmentService.GetAttachmentMeta(attachmentReference.AttachmentId);

                        zipStream.PutFileEntry($"Attachments/{attachmentReference.AttachmentId.FormatGuid()}/{attachmentMeta?.FileName ?? "unknown-file-name"}", attachmentContent.Content);
                        zipStream.PutTextFileEntry($"Attachments/{attachmentReference.AttachmentId.FormatGuid()}/Content-Type.txt", attachmentContent.ContentType);
                    }
                    else
                    {
                        zipStream.PutTextFileEntry(
                            $"Attachments/Invalid/missing attachment #{attachmentIndex + 1} ({attachmentReference.AttachmentId.FormatGuid()}).txt",
                            $"Attachment '{attachmentReference.Name}' is missing.");
                    }
                }
                catch (Exception exception)
                {
                    this.logger.LogWarning($"Failed to backup attachment #{attachmentIndex + 1} from questionnaire '{questionnaireView.Title}' ({id}).", exception);
                    zipStream.PutTextFileEntry(
                        $"Attachments/Invalid/broken attachment #{attachmentIndex + 1}.txt",
                        $"Failed to backup attachment. See error below.{Environment.NewLine}{exception}");
                }
            }

            Dictionary<Guid, string> lookupTables = this.lookupTableService.GetQuestionnairesLookupTables(id);

            foreach (KeyValuePair<Guid, string> lookupTable in lookupTables)
            {
                zipStream.PutTextFileEntry($"Lookup Tables/{lookupTable.Key.FormatGuid()}.txt", lookupTable.Value);
            }

            foreach (var translation in questionnaireDocument.Translations)
            {
                TranslationFile excelFile = this.translationsService.GetAsExcelFile(id, translation.Id);
                zipStream.PutFileEntry($"Translations/{translation.Id.FormatGuid()}.xlsx", excelFile.ContentAsExcelFile);
            }

            foreach (var categories in questionnaireDocument.Categories)
            {
                var excelFile = this.categoriesService.GetAsExcelFile(id, categories.Id);
                if (excelFile?.Content == null)
                    continue;
                zipStream.PutFileEntry($"Categories/{categories.Id.FormatGuid()}.xlsx", excelFile.Content);
            }

            zipStream.Finish();
            output.Position = 0;

            return output;
        }
    }
}
