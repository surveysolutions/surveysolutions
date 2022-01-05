using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using WB.Core.BoundedContexts.Designer;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Services.Restore;

namespace WB.UI.Designer.Areas.Admin.Pages
{
    [Authorize(Roles = nameof(SimpleRoleEnum.Administrator))]
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    public class RestoreQuestionnaireModel : PageModel
    {
        private readonly ILogger<RestoreQuestionnaireModel> logger;
        private readonly IQuestionnaireRestoreService restoreService;

        public RestoreQuestionnaireModel(ILogger<RestoreQuestionnaireModel> logger, 
            IQuestionnaireRestoreService restoreService)
        {
            this.logger = logger;
            this.restoreService = restoreService;
        }

        public void OnGet()
        {
            this.Error = null;
            this.Success = null;
        }

        [BindProperty]
        public IFormFile? Upload { get; set; }

        [BindProperty] public bool CreateNew { get; set; } = true;

        public IActionResult OnPost()
        {
            var state = new RestoreState();

            try
            {
                var areFileNameAndContentAvailable = Upload?.FileName != null && Upload.Length > 0;
                if (!areFileNameAndContentAvailable)
                {
                    this.Error = "Uploaded file is not specified or empty.";
                    return Page();
                }

                if (Upload!.FileName!.ToLower().EndsWith(".tmpl"))
                {
                    this.Error = "You are trying to restore old format questionnaire. Please use 'Old Format Restore' option.";
                    return Page();
                }

                if (!Upload.FileName.ToLower().EndsWith(".zip"))
                {
                    this.Error = "Only zip archives are supported. Please upload correct zip backup.";
                    return Page();
                }
                
                var openReadStream = Upload.OpenReadStream();
                
                var questionnaireId = restoreService.RestoreQuestionnaire(openReadStream, User.GetId(), state, CreateNew);
                
                this.Success = $"Restore finished. Restored {state.RestoredEntitiesCount} entities. Questionnaire Id: {questionnaireId.FormatGuid()}";
                this.Error = state.Error;
                return Page();
            }
            catch (Exception exception)
            {
                this.Success = state.Success.ToString();
                this.Error = state.Error;
                this.logger.LogError(exception, "Unexpected error occurred during restore of questionnaire from backup.");
                this.Error = $"Unexpected error occurred.{Environment.NewLine}{exception}";
                return Page();
            }
        }

        [TempData]
        public string? Success { get; set; } = string.Empty;

        [TempData]
        public string? Error { get; set; } = string.Empty;
    }
}
