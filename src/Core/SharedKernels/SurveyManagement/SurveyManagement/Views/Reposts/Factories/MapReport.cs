﻿using System.Linq;
using Main.Core.View;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.SurveyManagement.Views.Interview;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Views;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories
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

            var answersCollection = this.answersByVariableStorage.GetById(key);

            return new MapReportView()
            {
                Answers = answersCollection == null
                    ? new string[0]
                    : answersCollection.Answers.Select(x => string.Join("|", x.Value.Values)).ToArray()
            };
        }
    }
}
