using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using WB.Core.BoundedContexts.Designer.Verifier;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Documents;

namespace WB.Core.BoundedContexts.Designer.Classifications
{
    public interface IClassificationEntity
    {
        Guid Id { get; }
        string Title { get; }
    }

    public class Category : IClassificationEntity
    {
        public Guid Id { get; set; }
        public int Value { get; set; }
        public string Title { get;set; }
        public int Order { get; set; }
    }

    public class Classification : IClassificationEntity
    {
        public Guid Id { get; set;}
        public string Title { get;set; }
        public ClassificationGroup Group { get; set; }
        public int CategoriesCount { get; set; }

    }

    public class ClassificationGroup : IClassificationEntity
    {
        public Guid Id { get; set;}
        public string Title { get; set;}
    }

    public class MysqlClassificationEntity
    {
        public int Id { get; set; }
        public Guid IdGuid { get; set; }
        public string Title { get; set; }
        public int? Parent { get; set; }
        public Guid? ParentGuid { get; set; }
        public ClassificationEntityType Type { get; set; }
        public int? Value { get; set; }
        public int? Order { get; set; }
    }

    public class ClassificationEntity : IClassificationEntity
    {
        public virtual Guid Id { get; set; }
        public virtual string Title { get; set; }
        public virtual Guid? Parent { get; set; }
        public virtual ClassificationEntityType Type { get; set; }
        public virtual int? Value { get; set; }
        public virtual int? Index { get; set; }
    }

    public enum ClassificationEntityType
    {
        Group = 1,
        Classification = 2,
        Category = 3
    }

    public class ClassificationsStorage : IClassificationsStorage
    {
        private readonly IPlainStorageAccessor<ClassificationEntity> classificationsStorage;

        public ClassificationsStorage(IPlainStorageAccessor<ClassificationEntity> classificationsStorage)
        {
            this.classificationsStorage = classificationsStorage;
        }

        public Task<IEnumerable<ClassificationGroup>> GetClassificationGroups()
        {
            var dbEntities = classificationsStorage.Query(_ => _.Where(x => x.Type == ClassificationEntityType.Group).ToList());

            var groups = dbEntities.Select(x => new ClassificationGroup
            {
                Id = x.Id,
                Title = x.Title
            });

            return Task.FromResult(groups);
        }

        public Task<IEnumerable<Classification>> GetClassifications()
        {
            var dbEntities = classificationsStorage.Query(_ => _.Where(x => x.Type == ClassificationEntityType.Classification).ToList());

            var classifications = dbEntities.Select(x => new Classification
            {
                Id = x.Id,
                Title = x.Title
            });
            return Task.FromResult(classifications);
        }

        public async Task<ClassificationsSearchResult> SearchAsync(string query, Guid? groupId)
        {
            var dbEntities = classificationsStorage.Query(_ =>
            {
                var searchQuery = ApplySearchFilter(_, query, groupId);

                var ids = searchQuery.Select(x => x.Type == ClassificationEntityType.Classification ? x.Id : x.Parent)
                    .GroupBy(x => x)
                    .Select(x => x.Key)
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
                    CategoriesCount = categoriesCounts.ContainsKey(x.Id) ? categoriesCounts[x.Id] : 0
                }).ToList();

            var total = classificationsStorage.Query(_ => ApplySearchFilter(_, query, groupId)
                .Select(x => x.Type == ClassificationEntityType.Classification ? x.Id : x.Parent)
                .GroupBy(x => x)
                .Count());

            return await Task.FromResult(new ClassificationsSearchResult
            {
                Classifications = classifications,  
                Total = total
            });
        }

        private IQueryable<ClassificationEntity> ApplySearchFilter(IQueryable<ClassificationEntity> entities, 
            string query, Guid? groupId)
        {
            var searchQuery = entities;
            var lowercaseQuery = (query?? string.Empty).ToLower().Trim();
            if (!string.IsNullOrWhiteSpace(lowercaseQuery))
            {
                searchQuery = entities.Where(x => x.Title.ToLower().Contains(lowercaseQuery));
            }

            if (groupId.HasValue)
            {
                searchQuery = searchQuery.Where(x => x.Parent == groupId);
            }
            return searchQuery;
        }

        public void Store(ClassificationEntity[] classifications)
        {
            classificationsStorage.Store(entities: classifications.Select(x =>
                new Tuple<ClassificationEntity, object>(x, x.Id)));
        }

        public Task<List<Category>> GetCategories(Guid classificationId)
        {
            var dbEntities = classificationsStorage.Query(_ => _.Where(x => x.Parent == classificationId).Take(AbstractVerifier.MaxOptionLength).ToList());

            var categories = dbEntities.Select(x => new Category
                {
                    Id = x.Id,
                    Value = x.Value ?? 0,
                    Title = x.Title,
                    Order = x.Index ?? 0
                })
                .OrderBy(x => x.Order).ThenBy(x => x.Value)
                .ToList();

            return Task.FromResult(categories);
        }
    }

    public interface IClassificationsStorage
    {
        Task<IEnumerable<ClassificationGroup>> GetClassificationGroups();
        Task<IEnumerable<Classification>> GetClassifications();
        Task<ClassificationsSearchResult> SearchAsync(string query, Guid? groupId);
        void Store(ClassificationEntity[] bdEntities);
        Task<List<Category>> GetCategories(Guid classificationId);
    }

    public class ClassificationsSearchResult
    {
        public List<Classification> Classifications { get; set; }
        public int Total { get; set; }
    }
}
