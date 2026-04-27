using System;
using System.Collections.Generic;
using AutoMapper;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Unit.Applications.Headquarters.PublicApiTests.AssignmentsTests
{
    /// <summary>
    /// Tests for AssignmentsPublicApiMapProfile and AssignmentProfile mappings.
    /// All assertions are on output values (not AutoMapper internals), so they stay valid
    /// when AutoMapper is replaced with manual mapping — only the mapper setup changes.
    /// </summary>
    [TestOf(typeof(AssignmentsPublicApiMapProfile))]
    public class AssignmentsMappingTests
    {
        private IMapper _mapper;
        private IQuestionnaire _questionnaire;
        private Mock<IQuestionnaireStorage> _questionnaireMock;

        [OneTimeSetUp]
        public void SetUp()
        {
            _questionnaire = Create.Entity.PlainQuestionnaire(
                Create.Entity.QuestionnaireDocumentWithOneChapter(Id.g1, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: Id.g2, variable: "var2", preFilled: true),
                    Create.Entity.TextQuestion(questionId: Id.g3, variable: "var3", preFilled: true),
                    Create.Entity.NumericIntegerQuestion(id: Id.g4, variable: "var4", isPrefilled: true)
                }));

            _questionnaireMock = new Mock<IQuestionnaireStorage>();
            _questionnaireMock
                .Setup(s => s.GetQuestionnaire(It.IsAny<WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionnaireIdentity>(), It.IsAny<string>()))
                .Returns(_questionnaire);

            _mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<AssignmentsPublicApiMapProfile>();
                cfg.AddProfile<AssignmentProfile>();
                cfg.ConstructServicesUsing(t =>
                    t == typeof(IQuestionnaireStorage) ? _questionnaireMock.Object : null);
            }).CreateMapper();
        }

        // ── Assignment → AssignmentDetails ──────────────────────────────────────


        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_InterviewsCount_from_summaries()
        {
            var assignment = Create.Entity.Assignment(1,
                interviewSummary: new HashSet<InterviewSummary>
                {
                    new InterviewSummary { SummaryId = "a" },
                    new InterviewSummary { SummaryId = "b" },
                    new InterviewSummary { SummaryId = "c" }
                });
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.InterviewsCount, Is.EqualTo(3));
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_zero_interviews_when_none()
        {
            var assignment = Create.Entity.Assignment(1);
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.InterviewsCount, Is.EqualTo(0));
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_ResponsibleName()
        {
            var assignment = Create.Entity.Assignment(1, responsibleName: "John Doe");
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.ResponsibleName, Is.EqualTo("John Doe"));
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_ResponsibleId()
        {
            var responsibleId = Guid.NewGuid();
            var assignment = Create.Entity.Assignment(1, responsibleId: responsibleId);
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.ResponsibleId, Is.EqualTo(responsibleId));
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_Archived()
        {
            var assignment = Create.Entity.Assignment(1);
            assignment.Archived = true;
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.Archived, Is.True);
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_IsAudioRecordingEnabled()
        {
            var assignment = Create.Entity.Assignment(1, isAudioRecordingEnabled: true);
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.IsAudioRecordingEnabled, Is.True);
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_IsAudioRecording_false_by_default()
        {
            var assignment = Create.Entity.Assignment(1);
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.IsAudioRecordingEnabled, Is.False);
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_CreatedAtUtc()
        {
            var created = new DateTime(2024, 1, 15, 12, 0, 0, DateTimeKind.Utc);
            var assignment = Create.Entity.Assignment(1);
            assignment.CreatedAtUtc = created;
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.CreatedAtUtc, Is.EqualTo(created));
        }

        // ── Assignment → FullAssignmentDetails ──────────────────────────────────

        [Test]
        public void Map_Assignment_to_FullAssignmentDetails_maps_Id()
        {
            var assignment = Create.Entity.Assignment(7);
            var result = _mapper.Map<FullAssignmentDetails>(assignment);
            Assert.That(result.Id, Is.EqualTo(7));
        }

        [Test]
        public void Map_Assignment_to_FullAssignmentDetails_maps_Quantity()
        {
            var assignment = Create.Entity.Assignment(1, quantity: 25);
            var result = _mapper.Map<FullAssignmentDetails>(assignment);
            Assert.That(result.Quantity, Is.EqualTo(25));
        }

        [Test]
        public void Map_Assignment_to_FullAssignmentDetails_maps_QuestionnaireId_as_string()
        {
            var qid = Create.Entity.QuestionnaireIdentity(Id.g1, 3);
            var assignment = Create.Entity.Assignment(1, qid);
            var result = _mapper.Map<FullAssignmentDetails>(assignment);
            Assert.That(result.QuestionnaireId, Is.EqualTo(qid.ToString()));
        }

        [Test]
        public void Map_Assignment_to_FullAssignmentDetails_maps_InterviewsCount()
        {
            var assignment = Create.Entity.Assignment(1,
                interviewSummary: new HashSet<InterviewSummary>
                {
                    new InterviewSummary { SummaryId = "a" },
                    new InterviewSummary { SummaryId = "b" }
                });
            var result = _mapper.Map<FullAssignmentDetails>(assignment);
            Assert.That(result.InterviewsCount, Is.EqualTo(2));
        }

        [Test]
        public void Map_Assignment_to_FullAssignmentDetails_maps_IsAudioRecordingEnabled()
        {
            var assignment = Create.Entity.Assignment(1, isAudioRecordingEnabled: true);
            var result = _mapper.Map<FullAssignmentDetails>(assignment);
            Assert.That(result.IsAudioRecordingEnabled, Is.True);
        }

        [Test]
        public void Map_Assignment_to_FullAssignmentDetails_maps_ResponsibleName()
        {
            var assignment = Create.Entity.Assignment(1, responsibleName: "Jane Smith");
            var result = _mapper.Map<FullAssignmentDetails>(assignment);
            Assert.That(result.ResponsibleName, Is.EqualTo("Jane Smith"));
        }


        // ── AssignmentRow → AssignmentViewItem ───────────────────────────────────


        [Test]
        public void Map_AssignmentRow_to_AssignmentViewItem_maps_QuestionnaireId()
        {
            var qid = Create.Entity.QuestionnaireIdentity(Id.g1, 2);
            var row = new AssignmentRow { Id = 1, QuestionnaireId = qid };
            var result = _mapper.Map<AssignmentViewItem>(row);
            Assert.That(result.QuestionnaireId, Is.EqualTo(qid.ToString()));
        }

        [Test]
        public void Map_AssignmentRow_null_ReceivedByTablet_is_preserved()
        {
            var row = new AssignmentRow { Id = 1, ReceivedByTabletAtUtc = null };
            var result = _mapper.Map<AssignmentViewItem>(row);
            Assert.That(result.ReceivedByTabletAtUtc, Is.Null);
        }

        // ── AssignmentProfile: Assignment → AssignmentApiDocument ───────────────

        [Test]
        public void Map_Assignment_to_AssignmentApiDocument_maps_Id()
        {
            var assignment = Create.Entity.Assignment(99);
            var result = _mapper.Map<WB.Core.SharedKernels.DataCollection.WebApi.AssignmentApiDocument>(assignment);
            Assert.That(result.Id, Is.EqualTo(99));
        }

        [Test]
        public void Map_Assignment_to_AssignmentApiDocument_maps_Quantity()
        {
            var assignment = Create.Entity.Assignment(1, quantity: 15);
            var result = _mapper.Map<WB.Core.SharedKernels.DataCollection.WebApi.AssignmentApiDocument>(assignment);
            Assert.That(result.Quantity, Is.EqualTo(15));
        }

        [Test]
        public void Map_Assignment_to_AssignmentApiDocument_maps_QuestionnaireId()
        {
            var qid = Create.Entity.QuestionnaireIdentity(Id.g1, 7);
            var assignment = Create.Entity.Assignment(1, qid);
            var result = _mapper.Map<WB.Core.SharedKernels.DataCollection.WebApi.AssignmentApiDocument>(assignment);
            Assert.That(result.QuestionnaireId.QuestionnaireId, Is.EqualTo(qid.QuestionnaireId));
            Assert.That(result.QuestionnaireId.Version, Is.EqualTo(qid.Version));
        }

        // ── AssignmentDetails with IdentifyingData list ──────────────────────────

        [Test]
        public void Map_Assignment_to_AssignmentDetails_maps_IdentifyingData_collection()
        {
            var assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.g1, 1),
                responsibleName: "Joe");
            assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(assignment, identity: Create.Identity(Id.g2),
                    answer: "A2", variable: "var2"),
                Create.Entity.IdentifyingAnswer(assignment, identity: Create.Identity(Id.g3),
                    answer: "A3", variable: "var3"),
            };
            var result = _mapper.Map<AssignmentDetails>(assignment);

            Assert.That(result.IdentifyingData, Has.Exactly(2).Items);
            Assert.That(result.IdentifyingData[0].Answer, Is.EqualTo("A2"));
            Assert.That(result.IdentifyingData[1].Answer, Is.EqualTo("A3"));
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_with_empty_IdentifyingData_yields_empty_list()
        {
            var assignment = Create.Entity.Assignment(1);
            assignment.IdentifyingData = new List<IdentifyingAnswer>();
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.IdentifyingData, Is.Empty);
        }

        [Test]
        public void Map_Assignment_to_AssignmentDetails_IdentifyingData_identity_is_string()
        {
            var assignment = Create.Entity.Assignment(1, Create.Entity.QuestionnaireIdentity(Id.g1, 1));
            var identity = Create.Identity(Id.g2);
            assignment.IdentifyingData = new List<IdentifyingAnswer>
            {
                Create.Entity.IdentifyingAnswer(assignment, identity: identity, variable: "var2")
            };
            var result = _mapper.Map<AssignmentDetails>(assignment);
            Assert.That(result.IdentifyingData[0].Identity, Is.EqualTo(identity.ToString()));
        }
    }
}
