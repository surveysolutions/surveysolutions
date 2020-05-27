using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.SharedKernels.NonConficltingNamespace // we cannot put any classes here without compilation errors since namespace will conflict with class names )))
{
    public static class ValidationConditionsBackwardCompatibility
    {
        public static IList<ValidationCondition> ConcatWithOldConditionIfNotEmpty(
            this IList<ValidationCondition> validationConditions, string? expression, string? message)
        {
            if (!string.IsNullOrEmpty(expression) || !string.IsNullOrEmpty(message))
            {
                return validationConditions.Concat(new ValidationCondition
                {
                    Expression = expression ?? String.Empty,
                    Message = message ?? String.Empty
                }.ToEnumerable()).ToList();
            }

            return validationConditions;
        } 
    }
}
