﻿using System;
using System.Linq;
using Moq;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.AssignmentImport;
using WB.Core.BoundedContexts.Headquarters.Assignments;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services;
using WB.Core.BoundedContexts.Headquarters.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.Invitations;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Jobs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Services.DeleteQuestionnaireTemplate;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.BoundedContexts.Headquarters.Views.Questionnaire;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.Implementation.Aggregates;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.Questionnaire;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    [NUnit.Framework.TestOf(typeof(DeleteQuestionnaireService))]
    internal class DeleteQuestionnaireServiceTestContext
    {
        protected static DeleteQuestionnaireService CreateDeleteQuestionnaireService(IInterviewsToDeleteFactory interviewsToDeleteFactory = null,
            ICommandService commandService = null, 
            IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null, 
            IQuestionnaireStorage questionnaireStorage = null,
            IAssignmentsImportService interviewImportService = null,
            IPlainKeyValueStorage<QuestionnaireLookupTable> lookupStorage = null,
            IAssignmentsToDeleteFactory assignmentsToDeleteFactory = null)
        {
            SetUp.InstanceToMockedServiceLocator(questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>());
            return
                new DeleteQuestionnaireService(
                    interviewsToDeleteFactory ?? Mock.Of<IInterviewsToDeleteFactory>(f => f.LoadBatch(It.IsAny<Guid>(), It.IsAny<long>()) == Enumerable.Empty<InterviewSummary>().ToList()),
                    commandService ?? Mock.Of<ICommandService>(),
                    Mock.Of<ILogger>(),
                    Mock.Of<ITranslationManagementService>(),
                    interviewImportService ?? Mock.Of<IAssignmentsImportService>(_ => _.GetImportStatus() == new AssignmentsImportStatus()),
                    Mock.Of<ISystemLog>(),
                    questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(),
                    lookupStorage ?? Mock.Of<IPlainKeyValueStorage<QuestionnaireLookupTable>>(),
                    questionnaireStorage ?? Mock.Of<IQuestionnaireStorage>(),
                    new DeleteQuestionnaireJobScheduler(Mock.Of<IScheduler>()),
                    Mock.Of<IInvitationsDeletionService>(),
                    Mock.Of<IAggregateRootCacheCleaner>(),
                    assignmentsToDeleteFactory ?? Mock.Of<IAssignmentsToDeleteFactory>(f => f.LoadBatch(It.IsAny<Guid>(), It.IsAny<long>()) == Enumerable.Empty<Assignment>().ToList()),
                    Mock.Of<IPlainKeyValueStorage<QuestionnaireReusableCategories>>());
        }
    }
}
