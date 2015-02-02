using System;
using System.Collections.Generic;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.UI.QuestionnaireTester.ViewModels;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    public class DashboardStorageViewModelRepository : IReadSideStorage<DashboardStorageViewModel>
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
    }
}