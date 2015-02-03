using System;
using System.Collections.Generic;
using Sqo.Transactions;
using WB.Core.Infrastructure.PlainStorage;
using WB.UI.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    public class DashboardStorageViewModelRepository : IPlainStorageAccessor<DashboardStorageViewModel>
    {
        public DashboardStorageViewModel GetById(string id)
        {
            return SiaqodbFactory.GetInstance().Query<DashboardStorageViewModel>().SqoFirstOrDefault(_ => _.Id == id);
        }

        public void Remove(string id)
        {
            SiaqodbFactory.GetInstance().DeleteObjectBy<DashboardStorageViewModel>(new Dictionary<string, object>() {{"Id", id}});
        }

        public void Store(DashboardStorageViewModel view, string id)
        {
            view.Id = id;
            SiaqodbFactory.GetInstance().StoreObject(view);
        }

        public void Store(IEnumerable<Tuple<DashboardStorageViewModel, string>> entities)
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