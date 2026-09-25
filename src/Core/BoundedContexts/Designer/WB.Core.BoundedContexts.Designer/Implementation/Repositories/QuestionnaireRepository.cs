using System;
using System.Linq;
using Main.Core.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.Aggregates;
using WB.Core.Infrastructure.PlainStorage;

namespace WB.Core.BoundedContexts.Designer.Implementation.Repositories
{
    internal class QuestionnaireRepository : IPlainAggregateRootRepository<Questionnaire>, IPlainAggregateRootRepository
    {
        private const int AggregateLockClass = 20501;

        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage;
        private readonly IServiceProvider locator;
        private readonly DesignerDbContext dbContext;

        public QuestionnaireRepository(IPlainKeyValueStorage<QuestionnaireDocument> questionnaireStorage,
            IServiceProvider locator,
            DesignerDbContext dbContext)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.locator = locator;
            this.dbContext = dbContext;
        }

        public Questionnaire? Get(Guid aggregateId)
        {
            // Serialize the whole read-modify-write: the in-process aggregate lock releases before the request commits,
            // so without a DB lock held to commit a concurrent writer could read this document and overwrite the edit.
            this.LockQuestionnaireForUpdate(aggregateId);

            var questionnaireDocument = this.questionnaireStorage.GetById(aggregateId.FormatGuid());

            if (questionnaireDocument == null)
                return null;

            var questionnaireListItem = this.dbContext.Questionnaires.Include(x => x.SharedPersons).FirstOrDefault(x => x.QuestionnaireId == aggregateId.FormatGuid());

            var personsCollection = questionnaireListItem?.SharedPersons ?? Enumerable.Empty<SharedPerson>();
            var questionnaire = locator.GetRequiredService<Questionnaire>();
            questionnaire.Initialize(aggregateId, questionnaireDocument, personsCollection);
            return questionnaire;
        }

        public void Save(Questionnaire aggregateRoot)
        {
            var questionnaireDocument = aggregateRoot.QuestionnaireDocument;
            this.questionnaireStorage.Store(questionnaireDocument, questionnaireDocument.PublicKey.FormatGuid());
        }

        public T Get<T>(Guid aggregateId) where T : class, IPlainAggregateRoot
        {
            throw new NotImplementedException();
        }

        public IPlainAggregateRoot? Get(Type aggregateRootType, Guid aggregateId)
        {
            if (aggregateRootType != typeof(Questionnaire))
                throw new InvalidOperationException();
            var questionnaire = this.Get(aggregateId);
            return questionnaire;
        }

        public void Save(IPlainAggregateRoot aggregateRoot)
        {
            this.Save((Questionnaire)aggregateRoot);
        }

        // Transaction-scoped advisory lock keyed by the questionnaire id; no-op outside a Npgsql transaction
        // (read requests and in-memory tests), so it only engages for the ambient write transaction.
        private void LockQuestionnaireForUpdate(Guid aggregateId)
        {
            if (!this.dbContext.Database.IsNpgsql() || this.dbContext.Database.CurrentTransaction == null)
                return;

            var bytes = aggregateId.ToByteArray();
            var key = BitConverter.ToInt32(bytes, 0) ^ BitConverter.ToInt32(bytes, 4)
                    ^ BitConverter.ToInt32(bytes, 8) ^ BitConverter.ToInt32(bytes, 12);

            this.dbContext.Database.ExecuteSqlInterpolated(
                $"SELECT pg_advisory_xact_lock({AggregateLockClass}, {key})");
        }
    }
}

