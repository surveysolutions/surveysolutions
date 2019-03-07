using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Macros;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Translations;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Translations;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.SharedKernels.Questionnaire.Translations;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using QuestionnaireEditor = WB.UI.Designer.Resources.QuestionnaireEditor;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class CommandController : Controller
    {
        public struct CommandExecutionModel
        {
            public string Type { get; set; }
            public string Command { get; set; }
        }

        private readonly ICommandService commandService;
        private readonly ILogger logger;
        private readonly ICommandInflater commandInflater;

        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;
        private readonly ITranslationsService translationsService;

        // Get the default form options so that we can use them to set the default limits for
        // request body data
        private static readonly FormOptions defaultFormOptions = new FormOptions();

        public CommandController(
            ICommandService commandService,
            ILogger logger,
            ICommandInflater commandPreprocessor,
            ILookupTableService lookupTableService,
            IAttachmentService attachmentService,
            ITranslationsService translationsService)
        {
            this.logger = logger;
            this.commandInflater = commandPreprocessor;
            this.commandService = commandService;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
            this.translationsService = translationsService;
        }

        public class AttachmentModel
        {
            public IFormFile File { get; set; }
            public string FileName { get; set; }
            public string Command { get; set; }
        }

        [Route("~/api/command/attachment")]
        [HttpPost]
        public async Task<IActionResult> UpdateAttachment(AttachmentModel model)
        {
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
                    command.AttachmentContentId = this.attachmentService.GetAttachmentContentId(command.OldAttachmentId.Value);
                }

                this.attachmentService.SaveMeta(
                    attachmentId: command.AttachmentId,
                    contentId: command.AttachmentContentId,
                    questionnaireId: command.QuestionnaireId,
                    fileName: model.File?.FileName ?? model.FileName);
            }
            catch (FormatException e)
            {
                return StatusCode((int)HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ArgumentException e)
            {
                this.logger.Error($"Error on command of type ({commandType}) handling ", e);
                return StatusCode((int)HttpStatusCode.NotAcceptable, e.Message);
            }

            return this.ProcessCommand(command, commandType).Response;
        }

        [Route("~/api/command/updateLookupTable")]
        [HttpPost]
        public async Task<IActionResult> UpdateLookupTable()
        {
            var commandType = "UpdateLookupTable";
            const string fileParameterName = "file";
            const string commandParameterName = "command";

            if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
            {
                return this.StatusCode((int)HttpStatusCode.UnsupportedMediaType);
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
                while (section != null)
                {
                    Dictionary<string, StringValues> sectionHeaders = section.Headers;
                    switch (sectionHeaders["Content-Disposition"].ToString().Replace("\"", string.Empty))
                    {
                        case fileParameterName:
                            fileStreamContent = await section.ReadAsStringAsync();
                            break;
                        case commandParameterName:
                            commandContent = await section.ReadAsStringAsync();
                            break;
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
                return StatusCode((int) HttpStatusCode.NotAcceptable,
                    Resources.QuestionnaireController.SelectTabFile);
            }
            catch (ArgumentException e)
            {
                this.logger.Error($"Error on command of type ({commandType}) handling ", e);
                return StatusCode((int) HttpStatusCode.NotAcceptable, e.Message);
            }

            return this.ProcessCommand(updateLookupTableCommand, commandType).Response;
        }

        public IActionResult Post(CommandExecutionModel model)
        {
            try
            {
                var concreteCommand = this.Deserialize(model.Type, model.Command);
                return this.ProcessCommand(concreteCommand, model.Type).Response;
            }
            catch (Exception e)
            {
                this.logger.Error(string.Format("Error on command of type ({0}) handling ", model.Type), e);
                throw;
            }
        }

        public class TranslationModel
        {
            public IFormFile File { get; set; }
            public string Command { get; set; }
        }


        [Route("~/api/command/translation")]
        [HttpPost]
        public async Task<IActionResult> UpdateTranslation(TranslationModel model)
        {
            var commandType = typeof(AddOrUpdateTranslation).Name;
            AddOrUpdateTranslation command;
            try
            {
                command = (AddOrUpdateTranslation)this.Deserialize(commandType, model.Command);
                if (model.File != null)
                {
                    byte[] postedFile;
                    using (var stream = new MemoryStream())
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
                return StatusCode((int) HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ArgumentException e)
            {
                this.logger.Error($"Error on command of type ({commandType}) handling ", e);
                return StatusCode((int) HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (InvalidExcelFileException e)
            {
                this.logger.Error($"Error on command of type ({commandType}) handling ", e);
                return StatusCode((int) HttpStatusCode.NotAcceptable, e.Message);
            }

            var commandResponse = this.ProcessCommand(command, commandType);

            if (commandResponse.HasErrors || model.File == null)
                return commandResponse.Response;

            var storedTranslationsCount =
                this.translationsService.Count(command.QuestionnaireId, command.TranslationId);
            var resultMessage = storedTranslationsCount == 1
                ? string.Format(QuestionnaireEditor.TranslationsObtained, storedTranslationsCount)
                : string.Format(QuestionnaireEditor.TranslationsObtained_plural, storedTranslationsCount);
            return Ok(resultMessage);
        }

        public ICommand Deserialize(string commandType, string serializedCommand)
        {
            try
            {
                Type resultCommandType = GetTypeOfResultCommandOrThrowArgumentException(commandType);
                ICommand command = (ICommand)JsonConvert.DeserializeObject(serializedCommand, resultCommandType);

                return command;
            }
            catch (Exception e)
            {
                logger.Error("Error on command deserialization.", e);
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
                    return new CommandProcessResult(StatusCode((int) HttpStatusCode.Forbidden,
                        exc.Message));
                }

                if (exc.ExceptionType == CommandInflatingExceptionType.EntityNotFound)
                {
                    return new CommandProcessResult(StatusCode((int) HttpStatusCode.NotFound,
                        exc.Message));
                }

                return new CommandProcessResult(StatusCode((int) HttpStatusCode.NotAcceptable,
                    exc.Message));
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.Error(string.Format("Error on command of type ({0}) handling ", commandType), e);
                    throw;
                }

                if (domainEx.ErrorType == DomainExceptionType.DoesNotHavePermissionsForEdit)
                {
                    return new CommandProcessResult(StatusCode((int) HttpStatusCode.Forbidden,
                        domainEx.Message));
                }

                return new CommandProcessResult(StatusCode((int) HttpStatusCode.NotAcceptable,
                    domainEx.Message));
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

    // https://github.com/aspnet/Docs/blob/master/aspnetcore/mvc/models/file-uploads/sample/FileUploadSample/MultipartRequestHelper.cs
    public static class MultipartRequestHelper
    {
        // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
        // The spec says 70 characters is a reasonable limit.
        public static StringSegment GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
        {
            var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary);
            if (string.IsNullOrWhiteSpace(boundary.ToString()))
            {
                throw new InvalidDataException("Missing content-type boundary.");
            }

            if (boundary.Length > lengthLimit)
            {
                throw new InvalidDataException(
                    $"Multipart boundary length limit {lengthLimit} exceeded.");
            }

            return boundary;
        }

        public static bool IsMultipartContentType(string contentType)
        {
            return !string.IsNullOrEmpty(contentType)
                   && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
