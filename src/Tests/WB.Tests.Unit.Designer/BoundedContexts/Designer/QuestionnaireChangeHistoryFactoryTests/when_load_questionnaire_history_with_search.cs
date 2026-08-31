using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using FluentAssertions;
using Main.Core.Documents;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;
using WB.Core.BoundedContexts.Designer.MembershipProvider.Roles;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Tests.Abc;

namespace WB.Tests.Unit.Designer.BoundedContexts.Designer.QuestionnaireChangeHistoryFactoryTests
{
    internal class when_load_questionnaire_history_with_search : QuestionnaireChangeHistoryFactoryTestContext
    {
        [OneTimeSetUp]
        public void Context()
        {
            var questionnaireDocument = Create.QuestionnaireDocument(children: new[]
            {
                Create.Group(groupId: sectionId, title: "Household members", variable: "hh_members", children: new[]
                {
                    Create.Question(questionId: questionId, variable: "respondent_age", title: "Age of respondent")
                })
            });
            questionnaireDocument.Title = "Current questionnaire title";
            questionnaireDocument.VariableName = "current_questionnaire";

            db = Create.InMemoryDbContext();

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: questionId,
                targetType: QuestionnaireItemType.Question,
                action: QuestionnaireActionType.Update,
                targetTitle: "respondent_age"));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: Guid.NewGuid(),
                targetType: QuestionnaireItemType.Question,
                action: QuestionnaireActionType.Update,
                targetTitle: "othervar"));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: Guid.NewGuid(),
                targetType: QuestionnaireItemType.Question,
                action: QuestionnaireActionType.Update,
                targetTitle: "legacy_var_marker"));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: sectionId,
                targetType: QuestionnaireItemType.Section,
                action: QuestionnaireActionType.Add,
                targetTitle: "Household members",
                reference: new[] { Create.QuestionnaireChangeReference(referenceId: questionId, referenceType: QuestionnaireItemType.Question, referenceTitle: "respondent_age") }));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: sectionId,
                targetType: QuestionnaireItemType.Section,
                action: QuestionnaireActionType.Update,
                targetTitle: "Legacy chapter label"));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: sectionId,
                targetType: QuestionnaireItemType.Section,
                action: QuestionnaireActionType.Clone,
                reference: new[]
                {
                    Create.QuestionnaireChangeReference(referenceId: sourceQuestionnaireId,
                        referenceType: QuestionnaireItemType.Questionnaire, referenceTitle: "Source questionnaire title")
                }));

            db.Add(Create.QuestionnaireChangeRecord(
                questionnaireId: questionnaireId.FormatGuid(),
                targetId: Guid.NewGuid(),
                targetType: QuestionnaireItemType.Question,
                action: QuestionnaireActionType.Update,
                targetTitle: "another_var",
                reference: new[]
                {
                    Create.QuestionnaireChangeReference(referenceId: Guid.NewGuid(), referenceType: QuestionnaireItemType.Question,
                        referenceTitle: "reference_only_marker")
                }));

            db.SaveChanges();

            var userManagerMock = new Mock<IUserManager>();
            userManagerMock
                .Setup(m => m.GetUsersInRoleAsync(SimpleRoleEnum.Administrator))
                .Returns(Task.FromResult((IList<DesignerIdentityUser>)new List<DesignerIdentityUser>()));

            factory = CreateQuestionnaireChangeHistoryFactory(
                db,
                Mock.Of<IPlainKeyValueStorage<QuestionnaireDocument>>(
                    _ => _.GetById(Moq.It.IsAny<string>()) == questionnaireDocument),
                userManager: userManagerMock.Object);
        }

        [Test]
        public async Task should_return_all_records_when_no_search()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user);
            result.ChangeHistory.Count.Should().Be(7);
        }

        [Test]
        public async Task should_filter_by_target_text()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "respondent");
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_filter_by_reference_text()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "reference_only_marker");
            result.ChangeHistory.Count.Should().Be(1);
        }

        [Test]
        public async Task should_filter_by_entity_id_when_requested()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "respondent_age", searchIdsOnly: true);
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_not_match_persisted_title_when_searching_by_entity_id()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "legacy_var_marker", searchIdsOnly: true);
            result.ChangeHistory.Count.Should().Be(0);
        }

        [Test]
        public async Task should_match_persisted_title_in_text_mode()
        {
            // With EF-level filtering on persisted titles, a value that was stored as TargetItemTitle
            // is matched even when it looks like a variable name.
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "respondent_age");
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_return_empty_when_no_match()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "nomatch_xyz");
            result.ChangeHistory.Count.Should().Be(0);
        }

        [Test]
        public async Task should_search_case_insensitively()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "RESPONDENT");
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_match_whole_words_only()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "age", searchWholeWord: true);
            result.ChangeHistory.Count.Should().Be(2);

            result = await factory.LoadAsync(questionnaireId, 1, 20, user, "pond", searchWholeWord: true);
            result.ChangeHistory.Count.Should().Be(0);
        }

        [Test]
        public async Task should_match_current_entity_title_in_text_mode()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "Age of respondent");
            result.ChangeHistory.Count.Should().Be(2);
        }

        [Test]
        public async Task should_match_historical_title_in_whole_word_mode()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "legacy", searchWholeWord: true);
            result.ChangeHistory.Count.Should().Be(1);
        }

        [Test]
        public async Task should_not_match_source_questionnaire_reference_by_current_questionnaire_title()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "Current questionnaire title");
            result.ChangeHistory.Count.Should().Be(0);
        }

        [Test]
        public async Task should_preserve_search_options_on_result()
        {
            var result = await factory.LoadAsync(questionnaireId, 1, 20, user, "respondent", searchIdsOnly: true, searchWholeWord: true);
            result.Search.Should().Be("respondent");
            result.SearchIdsOnly.Should().BeTrue();
            result.SearchWholeWord.Should().BeTrue();
        }

        private QuestionnaireChangeHistoryFactory factory;
        private DesignerDbContext db;
        private readonly Guid questionnaireId = Guid.Parse("22222222222222222222222222222222");
        private readonly Guid questionId = Guid.Parse("33333333333333333333333333333333");
        private readonly Guid sectionId = Guid.Parse("44444444444444444444444444444444");
        private readonly Guid sourceQuestionnaireId = Guid.Parse("55555555555555555555555555555555");

        private readonly ClaimsPrincipal user = new ClaimsPrincipal(new List<ClaimsIdentity>
        {
            new ClaimsIdentity(Mock.Of<IIdentity>(), new[]
            {
                new Claim(ClaimTypes.Role, SimpleRoleEnum.User.ToString()),
                new Claim(ClaimTypes.NameIdentifier, Id.gB.ToString())
            })
        });
    }
}
