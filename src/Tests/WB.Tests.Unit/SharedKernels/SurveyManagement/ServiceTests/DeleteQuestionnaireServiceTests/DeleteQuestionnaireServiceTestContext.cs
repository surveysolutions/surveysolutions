﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Services.DeleteQuestionnaireTemplate;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.ServiceTests.DeleteQuestionnaireServiceTests
{
    [Subject(typeof(DeleteQuestionnaireService))]
    internal class DeleteQuestionnaireServiceTestContext
    {
        protected static DeleteQuestionnaireService CreateDeleteQuestionnaireService(IInterviewsToDeleteFactory interviewsToDeleteFactory = null,
           ICommandService commandService = null, IPlainStorageAccessor<QuestionnaireBrowseItem> questionnaireBrowseItemStorage = null, IPlainQuestionnaireRepository plainQuestionnaireRepository=null)
        {
            Func<IInterviewsToDeleteFactory> factory = () => (interviewsToDeleteFactory ?? Mock.Of<IInterviewsToDeleteFactory>());
            Setup.InstanceToMockedServiceLocator(questionnaireBrowseItemStorage ?? Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>());
            return
                new DeleteQuestionnaireService(
                    factory,
                    commandService ?? Mock.Of<ICommandService>(), Mock.Of<ILogger>());
        }
    }
}
