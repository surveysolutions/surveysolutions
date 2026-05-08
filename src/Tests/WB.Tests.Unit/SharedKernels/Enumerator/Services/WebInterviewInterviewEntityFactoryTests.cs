using System;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.PublicApi.Models;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services
{
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public class WebInterviewInterviewEntityFactoryTests
    {
        [Test]
        public void When_section_disabled_and_hide_if_disabled_Then_view_model_of_that_section_should_not_be_visible()
        {
            //arrange
            var section1Id = Id.g4;
            var disabledSectionId = Id.g2;
            var section3Id = Id.g3;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithHideIfDisabled(hideIfDisabled: false, children: new IComposite[]
            {
                Create.Entity.Group(section1Id),
                Create.Entity.Group(disabledSectionId, hideIfDisabled: true, title: "disabled group"),
                Create.Entity.Group(section3Id)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.GroupsDisabled(disabledSectionId, RosterVector.Empty));

            var factory = this.CreateWebInterviewInterviewEntityFactory();
            
            //act
            var sidebar = factory.GetSidebarChildSectionsOf(null, interview, questionnaire, new[] {"null"}, false);
            
            //assert
            Assert.That(sidebar.Groups, Has.Exactly(2).Items);
            Assert.That(sidebar.Groups.Find(x=>x.Title == "disabled group"), Is.Null);
        }
        
        [Test]
        public void When_section_disabled_and_set_hide_if_disabled_for_questionnaire_Then_view_model_of_that_section_should_not_be_visible()
        {
            //arrange
            var section1Id = Id.g4;
            var disabledSectionId = Id.g2;
            var section3Id = Id.g3;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithHideIfDisabled(hideIfDisabled: true, children: new IComposite[]
            {
                Create.Entity.Group(section1Id),
                Create.Entity.Group(disabledSectionId, hideIfDisabled: false, title: "disabled group"),
                Create.Entity.Group(section3Id)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.GroupsDisabled(disabledSectionId, RosterVector.Empty));

            var factory = this.CreateWebInterviewInterviewEntityFactory();
            
            //act
            var sidebar = factory.GetSidebarChildSectionsOf(null, interview, questionnaire, new[] {"null"}, false);
            //assert
            Assert.That(sidebar.Groups, Has.Exactly(2).Items);
            Assert.That(sidebar.Groups.Find(x=>x.Title == "disabled group"), Is.Null);
        }
        
        [Test]
        public void When_section_disabled_and_hide_if_disabled_for_questionnaire_is_not_set_Then_view_model_of_that_section_should_be_visible()
        {
            //arrange
            var section1Id = Id.g4;
            var disabledSectionId = Id.g2;
            var section3Id = Id.g3;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithHideIfDisabled(hideIfDisabled: false, children: new IComposite[]
            {
                Create.Entity.Group(section1Id),
                Create.Entity.Group(disabledSectionId, title: "disabled group"),
                Create.Entity.Group(section3Id)
            });
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.GroupsDisabled(disabledSectionId, RosterVector.Empty));

            var factory = this.CreateWebInterviewInterviewEntityFactory();
            
            //act
            var sidebar = factory.GetSidebarChildSectionsOf(null, interview, questionnaire, new[] {"null"}, false);
            //assert
            Assert.That(sidebar.Groups, Has.Exactly(3).Items);
            Assert.That(sidebar.Groups.Find(x=>x.Title == "disabled group"), Is.Not.Null);
            Assert.That(sidebar.Groups.FindIndex(x => x.Title == "disabled group"), Is.EqualTo(1));
        }

        [Test]
        public void When_new_questionnaire_with_new_cover_without_questions_then_view_model_of_that_section_should_skipped()
        {
            //arrange
            var coverId = Id.g1;
            var sectionId = Id.g2;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithHideIfDisabled(hideIfDisabled: false, children: new IComposite[]
            {
                Create.Entity.Group(coverId),
                Create.Entity.Group(sectionId)
            });
            questionnaireDocument.CoverPageSectionId = coverId;
            
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            
            //act
            var sidebar = factory.GetSidebarChildSectionsOf(null, interview, questionnaire, new[] {"null"}, false);

            //assert
            Assert.That(sidebar.Groups, Has.Exactly(1).Items);
            Assert.That(sidebar.Groups.Find(x=>x.Id == questionnaireDocument.CoverPageSectionId.FormatGuid()), Is.Null);
        }

        [Test]
        public void When_new_questionnaire_with_new_cover_with_static_text_then_section_should_be_visible()
        {
            //arrange
            var coverId = Id.g1;
            var sectionId = Id.g2;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithHideIfDisabled(hideIfDisabled: false, children: new IComposite[]
            {
                Create.Entity.Group(coverId, children: new IComposite[]
                {
                    Create.Entity.StaticText()
                }),
                Create.Entity.Group(sectionId)
            });
            questionnaireDocument.CoverPageSectionId = coverId;
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);

            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            
            //act
            var sidebar = factory.GetSidebarChildSectionsOf(null, interview, questionnaire, new[] {"null"}, false);

            //assert
            Assert.That(sidebar.Groups, Has.Exactly(2).Items);
            Assert.That(sidebar.Groups.Find(x=>x.Id == questionnaireDocument.CoverPageSectionId.FormatGuid()), Is.Not.Null);
        }

        #region ApplyValidity tests

        [TestCase(GroupStatus.StartedInvalid, false)]
        [TestCase(GroupStatus.CompletedInvalid, false)]
        [TestCase(GroupStatus.Completed, true)]
        [TestCase(GroupStatus.Started, true)]
        [TestCase(GroupStatus.NotStarted, true)]
        [TestCase(GroupStatus.Disabled, true)]
        public void When_ApplyValidity_with_status_Then_IsValid_is_set_correctly(GroupStatus status, bool expectedIsValid)
        {
            //arrange
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            var validity = new Validity();

            //act
            factory.ApplyValidity(validity, status);

            //assert
            Assert.That(validity.IsValid, Is.EqualTo(expectedIsValid));
        }

        #endregion

        #region GetEntityType tests

        [Test]
        public void When_GetEntityType_for_text_question_Then_returns_TextQuestion()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.TextQuestion));
        }

        [Test]
        public void When_GetEntityType_for_integer_question_Then_returns_Integer()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Integer));
        }

        [Test]
        public void When_GetEntityType_for_double_question_Then_returns_Double()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRealQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Double));
        }

        [Test]
        public void When_GetEntityType_for_datetime_question_Then_returns_DateTime()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.DateTimeQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.DateTime));
        }

        [Test]
        public void When_GetEntityType_for_gps_question_Then_returns_Gps()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Gps));
        }

        [Test]
        public void When_GetEntityType_for_qrbarcode_question_Then_returns_QRBarcode()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.QRBarcodeQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.QRBarcode));
        }

        [Test]
        public void When_GetEntityType_for_audio_question_Then_returns_Audio()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.AudioQuestion(qId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Audio));
        }

        [Test]
        public void When_GetEntityType_for_multimedia_question_Then_returns_Multimedia()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultimediaQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Multimedia));
        }

        [Test]
        public void When_GetEntityType_for_single_option_question_Then_returns_CategoricalSingle()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.CategoricalSingle));
        }

        [Test]
        public void When_GetEntityType_for_multi_option_question_Then_returns_CategoricalMulti()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.CategoricalMulti));
        }

        [Test]
        public void When_GetEntityType_for_yesno_question_Then_returns_CategoricalYesNo()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, isYesNo: true));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.CategoricalYesNo));
        }

        [Test]
        public void When_GetEntityType_for_textlist_question_Then_returns_TextList()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.TextList));
        }

        [Test]
        public void When_GetEntityType_for_area_question_in_review_mode_Then_returns_Area()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GeographyQuestion(id: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, isReviewMode: true, includeVariables: false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Area));
        }

        [Test]
        public void When_GetEntityType_for_area_question_not_in_review_mode_Then_returns_Unsupported()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GeographyQuestion(id: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(questionId, RosterVector.Empty), questionnaire, interview, isReviewMode: false, includeVariables: false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Unsupported));
        }

        [Test]
        public void When_GetEntityType_for_variable_with_includeVariables_true_Then_returns_Variable()
        {
            //arrange
            var variableId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Variable(id: variableId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(variableId, RosterVector.Empty), questionnaire, interview, false, includeVariables: true);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Variable));
        }

        [Test]
        public void When_GetEntityType_for_variable_with_includeVariables_false_and_not_prefilled_Then_returns_Unsupported()
        {
            //arrange
            var variableId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Variable(id: variableId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(variableId, RosterVector.Empty), questionnaire, interview, false, includeVariables: false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Unsupported));
        }

        [Test]
        public void When_GetEntityType_for_group_Then_returns_Group()
        {
            //arrange
            var chapterId = Id.g1;
            var groupId = Id.g2;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.Group(groupId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(groupId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.Group));
        }

        [Test]
        public void When_GetEntityType_for_static_text_Then_returns_StaticText()
        {
            //arrange
            var staticTextId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.StaticText(staticTextId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var entityType = factory.GetEntityType(Identity.Create(staticTextId, RosterVector.Empty), questionnaire, interview, false, false);

            //assert
            Assert.That(entityType, Is.EqualTo(InterviewEntityType.StaticText));
        }

        #endregion

        #region GetEntityDetails tests

        [Test]
        public void When_GetEntityDetails_for_text_question_Then_returns_InterviewTextQuestion()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<InterviewTextQuestion>());
        }

        [Test]
        public void When_GetEntityDetails_for_static_text_Then_returns_InterviewStaticText()
        {
            //arrange
            var staticTextId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.StaticText(staticTextId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(staticTextId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<InterviewStaticText>());
        }

        [Test]
        public void When_GetEntityDetails_for_group_Then_returns_InterviewGroupOrRosterInstance()
        {
            //arrange
            var chapterId = Id.g1;
            var groupId = Id.g2;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.Group(groupId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(groupId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<InterviewGroupOrRosterInstance>());
        }

        [Test]
        public void When_GetEntityDetails_for_variable_Then_returns_InterviewVariable()
        {
            //arrange
            var variableId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.Variable(id: variableId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(variableId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<InterviewVariable>());
        }

        [Test]
        public void When_GetEntityDetails_for_text_question_with_includeVariableName_Then_Name_is_set()
        {
            //arrange
            var questionId = Id.g1;
            var variableName = "test_var";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId, variable: variableName));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false, includeVariableName: true);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Name, Is.EqualTo(variableName));
        }

        [Test]
        public void When_GetEntityDetails_for_unknown_identity_Then_returns_null()
        {
            //arrange
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            var unknownId = Id.gA;

            //act
            var result = factory.GetEntityDetails(Identity.Create(unknownId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void When_GetEntityDetails_for_disabled_entity_with_HideIfDisabled_Then_HideIfDisabled_is_set()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId, hideIfDisabled: true));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.HideIfDisabled, Is.True);
        }

        #endregion

        #region GetUIParent tests

        [Test]
        public void When_GetUIParent_for_question_in_regular_group_Then_returns_group_identity()
        {
            //arrange
            var chapterId = Id.g1;
            var groupId = Id.g2;
            var questionId = Id.g3;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.Group(groupId, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId)
                }));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var parent = factory.GetUIParent(interview, questionnaire, Identity.Create(questionId, RosterVector.Empty));

            //assert
            Assert.That(parent, Is.Not.Null);
            Assert.That(parent.Id, Is.EqualTo(groupId));
        }

        [Test]
        public void When_GetUIParent_for_question_in_flat_roster_Then_skips_flat_roster_and_returns_grandparent()
        {
            //arrange
            var chapterId = Id.g1;
            var flatRosterId = Id.g2;
            var questionId = Id.g3;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.FixedRoster(rosterId: flatRosterId,
                    displayMode: Main.Core.Entities.SubEntities.RosterDisplayMode.Flat,
                    fixedTitles: new[]
                    {
                        Create.Entity.FixedTitle(1, "Row 1")
                    },
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId)
                    }));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            
            // The question inside the flat roster has RosterVector(1) since fixed title code is 1
            var questionIdentity = Identity.Create(questionId, Create.RosterVector(1));

            //act
            var parent = factory.GetUIParent(interview, questionnaire, questionIdentity);

            //assert
            // The flat roster (custom view roster) should be skipped, returning the section/chapter
            Assert.That(parent, Is.Not.Null);
            Assert.That(parent.Id, Is.EqualTo(chapterId));
        }

        #endregion

        #region CalculateSimpleStatus tests

        [Test]
        public void When_CalculateSimpleStatus_not_in_review_mode_Then_uses_enumerator_strategy()
        {
            //arrange
            var chapterId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.TextQuestion(Id.g2));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            var group = interview.GetGroup(Identity.Create(chapterId, RosterVector.Empty));

            //act
            var status = factory.CalculateSimpleStatus(group, isReviewMode: false, interview, questionnaire);

            //assert
            // Not started (no answers given) should result in NotStarted status
            Assert.That(status, Is.EqualTo(GroupStatus.NotStarted));
        }

        [Test]
        public void When_CalculateSimpleStatus_in_review_mode_Then_uses_supervisor_strategy()
        {
            //arrange
            var chapterId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.TextQuestion(Id.g2));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            var group = interview.GetGroup(Identity.Create(chapterId, RosterVector.Empty));

            //act
            var status = factory.CalculateSimpleStatus(group, isReviewMode: true, interview, questionnaire);

            //assert
            // supervisor strategy should also return NotStarted for empty interview
            Assert.That(status, Is.EqualTo(GroupStatus.NotStarted));
        }

        #endregion

        #region GetCriticalRuleMessage tests

        [Test]
        public void When_GetCriticalRuleMessage_Then_calls_navigation_service_with_message()
        {
            //arrange
            var conditionId = Id.g1;
            var criticalRuleMessage = "Critical rule message";
            var expectedMessage = "Critical rule message with links";

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            questionnaireDocument.CriticalRules = new System.Collections.Generic.List<WB.Core.SharedKernels.SurveySolutions.Documents.CriticalRule>
            {
                new WB.Core.SharedKernels.SurveySolutions.Documents.CriticalRule(conditionId, "1 == 0", criticalRuleMessage)
            };
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);

            var webNavigationService = new Mock<IWebNavigationService>();
            webNavigationService.Setup(x => x.MakeNavigationLinks(
                    criticalRuleMessage, It.IsAny<Identity>(), It.IsAny<WB.Core.SharedKernels.DataCollection.Aggregates.IQuestionnaire>(),
                    It.IsAny<WB.Core.SharedKernels.DataCollection.Aggregates.IStatefulInterview>(), It.IsAny<string>()))
                .Returns(expectedMessage);

            var factory = new WebInterviewInterviewEntityFactory(
                Create.Service.EnumeratorGroupGroupStateCalculationStrategy(),
                Create.Service.SupervisorGroupStateCalculationStrategy(),
                webNavigationService.Object,
                Create.Service.SubstitutionTextFactory());

            //act
            var result = factory.GetCriticalRuleMessage(conditionId, interview, questionnaire, false);

            //assert
            Assert.That(result, Is.EqualTo(expectedMessage));
        }

        #endregion

        #region SubstituteText tests

        [Test]
        public void When_SubstituteText_Then_calls_navigation_service_and_returns_result()
        {
            //arrange
            var entityId = Identity.Create(Id.g1, RosterVector.Empty);
            var inputText = "Hello [variable]";
            var expectedText = "Hello World";

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter();
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);

            var webNavigationService = new Mock<IWebNavigationService>();
            webNavigationService.Setup(x => x.MakeNavigationLinks(
                    inputText, entityId, It.IsAny<WB.Core.SharedKernels.DataCollection.Aggregates.IQuestionnaire>(),
                    It.IsAny<WB.Core.SharedKernels.DataCollection.Aggregates.IStatefulInterview>(), It.IsAny<string>()))
                .Returns(expectedText);

            var factory = new WebInterviewInterviewEntityFactory(
                Create.Service.EnumeratorGroupGroupStateCalculationStrategy(),
                Create.Service.SupervisorGroupStateCalculationStrategy(),
                webNavigationService.Object,
                Create.Service.SubstitutionTextFactory());

            //act
            var result = factory.SubstituteText(inputText, entityId, interview, questionnaire, false);

            //assert
            Assert.That(result, Is.EqualTo(expectedText));
        }

        #endregion

        #region AutoMapper mapping tests

        [Test]
        public void When_GetEntityDetails_maps_answered_text_question_Then_Answer_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            var expectedAnswer = "hello world";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, expectedAnswer);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewTextQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EqualTo(expectedAnswer));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_answered_integer_question_Then_Answer_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            const int expectedAnswer = 42;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerNumericIntegerQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, expectedAnswer);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewIntegerQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EqualTo(expectedAnswer));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_answered_double_question_Then_Answer_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            const double expectedAnswer = 3.14;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericRealQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerNumericRealQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, expectedAnswer);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewDoubleQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EqualTo(expectedAnswer).Within(0.0001));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_answered_datetime_question_Then_Answer_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            var expectedAnswer = new DateTime(2024, 5, 15, 10, 30, 0, DateTimeKind.Utc);
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.DateTimeQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerDateTimeQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, expectedAnswer);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewDateQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EqualTo(expectedAnswer));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_single_option_question_Then_Answer_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            const int selectedValue = 2;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.SingleQuestion(questionId, options: new System.Collections.Generic.List<Main.Core.Entities.SubEntities.Answer>
                {
                    new Main.Core.Entities.SubEntities.Answer { AnswerValue = "1", AnswerText = "Option 1" },
                    new Main.Core.Entities.SubEntities.Answer { AnswerValue = "2", AnswerText = "Option 2" },
                }));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerSingleOptionQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, selectedValue);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewSingleOptionQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EqualTo(selectedValue));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_multi_option_question_Then_Answer_contains_selected_values()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            var selectedValues = new[] { 1, 3 };
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, textAnswers: new[]
                {
                    Create.Entity.Option(1, "Option 1"),
                    Create.Entity.Option(2, "Option 2"),
                    Create.Entity.Option(3, "Option 3"),
                }));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerMultipleOptionsQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, selectedValues);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewMutliOptionQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EquivalentTo(selectedValues));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_yesno_question_Then_Answer_contains_yes_no_values()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.MultipleOptionsQuestion(questionId, isYesNo: true, textAnswers: new[]
                {
                    Create.Entity.Option(1, "Option 1"),
                    Create.Entity.Option(2, "Option 2"),
                }));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerYesNoQuestion(new WB.Core.SharedKernels.DataCollection.Commands.Interview.AnswerYesNoQuestion(
                interview.Id, userId, questionId, RosterVector.Empty,
                new[]
                {
                    new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.AnsweredYesNoOption(1, true),
                    new WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos.AnsweredYesNoOption(2, false),
                }));
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewYesNoQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Has.Length.EqualTo(2));
            Assert.That(result.Answer.Single(a => a.Value == 1).Yes, Is.True);
            Assert.That(result.Answer.Single(a => a.Value == 2).Yes, Is.False);
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_textlist_question_Then_Rows_contain_answers()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerTextListQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow,
                new[] { new Tuple<decimal, string>(1, "Row One"), new Tuple<decimal, string>(2, "Row Two") });
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewTextListQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Rows, Has.Count.EqualTo(2));
            Assert.That(result.Rows[0].Text, Is.EqualTo("Row One"));
            Assert.That(result.Rows[1].Text, Is.EqualTo("Row Two"));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_gps_question_Then_Answer_contains_coordinates()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            const double latitude = 51.5074;
            const double longitude = -0.1278;
            const double accuracy = 5.0;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.GpsCoordinateQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerGeoLocationQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow,
                latitude, longitude, accuracy, altitude: 0, timestamp: DateTimeOffset.UtcNow);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewGpsQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.Not.Null);
            Assert.That(result.Answer.Latitude, Is.EqualTo(latitude).Within(0.0001));
            Assert.That(result.Answer.Longitude, Is.EqualTo(longitude).Within(0.0001));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_qrbarcode_question_Then_Answer_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var userId = Id.g9;
            const string expectedAnswer = "BARCODE-12345";
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.QRBarcodeQuestion(questionId: questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.AnswerQRBarcodeQuestion(userId, questionId, RosterVector.Empty, DateTimeOffset.UtcNow, expectedAnswer);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewBarcodeQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.EqualTo(expectedAnswer));
            Assert.That(result.IsAnswered, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_generic_question_fields_Then_Id_and_IsDisabled_are_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            var identity = Identity.Create(questionId, RosterVector.Empty);

            //act
            var result = factory.GetEntityDetails(identity.ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Id, Is.EqualTo(identity.ToString()));
            Assert.That(result.IsDisabled, Is.False);
        }

        [Test]
        public void When_GetEntityDetails_maps_roster_Then_IsRoster_and_RosterTitle_are_correctly_mapped()
        {
            //arrange
            var chapterId = Id.g1;
            var rosterId = Id.g2;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.Roster(rosterId: rosterId,
                    fixedTitles: new[] { "Item 1" },
                    rosterSizeSourceType: Main.Core.Entities.SubEntities.RosterSizeSourceType.FixedTitles));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();
            var rosterIdentity = Identity.Create(rosterId, Create.RosterVector(0));

            //act
            var result = factory.GetEntityDetails(rosterIdentity.ToString(), interview, questionnaire, false)
                as InterviewGroupOrRosterInstance;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsRoster, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_group_Then_IsRoster_is_false()
        {
            //arrange
            var chapterId = Id.g1;
            var groupId = Id.g2;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId,
                Create.Entity.Group(groupId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(groupId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewGroupOrRosterInstance;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsRoster, Is.False);
        }

        [Test]
        public void When_GetEntityDetails_for_unanswered_text_question_Then_Answer_is_null_and_IsAnswered_is_false()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewTextQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void When_GetEntityDetails_for_unanswered_integer_question_Then_Answer_is_null_and_IsAnswered_is_false()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewIntegerQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Answer, Is.Null);
            Assert.That(result.IsAnswered, Is.False);
        }

        [Test]
        public void When_GetEntityDetails_maps_disabled_question_Then_IsDisabled_is_true()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            interview.Apply(Create.Event.QuestionsDisabled(new[] { Identity.Create(questionId, RosterVector.Empty) }));
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false);

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.IsDisabled, Is.True);
        }

        [Test]
        public void When_GetEntityDetails_maps_integer_question_with_useFormatting_Then_UseFormatting_is_correctly_mapped()
        {
            //arrange
            var questionId = Id.g1;
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(questionId, useFormatting: true));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Abc.SetUp.StatefulInterview(questionnaireDocument);
            var factory = this.CreateWebInterviewInterviewEntityFactory();

            //act
            var result = factory.GetEntityDetails(Identity.Create(questionId, RosterVector.Empty).ToString(), interview, questionnaire, false)
                as InterviewIntegerQuestion;

            //assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.UseFormatting, Is.True);
        }

        #endregion

        private WebInterviewInterviewEntityFactory CreateWebInterviewInterviewEntityFactory() =>
            new WebInterviewInterviewEntityFactory(
                Create.Service.EnumeratorGroupGroupStateCalculationStrategy(),
                Create.Service.SupervisorGroupStateCalculationStrategy(),
                Mock.Of<IWebNavigationService>(),
                Create.Service.SubstitutionTextFactory());
    }
}
