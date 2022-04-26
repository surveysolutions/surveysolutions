using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using Dapper;
using Microsoft.EntityFrameworkCore;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public class ClassificationsStorage : IClassificationsStorage
    {
        private readonly DesignerDbContext dbContext;

        public ClassificationsStorage(
            DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<ClassificationGroup>> GetClassificationGroups(Guid userId)
        {
            var dbEntities = await this.dbContext.ClassificationEntities
                .Where(x => x.Type == ClassificationEntityType.Group)
                .Where(x => x.UserId == null || x.UserId == userId)
                .OrderBy(x => x.Title)
                .ToListAsync();

            var list = await dbContext.ClassificationEntities.Where(x => x.Type == ClassificationEntityType.Classification)
                .Where(x => x.Parent!=null)
                .GroupBy(x => x.Parent)
                .Where(x=> x.Key != null)
                .Select(x => new {Id = x.Key!.Value, Count = x.Count()})
                .ToListAsync();
            
            var classificationCounts = list
                .ToDictionary(x => x.Id, x => x.Count);

            var groups = dbEntities.Select(x => new ClassificationGroup
            {
                Id = x.Id,
                Title = x.Title,
                Count = classificationCounts.ContainsKey(x.Id) ? classificationCounts[x.Id] : 0
            });

            return groups;
        }

        public async Task<IEnumerable<Classification>> GetClassifications(Guid groupId, Guid userId)
        {
            var tableNameWithSchema = "plainstore.classificationentities";

            var sqlSelect =
                $"SELECT c.id, c.title, c.parent, c.userid, d.count " +
                $"FROM {tableNameWithSchema} as c LEFT JOIN " +
                $"(select parent, count(*) as count from {tableNameWithSchema} where type = @childtype group by parent) as d ON d.parent = c.id " +
                $"WHERE c.type = @entitytype AND (userid IS NULL OR userid = @userid) AND c.parent = @groupid " +
                $"ORDER BY c.title"; 
            
            var classifications = await dbContext.Database.GetDbConnection().QueryAsync<Classification>(sqlSelect, new
            {
                entitytype = ClassificationEntityType.Classification,
                childtype = ClassificationEntityType.Category,
                userid = userId,
                groupid = groupId
            });

            return classifications;
        }

        public async Task<ClassificationsSearchResult> SearchAsync(string? query, Guid? groupId, bool privateOnly, Guid userId)
        {
            
                var searchQuery = ApplySearchFilter(dbContext.ClassificationEntities, query, groupId, privateOnly, userId);

                var ids = searchQuery.Select(x => x.ClassificationId)
                    .Distinct()
                    .Take(20)
                    .ToList();

                var items1 = dbContext.ClassificationEntities.Where(x => ids.Contains(x.Id) || x.Type == ClassificationEntityType.Group);

                var dbEntities = items1.ToList();
            

            var classificationIds = dbEntities.Where(x => x.Type == ClassificationEntityType.Classification).Select(x => x.Id).ToList();

            var items = dbContext.ClassificationEntities.Where(x => x.Parent != null && classificationIds.Contains(x.Parent.Value))
                .GroupBy(x => x.Parent)
                .Where(x=> x.Key != null)
                .Select(x => new {Id = x.Key!.Value, Count = x.Count()});

            var categoriesCounts = items.ToList().ToDictionary(x => x.Id, x => x.Count);

            var groups = dbEntities.Where(x => x.Type == ClassificationEntityType.Group).ToDictionary(x => x.Id, x =>
                new ClassificationGroup
                {
                    Id = x.Id,
                    Title = x.Title
                });

            var classifications = dbEntities.Where(x => x.Type == ClassificationEntityType.Classification)
                .Select(x => new Classification
                (
                    id : x.Id,
                    title : x.Title,
                    group : (groups.ContainsKey(x.Parent ?? Guid.Empty) ? groups[x.Parent ?? Guid.Empty] : null) ?? groups.Values.FirstOrDefault(),
                    categoriesCount : categoriesCounts.ContainsKey(x.Id) ? categoriesCounts[x.Id] : 0,
                    userId : x.UserId,
                    parent : x.Parent
                )).ToList();

            var total = ApplySearchFilter(dbContext.ClassificationEntities, query, groupId, privateOnly, userId)
                .Select(x => x.ClassificationId)
                .Distinct()
                .Count();

            return await Task.FromResult(new ClassificationsSearchResult(classifications, total));
        }

        private IQueryable<ClassificationEntity> ApplySearchFilter(IQueryable<ClassificationEntity> entities, 
            string? query, Guid? groupId, bool privateOnly, Guid userId)
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
            foreach (var classificationEntity in classifications)
            {
                dbContext.ClassificationEntities.Add(classificationEntity);
            }

            dbContext.SaveChanges();
        }

        public async Task<List<Category>> GetCategories(Guid classificationId)
        {
            var dbEntities = await dbContext.ClassificationEntities
                .Where(x => x.Parent == classificationId)
                .OrderBy(x => x.Index)
                .ThenBy(x => x.Value)
                .Take(Constants.MaxLongRosterRowCount)
                .ToListAsync();

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

            return categories;
        }

        public async Task CreateClassification(Classification classification, Guid userId)
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

            await this.dbContext.AddAsync(entity);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task UpdateClassification(Classification classification, Guid userId, bool isAdmin)
        {
            var entity = await this.dbContext.ClassificationEntities.FindAsync(classification.Id);
            if(entity == null) throw new ClassificationException(ClassificationExceptionType.Undefined, "Classification was not found.");            
            
            ThrowIfUserDoesNotHaveAccessToPublicEntity(entity, isAdmin);
            ThrowIfUserDoesNotHaveAccessToPrivate(entity, userId);

            entity.Parent = classification.Parent;
            entity.Title = classification.Title;

            this.dbContext.Update(entity);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task DeleteClassificationAsync(Guid classificationId, Guid userId, bool isAdmin)
        {
            var classification = await this.dbContext.ClassificationEntities.FindAsync(classificationId);
            if(classification == null)
                throw new ClassificationException(ClassificationExceptionType.Undefined, "Classification was not found.");
            
            ThrowIfUserDoesNotHaveAccessToPublicEntity(classification, isAdmin);
            ThrowIfUserDoesNotHaveAccessToPrivate(classification, userId);
            
            await this.DeleteClassificationAsync(classification);
            await dbContext.SaveChangesAsync();
        }

        public async Task CreateClassificationGroup(ClassificationGroup group, bool isAdmin)
        {
            if (!isAdmin)
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot create public groups");

            await this.dbContext.ClassificationEntities.AddAsync(new ClassificationEntity
            {
                Id = group.Id,
                Title = group.Title,
                Type = ClassificationEntityType.Group
            });
            await this.dbContext.SaveChangesAsync();
        }

        public async Task UpdateClassificationGroup(ClassificationGroup group, bool isAdmin)
        {
            if (!isAdmin)
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot change public records");

            var entity = await this.dbContext.ClassificationEntities.FindAsync(group.Id);
            if(entity == null) throw new ClassificationException(ClassificationExceptionType.Undefined, "Classification was not found.");
            entity.Title = group.Title;
            this.dbContext.ClassificationEntities.Update(entity);
            await this.dbContext.SaveChangesAsync();
        }

        public async Task DeleteClassificationGroup(Guid groupId, bool isAdmin)
        {
            if (!isAdmin)
                throw new ClassificationException(ClassificationExceptionType.NoAccess, "Cannot change public records");

            var classificationsToDelete = await this.dbContext.ClassificationEntities.Where(x => x.Parent == groupId).ToListAsync();
            foreach (var classificationEntity in classificationsToDelete)
            {
                await DeleteClassificationAsync(classificationEntity);
            }

            var classification = await this.dbContext.ClassificationEntities.FindAsync(groupId);
            if(classification == null) throw new ClassificationException(ClassificationExceptionType.Undefined, "Classification was not found.");
            this.dbContext.ClassificationEntities.Remove(classification);

            await this.dbContext.SaveChangesAsync();
        }

        private async Task DeleteClassificationAsync(ClassificationEntity classificationEntity)
        {
            var categories = await this.dbContext.ClassificationEntities.Where(x => x.Parent == classificationEntity.Id).ToListAsync();
            this.dbContext.ClassificationEntities.RemoveRange(categories);
            this.dbContext.Remove(classificationEntity);
        }

        public async Task UpdateCategories(Guid classificationId, Category[] categories, Guid userId, bool isAdmin)
        {
            var classification = await this.dbContext.ClassificationEntities.FindAsync(classificationId);
            if(classification == null) throw new ClassificationException(ClassificationExceptionType.Undefined, "Classification was not found.");
            
            ThrowIfUserDoesNotHaveAccessToPublicEntity(classification, isAdmin);
            ThrowIfUserDoesNotHaveAccessToPrivate(classification, userId);

            var categoryIds = categories.Select(x => x.Id).ToList();

            var categoriesInClassification = await this.dbContext.ClassificationEntities
                .Where(x => x.Parent == classificationId && !categoryIds.Contains(x.Id))
                .ToListAsync();

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
            });

            var categoriesToDelete = categoriesInClassification.Where(x => categories.All(c => c.Id != x.Id)).ToList();

            foreach (var classificationEntity in categoryEntities)
            {
                var entity = dbContext.Find<ClassificationEntity>(classificationEntity.Id);
                if (entity != null)
                    dbContext.Remove(entity);

                await dbContext.ClassificationEntities.AddAsync(classificationEntity);
            }

            foreach (var classificationEntity in categoriesToDelete)
                this.dbContext.Remove(classificationEntity);

            await this.dbContext.SaveChangesAsync();
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
