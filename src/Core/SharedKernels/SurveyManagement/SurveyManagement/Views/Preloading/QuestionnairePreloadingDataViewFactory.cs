using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.Infrastructure.ReadSide;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Repositories;
using WB.Core.SharedKernels.SurveyManagement.Views.DataExport;

namespace WB.Core.SharedKernels.SurveyManagement.Views.Preloading
{
    public class QuestionnairePreloadingDataViewFactory : IViewFactory<QuestionnairePreloadingDataInputModel, QuestionnairePreloadingDataItem>
    {
        private readonly IQuestionnaireProjectionsRepository questionnaireProjectionsRepository;

        public QuestionnairePreloadingDataViewFactory(IQuestionnaireProjectionsRepository questionnaireProjectionsRepository)
        {
            this.questionnaireProjectionsRepository = questionnaireProjectionsRepository;
        }

        public QuestionnairePreloadingDataItem Load(QuestionnairePreloadingDataInputModel input)
        {
            var questionnaire =
                this.questionnaireProjectionsRepository.GetQuestionnaireExportStructure(
                    new QuestionnaireIdentity(input.QuestionnaireId, input.QuestionnaireVerstion));
            if (questionnaire == null)
                return null;
            var firstLevel = questionnaire.HeaderToLevelMap[new ValueVector<Guid>()];
            return new QuestionnairePreloadingDataItem(input.QuestionnaireId, input.QuestionnaireVerstion, firstLevel.LevelName,
                this.BuildTopLevelQuestionList(firstLevel).ToArray());
        }

        private IEnumerable<QuestionDescription> BuildTopLevelQuestionList(HeaderStructureForLevel level)
        {
            return
                level.HeaderItems.Values.SelectMany(
                    headerItem =>
                        headerItem.ColumnNames.Select(
                            (column, i) => new QuestionDescription(headerItem.PublicKey, headerItem.Titles[i], column)));
        }
    }
}
