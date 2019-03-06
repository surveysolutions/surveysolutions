using System;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Web.Http;
using WB.Core.BoundedContexts.Designer.Services;
using WB.UI.Designer.Filters;
using WB.UI.Designer.Resources;

namespace WB.UI.Designer.Api
{
    [CamelCase]
    [Authorize]
    public class FindReplaceController : ApiController
    {
        private const int SearchForAllowedLength = 500;
        private readonly IFindReplaceService replaceService;

        public FindReplaceController(IFindReplaceService replaceService)
        {
            this.replaceService = replaceService;
        }

        [HttpGet]
        public HttpResponseMessage FindAll(Guid id, string searchFor, bool matchCase, bool matchWholeWord, bool useRegex)
        {
            if (searchFor?.Length > SearchForAllowedLength)
            {
                var message = string.Format(FindReplaceResources.SearchForTooLong, SearchForAllowedLength);
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, message);
            }

            if (useRegex && !IsValidRegex(searchFor))
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, FindReplaceResources.SearchForRegexNotValid);
            }

            return Request.CreateResponse(this.replaceService.FindAll(id, searchFor, matchCase, matchWholeWord, useRegex));
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