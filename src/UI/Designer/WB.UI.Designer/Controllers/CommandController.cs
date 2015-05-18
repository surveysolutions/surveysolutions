using System;
using System.Web.Mvc;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.CommandDeserialization;


namespace WB.UI.Designer.Controllers
{
    [CustomAuthorize]
    [Obsolete("API controller should be used instead")]
    public class CommandController : Controller
    {
        private readonly ICommandService commandService;
        private readonly ICommandDeserializer commandDeserializer;
        private readonly ILogger logger;
        private readonly ICommandPreprocessor commandPreprocessor;

        public CommandController(ICommandService commandService, ICommandDeserializer commandDeserializer, ILogger logger, ICommandPreprocessor commandPreprocessor)
        {
            this.logger = logger;
            this.commandPreprocessor = commandPreprocessor;
            this.commandService = commandService;
            this.commandDeserializer = commandDeserializer;
        }

        [HttpPost]
        public JsonResult Execute(string type, string command)
        {
            var returnValue = new JsonQuestionnaireResult();
            try
            {
                var concreteCommand = this.commandDeserializer.Deserialize(type, command);
                this.commandPreprocessor.PrepareDeserializedCommandForExecution(concreteCommand);
                this.commandService.Execute(concreteCommand);
            }
            catch (Exception e)
            {
                var domainEx = e.GetSelfOrInnerAs<QuestionnaireException>();
                if (domainEx == null)
                {
                    this.logger.Error(string.Format("Error on command of type ({0}) handling ", type), e);
                    throw;
                }
                else
                {
                    returnValue.IsSuccess = false;
                    returnValue.HasPermissions = domainEx.ErrorType != DomainExceptionType.DoesNotHavePermissionsForEdit;
                    returnValue.Error = domainEx.Message;
                }
            }

            return this.Json(returnValue);
        }
    }
}