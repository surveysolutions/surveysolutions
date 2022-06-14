using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.Core.BoundedContexts.Designer.QuestionnaireCompilationForOldVersions
{
    public class QuestionnaireCompilationVersionService : IQuestionnaireCompilationVersionService
    {
        private readonly DesignerDbContext dbContext;

        public QuestionnaireCompilationVersionService(DesignerDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<QuestionnaireCompilationVersion> GetCompilationVersions()
        {
            return this.dbContext.QuestionnaireCompilationVersions.ToList();
        }

        public void Update(QuestionnaireCompilationVersion version)
        {
            this.dbContext.QuestionnaireCompilationVersions.Update(version);
            this.dbContext.SaveChanges();
        }

        public void Remove(Guid questionnaireId)
        {
            var questionnaireCompilationVersion = this.dbContext.QuestionnaireCompilationVersions.Find(questionnaireId);
            if (questionnaireCompilationVersion != null)
            {
                this.dbContext.Remove(questionnaireCompilationVersion);
                this.dbContext.SaveChanges();
            }
        }

        public void Add(QuestionnaireCompilationVersion version)
        {
            this.dbContext.QuestionnaireCompilationVersions.Add(version);
            this.dbContext.SaveChanges();
        }

        public QuestionnaireCompilationVersion? GetById(Guid questionnaireId)
        {
            return this.dbContext.QuestionnaireCompilationVersions.Find(questionnaireId);
        }
    }
}
