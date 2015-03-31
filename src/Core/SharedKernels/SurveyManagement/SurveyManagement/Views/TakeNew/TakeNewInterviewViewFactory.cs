using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Implementation.ReadSide.Indexes;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> surveys;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;
        private readonly IReadSideRepositoryIndexAccessor indexAccessor;

        public TakeNewInterviewViewFactory(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> surveys, 
            IQueryableReadSideRepositoryReader<UserDocument> users,
            IReadSideRepositoryIndexAccessor indexAccessor)
        {
            this.surveys = surveys;
            this.users = users;
            this.indexAccessor = indexAccessor;
        }

        public TakeNewInterviewView Load(TakeNewInterviewInputModel input)
        {
            var questionnaire = input.QuestionnaireVersion.HasValue
                ? this.surveys.AsVersioned().Get(input.QuestionnaireId.FormatGuid(), input.QuestionnaireVersion.Value)
                : this.surveys.GetById(input.QuestionnaireId);

            var view = new TakeNewInterviewView(questionnaire.Questionnaire, questionnaire.Version) {
                Supervisors = this.GetSupervisorsListForViewer(input.ViewerId).ToList()
            };

            return view;
        }

        private IEnumerable<UserDocument> GetSupervisorsListForViewer(Guid viewerId)
        {
            var viewer = this.users.GetById(viewerId);

            if (viewer == null || !(viewer.IsHq() || viewer.IsAdmin()))
                return Enumerable.Empty<UserDocument>();

            var indexName = typeof(UserDocumentsByBriefFields).Name;

            var supervisorsListForViewer = new List<UserDocument>();
            var batch = new List<UserDocument>();
            int taken = 0;
            int batchSize = 128;
            do
            {
                batch = this.indexAccessor.Query<UserDocument>(indexName)
                    .OrderBy(x => x.UserName)
                    .Where(user => user.Roles.Any(role => role == UserRoles.Supervisor))
                    .Skip(taken)
                    .Take(batchSize)
                    .ToList();
                taken += batch.Count;
                supervisorsListForViewer.AddRange(batch);

            } while (batch.Count == batchSize);

            return supervisorsListForViewer;
        }
    }
}