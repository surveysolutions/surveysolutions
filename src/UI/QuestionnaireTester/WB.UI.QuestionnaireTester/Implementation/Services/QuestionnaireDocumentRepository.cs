using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Sqo.Attributes;
using Sqo.Transactions;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.UI.QuestionnaireTester.Implementation.Services
{
    public class QuestionnaireDocumentRepository : IPlainStorageAccessor<QuestionnaireDocument>
    {
        private class QuestionnaireStorageViewModel
        {
            public string Id { get; set; }
            [Document]
            public QuestionnaireDocument Questionnaire { get; set; }
        }

        public QuestionnaireDocument GetById(string id)
        {
            var storedQuestionnaire = SiaqodbFactory.GetInstance().Query<QuestionnaireStorageViewModel>().SqoFirstOrDefault(_ => _.Id == id);

            return storedQuestionnaire == null ? null : storedQuestionnaire.Questionnaire;
        }

        public void Remove(string id)
        {
            SiaqodbFactory.GetInstance().DeleteObjectBy<QuestionnaireStorageViewModel>(new Dictionary<string, object>() { { "Id", id } });
        }

        public void Store(QuestionnaireDocument view, string id)
        {
            SiaqodbFactory.GetInstance().StoreObject(new QuestionnaireStorageViewModel{ Id = id, Questionnaire = view});
        }

        public void Store(IEnumerable<Tuple<QuestionnaireDocument, string>> entities)
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