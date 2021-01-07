using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Headquarters.Workspaces;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    public class UniqueWorkspaceNameAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var plainStorageAccessor = validationContext.GetService<IPlainStorageAccessor<Workspace>>();
            if (plainStorageAccessor == null)
                throw new InvalidOperationException("Workspace accessor was not resolved");
            
            var suchNameStoredInDatabase = plainStorageAccessor.Query(_ => _.Any(x => x.Name == value as string));

            return !suchNameStoredInDatabase ? ValidationResult.Success : new ValidationResult(ErrorMessageString);
        }
    }
}
