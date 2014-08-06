using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Machine.Specifications;
using Moq;
using Ncqrs.Commanding;
using Ncqrs.Commanding.ServiceModel;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Repositories;

namespace WB.Core.BoundedContexts.Supervisor.Tests.Synchronization.QuestionnaireSynchronizerTests
{
    [Subject(typeof(QuestionnaireSynchronizer))]
    internal class QuestionnaireSynchronizerTestContext
    {
        protected static QuestionnaireSynchronizer CreateQuestionnaireSynchronizer(IAtomFeedReader atomFeedReader = null,
            IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry> plainStorage = null, IPlainQuestionnaireRepository plainQuestionnaireRepository=null,
            IHeadquartersQuestionnaireReader headquartersQuestionnaireReader=null)
        {
            return new QuestionnaireSynchronizer(atomFeedReader ?? Mock.Of<IAtomFeedReader>(),
                new HeadquartersSettings(new Uri("http://localhost"), new Uri("http://localhost"), new Uri("http://localhost"), "http://localhost", "http://localhost",
                    new Uri("http://localhost"), new Uri("http://localhost")),
                new HeadquartersPullContext(Mock.Of<IPlainStorageAccessor<SynchronizationStatus>>()),
                plainStorage ?? Mock.Of<IQueryablePlainStorageAccessor<LocalQuestionnaireFeedEntry>>(), Mock.Of<ILogger>(),
                plainQuestionnaireRepository ?? Mock.Of<IPlainQuestionnaireRepository>(), Mock.Of<ICommandService>(), headquartersQuestionnaireReader ??Mock.Of<IHeadquartersQuestionnaireReader>());
        }

        protected static AtomFeedEntry<T> CreateAtomFeedEntry<T>(T entry)
        {
            return new AtomFeedEntry<T>(){Content = entry};
        }

        protected static LocalQuestionnaireFeedEntry CreateLocalQuestionnaireFeedEntry(Guid? entryId = null, bool allowCensusMode = false,
            Guid? questionnaireId = null, long? questionnaireVersion = null)
        {
            return new LocalQuestionnaireFeedEntry(questionnaireId ?? Guid.NewGuid(), questionnaireVersion ?? 1,
                (entryId ?? Guid.NewGuid()).FormatGuid(), allowCensusMode, DateTime.Now);
        }
    }
}
