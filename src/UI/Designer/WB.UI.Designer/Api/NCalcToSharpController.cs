using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.UI.WebControls;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
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

        private static readonly object LockObject = new object();
        private static bool isMigrationInProgress = false;
        private static string lastStatusMessage = "no operations were performed so far";

        private readonly ILogger logger;
        private readonly ICommandService commandService;
        private readonly IQuestionnaireInfoViewFactory questionnaireInfoViewFactory;
        private readonly IAsyncExecutor asyncExecutor;

        public NCalcToSharpController(ILogger logger, ICommandService commandService,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory, IAsyncExecutor asyncExecutor)
        {
            this.logger = logger;
            this.commandService = commandService;
            this.questionnaireInfoViewFactory = questionnaireInfoViewFactory;
            this.asyncExecutor = asyncExecutor;
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

            this.asyncExecutor.ExecuteAsync(() => PerformMigration(() => this.MigrateOneQuestionnaire(model.Id)));

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpResponseMessage MigrateAll()
        {
            if (isMigrationInProgress)
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Migration is already in progress.");

            this.asyncExecutor.ExecuteAsync(() => PerformMigration(() => this.MigrateAllQuestionnaires()));

            return this.Request.CreateResponse(HttpStatusCode.OK);
        }

        private void MigrateOneQuestionnaire(Guid questionnaireId)
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

        private void MigrateAllQuestionnaires()
        {
            int total = -1, migrated = 0, failed = 0;

            UpdateStatus("started to migrate all questionnaires");
            try
            {
                UpdateStatus("detecting questionnaires which should be migrated");
                total = this.questionnaireInfoViewFactory.CountQuestionnairesNotMigratedToCSharp();

                IEnumerable<Guid> questionnaireIds = this.questionnaireInfoViewFactory.GetQuestionnairesNotMigratedToCSharp();

                foreach (var questionnaireId in questionnaireIds)
                {
                    try
                    {
                        UpdateStatus(string.Format("migrating questionnaire {0} (total: {1}, migrated: {2}, failed: {3})",
                            questionnaireId.FormatGuid(), total, migrated, failed));

                        this.commandService.Execute(new MigrateExpressionsToCSharp(questionnaireId));

                        migrated++;
                    }
                    catch (Exception exception)
                    {
                        failed++;

                        this.logger.Error(string.Format("Error migrating questionnaire '{0}'.", questionnaireId), exception);
                    }
                }

                UpdateStatus(string.Format("finished migration of all questionnaires (total: {0}, migrated: {1}, failed: {2})",
                    total, migrated, failed));
            }
            catch (Exception exception)
            {
                this.logger.Error("Unexpected unhandled error while migrating all questionnaires.", exception);

                UpdateStatus(string.Format("unexpectedly failed to migrate all questionnaires (total: {1}, migrated: {2}, failed: {3}){0}{0}{4}",
                    Environment.NewLine, total, migrated, failed, exception));
            }
        }

        private static void PerformMigration(Action migrate)
        {
            if (!isMigrationInProgress)
            {
                lock (LockObject)
                {
                    if (!isMigrationInProgress)
                    {
                        isMigrationInProgress = true;
                        try
                        {
                            migrate();
                        }
                        finally
                        {
                            isMigrationInProgress = false;
                        }
                    }
                }
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