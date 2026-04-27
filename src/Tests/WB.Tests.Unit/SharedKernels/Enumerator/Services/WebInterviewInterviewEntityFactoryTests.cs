using System;
using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Microsoft.Extensions.Logging.Abstractions;
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
using WB.UI.Headquarters.Controllers.Api.PublicApi;
using WB.UI.Headquarters.Models.Api;

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
                new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new WebInterviewAutoMapProfile());
                    cfg.AddProfile(new AssignmentProfile());
                    cfg.AddProfile(new AssignmentsPublicApiMapProfile());
                }).CreateMapper(),
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
                new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new WebInterviewAutoMapProfile());
                    cfg.AddProfile(new AssignmentProfile());
                    cfg.AddProfile(new AssignmentsPublicApiMapProfile());
                }).CreateMapper(),
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

        private WebInterviewInterviewEntityFactory CreateWebInterviewInterviewEntityFactory() =>
            new WebInterviewInterviewEntityFactory(
                new MapperConfiguration(cfg =>
                {
                    cfg.AddProfile(new WebInterviewAutoMapProfile());
                    cfg.AddProfile(new AssignmentProfile());
                    cfg.AddProfile(new AssignmentsPublicApiMapProfile());
                }).CreateMapper(),
                Create.Service.EnumeratorGroupGroupStateCalculationStrategy(),
                Create.Service.SupervisorGroupStateCalculationStrategy(),
                Mock.Of<IWebNavigationService>(),
                Create.Service.SubstitutionTextFactory());
    }
}
