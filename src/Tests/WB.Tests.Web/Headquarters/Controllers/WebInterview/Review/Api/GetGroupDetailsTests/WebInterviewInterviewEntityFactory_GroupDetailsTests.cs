using System;
using System.Linq;
using AutoMapper;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using NUnit.Framework;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Services;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Services;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.PublicApi;
using WB.UI.Headquarters.Models.Api;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Api.GetGroupDetailsTests
{
    [TestOf(typeof(WebInterviewInterviewEntityFactory))]
    public class WebInterviewInterviewEntityFactory_GroupDetailsTests
    {
        [Test]
        public void when_get_group_info_for_sidebar_with_plain_rosters_should_don_not_return_rosters_in_plain_mode()
        {
            var sectionId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var factory = CreateWebInterviewInterviewEntityFactory();
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(intQuestionId),
                Create.Entity.NumericRoster(rosterSizeQuestionId: intQuestionId, displayMode: RosterDisplayMode.Flat,
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion()
                    }),
                Create.Entity.FixedRoster(displayMode: RosterDisplayMode.Flat, fixedTitles: new FixedRosterTitle[]
                {
                    Create.Entity.FixedTitle(1, "1"),
                    Create.Entity.FixedTitle(2, "2"),
                }, children: new IComposite[] { Create.Entity.TextQuestion() })
            });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);

            var sidebar = factory.GetSidebarChildSectionsOf(sectionId.FormatGuid(), 
                statefulInterview, plainQuestionnaire, new []{ sectionId.FormatGuid() }, false );

            Assert.That(sidebar.Groups.Count, Is.EqualTo(0));
        }

        [Test]
        public void when_get_section_info_for_sidebar_for_inner_group_with_plain_rosters_should_don_not_return_has_children_flag_if_only_roster_in_plain_mode_inside()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            var factory = CreateWebInterviewInterviewEntityFactory();
            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(sectionId, new IComposite[]
            {
                Create.Entity.Group(groupId, children: new IComposite[]
                {
                    Create.Entity.NumericIntegerQuestion(intQuestionId),
                    Create.Entity.NumericRoster(rosterSizeQuestionId: intQuestionId, displayMode: RosterDisplayMode.Flat,
                        children: new IComposite[]
                        {
                            Create.Entity.TextQuestion()
                        }),
                    Create.Entity.FixedRoster(displayMode: RosterDisplayMode.Flat, fixedTitles: new FixedRosterTitle[]
                    {
                        Create.Entity.FixedTitle(1, "1"),
                        Create.Entity.FixedTitle(2, "2"),
                    }, children: new IComposite[] { Create.Entity.TextQuestion() })
                })
            });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);

            var sidebar = factory.GetSidebarChildSectionsOf(sectionId.FormatGuid(), 
                statefulInterview, plainQuestionnaire, new []{ sectionId.FormatGuid() }, false );

            Assert.That(sidebar.Groups.Count, Is.EqualTo(1));
            Assert.That(sidebar.Groups.Single().HasChildren, Is.EqualTo(false));
        }

        private WebInterviewInterviewEntityFactory CreateWebInterviewInterviewEntityFactory()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new WebInterviewAutoMapProfile());
            }).CreateMapper();
            var enumeratorGroupStateCalculationStrategy = Mock.Of<IEnumeratorGroupStateCalculationStrategy>();
            var supervisorGroupStateCalculationStrategy = Mock.Of<ISupervisorGroupStateCalculationStrategy>();
            return new WebInterviewInterviewEntityFactory(mapper, 
                enumeratorGroupStateCalculationStrategy,
                supervisorGroupStateCalculationStrategy,
                Mock.Of<IWebNavigationService>(), 
                Mock.Of<ISubstitutionTextFactory>());
        }
    }
}
