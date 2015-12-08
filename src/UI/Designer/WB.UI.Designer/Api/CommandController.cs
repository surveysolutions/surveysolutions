using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Main.Core.Documents;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.LookupTables;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
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
        private readonly string fileParameterName = "file";
        private readonly ILookupTableService lookupTableService;

        public CommandController(
            ICommandService commandService, 
            ICommandDeserializer commandDeserializer, 
            ILogger logger, 
            ICommandInflater commandPreprocessor,
            ICommandPostprocessor commandPostprocessor, 
            ILookupTableService lookupTableService)
        {
            this.logger = logger;
            this.commandInflater = commandPreprocessor;
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
            this.commandPostprocessor = commandPostprocessor;
            this.lookupTableService = lookupTableService;
        }

        [Route("~/api/command/updateLookupTable")]
        [HttpPost]
        public async Task<HttpResponseMessage> UpdateLookupTable()
        {
            HttpRequestMessage request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var provider = new MultipartMemoryStreamProvider();

            var commandType = "UpdateLookupTable";
            UpdateLookupTable updateLookupTableCommand;
            try
            {
                await Request.Content.ReadAsMultipartAsync(provider);

                var fileSreamContent =
                    provider.Contents.FirstOrDefault(
                        x =>
                            x.Headers.ContentDisposition.Name.Replace("\"", string.Empty) == fileParameterName &&
                            x.Headers.ContentDisposition.FileName != null);
                var commandSreamContent =
                    provider.Contents.Single(
                        x => x.Headers.ContentDisposition.Name.Replace("\"", string.Empty) == "command");


                var command = commandSreamContent.ReadAsStringAsync().Result;
                updateLookupTableCommand =
                    (UpdateLookupTable) this.commandDeserializer.Deserialize(commandType, command);

                if (fileSreamContent != null)
                {
                    var fileName = fileSreamContent.Headers.ContentDisposition.FileName.Trim('"');
                    var fileContent = fileSreamContent.ReadAsStringAsync().Result;

                    this.lookupTableService.SaveLookupTableContent(updateLookupTableCommand.QuestionnaireId,
                        updateLookupTableCommand.LookupTableId, updateLookupTableCommand.LookupTableName, fileContent);

                    updateLookupTableCommand.LookupTableFileName = fileName;
                }

            }
            catch (Exception e)
            {
                this.logger.Error(string.Format("Error on command of type ({0}) handling ", commandType), e);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, e);
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