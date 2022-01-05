using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public interface IClassificationsStorage
    {
        Task<IEnumerable<ClassificationGroup>> GetClassificationGroups(Guid userId);
        Task<IEnumerable<Classification>> GetClassifications(Guid groupId, Guid userId);
        Task<ClassificationsSearchResult> SearchAsync(string? query, Guid? groupId, bool privateOnly, Guid userId);
        void Store(ClassificationEntity[] classifications);
        Task<List<Category>> GetCategories(Guid classificationId);
        Task CreateClassification(Classification classification, Guid userId);
        Task UpdateClassification(Classification classification, Guid userId, bool isAdmin);
        Task DeleteClassificationAsync(Guid classificationId, Guid userId, bool isAdmin);
        Task CreateClassificationGroup(ClassificationGroup group, bool isAdmin);
        Task UpdateClassificationGroup(ClassificationGroup group, bool isAdmin);
        Task DeleteClassificationGroup(Guid groupId, bool isAdmin);
        Task UpdateCategories(Guid classificationId, Category[] categories, Guid userId, bool isAdmin);
    }
}
