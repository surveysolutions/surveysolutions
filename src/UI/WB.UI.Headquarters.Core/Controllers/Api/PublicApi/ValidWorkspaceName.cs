using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi
{
    public class ValidWorkspaceName : ValidationAttribute
    {
        private static readonly string[] NotAllowedNames =
        {
            "api", "graphql"
        };
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            bool isForbiddenWord =
                NotAllowedNames.Any(x => string.Equals(x, value?.ToString(), StringComparison.InvariantCultureIgnoreCase));
            return !isForbiddenWord ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
        }
    }
}
