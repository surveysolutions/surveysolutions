using System;
using System.Collections.Generic;
using System.Linq;
using Sqo.Transactions;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernel.Structures.Synchronization.Designer;
using WB.UI.QuestionnaireTester.Implementation.Services;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    internal class QuestionnaireMetaInfoAccessor : IQueryablePlainStorageAccessor<QuestionnaireMetaInfo>
    {
        private readonly IPrincipal principal;

        public QuestionnaireMetaInfoAccessor(IPrincipal principal)
        {
            this.principal = principal;
        }

        private class QuestionnaireMetaInfoStorageViewModel
        {
            public string UserName { get; set; }
            public QuestionnaireMetaInfo MetaInfo { get; set; }
        }

        public QuestionnaireMetaInfo GetById(string id)
        {
            var storedQuestionnaireMetaInfo = SiaqodbFactory.GetInstance().Query<QuestionnaireMetaInfoStorageViewModel>().SqoFirstOrDefault(_ => _.MetaInfo.Id == id);

            return storedQuestionnaireMetaInfo == null ? null : storedQuestionnaireMetaInfo.MetaInfo;
        }

        public void Remove(string id)
        {
            SiaqodbFactory.GetInstance().DeleteObjectBy<QuestionnaireMetaInfoStorageViewModel>(new Dictionary<string, object>() { { "MetaInfo.Id", id } });
        }

        public void Store(QuestionnaireMetaInfo view, string id)
        {
            this.Store(new[] {new Tuple<QuestionnaireMetaInfo, string>(view, view.Id)});
        }

        public void Store(IEnumerable<Tuple<QuestionnaireMetaInfo, string>> entities)
        {
            var siaqodb = SiaqodbFactory.GetInstance();

            ITransaction transaction = siaqodb.BeginTransaction();
            try
            {
                foreach (var entity in entities)
                {
                    siaqodb.StoreObject(new QuestionnaireMetaInfoStorageViewModel()
                    {
                        MetaInfo = entity.Item1,
                        UserName = this.principal.CurrentIdentity.Name
                    }, transaction);
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

        private IEnumerable<QuestionnaireMetaInfoStorageViewModel> AllEntities()
        {
            return SiaqodbFactory.GetInstance().LoadAllLazy<QuestionnaireMetaInfoStorageViewModel>().Where(_=>_.UserName == this.principal.CurrentIdentity.Name);
        }

        public IEnumerable<QuestionnaireMetaInfo> LoadAll()
        {
            return AllEntities().Select(_=>_.MetaInfo);
        }

        public void RemoveAll()
        {
            var siaqodb = SiaqodbFactory.GetInstance();

            ITransaction transaction = siaqodb.BeginTransaction();
            try
            {
                foreach (var entity in this.AllEntities())
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