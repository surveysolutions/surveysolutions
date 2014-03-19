using System;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.SupervisorFeed;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Headquarters.Implementation.Services
{
    class SupervisorFeedService : ISupervisorFeedService 
    {
        private readonly IQueryableReadSideRepositoryReader<SupervisorRegisteredEntry> reader;

        public SupervisorFeedService(IQueryableReadSideRepositoryReader<SupervisorRegisteredEntry> reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            this.reader = reader;
        }

        public SupervisorRegisteredEntry GetEntry(string login)
        {
            SupervisorRegisteredEntry supervisorRegisteredEntry = reader.GetById(login);
            return supervisorRegisteredEntry;
        }
    }
}