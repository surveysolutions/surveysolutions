using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public interface IClassificationsStorage
    {
        Task<IEnumerable<ClassificationGroup>> GetClassificationGroups();
        Task<IEnumerable<Classification>> GetClassifications(Guid? groupId);
        Task<ClassificationsSearchResult> SearchAsync(string query, Guid? groupId);
        void Store(ClassificationEntity[] bdEntities);
        Task<List<Category>> GetCategories(Guid classificationId);
        Task CreateClassification(Classification classification);
        Task UpdateClassification(Classification classification);
        Task DeleteClassification(Guid classificationId);
        Task CreateClassificationGroup(ClassificationGroup group);
        Task UpdateClassificationGroup(ClassificationGroup group);
        Task DeleteClassificationGroup(Guid groupId);
        Task UpdateCategories(Guid classificationId, Category[] categories);
    }
}
