using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Api
{
    [Authorize]
    public class FindReplaceController : Controller
    {
        private const int SearchForAllowedLength = 500;
        private readonly IFindReplaceService replaceService;

        public FindReplaceController(IFindReplaceService replaceService)
        {
            this.replaceService = replaceService;
        }

        [HttpGet]
        public IActionResult FindAll(Guid id, string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            if (searchFor?.Length > SearchForAllowedLength)
            {
                var message = string.Format(FindReplaceResources.SearchForTooLong, SearchForAllowedLength);
                return StatusCode((int) HttpStatusCode.BadRequest, message);
            }

            if (useRegex && !IsValidRegex(searchFor))
            {
                return StatusCode((int) HttpStatusCode.BadRequest, FindReplaceResources.SearchForRegexNotValid);
            }

            return Ok(this.replaceService.FindAll(id, searchFor, matchCase, matchWholeWord, useRegex));
        }

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;

            try
            {
                var regex = new Regex(pattern);
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
