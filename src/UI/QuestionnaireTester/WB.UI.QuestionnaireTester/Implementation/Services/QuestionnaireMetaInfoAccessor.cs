using System;
using System.Collections.Generic;
using System.Linq;
using Sqo.Transactions;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class QuestionnaireMetaInfoAccessor : IQueryablePlainStorageAccessor<QuestionnaireMetaInfo>
    {
        public QuestionnaireMetaInfo GetById(string id)
        {
            return SiaqodbFactory.GetInstance().Query<QuestionnaireMetaInfo>().SqoFirstOrDefault(_ => _.Id == id);
        }

        public void Remove(string id)
        {
            SiaqodbFactory.GetInstance().DeleteObjectBy<QuestionnaireMetaInfo>(new Dictionary<string, object>() { { "Id", id } });
        }

        public void Store(QuestionnaireMetaInfo view, string id)
        {
            SiaqodbFactory.GetInstance().StoreObject(view);
        }

        public void Store(IEnumerable<Tuple<QuestionnaireMetaInfo, string>> entities)
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

        public TResult Query<TResult>(Func<IQueryable<QuestionnaireMetaInfo>, TResult> query)
        {
            throw new NotImplementedException();

        }

        public IEnumerable<QuestionnaireMetaInfo> LoadAll()
        {
            return SiaqodbFactory.GetInstance().LoadAllLazy<QuestionnaireMetaInfo>();
        }

        public void RemoveAll()
        {
            var siaqodb = SiaqodbFactory.GetInstance();

            ITransaction transaction = siaqodb.BeginTransaction();
            try
            {
                foreach (var entity in this.LoadAll())
                {
                    siaqodb.Delete(entity, transaction);
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