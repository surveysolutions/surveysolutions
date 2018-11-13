using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.Infrastructure.PlainStorage;

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
            return await Task.FromResult(new ClassificationsSearchResult());
        }

        public void Store(ClassificationEntity[] classifications)
        {
            classificationsStorage.Store(entities: classifications.Select(x =>
                new Tuple<ClassificationEntity, object>(x, x.Id)));
        }
    }

    public interface IClassificationsStorage
    {
        Task<IEnumerable<ClassificationGroup>> GetClassificationGroups();
        Task<IEnumerable<Classification>> GetClassifications();
        Task<ClassificationsSearchResult> SearchAsync(string query, Guid? groupId);
        void Store(ClassificationEntity[] bdEntities);
    }

    public class ClassificationsSearchResult
    {
    }
}
