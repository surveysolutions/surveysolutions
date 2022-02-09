using System;
using WB.Core.BoundedContexts.Designer.Classifications;

namespace WB.Tests.Integration.Designer
{
    internal class ClassificationsFactory
    {
        public ClassificationEntity Group(Guid? id = null, string title = null, Guid? userId = null)
        {
            return new ClassificationEntity
            {
                Type = ClassificationEntityType.Group,
                Id = id ?? Guid.NewGuid(),
                Parent = null,
                Title = title ?? "Group 1",
                UserId = userId
            };
        }

        public ClassificationEntity Classification(Guid? id = null, string title = null, Guid? userId = null, Guid? parent = null)
        {
            var classificationId = id ?? Guid.NewGuid();
            return new ClassificationEntity
            {
                Type = ClassificationEntityType.Classification,
                Id = classificationId,
                Parent = parent,
                Title = title ?? "Classification 1",
                UserId = userId,
                ClassificationId = classificationId
            };
        }

        public ClassificationEntity Category(Guid? id = null, string title = null, int? value = null, Guid? userId = null, Guid? parent = null, int? index = null)
        {
            return new ClassificationEntity
            {
                Type = ClassificationEntityType.Category,
                Id = id ?? Guid.NewGuid(),
                Parent = parent,
                Title = title ?? "Category 1",
                UserId = userId,
                Value = value,
                Index = index,
                ClassificationId = parent
            };
        }
    }
}
