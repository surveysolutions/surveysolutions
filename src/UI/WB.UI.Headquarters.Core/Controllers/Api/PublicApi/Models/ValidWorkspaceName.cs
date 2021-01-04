using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class ValidWorkspaceName : ValidationAttribute
    {
        private static readonly string[] NotAllowedNames =
        {
            "api", "graphql"
        };
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var notValid = NotAllowedNames.Union(WorkspaceRedirectMiddleware
                .NotScopedToWorkspacePaths.Select(w => w.Trim('/').ToLower()));
            
            bool isForbiddenWord = notValid.Any(x => string.Equals(x, value?.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return !isForbiddenWord ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
        }
    }
}
