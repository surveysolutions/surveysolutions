using AutoMapper;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
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
