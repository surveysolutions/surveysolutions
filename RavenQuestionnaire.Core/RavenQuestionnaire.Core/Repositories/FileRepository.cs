using Raven.Client;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;

namespace RavenQuestionnaire.Core.Repositories
{
    public class FileRepository : EntityRepository<File, FileDocument>, IFileRepository
    {
        public FileRepository(IDocumentSession documentSession) : base(documentSession) { }

        protected override File Create(FileDocument doc)
        {
            return new File(doc);
        }
    }
}
