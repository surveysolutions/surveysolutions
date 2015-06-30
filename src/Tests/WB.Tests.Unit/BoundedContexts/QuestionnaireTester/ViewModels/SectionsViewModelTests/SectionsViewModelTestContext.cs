using System;

using Cirrious.MvvmCross.Plugins.Messenger;

using Moq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Aggregates;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Tests.Unit.BoundedContexts.Tester.SectionsViewModelTests
{
    public class SectionsViewModelTestContext
    {
        protected static SectionsViewModel CreateSectionsViewModel(
            IMvxMessenger messenger = null,
            IStatefulInterviewRepository interviewRepository = null,
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireRepository = null,
            ISubstitutionService substitutionService = null)
        {
            return new SectionsViewModel(
                messenger ?? Mock.Of<IMvxMessenger>(),
                interviewRepository ?? Mock.Of<IStatefulInterviewRepository>(),
                questionnaireRepository ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireModel>>(),
                substitutionService ?? Create.SubstitutionService(),
                Create.LiteEventRegistry(),
                Stub.MvxMainThreadDispatcher());
        }


        protected static SectionsViewModel CreateSectionsViewModel(QuestionnaireModel questionnaire, IStatefulInterview interview)
        {
            var questionnaireRepository = new Mock<IPlainKeyValueStorage<QuestionnaireModel>>();
            questionnaireRepository.SetReturnsDefault(questionnaire);

            var interviewsRepository = new Mock<IStatefulInterviewRepository>();
            interviewsRepository.SetReturnsDefault(interview);

            return CreateSectionsViewModel(questionnaireRepository: questionnaireRepository.Object,
                interviewRepository: interviewsRepository.Object);
        }

        protected static GroupsHierarchyModel CreateGroupsHierarchyModel(Guid id, string title)
        {
            return new GroupsHierarchyModel
                   {
                       Id = id,
                       Title = title,
                       IsRoster = false
                   };
        }
    }
}