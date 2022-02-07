using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.UI.Designer.Code;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    [Authorize]
    [QuestionnairePermissions]
    public class FindReplaceController : ControllerBase
    {
        private const int SearchForAllowedLength = 500;
        private readonly IFindReplaceService replaceService;

        public FindReplaceController(IFindReplaceService replaceService)
        {
            this.replaceService = replaceService;
        }

        [HttpGet]
        [Route("api/findReplace/findAll")]
        public IActionResult FindAll(QuestionnaireRevision id, string? searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            searchFor ??= String.Empty;

            if (searchFor.Length > SearchForAllowedLength)
            {
                var message = string.Format(FindReplaceResources.SearchForTooLong, SearchForAllowedLength);
                return this.Error((int) HttpStatusCode.BadRequest, message);
            }

            if (useRegex && !IsValidRegex(searchFor))
            {
                return this.Error((int) HttpStatusCode.BadRequest, FindReplaceResources.SearchForRegexNotValid);
            }

            return Ok(this.replaceService.FindAll(id, searchFor, matchCase, matchWholeWord, useRegex));
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                var regex = new Regex(Regex.Escape(pattern));
                regex.IsMatch("");
            }
            catch (ArgumentException)
            {
                return false;
            }

            return true;
        }
    }
}
