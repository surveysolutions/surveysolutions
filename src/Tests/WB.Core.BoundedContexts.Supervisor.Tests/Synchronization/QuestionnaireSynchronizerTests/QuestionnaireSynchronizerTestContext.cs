﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding;

using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.CommandBus;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.QuestionnaireSynchronizerTests
{
    [Subject(typeof(QuestionnaireSynchronizer))]
    internal class QuestionnaireSynchronizerTestContext
    {
        protected static QuestionnaireSynchronizer CreateQuestionnaireSynchronizer(IAtomFeedReader atomFeedReader = null,
            IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry> plainStorage = null,
            IPlainQuestionnaireRepository plainQuestionnaireRepository = null,
            IHeadquartersQuestionnaireReader headquartersQuestionnaireReader = null, HeadquartersPullContext headquartersPullContext = null,
            IQueryableReadSideRepositoryWriter<InterviewSummary> interviews = null, ICommandService commandService=null)
        {
            return new QuestionnaireSynchronizer(atomFeedReader ?? Mock.Of<IAtomFeedReader>(), Create.HeadquartersSettings(questionnaireDetailsEndpoint: "http://localhost", questionnaireAssemblyEndpoint: "http://localhost"),
                headquartersPullContext ?? new HeadquartersPullContext(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>()),
                plainStorage ?? Mock.Of<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>>(), Mock.Of<ILogger>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(), commandService ?? Mock.Of<ICommandService>(),
                headquartersQuestionnaireReader ?? Mock.Of<IHeadquartersQuestionnaireReader>(),
                interviews ?? Mock.Of<IQueryableReadSideRepositoryWriter<InterviewSummary>>());

        }

        protected static AtomFeedEntry<T> CreateAtomFeedEntry<T>(T entry)
        {
            return new AtomFeedEntry<T>(){Content = entry};
        }

        protected static LocalQuestionnaireFeedEntry CreateLocalQuestionnaireFeedEntry(Guid? entryId = null, QuestionnaireEntryType questionnaireEntryType = QuestionnaireEntryType.QuestionnaireCreated,
            Guid? questionnaireId = null, long? questionnaireVersion = null)
        {
            return new LocalQuestionnaireFeedEntry(questionnaireId ?? Guid.NewGuid(), questionnaireVersion ?? 1,
                (entryId ?? Guid.NewGuid()).FormatGuid(), questionnaireEntryType, DateTime.Now);
        }
    }
}
