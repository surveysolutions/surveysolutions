using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.SharedKernels.SurveyManagement.Views.TakeNew
{
    public class TakeNewInterviewViewFactory : IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView> 
    {
        private readonly IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> surveys;
        private readonly IQueryableReadSideRepositoryReader<UserDocument> users;

        public TakeNewInterviewViewFactory(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> surveys, 
            IQueryableReadSideRepositoryReader<UserDocument> users)
        {
            this.surveys = surveys;
            this.users = users;
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

            return this.users.Query(_ => 
                _.OrderBy(x => x.UserName)
                .Where(user => user.Roles.Any(role => role == UserRoles.Supervisor))
                .ToList());
        }
    }
}