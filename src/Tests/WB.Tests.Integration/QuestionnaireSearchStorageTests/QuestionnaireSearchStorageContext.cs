using System;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.QuestionnaireList;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.Core.GenericSubdomains.Portable;

namespace WB.Tests.Integration.QuestionnaireSearchStorageTests
{
    [TestOf(typeof(QuestionnaireSearchStorage))]
    internal class QuestionnaireSearchStorageContext : IntegrationTest
    {
        public QuestionnaireListViewItem CreateQuestionnaireListViewItem(Guid id, string title,
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

        public QuestionnaireListViewFolder CreateQuestionnaireListViewFolder(Guid id, string title, Guid? parent,
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
    }
}
