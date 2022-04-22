using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Designer.Models
{
    public class QuestionnaireRevisionBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.Metadata.ModelType == typeof(QuestionnaireRevision))
            {
                return new BinderTypeModelBinder(typeof(QuestionnaireRevisionBinder));
            }

            return null;
        }
    }

    public class QuestionnaireRevisionBinder : IModelBinder
    {
        private readonly DesignerDbContext db;

        public QuestionnaireRevisionBinder(DesignerDbContext db)
        {
            this.db = db;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            var modelName = bindingContext.ModelName;

            // Try to fetch the value of the argument by name
            var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

            if (valueProviderResult == ValueProviderResult.None)
            {
                return;
            }

            bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

            var value = valueProviderResult.FirstValue;

            // Check if the argument value is null or empty
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var parts = value.Split('$');

            if(!Guid.TryParse(parts[0],  out var questionnaireId))
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            Guid? revision = null;
            int sequence = 0;
            if (parts.Length == 2)
            {
                QuestionnaireChangeRecord? history;
                if (int.TryParse(parts[1], out sequence))
                {
                    history = await db.QuestionnaireChangeRecords.SingleOrDefaultAsync(r =>
                        r.QuestionnaireId == questionnaireId.FormatGuid() && r.Sequence == sequence);
                }
                else
                {
                    history = await db.QuestionnaireChangeRecords.SingleOrDefaultAsync(r => r.QuestionnaireChangeRecordId.StartsWith(parts[1]));
                }

                if (history == null)
                {
                    bindingContext.Result = ModelBindingResult.Failed();
                    return;
                }
                else
                {
                    revision = Guid.Parse(history.QuestionnaireChangeRecordId);
                    sequence = history.Sequence;
                }
            } 

            bindingContext.Result = ModelBindingResult.Success(new QuestionnaireRevision(questionnaireId, revision, revision != null? (int?)sequence : null));
        }
    }

}
