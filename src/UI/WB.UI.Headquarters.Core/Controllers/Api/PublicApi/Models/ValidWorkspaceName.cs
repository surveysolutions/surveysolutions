using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using WB.Infrastructure.Native.Workspaces;
using WB.UI.Headquarters.Code.Workspaces;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class ValidWorkspaceName : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var notValid = WorkspaceConstants.NotAllowedNames.Union(WorkspaceRedirectMiddleware
                .NotScopedToWorkspacePaths.Select(w => w.Trim('/').ToLower()));
            
            bool isForbiddenWord = notValid.Any(x => string.Equals(x, value?.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return !isForbiddenWord ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
        }
    }
}
