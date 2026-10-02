using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.GenericSubdomains.Portable;
using WB.UI.Designer.Code;

namespace WB.Tests.Unit.Designer.Code
{
    [TestFixture]
    [TestOf(typeof(HistoryExtensions))]
    public class HistoryExtensionsTests
    {
        private static readonly Guid QuestionnaireId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid ChapterId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private static readonly Guid ItemId = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        private readonly Mock<IHtmlHelper> helper = new Mock<IHtmlHelper>();
        private readonly Mock<IUrlHelper> urlHelper = new Mock<IUrlHelper>();

        [SetUp]
        public void SetUp()
        {
            helper.Setup(h => h.Encode(It.IsAny<string>())).Returns<string>(s => s);
            urlHelper.Setup(u => u.Content(It.IsAny<string>())).Returns<string>(s => s);
        }

        private static QuestionnaireChangeHistoricalRecord Record(
            QuestionnaireActionType action,
            QuestionnaireItemType type,
            string title = "Title",
            Guid? parentId = null,
            string newTitle = null,
            int? affected = null,
            DateTime? targetDate = null,
            string hqVersion = null,
            long? hqQuestionnaireVersion = null,
            List<QuestionnaireChangeHistoricalRecordReference> references = null)
            => new QuestionnaireChangeHistoricalRecord("1", "user", DateTime.UtcNow, action, ItemId, parentId, title,
                type, newTitle, affected, false, targetDate,
                references ?? new List<QuestionnaireChangeHistoricalRecordReference>(),
                null, hqVersion, hqQuestionnaireVersion, false);

        private string Format(QuestionnaireChangeHistoricalRecord record)
            => helper.Object.FormatQuestionnaireHistoricalRecord(urlHelper.Object, QuestionnaireId, record).Value;

        [Test]
        public void Revert_with_date_should_include_title_and_date()
        {
            var date = new DateTime(2020, 1, 2, 3, 4, 5);

            var text = Format(Record(QuestionnaireActionType.Revert, QuestionnaireItemType.Questionnaire, "Q1", targetDate: date));

            text.Should().Contain("Q1").And.Contain(date.ToString("s"));
        }

        [Test]
        public void Revert_without_date_should_include_only_title()
        {
            var text = Format(Record(QuestionnaireActionType.Revert, QuestionnaireItemType.Questionnaire, "Q1"));

            text.Should().Contain("Q1").And.NotContain("version from");
        }

        [Test]
        public void ReplaceAllTexts_should_include_titles_and_count()
        {
            var text = Format(Record(QuestionnaireActionType.ReplaceAllTexts, QuestionnaireItemType.Questionnaire,
                "old", newTitle: "new", affected: 7));

            text.Should().Contain("old").And.Contain("new").And.Contain("7");
        }

        [Test]
        public void MigrateToNewVersion_should_return_cover_page_message()
        {
            Format(Record(QuestionnaireActionType.MigrateToNewVersion, QuestionnaireItemType.Questionnaire))
                .Should().Be("Cover page generated");
        }

        [Test]
        public void ImportToHq_should_strip_domain_and_add_versions()
        {
            var text = Format(Record(QuestionnaireActionType.ImportToHq, QuestionnaireItemType.Questionnaire,
                title: "demo.mysurvey.solutions", hqVersion: "22.1", hqQuestionnaireVersion: 5));

            text.Should().Contain("demo v22.1").And.Contain("ver. 5").And.NotContain("mysurvey.solutions");
        }

        [Test]
        public void Mark_for_non_translation_should_return_empty()
        {
            Format(Record(QuestionnaireActionType.Mark, QuestionnaireItemType.Question)).Should().BeEmpty();
        }

        [Test]
        public void Add_question_should_link_to_item_inside_chapter()
        {
            var text = Format(Record(QuestionnaireActionType.Add, QuestionnaireItemType.Question, "Age", parentId: ChapterId));

            text.Should().Contain($"~/q/details/{QuestionnaireId.FormatGuid()}/chapter/{ChapterId.FormatGuid()}/question/{ItemId.FormatGuid()}");
            text.Should().Contain("\"Age\"");
        }

        [Test]
        public void Add_section_should_use_group_navigation_type()
        {
            var text = Format(Record(QuestionnaireActionType.Add, QuestionnaireItemType.Section, "S", parentId: ChapterId));

            text.Should().Contain("/group/");
        }

        [Test]
        public void Add_without_chapter_should_not_render_link()
        {
            var text = Format(Record(QuestionnaireActionType.Add, QuestionnaireItemType.Question, "Age"));

            text.Should().NotContain("<a ").And.Contain("\"Age\"");
        }

        [Test]
        public void Add_with_empty_title_should_show_no_title_placeholder()
        {
            var text = Format(Record(QuestionnaireActionType.Add, QuestionnaireItemType.Question, title: ""));

            text.Should().Contain("<<").And.Contain(">>");
        }

        [Test]
        public void Add_questionnaire_should_link_to_questionnaire_details()
        {
            var text = Format(Record(QuestionnaireActionType.Add, QuestionnaireItemType.Questionnaire, "Q"));

            text.Should().Contain($"~/q/details/{ItemId.FormatGuid()}'");
        }

        [Test]
        public void Title_should_have_empty_macro_and_lookup_markers_removed()
        {
            var text = Format(Record(QuestionnaireActionType.Add, QuestionnaireItemType.Macro, "Empty macro added"));

            text.Should().NotContain("Empty macro added");
        }

        [Test]
        public void Move_question_should_reference_target_section()
        {
            var target = new QuestionnaireChangeHistoricalRecordReference(ChapterId, null, "Target", QuestionnaireItemType.Section, true);

            var text = Format(Record(QuestionnaireActionType.Move, QuestionnaireItemType.Question, "Q", parentId: ChapterId,
                references: new List<QuestionnaireChangeHistoricalRecordReference> { target }));

            text.Should().Contain("Target");
        }

        [Test]
        public void Clone_question_without_reference_should_not_throw()
        {
            Action act = () => Format(Record(QuestionnaireActionType.Clone, QuestionnaireItemType.Question, "Q", parentId: ChapterId));

            act.Should().NotThrow();
        }
    }
}

