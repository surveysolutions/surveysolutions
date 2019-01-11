using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Exceptions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Infrastructure.Native.Storage.Postgre;
using Dapper;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public class ClassificationsStorage : IClassificationsStorage
    {
        private readonly IPlainStorageAccessor<ClassificationEntity> classificationsStorage;
        private readonly IUnitOfWork unitOfWork;

        public ClassificationsStorage(
            IPlainStorageAccessor<ClassificationEntity> classificationsStorage, 
            IUnitOfWork unitOfWork)
        {
            this.classificationsStorage = classificationsStorage;
            this.unitOfWork = unitOfWork;
        }

        public Task<IEnumerable<ClassificationGroup>> GetClassificationGroups(Guid userId)
        {
            var dbEntities = classificationsStorage.Query(_ => _
                .Where(x => x.Type == ClassificationEntityType.Group)
                .Where(x => x.UserId == null || x.UserId == userId)
                .OrderBy(x => x.Title)
                .ToList());
            
            var classificationCounts = classificationsStorage.Query(_ => _.Where(x => x.Type == ClassificationEntityType.Classification)
                .Where(x => x.Parent!=null)
                .GroupBy(x => x.Parent)
                .Select(x => new {Id = x.Key, Count = x.Count()})
                .ToList())
                .ToDictionary(x => x.Id, x => x.Count);

            var groups = dbEntities.Select(x => new ClassificationGroup
            {
                Id = x.Id,
                Title = x.Title,
                Count = classificationCounts.ContainsKey(x.Id) ? classificationCounts[x.Id] : 0
            });

            return Task.FromResult(groups);
        }

        public Task<IEnumerable<Classification>> GetClassifications(Guid groupId, Guid userId)
        {
            var tableNameWithSchema = "plainstore.classificationentities";

            var sqlSelect =
                $"SELECT c.id, c.title, c.parent, c.userid, d.count " +
                $"FROM {tableNameWithSchema} as c LEFT JOIN " +
                $"(select parent, count(*) as count from {tableNameWithSchema} where type = @childtype group by parent) as d ON d.parent = c.id " +
                $"WHERE c.type = @entitytype AND (userid IS NULL OR userid = @userid) AND c.parent = @groupid " +
                $"ORDER BY c.title"; 
            
            var classifications = unitOfWork.Session.Connection.QueryAsync<Classification>(sqlSelect, new
            {
                entitytype = ClassificationEntityType.Classification,
                childtype = ClassificationEntityType.Category,
                userid = userId,
                groupid = groupId
            });

            return classifications;
        }

        public async Task<ClassificationsSearchResult> SearchAsync(string query, Guid? groupId, bool privateOnly, Guid userId)
        {
            var dbEntities = classificationsStorage.Query(_ =>
            {
                var searchQuery = ApplySearchFilter(_, query, groupId, privateOnly, userId);

                var ids = searchQuery.Select(x => x.ClassificationId)
                    .Distinct()
                    .Take(20)
                    .ToList();

                var items = _.Where(x => ids.Contains(x.Id) || x.Type == ClassificationEntityType.Group);

                return items.ToList();
            });

            var classificationIds = dbEntities.Where(x => x.Type == ClassificationEntityType.Classification).Select(x => x.Id).ToList();

            var categoriesCounts = classificationsStorage.Query(_ =>
            {
                var items = _.Where(x => x.Parent != null && classificationIds.Contains(x.Parent.Value))
                    .GroupBy(x => x.Parent)
                    .Select(x => new {Id = x.Key, Count = x.Count()});

                return items.ToList();
            }).ToDictionary(x => x.Id, x => x.Count);

            var groups = dbEntities.Where(x => x.Type == ClassificationEntityType.Group).ToDictionary(x => x.Id, x =>
                new ClassificationGroup
                {
                    Id = x.Id,
                    Title = x.Title
                });

            var classifications = dbEntities.Where(x => x.Type == ClassificationEntityType.Classification)
                .Select(x => new Classification
                {
                    Id = x.Id,
                    Title = x.Title,
                    Group = (groups.ContainsKey(x.Parent ?? Guid.Empty) ? groups[x.Parent ?? Guid.Empty] : null) ?? groups.Values.FirstOrDefault(),
                    CategoriesCount = categoriesCounts.ContainsKey(x.Id) ? categoriesCounts[x.Id] : 0,
                    UserId = x.UserId,
                    Parent = x.Parent
                }).ToList();

            var total = classificationsStorage.Query(_ => ApplySearchFilter(_, query, groupId, privateOnly, userId)
                .Select(x => x.ClassificationId)
                .Distinct()
                .Count());

            return await Task.FromResult(new ClassificationsSearchResult
            {
                Classifications = classifications,  
                Total = total
            });
        }

        private IQueryable<ClassificationEntity> ApplySearchFilter(IQueryable<ClassificationEntity> entities, 
            string query, Guid? groupId, bool privateOnly, Guid userId)
        {
            var searchQuery = privateOnly
                ? entities.Where(x => x.UserId == userId)
                : entities.Where(x => x.UserId == null || x.UserId == userId);

            var lowercaseQuery = (query?? string.Empty).ToLower().Trim();
            if (!string.IsNullOrWhiteSpace(lowercaseQuery))
            {
                searchQuery = searchQuery.Where(x => x.Title.ToLower().Contains(lowercaseQuery));
            }

            if (groupId.HasValue)
            {
                searchQuery = searchQuery.Where(x => x.Parent == groupId);
            }
            return searchQuery;
        }

        public void Store(ClassificationEntity[] classifications)
        {
            var enumerable = classifications.Select(x =>  new Tuple<ClassificationEntity, object>(x, x.Id)).ToArray();
            classificationsStorage.Store(entities: enumerable);
        }

        public Task<List<Category>> GetCategories(Guid classificationId)
        {
            var dbEntities = classificationsStorage.Query(_ => _.Where(x => x.Parent == classificationId).OrderBy(x => x.Index).ThenBy(x => x.Value).Take(Constants.MaxLongRosterRowCount).ToList());

            var categories = dbEntities.Select(x => new Category
                {
                    Id = x.Id,
                    Value = x.Value ?? 0,
                    Title = x.Title,
                    Order = x.Index ?? 0,
                    Parent = x.Parent
                })
                .OrderBy(x => x.Order).ThenBy(x => x.Value)
                .ToList();

            return Task.FromResult(categories);
        }

        public Task CreateClassification(Classification classification, Guid userId)
        {
            var entity = new ClassificationEntity
            {
                Id = classification.Id,
                Title = classification.Title,
                Parent =  classification.Parent,
                Type = ClassificationEntityType.Classification,
                ClassificationId = classification.Id,
                UserId = userId
            };

            this.classificationsStorage.Store(entity, entity.Id);
            return Task.CompletedTask;
        }

        public Task UpdateClassification(Classification classification, Guid userId, bool isAdmin)
        {
            var entity = this.classificationsStorage.GetById(classification.Id);
            ThrowIfUserDoesNotHaveAccessToPublicEntity(entity, isAdmin);
            ThrowIfUserDoesNotHaveAccessToPrivate(entity, userId);

            entity.Parent = classification.Parent;
            entity.Title = classification.Title;

            this.classificationsStorage.Store(entity, entity.Id);

            return Task.CompletedTask;
        }

        public Task DeleteClassification(Guid classificationId, Guid userId, bool isAdmin)
        {
            var classification = this.classificationsStorage.GetById(classificationId);
            ThrowIfUserDoesNotHaveAccessToPublicEntity(classification, isAdmin);
            ThrowIfUserDoesNotHaveAccessToPrivate(classification, userId);
            
            this.DeleteClassification(classification);
            
            return Task.CompletedTask;
        }

        public Task CreateClassificationGroup(ClassificationGroup group, bool isAdmin)
        {
            if (!isAdmin)
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot create public groups");
            this.classificationsStorage.Store(new ClassificationEntity
            {
                Id = group.Id,
                Title = group.Title,
                Type = ClassificationEntityType.Group
            }, group.Id);
            return Task.CompletedTask;
        }

        public Task UpdateClassificationGroup(ClassificationGroup group, bool isAdmin)
        {
            if (!isAdmin)
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot change public records");

            var entity = this.classificationsStorage.GetById(group.Id);
            entity.Title = group.Title;
            this.classificationsStorage.Store(entity, entity.Id);
            return Task.CompletedTask;
        }

        public Task DeleteClassificationGroup(Guid groupId, bool isAdmin)
        {
            if (!isAdmin)
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot change public records");

            var classificationsToDelete = this.classificationsStorage.Query(_ => _.Where(x => x.Parent == groupId).ToList());
            foreach (var classificationEntity in classificationsToDelete)
            {
                DeleteClassification(classificationEntity);
            }
            this.classificationsStorage.Remove(groupId);

            return Task.CompletedTask;
        }

        private void DeleteClassification(ClassificationEntity classificationEntity)
        {
            var categories = this.classificationsStorage.Query(_ => _.Where(x => x.Parent == classificationEntity.Id).ToList());
            this.classificationsStorage.Remove(categories);
            this.classificationsStorage.Remove(classificationEntity.Id);
        }

        public Task UpdateCategories(Guid classificationId, Category[] categories, Guid userId, bool isAdmin)
        {
            var classification = this.classificationsStorage.GetById(classificationId);

            ThrowIfUserDoesNotHaveAccessToPublicEntity(classification, isAdmin);
            ThrowIfUserDoesNotHaveAccessToPrivate(classification, userId);

            var a = categories.Select(x => x.Id).ToList();

            var categoriesInClassification = this.classificationsStorage.Query(_ =>
                _.Where(x => x.Parent == classificationId && !a.Contains(x.Id)).ToList());

            var categoryEntities = categories.Select((x, index) => new ClassificationEntity
            {
                Id = x.Id,
                ClassificationId = classificationId,
                Parent = classificationId,
                Title = x.Title,
                Value = x.Value,
                Type = ClassificationEntityType.Category,
                Index = index,
                UserId = classification.UserId
            }).ToArray();

            var categoriesToDelete = categoriesInClassification.Where(x => categories.All(c => c.Id != x.Id)).ToList();

            Store(categoryEntities);

            if (categoriesToDelete.Any())
            {
                this.classificationsStorage.Remove(categoriesToDelete);
            }

            return Task.CompletedTask;
        }

        private void ThrowIfUserDoesNotHaveAccessToPublicEntity(ClassificationEntity entity, bool isAdmin)
        {
            if (!entity.UserId.HasValue && !isAdmin)
            {
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot change public records");
            }
        }
        private void ThrowIfUserDoesNotHaveAccessToPrivate(ClassificationEntity entity, Guid userId)
        {
            if (entity.UserId.HasValue && entity.UserId != userId)
            {
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot change private records");
            }
        }
    }
}
