using System;
using System.Collections.Generic;
using Sqo;
using Sqo.Transactions;
using WB.Core.BoundedContexts.QuestionnaireTester.Views;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class SiaqDbRepository<TEntity> : IPlainStorageAccessor<TEntity> where TEntity: Entity
    {
        public TEntity GetById(string id)
        {
            return SiaqodbFactory.GetInstance().Query<TEntity>().FirstOrDefault(_ => _.Id == id);
        }

        public void Remove(string id)
        {
            SiaqodbFactory.GetInstance().DeleteObjectBy<TEntity>(new Dictionary<string, object>() { { "Id", id } });
        }

        public void Store(TEntity view, string id)
        {
            this.Store(new[] {new Tuple<TEntity, string>(view, id),});
        }

        public void Store(IEnumerable<Tuple<TEntity, string>> entities)
        {
            var siaqodb = SiaqodbFactory.GetInstance();

            ITransaction transaction = siaqodb.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    siaqodb.StoreObject(entity, transaction);
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
            }
        }
    }
}