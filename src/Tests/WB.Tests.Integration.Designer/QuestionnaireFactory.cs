using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.NonConficltingNamespace;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.Designer
{
    internal class QuestionnaireFactory
    {
        public QuestionnaireListViewItem ListViewItem(Guid id, string title,
            bool isPublic = true, bool isDeleted = false, Guid? folderId = null)
        {
            return new QuestionnaireListViewItem
            {
                CreationDate = DateTime.UtcNow,
                CreatorName = "test",
                IsDeleted = isDeleted,
                IsPublic = isPublic,
                FolderId = folderId,
                Title = title,
                PublicId = id,
                QuestionnaireId = id.FormatGuid(),
            };
        }

        public QuestionnaireListViewFolder ListViewFolder(Guid id, string title, Guid? parent,
            int depth, string path = "")
        {
            return new QuestionnaireListViewFolder()
            {
                CreateDate = DateTime.UtcNow,
                CreatorName = "test",
                CreatedBy = Guid.NewGuid(),
                Title = title,
                Parent = parent,
                PublicId = id,
                Depth = depth,
                Path = path
            };
        }

        public TextQuestion TextQuestion(Guid? questionId = null, string enablementCondition = null, string validationExpression = null,
            string mask = null,
            string variable = null,
            string validationMessage = null,
            string text = "Question T",
            QuestionScope scope = QuestionScope.Interviewer,
            bool preFilled = false,
            string label = null,
            string instruction = null,
            IEnumerable<ValidationCondition> validationConditions = null,
            bool hideIfDisabled = false)
            => new TextQuestion
            {
                PublicKey = questionId ?? Guid.NewGuid(),
                ConditionExpression = enablementCondition,
                HideIfDisabled = hideIfDisabled,
                ValidationExpression = validationExpression,
                ValidationMessage = validationMessage,
                Mask = mask,
                QuestionText = text,
                StataExportCaption = variable ?? "vv" + Guid.NewGuid().ToString("N"),
                QuestionScope = scope,
                Featured = preFilled,
                VariableLabel = label,
                Instructions = instruction,
                ValidationConditions = validationConditions?.ToList().ConcatWithOldConditionIfNotEmpty(validationExpression, validationMessage)
            };
    }
}
