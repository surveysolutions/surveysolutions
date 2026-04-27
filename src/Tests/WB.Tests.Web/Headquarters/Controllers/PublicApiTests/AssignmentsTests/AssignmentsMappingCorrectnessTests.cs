using System;
using AutoMapper;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using AbcCreate = WB.Tests.Abc.Create;

namespace WB.Tests.Web.Headquarters.Controllers.PublicApiTests.AssignmentsTests
{
    /// <summary>
    /// Tests that verify the mapping behavior of AssignmentsPublicApiMapProfile.
    /// These tests are written against output values only (not AutoMapper internals),
    /// so they remain valid when AutoMapper is replaced with manual mapping.
    /// </summary>
    [TestOf(typeof(AssignmentsPublicApiMapProfile))]
    public class AssignmentsMappingCorrectnessTests
    {
        private IMapper mapper;
        private IQuestionnaire questionnaire;

        [SetUp]
        public void SetUp()
        {
            questionnaire = AbcCreate.Entity.PlainQuestionnaire(
                AbcCreate.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
                    children: new Main.Core.Entities.Composite.IComposite[]
                    {
                        AbcCreate.Entity.TextQuestion(questionId: Id.g2, variable: "varA", preFilled: true),
                        AbcCreate.Entity.TextQuestion(questionId: Id.g3, variable: "varB", preFilled: true),
                    }));

            mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AssignmentsPublicApiMapProfile>();
            }).CreateMapper();
        }

        // ──────────────────────────────────────────────────────────
        // IdentifyingAnswer → AssignmentIdentifyingDataItem
        // ──────────────────────────────────────────────────────────

        [Test]
        public void when_mapping_IdentifyingAnswer_to_DataItem_variable_already_set_should_keep_existing_variable()
        {
            var assignment = AbcCreate.Entity.Assignment(1, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));
            var item = AbcCreate.Entity.IdentifyingAnswer(assignment, answer: "hello",
                identity: AbcCreate.Identity(Id.g2), variable: "explicit_var");

            var result = mapper.Map<AssignmentIdentifyingDataItem>(item);

            // Explicit variable name must be preserved — not overridden by questionnaire lookup
            Assert.That(result.Variable, Is.EqualTo("explicit_var"));
        }

        [Test]
        public void when_mapping_IdentifyingAnswer_to_DataItem_with_null_variable_and_questionnaire_context_should_resolve_from_questionnaire()
        {
            var assignment = AbcCreate.Entity.Assignment(1, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));
            var item = AbcCreate.Entity.IdentifyingAnswer(assignment, answer: "hello",
                identity: AbcCreate.Identity(Id.g2), variable: null);

            var result = mapper.Map<AssignmentIdentifyingDataItem>(item,
                opts => opts.Items["questionnaire"] = questionnaire);

            Assert.That(result.Variable, Is.EqualTo("varA"));
        }

        [Test]
        public void when_mapping_IdentifyingAnswer_to_DataItem_should_map_answer()
        {
            var assignment = AbcCreate.Entity.Assignment(1, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));
            var item = AbcCreate.Entity.IdentifyingAnswer(assignment, answer: "my answer",
                identity: AbcCreate.Identity(Id.g2), variable: "v");

            var result = mapper.Map<AssignmentIdentifyingDataItem>(item);

            Assert.That(result.Answer, Is.EqualTo("my answer"));
        }

        [Test]
        public void when_mapping_IdentifyingAnswer_to_DataItem_should_map_identity_as_string()
        {
            var assignment = AbcCreate.Entity.Assignment(1, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));
            var identity = AbcCreate.Identity(Id.g2);
            var item = AbcCreate.Entity.IdentifyingAnswer(assignment, answer: "x",
                identity: identity, variable: "v");

            var result = mapper.Map<AssignmentIdentifyingDataItem>(item);

            Assert.That(result.Identity, Is.EqualTo(identity.ToString()));
        }

        // ──────────────────────────────────────────────────────────
        // Assignment → AssignmentDetails
        // ──────────────────────────────────────────────────────────

        [Test]
        public void when_mapping_Assignment_to_AssignmentDetails_should_map_Id()
        {
            var assignment = AbcCreate.Entity.Assignment(7, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));

            var result = mapper.Map<AssignmentDetails>(assignment);

            Assert.That(result.Id, Is.EqualTo(7));
        }

        [Test]
        public void when_mapping_Assignment_to_AssignmentDetails_should_map_QuestionnaireId_as_string()
        {
            var qi = AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 3);
            var assignment = AbcCreate.Entity.Assignment(1, qi);

            var result = mapper.Map<AssignmentDetails>(assignment);

            Assert.That(result.QuestionnaireId, Is.EqualTo(qi.ToString()));
        }

        [Test]
        public void when_mapping_Assignment_to_AssignmentDetails_should_map_Quantity()
        {
            var assignment = AbcCreate.Entity.Assignment(1, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));
            assignment.Quantity = 5;

            var result = mapper.Map<AssignmentDetails>(assignment);

            Assert.That(result.Quantity, Is.EqualTo(5));
        }

        [Test]
        public void when_mapping_Assignment_to_AssignmentDetails_null_quantity_should_remain_null()
        {
            var assignment = AbcCreate.Entity.Assignment(1, AbcCreate.Entity.QuestionnaireIdentity(Id.g1, 1));
            assignment.Quantity = null;

            var result = mapper.Map<AssignmentDetails>(assignment);

            Assert.That(result.Quantity, Is.Null);
        }

        // ──────────────────────────────────────────────────────────
        // AssignmentRow → AssignmentViewItem
        // ──────────────────────────────────────────────────────────

        [Test]
        public void when_mapping_AssignmentRow_to_AssignmentViewItem_should_map_all_scalar_fields()
        {
            var now = DateTime.UtcNow;
            var row = new AssignmentRow
            {
                Id = 42,
                QuestionnaireId = new QuestionnaireIdentity(Id.g1, 2),
                Quantity = 10,
                InterviewsCount = 3,
                ResponsibleId = Id.gA,
                Responsible = "John",
                CreatedAtUtc = now.AddDays(-5),
                UpdatedAtUtc = now.AddDays(-1),
                ReceivedByTabletAtUtc = now.AddDays(-2),
                Archived = false,
                IsAudioRecordingEnabled = true
            };

            var result = mapper.Map<AssignmentViewItem>(row);

            Assert.That(result.Id, Is.EqualTo(42));
            Assert.That(result.Quantity, Is.EqualTo(10));
            Assert.That(result.InterviewsCount, Is.EqualTo(3));
            Assert.That(result.ResponsibleId, Is.EqualTo(Id.gA));
            Assert.That(result.ResponsibleName, Is.EqualTo("John"));
            Assert.That(result.CreatedAtUtc, Is.EqualTo(row.CreatedAtUtc));
            Assert.That(result.UpdatedAtUtc, Is.EqualTo(row.UpdatedAtUtc));
            Assert.That(result.ReceivedByTabletAtUtc, Is.EqualTo(row.ReceivedByTabletAtUtc));
            Assert.That(result.Archived, Is.False);
            Assert.That(result.IsAudioRecordingEnabled, Is.True);
        }
    }
}
