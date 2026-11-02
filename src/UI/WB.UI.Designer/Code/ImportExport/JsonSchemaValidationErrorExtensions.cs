using System.Collections.Generic;
using System.Linq;
using NJsonSchema.Validation;

namespace WB.UI.Designer.Code.ImportExport
{
    public static class JsonSchemaValidationErrorExtensions
    {
        public static IEnumerable<ValidationError> Flatten(this IEnumerable<ValidationError> errors)
        {
            foreach (var error in errors)
            {
                if (error is ChildSchemaValidationError childError)
                {
                    foreach (var nested in childError.Errors.Values.SelectMany(e => e).Flatten())
                        yield return nested;
                }
                else
                {
                    yield return error;
                }
            }
        }
    }
}
