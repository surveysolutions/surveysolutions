using System.Linq;
using Core.Supervisor.Views.Reposts.InputModels;
using Core.Supervisor.Views.Reposts.Views;
using Main.Core.Utility;
using Main.Core.View;
using WB.Core.BoundedContexts.Supervisor.EventHandler;
using WB.Core.BoundedContexts.Supervisor.Views.Interview;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace Core.Supervisor.Views.Reposts.Factories
{
    public class MapReport : IViewFactory<MapReportInputModel, MapReportView>
    {
        private readonly IReadSideRepositoryReader<AnswersByVariableCollection> answersByVariableStorage;

        public MapReport(IReadSideRepositoryReader<AnswersByVariableCollection> answersByVariableStorage)
        {
            this.answersByVariableStorage = answersByVariableStorage;
        }

        public MapReportView Load(MapReportInputModel input)
        {
            var key = RepositoryKeysHelper.GetVariableByQuestionnaireKey(input.Variable, RepositoryKeysHelper.GetVersionedKey(input.QuestionnaireId, input.QuestionnaireVersion));

            var answersCollection = answersByVariableStorage.GetById(key);

            return new MapReportView()
            {
                Answers = answersCollection.Answers.Select(x => string.Join(";", x.Value.Values)).ToArray()
            };
        }
    }
}
