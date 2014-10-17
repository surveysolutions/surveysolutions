using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Designer.Api
{
    [LocalOrDevelopmentAccessOnly]
    public class NCalcToSharpController : ApiController
    {
        public class OneQuestionnaireModel
        {
            public Guid Id { get; set; }
        }

        private static object lockObject = new object();
        private static bool isMigrationInProgress = false;
        private static string lastStatusMessage = "no operations were performed so far";

        private readonly ILogger logger;
        private readonly ICommandService commandService;

        public NCalcToSharpController(ILogger logger, ICommandService commandService)
        {
            this.logger = logger;
            this.commandService = commandService;
        }

        public HttpResponseMessage Get()
        {
            return Request.CreateResponse(HttpStatusCode.OK, GetCurrentStatus());
        }

        [HttpPost]
        public HttpResponseMessage MigrateOne(OneQuestionnaireModel model)
        {
            if (isMigrationInProgress)
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Migration is already in progress.");

            new Task(() => this.MigrateOneQuestionnaire(model.Id)).Start();

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage MigrateAll()
        {
            try
            {
                throw new NotImplementedException();
            }
            catch (Exception exception)
            {
                this.logger.Error("Error migrating all questionnaires.", exception);

                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exception.ToString());
            }
        }

        private void MigrateOneQuestionnaire(Guid questionnaireId)
        {
            if (!isMigrationInProgress)
            {
                lock (lockObject)
                {
                    if (!isMigrationInProgress)
                    {
                        isMigrationInProgress = true;
                        try
                        {
                            this.MigrateOneQuestionnaireImpl(questionnaireId);
                        }
                        finally
                        {
                            isMigrationInProgress = false;
                        }
                    }
                }
            }
        }

        private void MigrateOneQuestionnaireImpl(Guid questionnaireId)
        {
            UpdateStatus(string.Format("started to migrate questionnaire {0}", questionnaireId.FormatGuid()));
            try
            {
                this.commandService.Execute(new MigrateExpressionsToCSharp(questionnaireId));

                UpdateStatus(string.Format("successfully migrated questionnaire {0}", questionnaireId.FormatGuid()));
            }
            catch (Exception exception)
            {
                this.logger.Error(string.Format("Error migrating questionnaire '{0}'.", questionnaireId), exception);

                UpdateStatus(string.Format("failed to migrate questionnaire {1}{0}{0}{2}", Environment.NewLine,
                    questionnaireId.FormatGuid(),
                    exception));
            }
        }

        private static void UpdateStatus(string message)
        {
            lastStatusMessage = DateTime.Now.ToShortTimeString() + " " + message;
        }

        private static string GetCurrentStatus()
        {
            return string.Format("Migration in progress: {1}{0}Status: {2}", Environment.NewLine, isMigrationInProgress, lastStatusMessage);
        }
    }
}