using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.Statistics;
using RavenQuestionnaire.Core.Entities.Statistics;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.CommandHandlers.Statistics
{
    public class GenerateQuestionnaireStatisticHandler : ICommandHandler<GenerateQuestionnaireStatisticCommand>
    {
        private IStatisticRepository statisticRepository;
        private ICompleteQuestionnaireRepository questionnaireRepository;
        public GenerateQuestionnaireStatisticHandler(IStatisticRepository repository, ICompleteQuestionnaireRepository questionnaireRepository)
        {
            this.statisticRepository = repository;
            this.questionnaireRepository = questionnaireRepository;
        }

        #region Implementation of ICommandHandler<in GenerateQuestionnaireStatisticCommand>

        public void Handle(GenerateQuestionnaireStatisticCommand command)
        {
            CompleteQuestionnaireStatistics statistics =
                this.statisticRepository.Load(IdUtil.CreateStatisticId(command.CompleteQuestionnaireId));
            var entity =
                this.questionnaireRepository.Load(IdUtil.CreateCompleteQuestionnaireId(command.CompleteQuestionnaireId));

            if (statistics == null)
            {
                statistics = new CompleteQuestionnaireStatistics(entity.GetInnerDocument());
                statisticRepository.Add(statistics);
            }
            else
            {
                statistics.UpdateStatistic(entity.GetInnerDocument());
            }
        }

        #endregion
    }
}
