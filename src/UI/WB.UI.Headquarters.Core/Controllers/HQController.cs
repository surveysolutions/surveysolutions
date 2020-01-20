﻿using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.Exceptions;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.UI.Headquarters.Filters;
using WB.UI.Headquarters.Services;

namespace WB.UI.Headquarters.Controllers
{
    [Authorize(Roles = "Administrator, Headquarter")]
    public class HQController : Controller
    {
        private readonly ICommandService commandService;
        private readonly IAuthorizedUser authorizedUser;
        private readonly IQuestionnaireExporter questionnaireExporter;
        private readonly EventBusSettings eventBusSettings;

        public HQController(ICommandService commandService,
            IAuthorizedUser authorizedUser,
            IQuestionnaireExporter questionnaireExporter,
            EventBusSettings eventBusSettings)
        {
            this.commandService = commandService;
            this.authorizedUser = authorizedUser;
            this.questionnaireExporter = questionnaireExporter;
            this.eventBusSettings = eventBusSettings;
        }

        public IActionResult Index()
        {
            return RedirectToActionPermanent("Index","Interviews");
        }

        public IActionResult Interviews(Guid? questionnaireId)
        {
            return RedirectToActionPermanent("Index", "Interviews");
        }
       
        [ObserverNotAllowed]
        [Authorize(Roles = "Administrator")]
        public IActionResult ExportQuestionnaire(Guid id, long version)
        {
            var file = questionnaireExporter.CreateZipExportFile(new QuestionnaireIdentity(id, version));
            return File(file.FileStream, "application/zip", file.Filename);
        }

        public IActionResult TakeNew(string id)
        {
            if (!QuestionnaireIdentity.TryParse(id, out QuestionnaireIdentity identity))
                return NotFound(id);
            
            var newInterviewId = Guid.NewGuid();
            this.eventBusSettings.IgnoredAggregateRoots.Add(newInterviewId.FormatGuid());

            var command = new CreateTemporaryInterviewCommand(newInterviewId, this.authorizedUser.Id, identity);

            try
            {
                this.commandService.Execute(command);
            }
            catch (InterviewException)
            {
                //Error(Assignments.ErrorToCreateAssignment + ie.Message); todo KP-13496
                return this.RedirectToAction("Index", "SurveySetup");
            }

            return this.RedirectToAction("TakeNewAssignment", new {id = newInterviewId.FormatGuid()});
        }

        public IActionResult TakeNewAssignment(string id)
        {
            if (!this.eventBusSettings.IsIgnoredAggregate(Guid.Parse(id)))
                return NotFound();

            return this.View(new
            {
                id = id,
                responsiblesUrl = Url.Action("ResponsiblesCombobox", "Teams"),
                createNewAssignmentUrl = Url.Action("Create", "AssignmentsApi"),
                maxInterviewsByAssignment = Constants.MaxInterviewsCountByAssignment,
                assignmentsUrl = Url.Action("Index", "Assignments", new { id = (int?)null })
            });
        }
    }
}
