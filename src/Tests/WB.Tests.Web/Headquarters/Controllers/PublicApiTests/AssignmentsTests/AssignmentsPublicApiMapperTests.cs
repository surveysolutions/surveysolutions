using System;
using System.Collections.Generic;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    [TestOf(typeof(AssignmentsPublicApiMapper))]
    public class AssignmentsPublicApiMapperTests
    {
        private IQuestionnaire Questionnaire { get; } = Create.Entity.PlainQuestionnaire(
            Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
                Create.Entity.TextQuestion(questionId: Id.g2, variable: "text_var", preFilled: true),
                Create.Entity.TextQuestion(questionId: Id.g3, variable: "text_var2", preFilled: true)
            ));

        [Test]
        public void ToAssignmentDetails_should_map_Id()
        {
            var assignment = Create.Entity.Assignment(42);
            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);
            Assert.That(result.Id, Is.EqualTo(42));
        }

        [Test]
        public void ToAssignmentDetails_should_map_Quantity()
        {
            var assignment = Create.Entity.Assignment(1, quantity: 5);
            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);
            Assert.That(result.Quantity, Is.EqualTo(5));
        }

        [Test]
        public void ToAssignmentDetails_should_map_QuestionnaireId()
        {
            var qid = Create.Entity.QuestionnaireIdentity(Id.g1, 3);
            var assignment = Create.Entity.Assignment(1, qid);
            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);
            Assert.That(result.QuestionnaireId, Is.EqualTo(qid.ToString()));
        }

        [Test]
        public void ToAssignmentDetails_should_map_ResponsibleName()
        {
            var assignment = Create.Entity.Assignment(1, responsibleName: "interviewer1");
            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);
            Assert.That(result.ResponsibleName, Is.EqualTo("interviewer1"));
        }

        [Test]
        public void ToAssignmentDetails_should_map_IdentifyingData_Answer()
        {
            var assignment = Create.Entity.Assignment(1);
            assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(assignment, answer: "answer_value",
                    identity: Create.Identity(Id.g2), variable: "text_var")
            };

            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);

            Assert.That(result.IdentifyingData[0].Answer, Is.EqualTo("answer_value"));
        }

        [Test]
        public void ToAssignmentDetails_should_map_IdentifyingData_Identity_as_full_identity_string()
        {
            var identity = Create.Identity(Id.g2);
            var assignment = Create.Entity.Assignment(1);
            assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(assignment, answer: "val",
                    identity: identity, variable: "v")
            };

            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);

            Assert.That(result.IdentifyingData[0].Identity, Is.EqualTo(identity.ToString()));
        }

        [Test]
        public void ToAssignmentDetails_should_use_variable_from_IdentifyingAnswer_when_provided()
        {
            var assignment = Create.Entity.Assignment(1);
            assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(assignment, answer: "val",
                    identity: Create.Identity(Id.g2), variable: "text_var")
            };

            var result = AssignmentsPublicApiMapper.ToAssignmentDetails(assignment);

            Assert.That(result.IdentifyingData[0].Variable, Is.EqualTo("text_var"));
        }

        [Test]
        public void ToAssignmentDetails_should_resolve_variable_from_questionnaire_when_variable_missing()
        {
            var assignment = Create.Entity.Assignment(1);
            assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                // no variable name — should fallback to questionnaire lookup
                Create.Entity.IdentifyingAnswer(assignment, answer: "val",
                    identity: Create.Identity(Id.g2), variable: null)
            };

            var result = AssignmentsPublicApiMapper.ToFullAssignmentDetails(assignment, Questionnaire);

            Assert.That(result.IdentifyingData[0].Variable, Is.EqualTo("text_var"));
        }

        [Test]
        public void ToAssignmentIdentifyingDataItem_from_QuestionRow_should_extract_id_from_roster_vector_identity()
        {
            // Identity has roster vector (e.g. {guid}_0) — must use .Id not .ToString() for variable lookup
            var identityWithRosterVector = Create.Identity(Id.g2, 0);
            var row = new AssignmentIdentifyingQuestionRow(
                title: "Question",
                answer: "42",
                identity: identityWithRosterVector,
                variable: string.Empty);

            var questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1,
                    Create.Entity.TextQuestion(questionId: Id.g2, variable: "my_var")));

            // Must not throw FormatException from Guid.Parse(identity.ToString())
            Assert.DoesNotThrow(() =>
            {
                var result = AssignmentsPublicApiMapper.ToAssignmentIdentifyingDataItem(row, questionnaire);
                Assert.That(result.Variable, Is.EqualTo("my_var"));
            });
        }

        [Test]
        public void ToAssignmentIdentifyingDataItem_from_QuestionRow_should_preserve_full_identity_string()
        {
            var identityWithRosterVector = Create.Identity(Id.g2, 0);
            var row = new AssignmentIdentifyingQuestionRow(
                title: "Question",
                answer: "42",
                identity: identityWithRosterVector,
                variable: "my_var");

            var result = AssignmentsPublicApiMapper.ToAssignmentIdentifyingDataItem(row, null);

            // The Identity field should contain the full identity string (including roster vector)
            Assert.That(result.Identity, Is.EqualTo(identityWithRosterVector.ToString()));
        }

        [Test]
        public void ToAssignmentViewItem_should_map_all_fields_from_AssignmentRow()
        {
            var row = new AssignmentRow
            {
                Id = 5,
                QuestionnaireId = Create.Entity.QuestionnaireIdentity(Id.g1, 1),
                Quantity = 10,
                InterviewsCount = 3,
                ResponsibleId = Id.g2,
                Responsible = "user1",
                Archived = false,
                IsAudioRecordingEnabled = true
            };

            var result = AssignmentsPublicApiMapper.ToAssignmentViewItem(row);

            Assert.That(result.Id, Is.EqualTo(5));
            Assert.That(result.Quantity, Is.EqualTo(10));
            Assert.That(result.InterviewsCount, Is.EqualTo(3));
            Assert.That(result.ResponsibleId, Is.EqualTo(Id.g2));
            Assert.That(result.ResponsibleName, Is.EqualTo("user1"));
            Assert.That(result.Archived, Is.False);
            Assert.That(result.IsAudioRecordingEnabled, Is.True);
        }
    }
}
