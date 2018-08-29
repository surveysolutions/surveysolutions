using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Main.Core.Documents;
using MvvmCross.Base;
using MvvmCross.Tests;
using MvvmCross.Views;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels
{
    [TestOf(typeof(SideBarSectionsViewModel))]
    public class SidebarSectionsViewModelTests : MvxIoCSupportingTest
    {
        private static readonly QuestionnaireDocument QuestionnaireDocument = Create.Entity.QuestionnaireDocument(children: new IComposite[]
        {
            Create.Entity.Group(Id.g1, children: new IComposite[]
            {
                Create.Entity.FixedRoster(Id.g4, variable: "r1", fixedTitles: Create.Entity.FixedTitles(1, 2), children: new IComposite[]
                {
                    Create.Entity.FixedRoster(Id.g5, variable: "r2", fixedTitles: Create.Entity.FixedTitles(3, 4), children: new IComposite[]
                    {
                        Create.Entity.FixedRoster(Id.g6, variable: "r3", fixedTitles: Create.Entity.FixedTitles(5, 6), children: new IComposite[]
                        {

                        })
                    })
                })
            }),
            Create.Entity.Group(Id.g2),
            Create.Entity.Group(Id.g3)
        });


        [Test]
        public async Task When_getting_sections_after_closing_section_for_interview_with_nested_rosters_and_nested_roster_open()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);

            //arrange
            var questionnaire = QuestionnaireDocument;

            var eventRegistry = Create.Service.LiteEventRegistry();

            var interview = Abc.Setup.StatefulInterview(questionnaire);

            var navigationState = Create.Other.NavigationState(Mock.Of<IStatefulInterviewRepository>(x => x.Get(It.IsAny<string>()) == interview));
            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(Id.g5, Create.RosterVector(1, 3))));
            //-Id.Identity1,
            // +-- Create.Identity(Id.g4, 1),
            // |   +-- Create.Identity(Id.g5, 3),
            // |   |   +-- Create.Identity(Id.g5, 5),
            // |   |   +-- Create.Identity(Id.g5, 6),
            // |   +--  Create.Identity(Id.g5, 4),
            // +-- Create.Identity(Id.g4, 2),
            //-Id.Identity2,
            //-Id.Identity3
            SideBarSectionsViewModel viewModel = Create.ViewModel.SidebarSectionsViewModel(questionnaire, interview, eventRegistry, navigationState);

            //act
            IEnumerable<Identity> resust = viewModel.GetSectionsAndExpandedSubSections(false, new ToggleSectionEventArgs
            {
                ToggledSection = Id.Identity1, 
                IsExpandedNow = false
            }).ToArray();

            //assert

            //-Id.Identity1,
            //-Id.Identity2,
            //-Id.Identity3
            Assert.That(resust, Is.EquivalentTo(new[]
            {
                Id.Identity1,
                Id.Identity2,
                Id.Identity3
            }));
        }

        [Test]
        public async Task When_getting_sections_for_interview_with_fisrt_section_open()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);

            //arrange
            var questionnaire = QuestionnaireDocument;

            var eventRegistry = Create.Service.LiteEventRegistry();

            var interview = Abc.Setup.StatefulInterview(questionnaire);

            var navigationState = Create.Other.NavigationState(Mock.Of<IStatefulInterviewRepository>(x => x.Get(It.IsAny<string>()) == interview));
            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Id.Identity1));

            var viewModel = Create.ViewModel.SidebarSectionsViewModel(questionnaire, interview, eventRegistry, navigationState);

            //act
            IEnumerable<Identity> resust = viewModel.GetSectionsAndExpandedSubSections(false).ToArray();

            //assert
            //-Id.Identity1,
            // +-- Create.Identity(Id.g4, 1),
            // +-- Create.Identity(Id.g4, 2),
            //-Id.Identity2,
            //-Id.Identity3
            Assert.That(resust, Is.EquivalentTo(new[]
            {
                Id.Identity1,
                Create.Identity(Id.g4, 1),
                Create.Identity(Id.g4, 2),
                Id.Identity2,
                Id.Identity3
            }));
        }

        [Test]
        public async Task When_getting_sections_for_interview_with_nested_rosters_and_deepest_roster_is_open()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);

            //arrange
            var questionnaire = QuestionnaireDocument;

            var eventRegistry = Create.Service.LiteEventRegistry();

            var interview = Abc.Setup.StatefulInterview(questionnaire);

            var navigationState = Create.Other.NavigationState(Mock.Of<IStatefulInterviewRepository>(x => x.Get(It.IsAny<string>()) == interview));
            await navigationState.NavigateTo(Create.Entity.NavigationIdentity(Identity.Create(Id.g6, Create.RosterVector(1, 3, 5))));

            var viewModel = Create.ViewModel.SidebarSectionsViewModel(questionnaire, interview, eventRegistry, navigationState);

            //act
            IEnumerable<Identity> resust = viewModel.GetSectionsAndExpandedSubSections(false).ToArray();

            //assert
            //-Id.Identity1,
            // +-- Create.Identity(Id.g4, 1),
            // |   +-- Create.Identity(Id.g5, 1, 3),
            // |   |   +-- Create.Identity(Id.g5, 1, 3, 5),
            // |   |   +-- Create.Identity(Id.g5, 1, 3, 6),
            // |   +--  Create.Identity(Id.g5, 1, 4),
            // +-- Create.Identity(Id.g4, 2),
            //-Id.Identity2,
            //-Id.Identity3
            Assert.That(resust, Is.EquivalentTo(new[]
            {
                Id.Identity1,
                Create.Identity(Id.g4, 1),
                Create.Identity(Id.g5, 1, 3),
                Create.Identity(Id.g6, 1, 3, 5),
                Create.Identity(Id.g6, 1, 3, 6),
                Create.Identity(Id.g5, 1, 4),
                Create.Identity(Id.g4, 2),
                Id.Identity2,
                Id.Identity3
            }));
        }

        [Test]
        public void When_section_enabled_Then_new_view_model_should_be_added_with_specified_index()
        {
            base.Setup();

            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);

            //arrange
            var section1Id = Guid.Parse("11111111111111111111111111111111");
            var disabledSectionId = Guid.Parse("22222222222222222222222222222222");
            var section3Id = Guid.Parse("33333333333333333333333333333333");

            var questionnaire = Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.Group(section1Id),
                Create.Entity.Group(disabledSectionId),
                Create.Entity.Group(section3Id)
            });

            var eventRegistry = Create.Service.LiteEventRegistry();

            var interview = Abc.Setup.StatefulInterview(questionnaire);
            interview.Apply(Create.Event.GroupsDisabled(disabledSectionId, RosterVector.Empty));

            var viewModel = Create.ViewModel.SidebarSectionsViewModel(questionnaire, interview, eventRegistry);
            var viewModelsWithoutDisabled = viewModel.AllVisibleSections.ToList();

            //act
            interview.Apply(Create.Event.GroupsEnabled(disabledSectionId, RosterVector.Empty));
            viewModel.Handle(Create.Event.GroupsEnabled(disabledSectionId, RosterVector.Empty));
            //assert
            var addedDisabledSectionViewModel = viewModel.AllVisibleSections.Except(viewModelsWithoutDisabled).Single();

            Assert.That(viewModel.AllVisibleSections.Count, Is.EqualTo(6));
            Assert.That(viewModel.AllVisibleSections.ToList().IndexOf(addedDisabledSectionViewModel), Is.EqualTo(2));
        }
    }
}
