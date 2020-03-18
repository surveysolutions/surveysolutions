using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace WB.UI.Headquarters.Models
{
    public static class ModelStateDictionaryExtensions
    {
        public static JsonResult ErrorsToJsonResult(this ModelStateDictionary modelState)
        {
            IEnumerable<KeyValuePair<string, string[]>> errors = modelState.IsValid
                ? null
                : modelState
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray())
                    .Where(m => m.Value.Any());

            var result = new JsonResult(errors);

            if (errors != null)
                result.StatusCode = (int) HttpStatusCode.BadRequest;

            return result;
        }
    }
}
