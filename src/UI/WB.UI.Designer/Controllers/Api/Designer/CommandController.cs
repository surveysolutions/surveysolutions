using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Categories;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.FileSystem;
using WB.Core.SharedKernels.Questionnaire.Categories;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Shared.Web.Controllers;
using QuestionnaireEditor = WB.UI.Designer.Resources.QuestionnaireEditor;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    public class CommandController : ControllerBase
    {
        public class CommandExecutionModel
        {
            public string? Type { get; set; }
            public string? Command { get; set; }
        }

        private readonly ICommandService commandService;
        private readonly DesignerDbContext dbContext;
        private readonly ILogger<CommandController> logger;
        private readonly ICommandInflater commandInflater;

        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly IDesignerTranslationService translationsService;
        private readonly ICategoriesService categoriesService;
        private readonly IFileSystemAccessor fileSystemAccessor;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions defaultFormOptions = new FormOptions();

        public CommandController(
            ICommandService commandService,
            DesignerDbContext dbContext,
            ILogger<CommandController> logger,
            ICommandInflater commandPreprocessor,
            ILookupTableService lookupTableService,
            IAttachmentService attachmentService,
            IDesignerTranslationService translationsService,
            ICategoriesService categoriesService,
            IFileSystemAccessor fileSystemAccessor)
        {
            this.logger = logger;
            this.commandInflater = commandPreprocessor;
            this.commandService = commandService;
            this.dbContext = dbContext;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
            this.categoriesService = categoriesService;
            this.fileSystemAccessor = fileSystemAccessor;
        }

        public class AttachmentModel
        {
            public IFormFile? File { get; set; }
            public string FileName { get; set; } = string.Empty;
            public string? Command { get; set; }
        }

        [Route("~/api/command/attachment")]
        [HttpPost]
        public async Task<IActionResult> UpdateAttachment(AttachmentModel model)
        {
            if(model?.Command == null)
                return this.Error((int)HttpStatusCode.NotAcceptable, "Invalid command");

            var commandType = typeof(AddOrUpdateAttachment).Name;
            AddOrUpdateAttachment command;
            try
            {
                command = (AddOrUpdateAttachment)this.Deserialize(commandType, model.Command);

                if (model.File != null)
                {
                    byte[] postedFile;
                    using (var stream = new MemoryStream())
                    {
                        await model.File.CopyToAsync(stream);
                        postedFile = stream.ToArray();
                    }
                    command.AttachmentContentId = this.attachmentService.CreateAttachmentContentId(postedFile);

                    this.attachmentService.SaveContent(
                        contentId: command.AttachmentContentId,
                        contentType: model.File.ContentType,
                        binaryContent: postedFile);
                }
                else
                {
                    if (!command.OldAttachmentId.HasValue)
                        throw new ArgumentException(string.Format(ExceptionMessages.OldAttachmentIdIsEmpty, command.AttachmentId, command.QuestionnaireId));
                    
                    command.AttachmentContentId = 
                        this.attachmentService.GetAttachmentContentId(command.OldAttachmentId.Value) 
                        ?? throw new ArgumentException(string.Format(ExceptionMessages.OldAttachmentIdIsEmpty, command.AttachmentId, command.QuestionnaireId));
                }

                this.attachmentService.SaveMeta(
                    attachmentId: command.AttachmentId,
                    questionnaireId: command.QuestionnaireId,
                    contentId: command.AttachmentContentId, 
                    fileName: model.File?.FileName ?? model.FileName);
            }
            catch (FormatException e)
            {
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, $"Error on command of type ({commandType}) handling ");
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }

            var updateAttachment = this.ProcessCommand(command, commandType).Response;
            await dbContext.SaveChangesAsync();
            return updateAttachment;
        }

        [Route("~/api/command/updateLookupTable")]
        [HttpPost]
        public async Task<IActionResult> UpdateLookupTable()
        {
            var commandType = "UpdateLookupTable";
            const string fileParameterName = "name=\"file\"";

            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return this.Error((int)HttpStatusCode.UnsupportedMediaType, string.Empty);
            }

            UpdateLookupTable updateLookupTableCommand;
            try
            {
                var boundary = MultipartRequestHelper.GetBoundary(MediaTypeHeaderValue.Parse(Request.ContentType),
                    defaultFormOptions.MultipartBoundaryLengthLimit);
                var reader = new MultipartReader(boundary.ToString(), HttpContext.Request.Body);

                string fileStreamContent = string.Empty;
                string commandContent = string.Empty;
                var section = await reader.ReadNextSectionAsync();
                while (section != null && section.Headers != null)
                {
                    Dictionary<string, StringValues> sectionHeaders = section.Headers;
                    //form-data; name="file"; filename="Book1.txt"
                    var header = sectionHeaders["Content-Disposition"].ToString();
                    if (header.Contains(fileParameterName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        fileStreamContent = await section.ReadAsStringAsync();
                    }
                    else
                    {
                        commandContent = await section.ReadAsStringAsync();

                    }

                    section = await reader.ReadNextSectionAsync();
                }

                updateLookupTableCommand = (UpdateLookupTable)this.Deserialize(commandType, commandContent);

                if (string.IsNullOrWhiteSpace(fileStreamContent) && updateLookupTableCommand.OldLookupTableId.HasValue)
                {
                    var lookupContent = this.lookupTableService.GetLookupTableContentFile(updateLookupTableCommand.QuestionnaireId, updateLookupTableCommand.OldLookupTableId.Value);

                    if (lookupContent != null)
                    {
                        fileStreamContent = Encoding.UTF8.GetString(lookupContent.Content);
                    }
                }

                if (!string.IsNullOrWhiteSpace(fileStreamContent))
                {
                    this.lookupTableService.SaveLookupTableContent(
                        updateLookupTableCommand.QuestionnaireId,
                        updateLookupTableCommand.LookupTableId,
                        fileStreamContent);
                }

            }
            catch (FormatException)
            {
                return this.Error((int)HttpStatusCode.NotAcceptable,
                    Resources.QuestionnaireController.SelectTabFile);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, $"Error on command of type ({commandType}) handling ");
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }

            var updateLookupTable = this.ProcessCommand(updateLookupTableCommand, commandType).Response;

            await dbContext.SaveChangesAsync();

            return updateLookupTable;
        }

        [Route("~/api/command")]
        public async Task<IActionResult> Post([FromBody]CommandExecutionModel model)
        {
            if (model?.Command == null || model?.Type == null)
                throw new InvalidOperationException("Invalid command");

            try
            {
                using (var transaction = await dbContext.Database.BeginTransactionAsync())
                {
                    var concreteCommand = this.Deserialize(model.Type, model.Command);
                    var commandProcessResult = this.ProcessCommand(concreteCommand, model.Type);

                    IActionResult actionResult = commandProcessResult.Response;

                    if (!commandProcessResult.HasErrors)
                    {
                        await dbContext.SaveChangesAsync();
                        transaction.Commit();
                    }
                    else
                    {
                        transaction.Rollback();
                    }

                    return actionResult;
                }
            }
            catch (InvalidOperationException exc)
            {
                this.logger.LogError(exc, $"Error on command of type ({model.Type}) handling ");
                return this.Error((int)HttpStatusCode.NotAcceptable, $"{exc.Message} Please reload page.");
            }
            catch (Exception e)
            {
                this.logger.LogError(e, $"Error on command of type ({model.Type}) handling ");
                throw;
            }
        }

        public class FileModel
        {
            public IFormFile? File { get; set; }
            public string? Command { get; set; }
        }


        [Route("~/api/command/translation")]
        [HttpPost]
        public async Task<IActionResult> UpdateTranslation(FileModel model)
        {
            if (model?.Command == null)
                return this.Error((int)HttpStatusCode.NotAcceptable, "Invalid command");

            var commandType = nameof(AddOrUpdateTranslation);
            AddOrUpdateTranslation command;
            try
            {
                command = (AddOrUpdateTranslation)this.Deserialize(commandType, model.Command);
                if (model.File != null)
                {
                    byte[] postedFile;
                    await using (var stream = new MemoryStream())
                    {
                        await model.File.CopyToAsync(stream);
                        postedFile = stream.ToArray();
                    }

                    this.translationsService.Store(command.QuestionnaireId,
                        command.TranslationId,
                        postedFile);
                }
            }
            catch (FormatException e)
            {
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, $"Error on command of type ({commandType}) handling ");
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (InvalidFileException e)
            {
                this.logger.LogError(e, $"Error on command of type ({commandType}) handling ");

                var sb = new StringBuilder();
                sb.AppendLine(e.Message);
                e.FoundErrors?.ForEach(x => sb.AppendLine(x.Message));

                return this.Error((int)HttpStatusCode.NotAcceptable, sb.ToString());
            }

            var commandResponse = this.ProcessCommand(command, commandType);

            if (commandResponse.HasErrors || model.File == null)
            {
                await dbContext.SaveChangesAsync();
                return commandResponse.Response;
            }

            var storedTranslationsCount =
                this.translationsService.Count(command.QuestionnaireId, command.TranslationId);
            var resultMessage = storedTranslationsCount == 1
                ? string.Format(QuestionnaireEditor.TranslationsObtained, storedTranslationsCount)
                : string.Format(QuestionnaireEditor.TranslationsObtained_plural, storedTranslationsCount);

            await dbContext.SaveChangesAsync();

            return Ok(resultMessage);
        }

        [Route("~/api/command/categories")]
        [HttpPost]
        public async Task<IActionResult> UpdateCategories(FileModel model)
        {
            if (model?.Command == null)
                return this.Error((int)HttpStatusCode.NotAcceptable, "Invalid command");

            var commandType = typeof(AddOrUpdateCategories).Name;
            AddOrUpdateCategories command;
            try
            {
                command = (AddOrUpdateCategories)this.Deserialize(commandType, model.Command);
                if (model.File != null)
                {
                    var extension = this.fileSystemAccessor.GetFileExtension(model.File.FileName);

                    var excelExtensions = new[] {".xlsx", ".ods", ".xls"};
                    var tsvExtensions = new[] {".txt", ".tab", ".tsv"};

                    if(!excelExtensions.Union(tsvExtensions).Contains(extension))
                        throw new ArgumentException(ExceptionMessages.ImportOptions_Tab_Or_Excel_Only);

                    var fileType = excelExtensions.Contains(extension)
                        ? CategoriesFileType.Excel
                        : CategoriesFileType.Tsv;

                    this.categoriesService.Store(command.QuestionnaireId, command.CategoriesId, model.File.OpenReadStream(), fileType);
                }
            }
            catch (FormatException e)
            {
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ArgumentException e)
            {
                this.logger.LogError(e, $"Error on command of type ({commandType}) handling ");
                return this.Error((int)HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (InvalidFileException e)
            {
                this.logger.LogError(e, $"Error on command of type ({commandType}) handling ");

                var sb = new StringBuilder();
                sb.AppendLine(e.Message);
                e.FoundErrors?.ForEach(x => sb.AppendLine(x.Message));

                return this.Error((int)HttpStatusCode.NotAcceptable, sb.ToString());
            }

            var commandResponse = this.ProcessCommand(command, commandType);

            if (commandResponse.HasErrors || model.File == null)
                return commandResponse.Response;

            await dbContext.SaveChangesAsync();

            var storedCategoriesCount = this.categoriesService.GetCategoriesById(command.QuestionnaireId, command.CategoriesId).Count();

            var resultMessage = storedCategoriesCount == 1
                ? string.Format(QuestionnaireEditor.CategoriesObtained, storedCategoriesCount)
                : string.Format(QuestionnaireEditor.CategoriesObtained_plural, storedCategoriesCount);

            return Ok(resultMessage);
        }

        public ICommand Deserialize(string commandType, string serializedCommand)
        {
            try
            {
                Type resultCommandType = GetTypeOfResultCommandOrThrowArgumentException(commandType);
                return JsonConvert.DeserializeObject(serializedCommand, resultCommandType) as ICommand 
                       ?? throw new ArgumentException(string.Format("Failed to deserialize command of type '{0}':\r\n{1}", commandType, serializedCommand));
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error on command deserialization.");
                throw new ArgumentException(string.Format("Failed to deserialize command of type '{0}':\r\n{1}", commandType, serializedCommand));
            }
        }

        protected Dictionary<string, Type> KnownCommandTypes => new Dictionary<string, Type>
         {
             { "UpdateQuestionnaire", typeof (UpdateQuestionnaire) },
             { "UpdateGroup", typeof (UpdateGroup) },
             { "AddGroup", typeof (AddGroup) },
             { "DeleteGroup", typeof (DeleteGroup) },
             { "MoveGroup", typeof (MoveGroup) },
             { "AddDefaultTypeQuestion", typeof (AddDefaultTypeQuestion) },
             { "DeleteQuestion", typeof (DeleteQuestion) },
             { "MoveQuestion", typeof (MoveQuestion) },
             { "AddSharedPersonToQuestionnaire", typeof (AddSharedPersonToQuestionnaire) },
             { "RemoveSharedPersonFromQuestionnaire", typeof (RemoveSharedPersonFromQuestionnaire) },
             { "ReplaceTexts", typeof (ReplaceTextsCommand) },
             //Update questions command
             { "UpdateTextQuestion", typeof (UpdateTextQuestion) },
             { "UpdateNumericQuestion", typeof (UpdateNumericQuestion) },
             { "UpdateDateTimeQuestion", typeof (UpdateDateTimeQuestion) },
             { "UpdateTextListQuestion", typeof (UpdateTextListQuestion) },
             { "UpdateQRBarcodeQuestion", typeof (UpdateQRBarcodeQuestion) },
             { "UpdateMultimediaQuestion", typeof (UpdateMultimediaQuestion) },
             { "UpdateMultiOptionQuestion", typeof (UpdateMultiOptionQuestion) },
             { "UpdateSingleOptionQuestion", typeof (UpdateSingleOptionQuestion) },
             { "UpdateGpsCoordinatesQuestion", typeof (UpdateGpsCoordinatesQuestion) },
             { "UpdateFilteredComboboxOptions", typeof (UpdateFilteredComboboxOptions) },
             { "UpdateAreaQuestion", typeof (UpdateAreaQuestion) },
             { "UpdateAudioQuestion", typeof (UpdateAudioQuestion) },
             { "ReplaceOptionsWithClassification", typeof(ReplaceOptionsWithClassification)},
             { "MigrateToNewVersion", typeof (MigrateToNewVersion) },
                    
             //Static text commands
             { "AddStaticText", typeof (AddStaticText) },
             { "UpdateStaticText", typeof (UpdateStaticText) },
             { "DeleteStaticText", typeof (DeleteStaticText) },
             { "MoveStaticText", typeof (MoveStaticText) },

             // Variables
             { "AddVariable", typeof(AddVariable) },
             { "UpdateVariable", typeof(UpdateVariable) },
             { "DeleteVariable", typeof(DeleteVariable) },
             { "MoveVariable", typeof(MoveVariable) },

             {"PasteAfter", typeof(PasteAfter) },
             {"PasteInto", typeof(PasteInto) },
             //Macro commands
             { "AddMacro", typeof (AddMacro) },
             { "UpdateMacro", typeof (UpdateMacro) },
             { "DeleteMacro", typeof (DeleteMacro) },
             //Lookup table commands
             { "AddLookupTable", typeof (AddLookupTable) },
             { "UpdateLookupTable", typeof (UpdateLookupTable) },
             { "DeleteLookupTable", typeof (DeleteLookupTable) },
             //Attachment commands
             { "AddOrUpdateAttachment", typeof (AddOrUpdateAttachment) },
             { "DeleteAttachment", typeof (DeleteAttachment) },
             //Translation commands
             { "AddOrUpdateTranslation", typeof (AddOrUpdateTranslation) },
             { "DeleteTranslation", typeof (DeleteTranslation) },
             { "SetDefaultTranslation", typeof (SetDefaultTranslation) },
             // Metadata
             { "UpdateMetadata", typeof (UpdateMetadata) },
             { nameof(PassOwnershipFromQuestionnaire), typeof(PassOwnershipFromQuestionnaire) },
             //Categories commands
             { "AddOrUpdateCategories", typeof (AddOrUpdateCategories) },
             { "DeleteCategories", typeof (DeleteCategories) },
         };

        private Type GetTypeOfResultCommandOrThrowArgumentException(string commandType)
        {
            if (!KnownCommandTypes.ContainsKey(commandType))
                throw new Exception(string.Format("Command type '{0}' is not supported.", commandType));

            return KnownCommandTypes[commandType];
        }

        private CommandProcessResult ProcessCommand(ICommand concreteCommand, string commandType)
        {
            try
            {
                this.commandInflater.PrepareDeserializedCommandForExecution(concreteCommand);

                this.commandService.Execute(concreteCommand);
            }
            catch (CommandInflaitingException exc)
            {
                if (exc.ExceptionType == CommandInflatingExceptionType.Forbidden)
                {
                    return new CommandProcessResult(this.Error(StatusCodes.Status403Forbidden, exc.Message));
                }

                if (exc.ExceptionType == CommandInflatingExceptionType.EntityNotFound)
                {
                    return new CommandProcessResult(this.Error(StatusCodes.Status404NotFound, exc.Message));
                }

                return new CommandProcessResult(this.Error(StatusCodes.Status406NotAcceptable, exc.Message));
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.LogError(e, string.Format("Error on command of type ({0}) handling ", commandType));
                    throw;
                }

                if (domainEx.ErrorType == DomainExceptionType.DoesNotHavePermissionsForEdit)
                {
                    return new CommandProcessResult(this.Error(StatusCodes.Status403Forbidden, domainEx.Message));
                }

                return new CommandProcessResult(this.Error(StatusCodes.Status406NotAcceptable, domainEx.Message));
            }

            return new CommandProcessResult(Ok(), false);
        }
    }

    public class CommandProcessResult
    {
        public CommandProcessResult(IActionResult httpResponseMessage, bool hasErrors = true)
        {
            this.Response = httpResponseMessage;
            this.HasErrors = hasErrors;
        }

        public bool HasErrors { get; set; }
        public IActionResult Response { get; set; }
    }

  
}
