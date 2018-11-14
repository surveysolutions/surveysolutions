using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Headquarters.Repositories;
using WB.Core.BoundedContexts.Headquarters.Views.DataExport;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.ValueObjects;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Headquarters.Views.Preloading
{
    public interface IQuestionnairePreloadingDataViewFactory
    {
        QuestionnairePreloadingDataItem Load(QuestionnairePreloadingDataInputModel input);
    }

    public class QuestionnairePreloadingDataViewFactory : IQuestionnairePreloadingDataViewFactory
    {
        private readonly IQuestionnaireExportStructureStorage questionnaireExportStructureStorage;

        public QuestionnairePreloadingDataViewFactory(IQuestionnaireExportStructureStorage questionnaireExportStructureStorage)
        {
            this.questionnaireExportStructureStorage = questionnaireExportStructureStorage;
        }

        public QuestionnairePreloadingDataItem Load(QuestionnairePreloadingDataInputModel input)
        {
            var questionnaire =
                this.questionnaireExportStructureStorage.GetQuestionnaireExportStructure(new QuestionnaireIdentity(input.QuestionnaireId, input.QuestionnaireVerstion));

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
                        headerItem.ColumnHeaders.Select(
                            (column, i) => new QuestionDescription(headerItem.PublicKey, column.Title, column.Name)));
        }
    }
}
