using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Attachments;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Implementation.Services.AttachmentService;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code;
using WB.UI.Designer.Code.Implementation;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class CommandController : ApiController
    {
        public struct CommandExecutionModel
        {
            public string Type { get; set; }
            public string Command { get; set; }
        }

        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ILogger logger;
        private readonly ICommandInflater commandInflater;
        private readonly ICommandPostprocessor commandPostprocessor;
       
        private readonly ILookupTableService lookupTableService;
        private readonly IAttachmentService attachmentService;

        public CommandController(
            ICommandService commandService, 
            ICommandDeserializer commandDeserializer, 
            ILogger logger, 
            ICommandInflater commandPreprocessor,
            ICommandPostprocessor commandPostprocessor, 
            ILookupTableService lookupTableService, 
            IAttachmentService attachmentService)
        {
            this.logger = logger;
            this.commandInflater = commandPreprocessor;
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.commandPostprocessor = commandPostprocessor;
            this.lookupTableService = lookupTableService;
            this.attachmentService = attachmentService;
        }

        [Route("~/api/command/updateAttachment")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateAttachment()
        {
            var commandType = typeof(UpdateAttachment).Name;
            string fileParameterName = "file";
            string commandParameterName = "command";

            if (!this.Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var multipartStreamProvider = new MultipartMemoryStreamProvider();

            UpdateAttachment updateAttachment;
            try
            {
                await this.Request.Content.ReadAsMultipartAsync(multipartStreamProvider);

                var multipartContents = multipartStreamProvider.Contents.Select(x => new
                {
                    ParamName = x.Headers.ContentDisposition.Name.Replace("\"", string.Empty),
                    Content = x
                }).ToDictionary(x => x.ParamName, x => x.Content);

                var fileStreamContent = new
                {
                    BinaryContent = multipartContents[fileParameterName].ReadAsByteArrayAsync().Result,
                    ContentType = multipartContents[fileParameterName].Headers.ContentType?.MediaType
                };

                var commandContent = multipartContents[commandParameterName].ReadAsStringAsync().Result;

                updateAttachment = (UpdateAttachment)this.commandDeserializer.Deserialize(commandType, commandContent);

                if (fileStreamContent.BinaryContent?.Length > 0)
                {
                    // save image here
                    AttachmentType attachmentType = GetAttachmentTypeByRequestMediaType(fileStreamContent.ContentType);

                    this.attachmentService.SaveAttachmentContent(
                        updateAttachment.QuestionnaireId, 
                        updateAttachment.AttachmentId,
                        attachmentType,
                        fileStreamContent.ContentType,
                        fileStreamContent.BinaryContent,
                        updateAttachment.AttachmentFileName);
                }
                this.attachmentService.UpdateAttachmentName(updateAttachment.QuestionnaireId, updateAttachment.AttachmentId, updateAttachment.AttachmentName);
            }
            catch (FormatException e)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }
            catch (ArgumentException e)
            {
                this.logger.Error(string.Format("Error on command of type ({0}) handling ", commandType), e);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

            return this.ProcessCommand(updateAttachment, commandType);
        }

        private AttachmentType GetAttachmentTypeByRequestMediaType(string contentType)
        {
            if (contentType.StartsWith("image/"))
                return AttachmentType.Image;

            throw new FormatException(ExceptionMessages.Attachments_Unsupported_content);
        }

        [Route("~/api/command/updateLookupTable")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateLookupTable()
        {
            var commandType = "UpdateLookupTable";
            string fileParameterName = "file";
            string commandParameterName = "command";

            if (!this.Request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var multipartStreamProvider = new MultipartMemoryStreamProvider(); 
            
            UpdateLookupTable updateLookupTableCommand;
            try
            {
                await this.Request.Content.ReadAsMultipartAsync(multipartStreamProvider);

                var multipartContents = multipartStreamProvider.Contents.Select(x => new
                {
                    ParamName = x.Headers.ContentDisposition.Name.Replace("\"", string.Empty),
                    StringContent = x.ReadAsStringAsync().Result
                }).ToList();

                var fileStreamContent = multipartContents.Single(x => x.ParamName == fileParameterName);
                var commandContent = multipartContents.Single(x => x.ParamName == commandParameterName);

                updateLookupTableCommand = (UpdateLookupTable)this.commandDeserializer.Deserialize(commandType, commandContent.StringContent);

                if (fileStreamContent.StringContent != null)
                {
                    this.lookupTableService.SaveLookupTableContent(
                        updateLookupTableCommand.QuestionnaireId,
                        updateLookupTableCommand.LookupTableId,
                        updateLookupTableCommand.LookupTableName,
                        fileStreamContent.StringContent);
                }
            }
            catch (FormatException)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, Resources.QuestionnaireController.SelectTabFile);
            }
            catch (ArgumentException e)
            {
                this.logger.Error(string.Format("Error on command of type ({0}) handling ", commandType), e);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, e.Message);
            }

            return this.ProcessCommand(updateLookupTableCommand, commandType);
        }

        public HttpResponseMessage Post(CommandExecutionModel model)
        {
            try
            {
                var concreteCommand = this.commandDeserializer.Deserialize(model.Type, model.Command);
                return this.ProcessCommand(concreteCommand, model.Type);
            }
            catch (Exception e)
            {
                this.logger.Error(string.Format("Error on command of type ({0}) handling ", model.Type), e);
                throw;
            }
        }

        private HttpResponseMessage ProcessCommand(ICommand concreteCommand, string commandType)
        {
            try
            {
                this.commandInflater.PrepareDeserializedCommandForExecution(concreteCommand);

                this.commandService.Execute(concreteCommand);

                this.commandPostprocessor.ProcessCommandAfterExecution(concreteCommand);
            }
            catch (CommandInflaitingException exc)
            {
                if (exc.ExceptionType == CommandInflatingExceptionType.Forbidden)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, exc.Message);
                }

                if (exc.ExceptionType == CommandInflatingExceptionType.EntityNotFound)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, exc.Message);
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, exc.Message);
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
                    return this.Request.CreateErrorResponse(HttpStatusCode.Forbidden, domainEx.Message);
                }

                return this.Request.CreateErrorResponse(HttpStatusCode.NotAcceptable, domainEx.Message);
            }

            return Request.CreateResponse(new JsonQuestionnaireResult());
        }
    }
}