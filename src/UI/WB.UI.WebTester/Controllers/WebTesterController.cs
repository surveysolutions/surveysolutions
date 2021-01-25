using Refit;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Questionnaire.Api;
using WB.UI.WebTester.Infrastructure;
using WB.UI.WebTester.Resources;
using WB.UI.WebTester.Services;

namespace WB.UI.WebTester.Controllers
{
    [Route("WebTester")]
    public class WebTesterController : Controller
    {
        private readonly IStatefulInterviewRepository statefulInterviewRepository;
        private readonly IEvictionNotifier evictionService;
        private readonly IInterviewFactory interviewFactory;
        private readonly IOptions<TesterConfiguration> testerConfig;

        public WebTesterController(
            IStatefulInterviewRepository statefulInterviewRepository,
            IEvictionNotifier evictionService,
            IInterviewFactory interviewFactory,
            IOptions<TesterConfiguration> testerConfig)
        {
            this.statefulInterviewRepository = statefulInterviewRepository ?? throw new ArgumentNullException(nameof(statefulInterviewRepository));
            this.evictionService = evictionService;
            this.interviewFactory = interviewFactory;
            this.testerConfig = testerConfig;
        }

        [Route("Run/{id:Guid}")]
        public IActionResult Run(Guid id, string sid, int? scenarioId = null)
        {
            return this.View(new InterviewPageModel
            {
                Id = id.ToString(),
                OriginalInterviewId = sid ?? string.Empty,
                ScenarioId = scenarioId
            });
        }

        [Route("Redirect/{id:Guid}")]
        public async Task<IActionResult> Redirect(Guid id, string originalInterviewId, string scenarioId)
        {
            if (this.statefulInterviewRepository.Get(id.FormatGuid()) != null)
            {
                evictionService.Evict(id);
            }

            try
            {
                if (!string.IsNullOrEmpty(scenarioId))
                {
                    var result = await this.interviewFactory.CreateInterview(id, int.Parse(scenarioId));
                    if (result != CreationResult.DataRestored)
                    {
                        TempData["Message"] = Common.ReloadInterviewErrorMessage;
                    }
                }
                else if (!string.IsNullOrEmpty(originalInterviewId))
                {
                    var result = await this.interviewFactory.CreateInterview(id, Guid.Parse(originalInterviewId));
                    if (result != CreationResult.DataRestored)
                    {
                        TempData["Message"] = Common.ReloadInterviewErrorMessage;
                    }
                }
                else
                {
                    await this.interviewFactory.ImportQuestionnaireAndCreateInterview(id);
                }
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return this.RedirectToAction("QuestionnaireWithErrors", "Error");
            }

            var interview = statefulInterviewRepository.Get(id.FormatGuid()) as WebTesterStatefulInterview;
            if (interview?.Questionnaire.IsCoverPageSupported ?? false)
                return this.Redirect($"~/WebTester/Interview/{id.FormatGuid()}/Section/{interview?.Questionnaire.CoverPageSectionId.FormatGuid()}");
            else
                return this.Redirect($"~/WebTester/Interview/{id.FormatGuid()}/Cover");
        }

        [Route("Interview/{id}")]
        [Route("Interview/{id}/Cover")]
        [Route("Interview/{id}/Section/{url}")]
        public IActionResult Interview(string id)
        {
            try
            {
                var interviewPageModel = GetInterviewPageModel(id);
                if (interviewPageModel == null)
                    return StatusCode(StatusCodes.Status404NotFound, string.Empty);

                return View(interviewPageModel);
            }
            catch (ApiException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                return StatusCode(StatusCodes.Status404NotFound, string.Empty);
            }
        }

        private InterviewPageModel? GetInterviewPageModel(string id)
        {
            var interview = statefulInterviewRepository.Get(id);

            if (interview == null)
            {
                return null;
            }

            WebTesterStatefulInterview webTesterInterview = (WebTesterStatefulInterview) interview;

            var questionnaire =
                webTesterInterview.Questionnaire;

            var designerUrl = testerConfig.Value.DesignerAddress;
            var reloadQuestionnaireUrl =
                $"{designerUrl}/WebTesterReload/Index/{interview.QuestionnaireIdentity.QuestionnaireId}?interviewId={id}";

            var saveScenarioDesignerUrl = $"{designerUrl}/api/WebTester/Scenarios/{interview.QuestionnaireIdentity.QuestionnaireId}";
                
            var interviewPageModel = new InterviewPageModel
            {
                Id = id,
                CoverPageId = questionnaire.IsCoverPageSupported ? questionnaire.CoverPageSectionId.FormatGuid() : String.Empty,
                Title = $"{questionnaire.Title} | Web Tester",
                GoogleMapsKey = testerConfig.Value.GoogleMapApiKey,
                ReloadQuestionnaireUrl = reloadQuestionnaireUrl
            };
            
            interviewPageModel.GetScenarioUrl = Url.Content("~/api/ScenariosApi");
            interviewPageModel.SaveScenarioUrl = saveScenarioDesignerUrl;
            interviewPageModel.DesignerUrl = designerUrl;
            
            return interviewPageModel;
        }
    }

    public class QuestionnaireAttachment
    {
        public QuestionnaireAttachment(Guid id, AttachmentContent content)
        {
            Content = content;
            Id = id;
        }

        public Guid Id { get; set; }
        public AttachmentContent Content { get; set; }
    }

    public class InterviewPageModel
    {
        public string? Title { get; set; }
        public string? GoogleMapsKey { get; set; }
        public string? Id { get; set; }
        public string? CoverPageId { get; set; }
        public string? ReloadQuestionnaireUrl { get; set; }
        public string? OriginalInterviewId { get; set; }
        public string? SaveScenarioUrl { get; set; }
        public string? GetScenarioUrl { get; set; }
        public int? ScenarioId { get; set; }
        public string? DesignerUrl { get; set; }
    }

    public class ApiTestModel
    {
        public ApiTestModel()
        {
            Attaches = new List<string>();
        }

        public Guid Id { get; set; }
        public DateTime LastUpdated { get; set; }
        public int NumOfTranslations { get; set; }
        public List<string> Attaches { get; set; }
        public string? Title { get; set; }
    }
}
