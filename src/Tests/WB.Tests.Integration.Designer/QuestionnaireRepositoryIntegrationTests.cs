#nullable enable
using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Implementation.Repositories;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.SharedPersons;
using WB.Core.GenericSubdomains.Portable;
using WB.Infrastructure.Native.Storage;

namespace WB.Tests.Integration.Designer
{
    [TestOf(typeof(QuestionnaireRepository))]
    [NonParallelizable]
    internal class QuestionnaireRepositoryIntegrationTests : IntegrationTest
    {
        [Test]
        public void when_share_is_revoked_while_waiting_for_the_lock_aggregate_and_list_item_see_the_revocation()
        {
            var dbContext = ServiceLocator.GetInstance<DesignerDbContext>();
            var questionnaireId = Guid.NewGuid();
            var revokedEditorId = Guid.NewGuid();
            SeedQuestionnaire(dbContext, questionnaireId, revokedEditorId);

            using var transaction = dbContext.Database.BeginTransaction();
            // What QuestionnairePermissionsAttribute does before the command reaches the repository.
            _ = dbContext.Questionnaires.Include(x => x.SharedPersons)
                .First(x => x.QuestionnaireId == questionnaireId.FormatGuid());

            RevokeShare(dbContext, questionnaireId, revokedEditorId);

            var questionnaire = CreateRepository(dbContext).Get(questionnaireId);

            Assert.That(questionnaire!.SharedPersons.Select(p => p.UserId), Does.Not.Contain(revokedEditorId));
            // ListViewPostProcessor resolves the list item through Find, so it must see the same committed state.
            var listItem = dbContext.Questionnaires.Find(questionnaireId.FormatGuid());
            Assert.That(listItem!.SharedPersons.Select(p => p.UserId), Does.Not.Contain(revokedEditorId));
        }

        private static QuestionnaireRepository CreateRepository(DesignerDbContext dbContext)
        {
            var memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            var evictionTokens = new KeyValueCacheEvictionTokens();
            var storage = new DesignerKeyValueStorage<QuestionnaireDocument>(dbContext, memoryCache,
                new EntitySerializer<QuestionnaireDocument>(),
                new TransactionalMemoryCacheInvalidation(memoryCache, evictionTokens), evictionTokens);

            var services = new ServiceCollection()
                .AddTransient(_ => new Questionnaire(null!, null!, null!, null!, null!, null!, null!, null!, null!))
                .BuildServiceProvider();

            return new QuestionnaireRepository(storage, services, dbContext);
        }

        private static void SeedQuestionnaire(DesignerDbContext dbContext, Guid questionnaireId, Guid editorId)
        {
            using var fresh = FreshContext(dbContext);

            var document = new QuestionnaireDocument { PublicKey = questionnaireId, Title = "questionnaire" };
            fresh.QuestionnaireDocuments.Add(new StoredQuestionnaireDocument
            {
                Id = questionnaireId.FormatGuid(),
                Value = new EntitySerializer<QuestionnaireDocument>().Serialize(document)
            });

            var listItem = Create.Questionnaire.ListViewItem(questionnaireId, "questionnaire");
            listItem.SharedPersons.Add(new SharedPerson
            {
                QuestionnaireId = questionnaireId.FormatGuid(),
                UserId = editorId,
                Email = "editor@example.com",
                ShareType = ShareType.Edit
            });
            fresh.Questionnaires.Add(listItem);

            fresh.SaveChanges();
        }

        private static void RevokeShare(DesignerDbContext dbContext, Guid questionnaireId, Guid editorId)
        {
            using var fresh = FreshContext(dbContext);
            var key = questionnaireId.FormatGuid();
            fresh.SharedPersons.RemoveRange(fresh.SharedPersons.Where(p => p.QuestionnaireId == key && p.UserId == editorId));
            fresh.SaveChanges();
        }

        private static DesignerDbContext FreshContext(DesignerDbContext dbContext)
            => new(new DbContextOptionsBuilder<DesignerDbContext>()
                .UseNpgsql(dbContext.Database.GetConnectionString())
                .Options);
    }
}
